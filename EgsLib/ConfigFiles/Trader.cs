using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using EgsLib.ConfigFiles.Ecf;
using EgsLib.ConfigFiles.Ecf.Attributes;

namespace EgsLib.ConfigFiles
{
    [EcfObject("Trader")]
    public class Trader : BaseConfig
    {
        /// <summary>
        /// The trader's internal name, this must be localized
        /// </summary>
        [EcfField("Name")]
        public string Name { get; private set; }

        /// <summary>
        /// The trader's sell text.
        /// </summary>
        [EcfProperty("SellingText")]
        public string SellingText { get; private set; }

        // TODO: Figure out what SellingGoods is exactly
        /// <summary>
        /// The trader's goods group?
        /// </summary>
        [EcfProperty("SellingGoods")]
        public string SellingGoods { get; private set; }

        /// <summary>
        /// Discount applied to the trader
        /// </summary>
        [EcfProperty("Discount")]
        public float? Discount { get; private set; }

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
    }
}
