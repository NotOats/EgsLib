using System;

namespace EgsLib.ConfigFiles
{
    public readonly struct TraderItem
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
            if(parts.Length != 3 && parts.Length != 5)
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
            if(buyValue != null)
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
    }
}
