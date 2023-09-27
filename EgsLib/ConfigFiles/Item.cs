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

        [EcfProperty] public string DropMeshfile { get; private set; }

        [EcfProperty] public float? LifetimeOnDrop { get; private set; }

        [EcfProperty] public PropertyDecorator<float>? MarketPrice { get; private set; }

        [EcfProperty] public string Material { get; private set; }

        [EcfProperty("Canhold")]
        public bool? CanHold { get; private set; }

        [EcfProperty] public int? HoldType { get; private set; }

        [EcfProperty] public bool? PickupToToolbar { get; private set; }

        [EcfProperty("Candrop")]
        public bool? CanDrop { get; private set; }

        [EcfProperty] public int? StackSize { get; private set; }

        [EcfProperty] public string ShowUser { get; private set; }

        [EcfProperty] public string Category { get; private set; }

        [EcfProperty] public bool? CreatesLight { get; private set; }

        [EcfProperty] public PropertyDecorator<string>? Info { get; private set; }

        [EcfProperty] public string AutoFill { get; private set; }

        [EcfProperty] public PropertyDecorator<float>? Mass { get; private set; }

        [EcfProperty] public PropertyDecorator<float>? Volume { get; private set; }

        [EcfProperty] public PropertyDecorator<int>? UnlockCost { get; private set; }

        [EcfProperty] public PropertyDecorator<int>? UnlockLevel { get; private set; }

        [EcfProperty] public string TechTreeParent { get; private set; }

        [EcfProperty] public string TechTreeNames { get; private set; }

        [EcfProperty] public PropertyDecorator<int>? Damage { get; private set; }

        [EcfProperty] public PropertyDecorator<int>? BlastRadius { get; private set; }

        [EcfProperty] public PropertyDecorator<int>? BlastDamage { get; private set; }

        [EcfProperty] public PropertyDecorator<int>? Durability { get; private set; }

        [EcfProperty("DegradationProb")] 
        public PropertyDecorator<float>? DegradationProbability { get; private set; }

        [EcfProperty] public PropertyDecorator<bool>? RepairDisabled { get; private set; }

        [EcfProperty] public PropertyDecorator<int>? RepairCount { get; private set; }

        [EcfProperty] public string SfxJammed { get; private set; }

        [EcfProperty] public string Crosshair { get; private set; }

        [EcfProperty] public bool? RadialMenu { get; private set; }

        [EcfProperty] public string CustomIcon { get; private set; }

        [EcfProperty] public bool? OverrideTradingConstraints { get; private set; }

        [EcfProperty] public PropertyDecorator<int>? FuelValue { get; private set; }

        [EcfProperty("O2Value")]
        public PropertyDecorator<int>? OxygenValue { get; private set; }

        [EcfProperty] public PropertyDecorator<string>? AllowPlacingAt { get; private set; }

        [EcfProperty] public string Model { get; private set; }

        [EcfProperty] public int? XpFactor { get; private set; }

        [EcfProperty] public bool? NoRepairInputItem { get; private set; }

        [EcfProperty] public PropertyDecorator<int>? Credits { get; private set; }

        [EcfProperty] public string MapIcon { get; private set; }

        [EcfProperty] public PropertyDecorator<int>? FoodDecayTime { get; private set; }

        [EcfProperty] public string FoodDecayedItem { get; private set; }

        [EcfProperty] public string Class { get; private set; }

        [EcfProperty] public string RecipeName { get; private set; }

        [EcfProperty] public PropertyDecorator<float>? VolumeCapacity { get; private set; }

        [EcfProperty] public PropertyDecorator<float>? DegradationFac {  get; private set; }

        [EcfProperty] public PropertyDecorator<int>? Oxygen { get; private set; }

        [EcfProperty] public PropertyDecorator<int>? Armor { get; private set; }

        [EcfProperty] public PropertyDecorator<int>? Heat { get; private set; }

        [EcfProperty] public PropertyDecorator<int>? Cold { get; private set; }

        [EcfProperty] public PropertyDecorator<int>? Radiation { get; private set; }

        [EcfProperty] public PropertyDecorator<float>? FallDamageFac { get; private set; }

        [EcfProperty] public PropertyDecorator<float>? PowerFac { get; private set; }

        [EcfProperty] public PropertyDecorator<float>? SpeedFac { get; private set; }

        [EcfProperty] public PropertyDecorator<float>? JumpFac { get; private set; }

        [EcfProperty] public PropertyDecorator<float>? JetpackFac { get; private set; }

        [EcfProperty] public PropertyDecorator<float>? FoodFac { get; private set; }

        [EcfProperty] public PropertyDecorator<float>? StaminaFac { get; private set; }

        [EcfProperty("NrSlots")]
        public PropertyDecorator<int>? NumberOfSlots { get; private set; }

        [EcfProperty] public string SlotItems { get; private set; }

        [EcfProperty] public bool? IsEquippableWithoutLocker { get; private set; }

        [EcfProperty] public bool? IsNightVisionItem { get; private set; }

        [EcfProperty] public bool? IsOreScannerItem { get; private set; }

        [EcfProperty] public PropertyDecorator<int>? Range { get; private set; }

        [EcfProperty] public PropertyDecorator<int>? RangeSpace { get; private set; }

        [EcfProperty] public bool? DurabilityBreaksAfter { get; private set; }

        [EcfProperty] public string BlastParticleName { get; private set; }

        [EcfProperty] public string PropHead { get; private set; }

        [EcfProperty] public bool? DropOnDeath { get; private set; }

        [EcfProperty] public bool? IsRadarItem { get; private set; }

        [EcfProperty] public string GlobalRef { get; private set; }

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
