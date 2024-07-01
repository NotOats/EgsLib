using EgsLib.Playfields.Files;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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

            PlayfieldType = ParsePlayfieldType();
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

        private PlayfieldType ParsePlayfieldType()
        {
            foreach(var file in _files)
            {
                if (file is PlayfieldStatic pfStatic)
                {
                    // TODO: Read type from pfStatic
                    return pfStatic.Contents.PlayfieldType;
                }

                if (file is SpaceDynamic pfSpace)
                {
                    // TODO: Read type from pfSpace
                    return pfSpace.Contents.PlayfieldType;
                }

                if (file is GenericPlayfieldFile generic)
                {
                    if (generic.TryReadProperty("PlayfieldType", out string typeString) 
                        && Enum.TryParse(typeString, out PlayfieldType type))
                    {
                        return type;
                    }
                }
            }

            return PlayfieldType.Undefined;
        }
    }
}
