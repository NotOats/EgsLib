using EgsLib.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using YamlDotNet.Serialization;

namespace EgsLib.Playfields.Files
{
    public class GenericPlayfieldFile : IPlayfieldFile
    {
        private static readonly IDeserializer Deserializer = new DeserializerBuilder().Build();

        public Dictionary<string, object> Properties { get; private set; }

        public string Name { get; private set; }

        public string File { get; private set; }

        public GenericPlayfieldFile(string file)
        {
            if (string.IsNullOrWhiteSpace(file))
                throw new ArgumentNullException(nameof(file));

            if (!System.IO.File.Exists(file))
                throw new FileNotFoundException();

            Name = Path.GetFileName(file);
            File = file;

            using (var reader = System.IO.File.OpenText(file))
            {
                Properties = Deserializer.Deserialize<Dictionary<string, object>>(reader);
            }
        }

        public bool TryReadProperty<T>(string name, out T value)
        {
            var index = name.IndexOf('.');
            if (index == -1)
                return TryReadSingleProperty(name, Properties, out value);
            else
                return TryReadNestedProperty(name, Properties, out value);
        }

        private static bool TryReadSingleProperty<T>(string name, Dictionary<string, object> properties, out T value)
        {
            if (!properties.TryGetValue(name, out object obj))
            {
                value = default;
                return false;
            }

            // Fast return, found exactly what we wanted
            if (obj is T)
            {
                value = (T)obj;
                return true;
            }

            // Some value types seem to be read from the yaml as strings, try to convert them.
            if (obj is string)
            {
                var str = (string)obj;
                if (str.ConvertType(typeof(T), out object converted))
                {
                    value = (T)converted;
                    return true;
                }
            }

            value = default;
            return false;
        }

        private static bool TryReadNestedProperty<T>(string name, Dictionary<string, object> properties, out T value)
        {
            var index = name.IndexOf('.');

            // We're done parsing
            if (index == -1)
            {
                return TryReadSingleProperty(name, properties, out value);
            }

            // Reads as dict<object, object> gotta convert it...
            if (!TryReadSingleProperty(name.Substring(0, index), properties, out Dictionary<object, object> nestedValue))
            {
                value = default;
                return false;
            }

            var nested = nestedValue.ToDictionary(x => x.Key as string, x => x.Value);
            if (nested.Any(x => x.Key == null))
            {
                value = default;
                return false;
            }

            return TryReadNestedProperty(name.Substring(name.IndexOf(".") + 1), nested, out value);
        }
    }
}
