using EgsLib.ConfigFiles.Ecf;
using EgsLib.ConfigFiles.Ecf.Attributes;
using EgsLib.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace EgsLib.ConfigFiles
{
    [EcfObject("Block", "+Block")]
    public class Block : BaseConfig<Block>
    {
        [EcfField] public int Id { get; private set; }

        [EcfField] public string Name { get; private set; }

        [EcfField("Ref")] 
        public string Reference { get; private set; }

        #region Properties from BlocksConfig.ecf comments
        [EcfProperty] public string CustomIcon { get; private set; }
        [EcfProperty] public string PickupTarget { get; private set; }
        [EcfProperty] public string TemplateRoot { get; private set; }
        [EcfProperty] public PropertyDecorator<string>? UpgradeTo { get; private set; }
        [EcfProperty] public PropertyDecorator<string>? DowngradeTo { get; private set; }

        public IReadOnlyList<string> Collide { get; private set; }

        [EcfProperty] public string Place {  get; private set; }
        [EcfProperty] public bool RemoveOnSI { get; private set; }
        [EcfProperty] public bool IsPhysicsColliders { get; private set; }
        [EcfProperty] public bool IsActivateable { get; private set; }
        [EcfProperty] public bool IsActivateableInCP { get; private set; }
        [EcfProperty] public bool IsActivatedOnPlace { get; private set; }
        [EcfProperty] public bool IsDuplicateable { get; private set; }
        [EcfProperty] public bool ForceMaxCount { get; private set; }
        [EcfProperty] public int BlockSizeScale { get; private set; }
        [EcfProperty] public bool Voxelize { get; private set; }
        [EcfProperty] public bool IsAntiInfantryWeapon { get; private set; }
        [EcfProperty] public bool IsKeepContainers { get; private set; }
        [EcfProperty] public PropertyDecorator<float>? VolumeCapacity { get; private set; }
        [EcfProperty] public bool OccupySizeInBlocks { get; private set; }
        [EcfProperty] public string ShieldMultiplier { get; private set; }
        [EcfProperty] public PropertyDecorator<bool>? RepairToTemplate { get; private set; }
        [EcfProperty] public bool DropOnDeath { get; private set; }
        [EcfProperty] public float RepFac { get; private set; }
        [EcfProperty] public string DropMeshfile { get; private set; }
        [EcfProperty] public string DropInventoryEntity { get; private set; }
        [EcfProperty] public PropertyDecorator<int>? ShieldHitCooldown { get; private set; }
        [EcfProperty] public string ExecuteOnActivate { get; private set; }
        [EcfProperty] public string ExecuteOnCollide { get; private set; }
        [EcfProperty] public bool DialogueSingleUserAccess { get; private set; }
        [EcfProperty] public string DialogueState { get; private set; }
        [EcfProperty] public bool OmitCone { get; private set; }
        [EcfProperty] public int SymType { get; private set; } = 1; // Default of 1 as per BlocksConfig.ecf spec

        #endregion

        #region Properties from parsed file
        [EcfProperty] public string Accept { get; private set; }
        [EcfProperty] public bool AboveTerrainCheck { get; private set; }
        [EcfProperty] public int ADBActionRadius { get; private set; }
        [EcfProperty] public float ADBDockingSpeedFac { get; private set; }
        [EcfProperty] public string ADBDroneAmmoType { get; private set; }
        [EcfProperty] public string ADBDroneType { get; private set; }
        [EcfProperty] public int ADBMaxActiveDrone { get; private set; }
        [EcfProperty] public int ADBMaxReserve { get; private set; }
        [EcfProperty] public int ADBMaxSpawner { get; private set; }
        [EcfProperty] public PropertyDecorator<bool>? AllowedInBlueprint { get; private set; }
        [EcfProperty] public PropertyDecorator<string>? AllowPlacingAt { get; private set; }
        [EcfProperty] public bool AllowWander { get; private set; }
        [EcfProperty] public PropertyDecorator<int>? AmmoCapacity { get; private set; }
        [EcfProperty] public PropertyDecorator<string>? AmmoType { get; private set; }
        [EcfProperty] public float AutoMinerFactor { get; private set; }
        [EcfProperty] public int BigDecorationRadius { get; private set; }
        [EcfProperty] public PropertyDecorator<int>? BlastDamage { get; private set; }
        [EcfProperty] public int BlastParticleIndex { get; private set; }
        [EcfProperty] public string BlastParticleName { get; private set; }
        [EcfProperty] public PropertyDecorator<int>? BlastRadius { get; private set; }
        [EcfProperty] public int BlastShieldDamage { get; private set; }
        [EcfProperty] public string BlockColor { get; private set; } // "R,G,B"
        [EcfProperty] public PropertyDecorator<string>? BuffNameActivate { get; private set; }
        [EcfProperty] public PropertyDecorator<string>? BuffNamesActivate { get; private set; }
        [EcfProperty] public bool CanDecorateOnSlopes { get; private set; }
        [EcfProperty] public bool CanPickup { get; private set; }
        [EcfProperty] public bool CanPlayersSpawnOn { get; private set; }
        [EcfProperty] public bool CanSetColor { get; private set; }
        [EcfProperty] public string Category { get; private set; }
        [EcfProperty] public bool CheckObstructed { get; private set; }
        public IReadOnlyList<string> ChildBlocks { get; private set; }
        public IReadOnlyList<string> ChildShapes { get; private set; }
        [EcfProperty] public string Class { get; private set; }
        [EcfProperty] public bool ConsumeFuelO2 { get; private set; }
        [EcfProperty] public PropertyDecorator<float>? CostPerAU { get; private set; }
        [EcfProperty] public PropertyDecorator<float>? CostPerLY { get; private set; }
        [EcfProperty] public PropertyDecorator<int>? CPUIn { get; private set; }
        [EcfProperty] public PropertyDecorator<int>? CPUOut { get; private set; }
        [EcfProperty] public PropertyDecorator<string>? CropType { get; private set; }
        [EcfProperty] public PropertyDecorator<int>? CropYield { get; private set; }
        [EcfProperty] public PropertyDecorator<int>? Damage { get; private set; }
        [EcfProperty] public PropertyDecorator<string>? DebuffNamesActivate { get; private set; }
        [EcfProperty] public string DecoParticle { get; private set; }
        [EcfProperty] public bool DontDecayFood { get; private set; }
        [EcfProperty] public bool DoorClosesOnPower { get; private set; }
        [EcfProperty] public string EnergyDynamicGroup { get; private set; }
        [EcfProperty] public PropertyDecorator<int>? EnergyIn { get; private set; }
        [EcfProperty] public PropertyDecorator<int>? EnergyInIdle { get; private set; }
        [EcfProperty] public PropertyDecorator<int>? EnergyOut { get; private set; }
        [EcfProperty] public string EssentialCategory { get; private set; }
        [EcfProperty] public int ExplosionHardness { get; private set; }
        [EcfProperty] public string Faction { get; private set; }
        [EcfProperty("fertile_level")] public int FertileLevelOld { get; private set; }
        [EcfProperty] public int FertileLevel { get; private set; }
        [EcfProperty] public bool ForceNoPvEDamage { get; private set; }
        public IReadOnlyList<string> FuelAccept { get; private set; }
        [EcfProperty] public PropertyDecorator<int>? FuelCapacity { get; private set; }
        [EcfProperty] public string GlobalRef { get; private set; }
        [EcfProperty] public PropertyDecorator<int>? GravityGeneratorRadius { get; private set; }
        [EcfProperty] public string GridSize { get; private set; }
        [EcfProperty] public string Group { get; private set; }
        [EcfProperty] public PropertyDecorator<int>? GrowthTimeInfo { get; private set; }
        [EcfProperty] public PropertyDecorator<float>? Hardness { get; private set; }
        [EcfProperty] public bool HideShapesWindow { get; private set; }
        [EcfProperty] public PropertyDecorator<int>? HitPoints { get; private set; }
        [EcfProperty] public PropertyDecorator<bool>? Homing { get; private set; }
        [EcfProperty] public PropertyDecorator<float>? HVEngineDampCoef { get; private set; }
        [EcfProperty] public PropertyDecorator<float>? HVEngineDampPow { get; private set; }
        [EcfProperty] public PropertyDecorator<int>? HVEngineForce { get; private set; }
        [EcfProperty] public string IndexName { get; private set; }
        [EcfProperty] public PropertyDecorator<string>? Info { get; private set; }
        [EcfProperty] public PropertyDecorator<bool>? IsAccessible { get; private set; }
        [EcfProperty] public PropertyDecorator<bool>? IsBlocksThrusters { get; private set; }
        [EcfProperty] public bool IsColorable { get; private set; }
        [EcfProperty] public bool IsDeco { get; private set; }
        [EcfProperty] public bool IsDestructible { get; private set; }
        [EcfProperty] public bool IsGPUInstance { get; private set; }
        [EcfProperty] public bool IsIgnoreLC { get; private set; }
        [EcfProperty] public bool IsLockable { get; private set; }
        [EcfProperty] public PropertyDecorator<string>? IsOxygenTight { get; private set; }
        [EcfProperty] public bool IsPlant { get; private set; }
        [EcfProperty] public bool IsRepairToBlueprint { get; private set; }
        [EcfProperty] public PropertyDecorator<bool>? IsRetractable { get; private set; }
        [EcfProperty] public bool IsSolarPanel { get; private set; }
        [EcfProperty] public bool IsTerrainDecoration { get; private set; }
        [EcfProperty] public bool IsTextureable { get; private set; }
        [EcfProperty] public bool IsTrapDoor { get; private set; }
        [EcfProperty] public bool IsUsingCPUSystem { get; private set; }
        [EcfProperty] public PropertyDecorator<string>? ItemPerAU { get; private set; }
        [EcfProperty] public PropertyDecorator<int>? ItemsPerHour { get; private set; }
        [EcfProperty] public PropertyDecorator<int>? ItemStorageLimit { get; private set; }
        [EcfProperty] public int LifetimeOnDrop { get; private set; }
        [EcfProperty] public int LootList { get; private set; }
        [EcfProperty] public string MapIcon { get; private set; }
        [EcfProperty] public string MapName { get; private set; }
        // mistype?
        //[EcfProperty] public PropertyDecorator<int>? Marketprice { get; private set; }
        [EcfProperty] public PropertyDecorator<int>? MarketPrice { get; private set; }
        [EcfProperty] public PropertyDecorator<float>? Mass { get; private set; }
        [EcfProperty] public string Material { get; private set; }
        [EcfProperty] public PropertyDecorator<int>? MaxCount { get; private set; }
        [EcfProperty] public PropertyDecorator<float>? MaxVolumeCapacity { get; private set; }
        [EcfProperty] public string Mesh { get; private set; }
        [EcfProperty("Mesh-Damage-1")] public string MeshDamage1 { get; private set; }
        [EcfProperty("Mesh-Damage-2")] public string MeshDamage2 { get; private set; }
        [EcfProperty("Mesh-Damage-3")] public string MeshDamage3 { get; private set; }
        [EcfProperty("Mesh-Damage-4")] public string MeshDamage4 { get; private set; }
        [EcfProperty] public string MirrorTo { get; private set; }
        [EcfProperty("Mod.RangeLY")] public string ModRangeLY { get; private set; }
        [EcfProperty] public string Model { get; private set; }
        [EcfProperty] public string ModelOffset { get; private set; }
        [EcfProperty] public bool ModelScaleLocked { get; private set; }
        [EcfProperty] public bool NPCModelRotation { get; private set; }
        public IReadOnlyList<string> O2Accept { get; private set; }
        [EcfProperty] public PropertyDecorator<int>? O2Capacity { get; private set; }
        [EcfProperty] public int PanelAngle { get; private set; }
        public IReadOnlyList<string> ParentBlocks { get; private set; }
        [EcfProperty] public string ParticleName { get; private set; }
        [EcfProperty] public string ParticleOffset { get; private set; }
        [EcfProperty] public bool PickupToToolbar { get; private set; }
        [EcfProperty] public bool PlayerTriggersOpen { get; private set; }
        [EcfProperty] public PropertyDecorator<float>? Radiation { get; private set; }
        [EcfProperty] public PropertyDecorator<int>? Range { get; private set; }
        [EcfProperty] public PropertyDecorator<int>? RangeAU { get; private set; }
        [EcfProperty] public PropertyDecorator<int>? RangeLY { get; private set; }
        [EcfProperty] public PropertyDecorator<int>? RangeSpace { get; private set; }
        [EcfProperty] public PropertyDecorator<float>? ReloadDelay { get; private set; }
        [EcfProperty] public PropertyDecorator<int>? RepairPerSecond { get; private set; }
        [EcfProperty] public PropertyDecorator<string>? Repairs { get; private set; }
        [EcfProperty] public PropertyDecorator<float>? ReturnFactor { get; private set; }
        [EcfProperty] public PropertyDecorator<float>? ROF { get; private set; }
        [EcfProperty] public PropertyDecorator<int>? RotSpeed { get; private set; }
        // Mistype or old?
        //[EcfProperty] public int RpFactor { get; private set; }
        [EcfProperty] public int RPFactor { get; private set; }
        [EcfProperty] public string SfxActivate { get; private set; }
        [EcfProperty] public string SfxBuffActivate { get; private set; }
        [EcfProperty] public string Shape { get; private set; }
        [EcfProperty] public PropertyDecorator<int>? ShieldCapacity { get; private set; }
        [EcfProperty] public PropertyDecorator<int>? ShieldCapacityBonus { get; private set; }
        [EcfProperty] public PropertyDecorator<int>? ShieldCooldown { get; private set; }
        [EcfProperty] public PropertyDecorator<int>? ShieldPerCrystal { get; private set; }
        [EcfProperty] public PropertyDecorator<int>? ShieldRecharge { get; private set; }
        [EcfProperty] public PropertyDecorator<int>? ShieldRechargeBonus { get; private set; }

        [EcfProperty] public bool ShowBlockName { get; private set; }
        // Mistype?
        //[EcfProperty] public string Showuser { get; private set; }
        [EcfProperty] public string ShowUser { get; private set; }
        [EcfProperty] public PropertyDecorator<string>? SizeInBlocks { get; private set; }
        public IReadOnlyList<string> SizeInBlocksLocked { get; private set; }
        [EcfProperty] public PropertyDecorator<float>? SolarPanelEfficiency { get; private set; }
        [EcfProperty] public string Sound { get; private set; }
        [EcfProperty] public string SoundClose { get; private set; }
        [EcfProperty] public string SoundOnEnter { get; private set; }
        [EcfProperty] public string SoundOpen { get; private set; }
        [EcfProperty] public string SoundRotate { get; private set; }
        [EcfProperty] public string SpawnClass { get; private set; }
        [EcfProperty] public int SpawnCount { get; private set; }
        [EcfProperty] public bool StabilitySupport { get; private set; }
        [EcfProperty] public int StackSize { get; private set; }

        [EcfProperty] public string Tag { get; private set; }
        public IReadOnlyList<string> TechTreeNames { get; private set; }
        [EcfProperty] public string TechTreeparent { get; private set; }
        [EcfProperty] public string TechTreeParent { get; private set; }
        [EcfProperty] public PropertyDecorator<float>? Temperature { get; private set; }
        [EcfProperty] public PropertyDecorator<float>? TemperatureGain { get; private set; }
        [EcfProperty] public string Texture { get; private set; }
        [EcfProperty] public int TextureTable { get; private set; }
        [EcfProperty] public PropertyDecorator<float>? ThrusterBoosterFactor { get; private set; }
        [EcfProperty] public PropertyDecorator<int>? ThrusterForce { get; private set; }
        [EcfProperty] public PropertyDecorator<int>? Torque { get; private set; }
        [EcfProperty] public string TraderEntity { get; private set; }
        [EcfProperty] public string TraderType { get; private set; }
        [EcfProperty] public int Transparent { get; private set; }
        [EcfProperty] public float Turbidity { get; private set; }
        [EcfProperty] public string TurbidityColor { get; private set; } // "R, G, B, A?"
        [EcfProperty] public bool TurretTargetIgnore { get; private set; }
        [EcfProperty] public PropertyDecorator<int> UnlockCost { get; private set; }
        [EcfProperty] public PropertyDecorator<int> UnlockLevel { get; private set; }
        [EcfProperty] public bool UseCockpitCamera { get; private set; }
        [EcfProperty] public PropertyDecorator<float>? Volume { get; private set; }
        [EcfProperty] public string WarpAccept { get; private set; }
        [EcfProperty] public string WaterColor { get; private set; } // "R, G, B, A?"
        [EcfProperty] public string WeaponItem { get; private set; }
        [EcfProperty] public bool WeaponItemRotation { get; private set; }
        [EcfProperty] public int WindSpeed { get; private set; }
        [EcfProperty] public float XpFactor { get; private set; }
        [EcfProperty] public int YawRotation { get; private set; }
        [EcfProperty] public PropertyDecorator<float>? Zoom { get; private set; }
        #endregion

        public Block(IEcfObject obj) : base(obj)
        {
            Collide = ParseList("Collide");
            ChildBlocks = ParseList("ChildBlocks");
            ChildShapes = ParseList("ChildShapes");
            FuelAccept = ParseList("FuelAccept");
            O2Accept = ParseList("O2Accept");
            ParentBlocks = ParseList("ParentBlocks");
            SizeInBlocksLocked = ParseList("SizeInBlocksLocked");
            TechTreeNames = ParseList("TechTreeNames");

            // TODO: Wait for this to be fixed upstream in the scenario
            // Fix possible misspellings
            var showUser = UnparsedProperties.FirstOrDefault(x => x.Key == "Showuser");
            if (showUser.Key != null && string.IsNullOrEmpty(ShowUser))
            {
                ShowUser = showUser.Value;
                MarkAsParsed("Showuser");
            }

            var rpFactor = UnparsedProperties.FirstOrDefault(x => x.Key == "RpFactor");
            if (rpFactor.Key != null && rpFactor.Value.ConvertType<int>(out var value))
            {
                RPFactor = value;
                MarkAsParsed("RpFactor");
            }

            var marketPrice = UnparsedProperties.FirstOrDefault(x => x.Key == "Marketprice");
            if (marketPrice.Key != null)
            {
                MarketPrice = new PropertyDecorator<int>(marketPrice.Value);
                MarkAsParsed("Marketprice");
            }
        }
        public static IEnumerable<Block> ReadFile(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentNullException(nameof(filePath));

            if (!File.Exists(filePath))
                throw new FileNotFoundException("ECF file not found", filePath);

            var ecf = new EcfFile(filePath);
            return ecf.ParseObjects().Select(obj => new Block(obj));
        }

        private IReadOnlyList<string> ParseList(string name)
        {
            if (!UnparsedProperties.TryGetValue(name, out string compounded))
                return Array.Empty<string>();

            MarkAsParsed(name);
            return compounded.Trim('"').Split(',').Select(x => x.Trim()).ToArray();
        }
    }
}
