using EgsLib.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using YamlDotNet.Serialization;

namespace EgsLib.Playfields.Files.Types
{
    public class PlayfieldYaml
    {
        // Private backing fields to replicate stock default values
        private string _planetClass;

        // Serializable properties below
        public PlayfieldType PlayfieldType { get; set; }

        public string Description { get; set; }

        public string PlanetClass
        {
            get => _planetClass.AsNullIfEmpty() ?? PlanetType;
            set => _planetClass = value;
        }

        public string PlanetType { get; set; }

        public int Difficulty { get; set; } = 1;

        public List<RandomResource> RandomResources { get; set; }

        public List<FixedResource> FixedREsources { get; set; }

        public List<AsteroidResource> AsteroidResources { get; set; }

        [YamlMember(Alias = "POIs", ApplyNamingConventions = false)]
        public PoiData PointsOfInterest { get; set; }
    }

    public class RandomResource
    {
        public string Name { get; set; }

        public bool IsImportant { get; set; }

        public bool IsScalingCount { get; set; }

        public List<int> CountMinMax { get; set; }

        public List<int> SizeMinMax { get; set; }

        public List<int> DepthMinMax { get; set; }

        [YamlMember(Alias = "DistTypeCylinder", ApplyNamingConventions = false)]
        public ResourceDistributionTypeCylinder DistributionTypeCylinder { get; set; }

        [YamlMember(Alias = "DistTypeHelix", ApplyNamingConventions = false)]
        public ResourceDistributionTypeHelix DistributionTypeHelix { get; set; }

        [YamlMember(Alias = "DistTypeWorm", ApplyNamingConventions = false)]
        public ResourceDistributionTypeWorm DistributionTypeWorm { get; set; }


        //public PlayfieldYamlData.ResourceDistTypeHelix DistTypeHelix { get; set; }
        //public PlayfieldYamlData.ResourceDistTypeWorm DistTypeWorm { get; set; }

        [YamlMember(Alias = "DroneProb", ApplyNamingConventions = false)]
        public float DroneProbability { get; set; }

        public int MaxDroneCount { get; set; } = 1;

        public string LevelMod { get; set; }

        public string Faction { get; set; }

        public PlacingTargets PlaceAt { get; set; }

        public List<string> Biome { get; set; }

        public List<string> BiomesExcluded { get; set; }
    }

    public class ResourceDistributionTypeCylinder
    {
        public int MaxRadius { get; set; }

        public int YVariance { get; set; }
    }

    public class ResourceDistributionTypeHelix
    {
        public float MaxRadius { get; set; }

        public float GrowthPerRound { get; set; }

        public float DropPerRound { get; set; }
    }

    public class ResourceDistributionTypeWorm
    {
        [YamlMember(Alias = "FreqDirChange", ApplyNamingConventions = false)]
        public float DirectionChangeFrequency { get; set; }

        [YamlMember(Alias = "DirChangeMagnitude", ApplyNamingConventions = false)]
        public float DirectionChangeMagnitude { get; set; }

        public float MaxLength { get; set; }

        public int DeepnessType { get; set; }
    }

    public class FixedResource
    {
        public string Name { get; set; }

        [YamlMember(Alias = "Pos", ApplyNamingConventions = false)]
        public List<int> Position { get; set; }

        public int Radius { get; set; }

        [YamlMember(Alias = "DistTypeCylinder", ApplyNamingConventions = false)]
        public ResourceDistributionTypeCylinder DistributionTypeCylinder { get; set; }

        [YamlMember(Alias = "DistTypeHelix", ApplyNamingConventions = false)]
        public ResourceDistributionTypeHelix DistributionTypeHelix { get; set; }

        [YamlMember(Alias = "DistTypeWorm", ApplyNamingConventions = false)]
        public ResourceDistributionTypeWorm DistributionTypeWorm { get; set; }

        public string Faction { get; set; }
    }

    public class AsteroidResource
    {
        public string Name { get; set; }

        public float Threshold { get; set; }

        public float Amount { get; set; }

        public float InitialDelay { get; set; } = -1;

        public float Delay { get; set; } = -1;

        public float DespawnDelay { get; set; }
    }

    public class PoiData
    {
        public List<RandomPoiData> Random { get; set; }
        public List<FixedPoiData> Fixed { get; set; }
        public List<PlayerStartData> FixedPlayerStart { get; set; }
    }

