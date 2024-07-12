using EgsLib.ConfigFiles.Ecf;
using EgsLib.ConfigFiles.Ecf.Attributes;
using EgsLib.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace EgsLib.ConfigFiles
{
    [EcfObject("Container", "+Container")]
    public class Container : BaseConfig<Container>
    {
        public readonly struct ItemEntry
        {
            public string Name { get; }
            public float WeightedProbability { get; }
            public Range<int> Count { get; }
            public int TokenId { get; }

            internal ItemEntry(string name, float weightedProbability, Range<int> count, int tokenId)
            {
                Name = name;
                WeightedProbability = weightedProbability;
                Count = count;
                TokenId = tokenId;
            }
        }

        public readonly struct GroupEntry
        {
            public string Name { get; }
            public float WeightedProbability { get; }

            internal GroupEntry(string name, float weightedProbability)
            {
                Name = name;
                WeightedProbability = weightedProbability;
            }
        }

        [EcfField] public int Id { get; private set; }


        [EcfProperty("Count", typeof(Container), "ParseRange")]
        public Range<int> Count { get; private set; }

        [EcfProperty] public string Size { get; private set; }
        [EcfProperty] public string SfxOpen { get; private set; }
        [EcfProperty] public string SfxClose { get; private set; }
        [EcfProperty] public bool DestroyOnClose { get; private set; }

        public IReadOnlyList<ItemEntry> Items { get; private set; }

        public IReadOnlyList<GroupEntry> Groups { get; private set; }

        public float WeightMax { get; private set; }

        public Container(IEcfObject obj) : base(obj)
        {
            var entry = UnparsedChildren.FirstOrDefault(x => x.Name == "Items");
            if (entry == null)
                return;

            Items = entry.Properties
                .Where(x => x.Key.StartsWith("Name_"))
                .Select(x =>
                {
                    try
                    {
                        return ParseItemEntry(x.Value);
                    }
                    catch (Exception)
                    {
                        // TODO: Error logging
                        //Console.WriteLine($"Failed to parse line in Container {Id} - {x.Key}: {x.Value}");
                        return default;
                    }
                })
                .Where(x => x.Name != default)
                .ToArray();

            Groups = entry.Properties
                .Where(x => x.Key.StartsWith("Group"))
                .Select(x =>
                {
                    try
                    {
                        return ParseGroupEntry(x.Value);
                    }
                    catch(Exception)
                    {
                        // TODO: Error logging
                        //Console.WriteLine($"Failed to parse line in Container {Id} - {x.Key}: {x.Value}");
                        return default;
                    }
                })
                .Where(x => x.Name != default)
                .ToArray();

            // Mark as parsed if we extracted some entries
            if (Items.Count > 0 || Groups.Count > 0)
                MarkChildAsParsed("Items");

            WeightMax = Items.Select(x => x.WeightedProbability).Sum();
            WeightMax += Groups.Select(x => x.WeightedProbability).Sum();
        }

        public static IEnumerable<Container> ReadFile(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentNullException(nameof(filePath));

            if (!File.Exists(filePath))
                throw new FileNotFoundException("ECF file not found", filePath);

            var ecf = new EcfFile(filePath);
            return ecf.ParseObjects().Select(obj => new Container(obj));
        }

        // This entire function is real mess but then again so is packing multiple types of data into a single field
        // Next time I am just using regex...
        private static ItemEntry ParseItemEntry(string input)
        {
            var match = Regex.Match(input, @"(.+?),[ ]+param1:[ ]+([+-]?([0-9]?[.])?[0-9]+),[ ]+param2:[ ]+(.+)$");
            if (match.Groups.Count != 5)
            {
                throw new Exception("Container item regex failed to match");
            }

            var name = match.Groups[1].Value.Trim();

            if (!match.Groups[2].Value.Trim().ConvertType(out float weight))
            {
                throw new FormatException("Container item weight isn't a float");
            }

            // Parse out count/token
            var count = Range<int>.Default;
            var tokenId = -1;

            var countRaw = match.Groups[4].Value.Trim('"');
            if (countRaw.Contains("meta"))
            {
                var tokenRaw = countRaw.Replace("meta=", "");
                if (!tokenRaw.ConvertType(out int token))
                {
                    throw new FormatException("Container item meta arg isn't an int");
                }

                tokenId = token;
            }
            else
            {
                var countParts = countRaw.Split(',');
                if (countParts.Length == 2
                    && countParts[0].ConvertType(out int min)
                    && countParts[1].ConvertType(out int max))
                {
                    count = new Range<int>(min, max);
                }
                else if (countRaw.ConvertType(out int parsedCount))
                {
                    // couldn't read a min/max, maybe it's a single value?
                    count = new Range<int>(parsedCount, parsedCount);
                }
                else
                {
                    throw new FormatException("Counter item count is not well formed");
                }
            }

            return new ItemEntry(name, weight, count, tokenId);
        }

        private static GroupEntry ParseGroupEntry(string input)
        {
            // Hardcoded override for empty containers
            if (input == "empty")
            {
                return new GroupEntry("empty", 1);
            }

            var match = Regex.Match(input, @"(.+?),[ ]+param1:[ ]+([+-]?([0-9]?[.])?[0-9]+)");
            if (match.Groups.Count != 4)
            {
                throw new Exception("Container item regex failed to match");
            }

            var name = match.Groups[1].Value;
            if (match.Groups[2].Value.ConvertType(out float weight))
            {
                throw new FormatException("Container group weight isn't a float");
            }

            return new GroupEntry(name, weight);
        }

        private static bool ParseRange(string input, out object output, Type type)
        {
            output = default;

            var underlying = FindUnderlyingType(type);
            object min = null;
            object max = null;

            var parts = input.Trim(' ', '"').Split(',');
            if (parts.Length < 2)
            {
                // Could be a single digit
                if (parts[0].ConvertType(underlying, out object single))
                    min = max = single;
                else
                    return false;
            }
            else if (!parts[0].ConvertType(underlying, out min)
                || !parts[1].ConvertType(underlying, out max))
            {
                return false;
            }

            if (underlying == typeof(float))
            {
                output = new Range<float>((float)min, (float)max);
                return true;
            }

            if (underlying == typeof(int))
            {
                output = new Range<int>((int)min, (int)max);
                return true;
            }

            return false;
        }

        private static Type FindUnderlyingType(Type type)
        {
            var underlying = type;

            // Unwrap nullable
            var nullable = Nullable.GetUnderlyingType(type);
            if (nullable != null)
                underlying = nullable;

            // Unwrap Range
            if (underlying.IsGenericType && !underlying.IsGenericTypeDefinition)
            {
                var genericType = underlying.GetGenericTypeDefinition();

                if (ReferenceEquals(genericType, typeof(Range<>)))
                    underlying = underlying.GetGenericArguments()[0];
            }

            return underlying;
        }
    }
}
