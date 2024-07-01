using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using YamlDotNet.Serialization;

namespace EgsLib.Playfields.Files
{
    public class BasePlayfieldFile<TContents> : IPlayfieldFile 
    {
        private static readonly IDeserializer Deserializer = 
            new DeserializerBuilder()
                .IgnoreUnmatchedProperties()
                .Build();

        public string Name { get; private set; }

        public string File { get; private set; }

        public TContents Contents { get; private set; }

        public BasePlayfieldFile(string file)
        {
            if (string.IsNullOrWhiteSpace(file))
                throw new ArgumentNullException(nameof(file));

            if (!System.IO.File.Exists(file))
                throw new FileNotFoundException();

            Name = Path.GetFileName(file);
            File = file;

            using (var reader = System.IO.File.OpenText(file))
            {
                Contents = Deserializer.Deserialize<TContents>(reader);
            }
        }
    }
}
