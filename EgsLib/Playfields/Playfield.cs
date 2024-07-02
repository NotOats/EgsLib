using EgsLib.Playfields.Files;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using YamlDotNet.Core;

namespace EgsLib.Playfields
{
    public class Playfield
    {
        private readonly IReadOnlyList<IPlayfieldFile> _files;

        public string Name => Path.GetFileName(Folder);

        public string Folder { get; private set; }

        public PlayfieldType PlayfieldType { get; private set; }

        public IEnumerable<IPlayfieldFile> Files => _files;

        public Playfield(string directory)
        {
            if (string.IsNullOrWhiteSpace(directory)) 
                throw new ArgumentNullException(nameof(directory));
            if (!Directory.Exists(directory)) 
                throw new DirectoryNotFoundException();

            Folder = directory;

            _files = Directory.GetFiles(directory, "*.yaml", SearchOption.TopDirectoryOnly)
                .Select(ParsePlayfieldFile)
                .Where(f => f != null)
                .ToArray();

            PlayfieldType = TryParseRootVariable("PlayfieldType", out PlayfieldType value) 
                ? value : PlayfieldType.Undefined;
        }

        public T GetPlayfieldFile<T>(string name = null) where T : class, IPlayfieldFile
        {
            bool Filter(IPlayfieldFile pf)
            {
                if (!string.IsNullOrEmpty(name) && name != pf.Name)
                    return false;

                return pf is T;
            }

            return _files.FirstOrDefault(Filter) as T;
        }

        private static IPlayfieldFile ParsePlayfieldFile(string file)
        {
            var fileName = Path.GetFileName(file);

            try
            {
                switch (fileName)
                {
                    case "space_dynamic.yaml":
                        return new SpaceDynamic(file);
                    case "playfield_dynamic.yaml":
                        return new PlayfieldDynamic(file);
                    case "playfield_static.yaml":
                        return new PlayfieldStatic(file);
                    case "playfield.yaml":
                        return new PlayfieldObsoleteFormat(file);

                    case "playfield_debug.yaml":
                    case "sun_static.yaml":
                        return new GenericPlayfieldFile(file);
                }
            }
            catch(SemanticErrorException)
            {
                // TODO: Logging
            }

            //throw new NotImplementedException($"unknown file type: {fileName}");
            return null;
        }

        private static readonly Dictionary<Type, Dictionary<string, PropertyInfo>> PropertyCache = new Dictionary<Type, Dictionary<string, PropertyInfo>>();
        public bool TryParseRootVariable<T>(string name, out T value)
        {
            foreach(var file in _files)
            {
                // Try strongly typed
                object root = null;
                switch (file)
                {
                    case PlayfieldStatic pf when file is PlayfieldStatic:
                        root = pf.Contents;
                        break;
                    case PlayfieldDynamic pf when file is PlayfieldDynamic:
                        root = pf.Contents;
                        break;
                    case PlayfieldObsoleteFormat pf when file is PlayfieldObsoleteFormat:
                        root = pf.Contents;
;                       break;
                    case SpaceDynamic pf when file is SpaceDynamic:
                        root = pf.Contents;
                        break;
                }

                if (root != null)
                {
                    var type = root.GetType();
                    if (!PropertyCache.TryGetValue(type, out Dictionary<string, PropertyInfo> properties))
                    {
                        properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                            .ToDictionary(x => x.Name, x => x);

                        PropertyCache.Add(type, properties);
                    }

                    if (properties.TryGetValue(name, out PropertyInfo property))
                    {
                        var obj = property.GetValue(root);
                        if (obj is T result)
                        {
                            value = result;
                            return true;
                        }
                    }
                }

                // Try generic
                if (file is GenericPlayfieldFile generic)
                {
                    if (generic.TryReadProperty(name, out T result))
                    {
                        value = result;
                        return true;
                    }
                }
            }

            value = default;
            return false;
        }
    }
}
