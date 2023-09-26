using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using EgsLib.ConfigFiles.Ecf;
using EgsLib.ConfigFiles.Ecf.Attributes;

namespace EgsLib.ConfigFiles
{
    [EcfObject("Token")]
    public class Token : BaseConfig
    {
        [EcfField]
        public int Id { get; private set; }

        [EcfField]
        public string Name { get; private set; }


        [EcfProperty]
        public string CustomIcon { get; private set; }

        [EcfProperty]
        public bool? DropOnDeath { get; private set; }

        [EcfProperty]
        public bool? RemoveOnUse { get; private set; }

        [EcfProperty]
        public string Description { get; private set; }

        [EcfProperty]
        public PropertyDecorator<int>? MarketPrice { get; private set; }

        public Token(IEcfObject obj) : base(obj)
        {
        }

        public static IEnumerable<Token> ReadFile(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentNullException(nameof(filePath));

            if (!File.Exists(filePath))
                throw new FileNotFoundException("ECF file not found", filePath);

            var ecf = new EcfFile(filePath);
            return ecf.ParseObjects().Select(obj => new Token(obj));
        }
    }
}
