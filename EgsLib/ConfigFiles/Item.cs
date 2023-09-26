using EgsLib.ConfigFiles.Ecf;
using EgsLib.ConfigFiles.Ecf.Attributes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace EgsLib.ConfigFiles
{
    [EcfObject("Item", "+Item")]
    public class Item : BaseConfig<Item>
    {
        [EcfField] public int Id { get; private set; }

        [EcfField] public string Name { get; private set; }

        [EcfField] public string Ref { get; private set; }

        [EcfProperty("Meshfile")]
        public string MeshFile { get; private set; }

        [EcfProperty] public float LifetimeOnDrop { get; private set; }

        [EcfProperty] public PropertyDecorator<float> MarketPrice { get; private set; }

        [EcfProperty] public string Material { get; private set; }

        [EcfProperty("Canhold")]
        public bool CanHold { get; private set; }

        [EcfProperty] public int HoldType { get; private set; }

        [EcfProperty] public bool PickupToToolbar { get; private set; }

        [EcfProperty("Candrop")]
        public bool CanDrop { get; private set; }

        [EcfProperty] public int StackSize { get; private set; }

        [EcfProperty] public string ShowUser { get; private set; }

        [EcfProperty] public string Category { get; private set; }

        [EcfProperty] public bool CreatesLight { get; private set; }

        [EcfProperty] public PropertyDecorator<float> Mass { get; private set; }

        [EcfProperty] public PropertyDecorator<float> Volume { get; private set; }

        [EcfProperty] public PropertyDecorator<int> UnlockCost { get; private set; }

        [EcfProperty] public PropertyDecorator<int> UnlockLevel { get; private set; }

        [EcfProperty] public string TechTreeNames { get; private set; }

        public Item(IEcfObject obj) : base(obj)
        {
        }

        public static IEnumerable<Item> ReadFile(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentNullException(nameof(filePath));

            if (!File.Exists(filePath))
                throw new FileNotFoundException("ECF file not found", filePath);

            var ecf = new EcfFile(filePath);
            return ecf.ParseObjects().Select(obj => new Item(obj));
        }
    }
}
