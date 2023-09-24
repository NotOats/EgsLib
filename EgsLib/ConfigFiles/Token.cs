using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using EgsLib.ConfigFiles.Ecf;

namespace EgsLib.ConfigFiles
{
    public class Token
    {
        public int Id { get; }
        public string Name { get; }

        public string CustomIcon { get; }
        public bool? DropOnDeath { get; }
        public bool? RemoveOnUse { get; }
        public string Description { get; }
        public PropertyDectoractor<int>? MarketPrice { get; }

        public Token(IEcfObject obj)
        {
            // Required
            if (obj.Type != "Token")
                throw new FormatException("IEcfObject is not a Token");

            if (!obj.ReadField("Id", out int id))
                throw new FormatException("Token has no id");

            if (!obj.ReadField("Name", out string name))
                throw new FormatException("Token has no name");

            Id = id;
            Name = name;

            // Optional
            if (obj.ReadProperty("CustomIcon", out string customIcon))
                CustomIcon = customIcon;

            if (obj.ReadProperty("DropOnDeath", out string dropOnDeath))
                DropOnDeath = dropOnDeath == "true";

            if (obj.ReadProperty("RemoveOnUse", out string removeOnUse))
                RemoveOnUse = removeOnUse == "true";

            if (obj.ReadProperty("Description", out string description))
                Description = description;

            if (obj.ReadProperty("MarketPrice", out string marketPrice) && !string.IsNullOrWhiteSpace(marketPrice))
                MarketPrice = new PropertyDectoractor<int>(marketPrice);
        }

        public static IEnumerable<Token> ReadFile(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentNullException(nameof(filePath));

            if (!File.Exists(filePath))
                throw new FileNotFoundException("ECF file not found", filePath);

            var ecf = new EcfFile(filePath);
            return ecf.ParseObjects().Select(obj => new Token(obj));
        }
    }
}
