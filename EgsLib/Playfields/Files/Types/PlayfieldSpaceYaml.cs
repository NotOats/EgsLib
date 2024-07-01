using System;
using System.Collections.Generic;
using System.Text;
using YamlDotNet.Serialization;

namespace EgsLib.Playfields.Files.Types
{
    public class PlayfieldSpaceYaml
    {
        public PlayfieldType PlayfieldType { get; set; }

        public string Description { get; set; }

        public string PlanetType { get; set; }

        [YamlMember(Alias = "POIs", ApplyNamingConventions = false)]
        public List<PoiDescription> PointsOfInterest { get; set; }

        public List<ResourceDescription> Resources { get; set; }

        public bool AllowSV { get; set; } = true;

        public bool AllowCV { get; set; } = true;

        public bool AllowHV { get; set; } = true;

        public bool AllowBA { get; set; } = true;
    }

    public class PoiDescription
    {
        public string Type { get; set; }

        public string[] Name { get; set; }

        public string[] FieldName { get; set; }

        public string GroupName { get; set; }

        public string DisplayName { get; set; }

        public int[] CountMinMax { get; set; }

        [YamlMember(Alias = "Position", ApplyNamingConventions = false)]
        public PositionDescription PositionDescription { get; set; }

        public float Probability { get; set; }

        public int[] RadiusMinMax { get; set; }

        public CompoundDescription Compound { get; set; }

        public List<Property> Properties { get; set; }

        public string Mode { get; set; }

        public string SubMode { get; set; }

        [YamlMember(Alias = "Pos", ApplyNamingConventions = false)]
        public float[] Position { get; set; }

        [YamlMember(Alias = "Rot", ApplyNamingConventions = false)]
        public int[] Rotation { get; set; }

        public string Faction { get; set; }

        public string[] FactionTerritory { get; set; }

        public bool SpaceDefenseOverrideDefaults { get; set; }

        public float SpaceDefenseProbability { get; set; }

        public int[] SpaceDefensePrice { get; set; }

        public int[] SpaceDefensePriceMinMax { get; set; }
    }

    public class PositionDescription
    {
        public int[] PosXZMinMax { get; set; } = new int[] { 1000, 15000 };

        public int PosYMax { get; set; } = 2000;

        public int[] PosMultiplier { get; set; } = new int[] { 2, 5 };

        public float[] RadialInfo { get; set; }
    }

    public class CompoundDescription
    {
        public int[] CountMinMax { get; set; }

        public float Probability { get; set; }

        public List<string> Name { get; set; }

        public int[] DistanceMinMax { get; set; }

        public bool Rotate { get; set; }
    }

    public class ResourceDescription
    {
        public string[] Name { get; set; }

        public string DisplayName { get; set; }

        public int[] CountMinMax { get; set; }

        public float Probability { get; set; }

        [YamlMember(Alias = "Position", ApplyNamingConventions = false)]
        public PositionDescription PositionDescription { get; set; } = new PositionDescription();

        [YamlMember(Alias = "Pos", ApplyNamingConventions = false)]
        public float[] Position { get; set; }

        [YamlMember(Alias = "Rot", ApplyNamingConventions = false)]
        public int[] Rotation { get; set; }

        public List<Property> Properties { get; set; }
    }
}
