using EgsLib.ConfigFiles.Ecf;
using EgsLib.ConfigFiles.Ecf.Attributes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace EgsLib.ConfigFiles
{
    [EcfObject("Template", "+Template")]
    public class Template : BaseConfig
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

        [EcfField]
        public string Name { get; private set; }


        [EcfProperty]
        public bool? BaseItem { get; private set; }

        [EcfProperty("DeconOverride", typeof(Template), "ParseDeconOverride")]
        public DeconstructorOverride DeconOverride { get; private set; }

        [EcfProperty]
        public int? OutputCount { get; private set; }

        [EcfProperty]
        public int? CraftTime { get; private set; }

        [EcfProperty("Target", typeof(Template), "ParseTarget")]
        public Constructors Target { get; private set; }

        public IReadOnlyDictionary<string, int> Inputs { get; }

        public Template(IEcfObject obj) : base(obj)
        {
            Inputs = UnparsedChildren.FirstOrDefault(x => x.Name == "Inputs")?.Properties?
                .ToDictionary(x => x.Key, x => int.Parse(x.Value)) ?? new Dictionary<string, int>();
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

        private static bool ParseDeconOverride(string input, out object output, Type type)
        {
            switch (input)
            {
                case "Continue":
                    output = DeconstructorOverride.Continue;
                    return true;
                case "Stop":
                    output = DeconstructorOverride.Stop;
                    return true;
                default:
                    output = DeconstructorOverride.None;
                    return true;
            }
        }

        private static bool ParseTarget(string input, out object output, Type type)
        {
            var constructors = Constructors.None;
            var parts = input.SplitWithQuotes(',');

            foreach (var part in parts)
            {
                switch (part)
                {
                    case "SuitC": constructors &= Constructors.Suit; break;
                    case "SurvC": constructors &= Constructors.Survival; break;
                    case "SmallC": constructors &= Constructors.SmallSv; break;
                    case "HoverC": constructors &= Constructors.HoverHv; break;
                    case "BaseC": constructors &= Constructors.Base; break;
                    case "LargeC": constructors &= Constructors.Large; break;
                    case "AdvC": constructors &= Constructors.Advanced; break;
                    case "FoodP": constructors &= Constructors.Food; break;
                    case "Furn": constructors &= Constructors.Furnace; break;
                }
            }

            output = constructors;
            return true;
        }
    }
}
