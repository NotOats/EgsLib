using EgsLib.ConfigFiles.Ecf;
using EgsLib.ConfigFiles.Ecf.Attributes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace EgsLib.ConfigFiles
{
    [EcfObject("BlockGroup")]
    public class BlockGroup : BaseConfig<BlockGroup>
    {
        [EcfField] public string Name { get; private set; }

        [EcfProperty] public int MaxCount { get; private set; }

        public IReadOnlyList<string> Blocks { get; private set; }

        public BlockGroup(IEcfObject obj) : base(obj)
        {
            Blocks = ParseBlocks();
        }

        public static IEnumerable<BlockGroup> ReadFile(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentNullException(nameof(filePath));

            if (!File.Exists(filePath))
                throw new FileNotFoundException("ECF file not found", filePath);

            var ecf = new EcfFile(filePath);
            return ecf.ParseObjects().Select(obj => new BlockGroup(obj));
        }

        private IReadOnlyList<string> ParseBlocks()
        {
            if (!UnparsedProperties.TryGetValue("Blocks", out string compounded))
                return Array.Empty<string>();

            MarkAsParsed("Blocks");
            return compounded.Trim('"').Split(',');
        }
    }
}
