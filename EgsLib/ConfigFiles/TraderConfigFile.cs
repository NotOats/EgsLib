using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace EgsLib.ConfigFiles
{

    public class TraderConfigFile
    {
        private readonly Dictionary<string, Trader> _traders = new Dictionary<string, Trader>();

        public string FilePath { get; }

        public IReadOnlyDictionary<string, Trader> Traders => _traders;

        public TraderConfigFile(string filePath)
        {
            FilePath = filePath ?? throw new ArgumentNullException(nameof(filePath));

            if (!File.Exists(FilePath))
                throw new FileNotFoundException("ECF file not found", FilePath);

            ReadTraders();
        }

        private void ReadTraders()
        {
            var ecf = new EcfFile(FilePath);

            foreach (var obj in ecf.ParseObjects())
            {
                var trader = CreateTrader(obj);
                _traders.Add(trader.Name, trader);
            }
        }

        private static Trader CreateTrader(IEcfObject obj)
        {
            // Read required
            if (obj.Type != "Trader")
                throw new FormatException("File contains non-Trader objects");

            if (!obj.Fields.TryGetValue("Name", out string name))
                throw new FormatException("Trader object has no name");

            // Read optional
            obj.Properties.TryGetValue("SellingText", out string sellingText);
            obj.Properties.TryGetValue("SellingGoods", out string sellingGoods);
            obj.Properties.TryGetValue("Discount", out string discount);

            // Read items
            var items = new List<TraderItem>();
            var kvps = obj.Properties
                .Where(kvp => kvp.Key.StartsWith("Item"))
                .OrderBy(kvp => kvp.Key);

            foreach(var line in kvps.Select(kvp =>kvp.Value.Trim(' ', '"')))
            {
                var trader = new TraderItem(line);
                items.Add(trader);
            }

            return new Trader(name, sellingText?.Trim('"'), sellingGoods?.Trim('"'), discount?.Trim('"'), items);
        }
    }
}
