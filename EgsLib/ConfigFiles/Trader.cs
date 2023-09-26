using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using EgsLib.ConfigFiles.Ecf;
using EgsLib.ConfigFiles.Ecf.Attributes;

namespace EgsLib.ConfigFiles
{
    [EcfObject("Trader")]
    public class Trader : BaseConfig<Trader>
    {
        /// <summary>
        /// The trader's internal name, this must be localized
        /// </summary>
        [EcfField] public string Name { get; private set; }

        /// <summary>
        /// The trader's sell text.
        /// </summary>
        [EcfProperty] public string SellingText { get; private set; }

        // TODO: Figure out what SellingGoods is exactly
        /// <summary>
        /// The trader's goods group?
        /// </summary>
        [EcfProperty] public string SellingGoods { get; private set; }

        /// <summary>
        /// Discount applied to the trader
        /// </summary>
        [EcfProperty] public float? Discount { get; private set; }

        /// <summary>
        /// Items the trader buys and sells
        /// </summary>
        public IReadOnlyList<TraderItem> Items { get; }

        /// <summary>
        /// Items the trader buys
        /// </summary>
        public IEnumerable<TraderItem> Buys => Items.Where(i => i.BuyAmount != Range<int>.Default);

        /// <summary>
        /// Items the trader sells
        /// </summary>
        public IEnumerable<TraderItem> Sells => Items.Where(i => i.SellAmount != Range<int>.Default);

        public Trader(IEcfObject obj) : base(obj)
        {
            Items = UnparsedProperties
                .Where(kvp => kvp.Key.StartsWith("Item"))
                .Select(kvp => new TraderItem(kvp.Value.Trim(' ', '"')))
                .ToArray();
        }

        public static IEnumerable<Trader> ReadFile(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentNullException(nameof(filePath));

            if (!File.Exists(filePath))
                throw new FileNotFoundException("ECF file not found", filePath);

            var ecf = new EcfFile(filePath);
            return ecf.ParseObjects().Select(obj => new Trader(obj));
        }

        public readonly struct TraderItem : IEquatable<TraderItem>
        {
            /// <summary>
            /// The item's name
            /// </summary>
            public string Name { get; }

            /// <summary>
            /// Flag if this item uses a market factor for <see cref="SellValue"/>
            /// </summary>
            public bool SellMarketFactor { get; }

            /// <summary>
            /// Sell value or market factor
            /// </summary>
            public Range<float> SellValue { get; }

            /// <summary>
            /// Quantity of items this trader sells
            /// </summary>
            public Range<int> SellAmount { get; }

            /// <summary>
            /// Flag if this item uses a market factor for <see cref="BuyValue"/>
            /// </summary>
            public bool BuyMarketFactor { get; }

            /// <summary>
            /// Buy value or market factor
            /// </summary>
            public Range<float> BuyValue { get; }

            /// <summary>
            /// Quantity of items this trader will buy
            /// </summary>
            public Range<int> BuyAmount { get; }

            /// <summary>
            /// Create a TraderItem instance from the specific TraderNPcConfig.ecf line
            /// Format: Item name, sell price range, available stock range[, buy price range, max stock range]
            /// Note: price range may be prefixed with "mf=" to specify market factor instead of static price
            /// </summary>d
            /// <param name="traderConfigLine"></param>
            /// <exception cref="FormatException"></exception>
            internal TraderItem(string traderConfigLine)
            {
                var parts = traderConfigLine.Split(',');
                if (parts.Length != 3 && parts.Length != 5)
                    throw new FormatException("Invalid TraderItem line format");

                Name = parts[0].Trim();
                var sellValue = parts[1].Trim();
                var sellAmount = parts[2].Trim();
                var buyValue = parts.Length == 5 ? parts[3].Trim() : null;
                var buyAmount = parts.Length == 5 ? parts[4].Trim() : null;

                SellAmount = ParseAmount(sellAmount);
                SellValue = ParseValue(sellValue, out bool sellMarketFactor);
                SellMarketFactor = sellMarketFactor;

                BuyAmount = buyAmount != null ? ParseAmount(buyAmount) : Range<int>.Default;
                if (buyValue != null)
                {
                    BuyValue = ParseValue(buyValue, out bool buyMarketFactor);
                    BuyMarketFactor = buyMarketFactor;
                }
                else
                {
                    BuyValue = Range<float>.Default;
                    BuyMarketFactor = false;
                }
            }

            private static Range<float> ParseValue(string text, out bool marketFactor)
            {
                marketFactor = false;

                var parts = text.Split('-');
                if (parts.Length != 2)
                {
                    // Could be single value range
                    if (float.TryParse(parts[0], out float val))
                        return new Range<float>(val, val);
                    else
                        throw new FormatException("Invalid item range");
                }


                if (parts[0].StartsWith("mf="))
                {
                    marketFactor = true;
                    parts[0] = parts[0].Substring(3);
                }

                var min = float.Parse(parts[0]);
                var max = float.Parse(parts[1]);

                return new Range<float>(min, max);
            }

            private static Range<int> ParseAmount(string text)
            {
                var parts = text.Split('-');
                if (parts.Length != 2)
                {
                    // Could be single value range
                    if (int.TryParse(parts[0], out int val))
                        return new Range<int>(val, val);
                    else
                        throw new FormatException("Invalid item range");
                }

                var min = int.Parse(parts[0]);
                var max = int.Parse(parts[1]);

                return new Range<int>(min, max);
            }
            public override string ToString()
            {
                var sell = SellMarketFactor ?
                    $"{SellValue.Minimum:0.##}-{SellValue.Maximum:0.##}, {SellAmount.Minimum}-{SellAmount.Maximum}" :
                    $"{(int)SellValue.Minimum}-{(int)SellValue.Maximum}, {SellAmount.Minimum}-{SellAmount.Maximum}";

                var buy = BuyMarketFactor ?
                    $"{BuyValue.Minimum:0.##}-{BuyValue.Maximum:0.##}, {BuyAmount.Minimum}-{BuyAmount.Maximum}" :
                    $"{(int)BuyValue.Minimum}-{(int)BuyValue.Maximum}, {BuyAmount.Minimum}-{BuyAmount.Maximum}";

                return $"{Name}, {sell}, {buy}";
            }

            public override bool Equals(object obj)
            {
                return obj is TraderItem item && Equals(item);
            }

            public bool Equals(TraderItem other)
            {
                return Name == other.Name &&
                       SellMarketFactor == other.SellMarketFactor &&
                       SellValue.Equals(other.SellValue) &&
                       SellAmount.Equals(other.SellAmount) &&
                       BuyMarketFactor == other.BuyMarketFactor &&
                       BuyValue.Equals(other.BuyValue) &&
                       BuyAmount.Equals(other.BuyAmount);
            }

            public override int GetHashCode()
            {
                int hashCode = -1897246606;
                hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Name);
                hashCode = hashCode * -1521134295 + SellMarketFactor.GetHashCode();
                hashCode = hashCode * -1521134295 + SellValue.GetHashCode();
                hashCode = hashCode * -1521134295 + SellAmount.GetHashCode();
                hashCode = hashCode * -1521134295 + BuyMarketFactor.GetHashCode();
                hashCode = hashCode * -1521134295 + BuyValue.GetHashCode();
                hashCode = hashCode * -1521134295 + BuyAmount.GetHashCode();
                return hashCode;
            }

            public static bool operator ==(TraderItem left, TraderItem right)
            {
                return left.Equals(right);
            }

            public static bool operator !=(TraderItem left, TraderItem right)
            {
                return !(left == right);
            }
        }
    }
}