    public class RandomPoiData
    {
        public string GroupName { get; set; }

        public bool UseEachGroupPoiOnlyOnce { get; set; }

        public string DroneBaseSetup { get; set; }

        public string DroneSetupID { get; set; }

        public string PlanetVesselBaseSetup { get; set; }

        public string Faction { get; set; }

        public string[] FactionTerritory { get; set; }

        public bool Territory { get; set; } = true;

        public bool AvoidFactionTerritory { get; set; }

        public bool NoShieldReload { get; set; }

        public bool IsCommandCenter { get; set; }

        public string Type { get; set; }

        public bool IsImportant { get; set; }

        [YamlMember(Alias = "AuxiliaryPOIs", ApplyNamingConventions = false)]
        public List<string> AuxiliaryPois { get; set; }

        public bool IsAuxPOI { get; set; }

        public bool IsScalingCount { get; set; }

        public List<int> CountMinMax { get; }

        public float DroneProb { get; set; }

        public bool TroopTransport { get; set; }

        public List<int> DronesMinMax { get; set; }

        public int ReserveCount { get; set; }

        public List<Property> Properties { get; set; }

        public bool InitPower { get; set; }

        public string LevelMod { get; set; }

        public bool PlayerStart { get; set; }

        [YamlMember(Alias = "SpawnPOINear", ApplyNamingConventions = false)]
        public List<string> SpawnPoiNear { get; set; }

        [YamlMember(Alias = "SpawnPOIAvoid", ApplyNamingConventions = false)]
        public List<string> SpawnPoiAvoid { get; set; }

        public List<string> SpawnResource { get; set; }

        public PlacingTargets PlaceAt { get; set; }

        public bool SpaceDefenseOverrideDefaults { get; set; }

        public float SpaceDefenseProbability { get; set; }

        public List<int> SpaceDefensePriceMinMax { get; set; }

        //[Obsolete(null, true)]
        public int POIDistance { get; set; }

        //[Obsolete(null, true)]
        [YamlMember(Alias = "SpawnPOINearDistance", ApplyNamingConventions = false)]
        public int SpawnPoiNearDistance { get; set; }

        public List<int> SpawnPOINearRange { get; set; }

        public int SpawnPOIAvoidDistance { get; set; }

        //[Obsolete(null, true)]
        public int ResourceDistance { get; set; }

        public List<int> SpawnResourceRange { get; set; }

        public List<string> Biome { get; set; }

        public List<string> BiomesExcluded { get; set; }
    }

    public class FixedPoiData
    {
        public string Type { get; set; }

        public string Prefab { get; set; }

        public string Mode { get; set; }

        public string SubMode { get; set; }

        public string Name { get; set; }

        public bool InitPower { get; set; }

        public string LevelMod { get; set; }

        public string Faction { get; set; }

        public string[] FactionTerritory { get; set; }

        public List<Property> Properties { get; set; }

        public bool NoShieldReload { get; set; }

        public bool IsCommandCenter { get; set; }

        [YamlMember(Alias = "Pos", ApplyNamingConventions = false)]
        public List<float> Position { get; set; }

        [YamlMember(Alias = "Rot", ApplyNamingConventions = false)]
        public List<int> Rotation { get; set; }

        public List<string> SpawnResource { get; set; }

        //[Obsolete(null, true)]
        public int ResourceDistance { get; set; }

        public List<int> SpawnResourceRange { get; set; }

        public bool SpaceDefenseOverrideDefaults { get; set; }

        public float SpaceDefenseProbability { get; set; }

        public List<int> SpaceDefensePriceMinMax { get; set; }
    }

    public class PlayerStartData
    {
        public string Mode { get; set; }

        public string SubMode { get; set; }

        public PlayerSpawnOption Spawn { get; set; }

        [YamlMember(Alias = "Pos", ApplyNamingConventions = false)]
        public List<float> Position { get; set; }

        [YamlMember(Alias = "RotY", ApplyNamingConventions = false)]
        public int RotationY { get; set; }

        public string Structure { get; set; }

        public List<string> Items { get; set; }

        public string Armor { get; set; }

        public List<string> Status { get; set; }

        public List<string> PlayerArmor { get; set; }
    }

}
