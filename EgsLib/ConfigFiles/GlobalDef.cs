using EgsLib.ConfigFiles.Ecf;
using EgsLib.ConfigFiles.Ecf.Attributes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace EgsLib.ConfigFiles
{
    [EcfObject("GlobalDef")]
    public class GlobalDef : BaseConfig<GlobalDef>
    {
        [EcfField] public string Name { get; private set; }


        // TODO: Figure out what other possible properties belong here.
        // From vanilla
        [EcfProperty("ROF")] 
        public PropertyDecorator<float>? RateOfFire { get; private set; }

        [EcfProperty] public int? Range { get; private set; }

        [EcfProperty] public PropertyDecorator<int>? Damage { get; private set; }

        [EcfProperty] public int? MaxHealth { get; private set; }


        // RE 1.10 b62 only has these entries
        [EcfProperty] public PropertyDecorator<string>? AllowPlacingAt {  get; private set; }

        [EcfProperty] public PropertyDecorator<float>? Mass { get; private set; }

        [EcfProperty] public PropertyDecorator<float>? Volume { get; private set; }

        [EcfProperty] public PropertyDecorator<int>? MarketPrice { get; private set; }

        public GlobalDef(IEcfObject obj) : base(obj)
        {
        }

        public static IEnumerable<GlobalDef> ReadFile(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentNullException(nameof(filePath));

            if (!File.Exists(filePath))
                throw new FileNotFoundException("ECF file not found", filePath);

            var ecf = new EcfFile(filePath);
            return ecf.ParseObjects().Select(obj => new GlobalDef(obj));
        }
    }
}
