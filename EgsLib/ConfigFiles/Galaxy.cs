using EgsLib.ConfigFiles.Ecf;
using EgsLib.ConfigFiles.Ecf.Attributes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace EgsLib.ConfigFiles
{

    [EcfObject("GalaxyConfig,")] // Note: why is there an irregular comma here?
    public class Galaxy : BaseConfig<Galaxy>
    {
        [EcfField] public string Name { get; set; }

        [EcfProperty("StarCount", typeof(Galaxy), "ParseRange")]
        public Range<int>? StarCount { get; private set; }

        [EcfProperty] public string Radius { get; private set; }

        [EcfProperty("NebulaCount", typeof(Galaxy), "ParseRange")]
        public Range<int>? NebulaCount { get; private set; }

        [EcfProperty("StarterSystemLYCoord", typeof(Galaxy), "ParseVector")]
        public Vector3<int>? StarterSystemLightYearCoordinates { get; private set; }

        [EcfProperty] public string StarterSystemName { get; private set; }

        [EcfProperty] public string StarterSystemStarClass { get; private set; }

        [EcfProperty] public string GalaxyMode { get; private set; }

        [EcfProperty("SectorsPerLY")]
        public int? SectorsPerLightYear { get; private set; }

        [EcfProperty] public string StarClass { get; private set; }
        [EcfProperty] public string SolarSystemConfigSuffix { get; private set; }
        [EcfProperty] public string Model { get; private set; }
        [EcfProperty] public float? Probability { get; private set; }
        [EcfProperty] public int? SizeClass { get; private set; }

        [EcfProperty("Color", typeof(Galaxy), "ParseVector")]
        public Vector3<float>? Color {  get; private set; }

        [EcfProperty("LightColor", typeof(Galaxy), "ParseVector")]
        public Vector3<float>? LightColor { get; private set; }

        [EcfProperty("ModelColor", typeof(Galaxy), "ParseVector")]
        public Vector3<float>? ModelColor { get; private set; }

        [EcfProperty("ModelColor2", typeof(Galaxy), "ParseVector")]
        public Vector3<float>? ModelColor2 { get; private set; }

        [EcfProperty] public float? ModelBrightness { get; private set; }

        [EcfProperty] public int? ModelNoiseTiling { get; private set; }

        [EcfProperty] public float? ModelCoronaSpikesSize { get; private set; }

        [EcfProperty] public float? ModelPlasmaArcsSize { get; private set; }

        [EcfProperty] public float? ModelFlareBrightness { get; private set; }

        [EcfProperty("SurfaceTemperature", typeof(Galaxy), "ParseRange")]
        public Range<int>? SurfaceTemperature { get; private set; }

        [EcfProperty] public float? Mass { get; private set; }
        [EcfProperty] public float? Luminosity { get; private set; }
        [EcfProperty] public string ColorName { get; private set; }
        [EcfProperty] public float? Age { get; private set; }

        [EcfProperty("HabitableZone", typeof(Galaxy), "ParseRange")]
        public Range<int>? HabitableZone { get; private set; }

        [EcfProperty("InnerSystem", typeof(Galaxy), "ParseRange")]
        public Range<int>? InnerSystem { get; private set; }

        [EcfProperty("HabitableHot", typeof(Galaxy), "ParseRange")]
        public Range<int>? HabitableHot { get; private set; }

        [EcfProperty("HabitableTemperate", typeof(Galaxy), "ParseRange")]
        public Range<int>? HabitableTemperate { get; private set; }

        [EcfProperty("HabitableCold", typeof(Galaxy), "ParseRange")]
        public Range<int>? HabitableCold { get; private set; }

        [EcfProperty("OuterSystem", typeof(Galaxy), "ParseRange")]
        public Range<int>? OuterSystem { get; private set; }

        [EcfProperty] public string Description { get; private set; }

        [EcfProperty("GalaxySpawnRadius", typeof(Galaxy), "ParseRange")]
        public Range<int>? GalaxySpawnRadius { get; private set; }

        [EcfProperty] public int? GalaxySpawnAmount { get; private set; }

        [EcfProperty] public string CompanionStarClass { get; private set; }

        [EcfProperty] public float? ClusterProb { get; private set; }

        [EcfProperty("ClusterRange", typeof(Galaxy), "ParseRange")]
        public Range<float>? ClusterRange { get; private set; }

        public IReadOnlyDictionary<int, string> NebulaColors { get; private set; }

        public Galaxy(IEcfObject obj) : base(obj)
        {
            var colors = UnparsedProperties
                .Where(kvp => kvp.Key.StartsWith("NebulaColors"))
                .ToDictionary(x => x.Key, x => x.Value);

            NebulaColors = colors
                .ToDictionary(kvp =>
                {
                    MarkAsParsed(kvp.Key);
                    return int.Parse(kvp.Key.Substring("NebulaColors".Length));
                }, kvp => kvp.Value);
        }

        public static IEnumerable<Galaxy> ReadFile(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentNullException(nameof(filePath));

            if (!File.Exists(filePath))
                throw new FileNotFoundException("ECF file not found", filePath);

            var ecf = new EcfFile(filePath);
            return ecf.ParseObjects().Select(obj => new Galaxy(obj));
        }

        #region Converters
        private static bool ParseVector(string input, out object output, Type type)
        {
            output = default;

            var parts = input.Trim(' ', '"').Split(',');
            if (parts.Length < 3)
                return false;

            var underlying = FindUnderlyingType(type);
            if (!parts[0].ConvertType(underlying, out object x)
                || !parts[1].ConvertType(underlying, out object y)
                || !parts[1].ConvertType(underlying, out object z))
            {
                return false;
            }

            if (underlying == typeof(int))
            {
                output = new Vector3<int>((int)x, (int)y, (int)z);
                return true;
            }

            if (underlying == typeof(float))
            {
                output = new Vector3<float>((float)x, (float)y, (float)z);
                return true;
            }

            return false;
        }

        private static bool ParseRange(string input, out object output, Type type)
        {
            output = default;

            var parts = input.Trim(' ', '"').Split(',');
            if (parts.Length < 2)
                return false;

            var underlying = FindUnderlyingType(type);
            if(!parts[0].ConvertType(underlying, out object min)
                || !parts[1].ConvertType(underlying, out object max))
            {
                return false;
            }

            if(underlying == typeof(float))
            {
                output = new Range<float>((float)min, (float)max);
                return true;
            }

            if (underlying == typeof(int))
            {
                output = new Range<int>((int)min, (int)max);
                return true;
            }

            return false;
        }

        private static Type FindUnderlyingType(Type type)
        {
            var underlying = type;

            // Unwrap nullable
            var nullable = Nullable.GetUnderlyingType(type);
            if (nullable != null)
                underlying = nullable;

            // Unwrap Range
            if (underlying.IsGenericType && !underlying.IsGenericTypeDefinition)
            {
                var genericType = underlying.GetGenericTypeDefinition();

                if (ReferenceEquals(genericType, typeof(Range<>)))
                    underlying = underlying.GetGenericArguments()[0];
            }

            // Unwrap Vector
            if(underlying.IsGenericType && !underlying.IsGenericTypeDefinition)
            {
                var genericType = underlying.GetGenericTypeDefinition();

                if (ReferenceEquals(genericType, typeof(Vector3<>)))
                    underlying = underlying.GetGenericArguments()[0];
            }

            return underlying;
        }
        #endregion
    }
}
