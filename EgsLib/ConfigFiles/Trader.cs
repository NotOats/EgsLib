using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using EgsLib.ConfigFiles.Ecf;

namespace EgsLib.ConfigFiles
{
    public class Trader
    {
        /// <summary>
        /// The trader's internal name, this must be localized
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The trader's sell text.
        /// </summary>
        public string SellingText { get; }

        // TODO: Figure out what SellingGoods is exactly
        /// <summary>
        /// The trader's goods group?
        /// </summary>
        public string SellingGoods { get; }

        /// <summary>
        /// Discount applied to the trader
        /// </summary>
        public float? Discount { get; }

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

        public Trader(IEcfObject obj)
        {
            // Required
            if (obj.Type != "Trader")
                throw new FormatException("IEcfObject is not a Trader");

            if (!obj.ReadField("Name", out string name))
                throw new FormatException("Trader has no name");

            Name = name;

            // Optional
            if (obj.ReadProperty("SellingText", out string sellingText))
                SellingText = sellingText;

            if (obj.ReadProperty("SellingGoods", out string sellingGoods))
                SellingGoods = sellingGoods;

            if (obj.ReadProperty("Discount", out float discount))
                Discount = discount;

            Items = obj.Properties
                .Where(kvp => kvp.Key.StartsWith("Item"))
                .Select(kvp => new TraderItem(kvp.Value.Trim(' ', '"')))
                .ToList();
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
    }
}
