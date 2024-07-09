using EgsLib.ConfigFiles.Ecf;
using EgsLib.ConfigFiles.Ecf.Attributes;
using EgsLib.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace EgsLib.ConfigFiles
{
    [EcfObject("LootGroup", "+LootGroup")]
    public class LootGroup : BaseConfig<LootGroup>
    {
        [EcfField] public string Name { get; private set; }

        [EcfProperty("Count", typeof(LootGroup), "ParseIntRange")]
        public Range<int> Count { get; private set; }

        public IReadOnlyList<LootItem> Items { get; }

        public LootGroup(IEcfObject obj) : base(obj)
        {
            var items = UnparsedProperties
                .Where(kvp => kvp.Key.StartsWith("Item_"))
                .ToDictionary(x => x.Key, x => x.Value);

            Items = items
                .OrderBy(kvp => kvp.Key)
                .Select(kvp =>
                {
                    MarkAsParsed(kvp.Key);
                    return new LootItem(kvp.Value);
                })
                .ToArray();
        }

        private static bool ParseIntRange(string input, out object output, Type type)
        {
            output = default;

            if (type != typeof(Range<int>))
                return false;

            // hard coded "all"
            if (input == "all")
            {
                output = new Range<int>(1, int.MaxValue);
                return true;
            }

            var parts = input.Trim('"').Split(',');
            if (parts.Length == 1 && parts[0].ConvertType(out int single))
            {
                // single value
                output = new Range<int>(single, single);
                return true;
            }
             
            if (parts.Length == 2 
                && parts[0].ConvertType(out int min) 
                && parts[1].ConvertType(out int max))
            {
                // min/max pair
                output = new Range<int>(min, max);
                return true;
            }

            return false;
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
            // TODO: Figure out how to represent Count.
            // Count can be 1 or "1,2" or "meta=1"
            public string Count { get; }
            public float? Weight { get; }

            internal LootItem(string line)
            {
                var parts = line.SplitWithQuotes(',').Select(s => s.Trim(' ')).ToArray();
                if (parts.Length != 2 && parts.Length != 3)
                    throw new FormatException("Invalid item format in LootGroup");

                // Name & Count
                Name = parts[0];
                Count = SplitParam(parts[1]);

                // Weight
                if (parts.Length != 3)
                {
                    Weight = null;
                    return;
                }

                if (!SplitParam(parts[2]).ConvertType(out float value))
                    throw new FormatException("Invalid weight in LootGroup");

                Weight = value;
            }

            private static string SplitParam(string param)
            {
                var parts = param.SplitWithQuotes(':').ToArray();
                if (parts.Length != 2)
                    throw new FormatException("Invalid item param format in LootGroup");

                return parts[1].Trim();
            }

            public override string ToString()
            {
                var parts = new List<string> { Name };

                if (!string.IsNullOrWhiteSpace(Count))
                    parts.Add($"Count: {Count}");

                if (Weight != null)
                    parts.Add($"Weight: {Weight}");

                return string.Join(", ", parts);
            }

            public override bool Equals(object obj)
            {
                return obj is LootItem item && Equals(item);
            }

            public bool Equals(LootItem other)
            {
                return Name == other.Name &&
                       Count == other.Count &&
                       Weight == other.Weight;
            }

            public override int GetHashCode()
            {
                int hashCode = 1697885511;
                hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Name);
                hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Count);
                hashCode = hashCode * -1521134295 + EqualityComparer<float?>.Default.GetHashCode(Weight);
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
