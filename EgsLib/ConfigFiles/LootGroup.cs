using EgsLib.ConfigFiles.Ecf;
using EgsLib.ConfigFiles.Ecf.Attributes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace EgsLib.ConfigFiles
{
    [EcfObject("LootGroup", "+LootGroup")]
    public class LootGroup : BaseConfig
    {
        [EcfField("Name")]
        public string Name { get; private set; }


        [EcfProperty("Count")]
        public string Count { get; private set; }

        public IReadOnlyList<LootItem> Items { get; }

        public LootGroup(IEcfObject obj) : base(obj)
        {
            Items = UnparsedProperties
                .OrderBy(kvp => kvp.Key)
                .Where(kvp => kvp.Key.StartsWith("Item_"))
                .Select(kvp => new LootItem(kvp.Value))
                .ToArray();
        }

        public static IEnumerable<LootGroup> ReadFile(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentNullException(nameof(filePath));

            if (!File.Exists(filePath))
                throw new FileNotFoundException("ECF file not found", filePath);

            var ecf = new EcfFile(filePath);
            return ecf.ParseObjects().Select(obj => new LootGroup(obj));
        }

        public readonly struct LootItem : IEquatable<LootItem>
        {
            public string Name { get; }
            public string Param1 { get; }
            public string Param2 { get; }

            internal LootItem(string line)
            {
                var parts = line.SplitWithQuotes(',').Select(s => s.Trim(' ')).ToArray();
                if (parts.Length != 2 && parts.Length != 3)
                    throw new FormatException("Invalid item format in LootGroup");

                Name = parts[0];
                Param1 = SplitParam(parts[1]);

                if (parts.Length == 3)
                    Param2 = SplitParam(parts[2]);
                else
                    Param2 = null;
            }

            private static string SplitParam(string param)
            {
                var parts = param.SplitWithQuotes(':').ToArray();
                if(parts.Length != 2)
                    throw new FormatException("Invalid item param format in LootGroup");

                return parts[1].Trim();
            }

            public override string ToString()
            {
                var parts = new List<string> { Name };

                if(!string.IsNullOrWhiteSpace(Param1))
                    parts.Add($"param1: {Param1}");

                if (!string.IsNullOrWhiteSpace(Param2))
                    parts.Add($"param2: {Param2}");

                return string.Join(", ", parts);
            }

            public override bool Equals(object obj)
            {
                return obj is LootItem item && Equals(item);
            }

            public bool Equals(LootItem other)
            {
                return Name == other.Name &&
                       Param1 == other.Param1 &&
                       Param2 == other.Param2;
            }

            public override int GetHashCode()
            {
                int hashCode = 1697885511;
                hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Name);
                hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Param1);
                hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Param2);
                return hashCode;
            }

            public static bool operator ==(LootItem left, LootItem right)
            {
                return left.Equals(right);
            }

            public static bool operator !=(LootItem left, LootItem right)
            {
                return !(left == right);
            }
        }
    }
}
