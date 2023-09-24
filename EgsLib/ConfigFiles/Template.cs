using EgsLib.ConfigFiles.Ecf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace EgsLib.ConfigFiles
{
    public  class Template
    {
        [Flags]
        public enum Constructors
        {
            None = 0,
            Suit = 1,
            Survival = 4,
            SmallSv = 8,
            HoverHv = 16,
            Base = 32,
            Large = 64,
            Advanced = 128,
            Food = 256,
            Furnace = 512
        }

        public enum DeconstructorOverride
        {
            None,
            Continue,
            Stop
        }

        public string Name { get; }


        public bool? BaseItem { get; }
        public DeconstructorOverride DeconOverride { get; }
        public int? OutputCount { get; }
        public int? CraftTime { get; }
        public Constructors Target { get; }
        public IReadOnlyDictionary<string, int> Inputs { get; }

        public Template(IEcfObject obj)
        {
            // Required
            if (obj.Type != "Template" && obj.Type != "+Template")
                throw new FormatException("IEcfObject is not a Template");

            if (!obj.ReadField("Name", out string name))
                throw new FormatException("Template has no name");

            Name = name;

            // Optional
            if (obj.ReadProperty("BaseItem", out string baseItem))
                BaseItem = baseItem == "true";

            if (obj.ReadProperty("OutputCount", out int outputCount))
                OutputCount = outputCount;

            if (obj.ReadProperty("CraftTime", out int craftTime))
                CraftTime = craftTime;

            if (obj.ReadProperty("DeconOverride", out string deconOverride))
                DeconOverride = DeconOverrideFromString(deconOverride);

            if (obj.ReadProperty("Target", out string target))
                Target = ConstructorsFromString(target);

            Inputs = obj.Children.FirstOrDefault(x => x.Name == "Inputs")?.Properties?
                .ToDictionary(x => x.Key, x => int.Parse(x.Value)) ?? new Dictionary<string, int>();
        }

        private static DeconstructorOverride DeconOverrideFromString(string str)
        {
            switch (str)
            {
                case "Continue": 
                    return DeconstructorOverride.Continue;
                case "Stop": 
                    return DeconstructorOverride.Stop;
                default: 
                    return DeconstructorOverride.None;
            }
        }

        private static Constructors ConstructorsFromString(string str)
        {
            var output = Constructors.None;

            var parts = str.SplitWithQuotes(',');
            foreach (var part in parts)
            {
                switch (part)
                {
                    case "SuitC": output &= Constructors.Suit; break;
                    case "SurvC": output &= Constructors.Survival; break;
                    case "SmallC": output &= Constructors.SmallSv; break;
                    case "HoverC": output &= Constructors.HoverHv; break;
                    case "BaseC": output &= Constructors.Base; break;
                    case "LargeC": output &= Constructors.Large; break;
                    case "AdvC": output &= Constructors.Advanced; break;
                    case "FoodP": output &= Constructors.Food; break;
                    case "Furn": output &= Constructors.Furnace; break;
                }
            }

            return output;
        }

        public static IEnumerable<Template> ReadFile(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentNullException(nameof(filePath));

            if (!File.Exists(filePath))
                throw new FileNotFoundException("ECF file not found", filePath);

            var ecf = new EcfFile(filePath);
            return ecf.ParseObjects().Select(obj => new Template(obj));
        }
    }
}
