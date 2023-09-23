using System;
using System.Collections.Generic;
using System.Linq;

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
        public float Discount { get; }

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

        internal Trader(string name, string sellingText, string sellingGoods, string discount, List<TraderItem> items)
        {
            // Required
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Items = items ?? throw new ArgumentNullException(nameof(items));

            // Optional
            SellingText = sellingText;
            SellingGoods = sellingGoods;
            
            if(!string.IsNullOrWhiteSpace(discount) && float.TryParse(discount, out float val))
                Discount = val;
            else
                Discount = 0;
        }
    }
}
