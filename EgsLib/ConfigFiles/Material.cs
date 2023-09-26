using EgsLib.ConfigFiles.Ecf;
using EgsLib.ConfigFiles.Ecf.Attributes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace EgsLib.ConfigFiles
{
    [EcfObject("Material")]
    public class Material : BaseConfig
    {
        [EcfField]
        public string Name { get; private set; }


        [EcfProperty("damage_category")]
        public string DamageCategory { get; private set; }

        [EcfProperty("surface_category")]
        public string SurfaceCategory { get; private set; }

        [EcfProperty("particle_category")]
        public string ParticleCategory { get; private set; }

        [EcfProperty("particle_destroy_category")]
        public string ParticleDestroyCategory { get; private set; }

        [EcfProperty("stepsound")]
        public string StepSound { get; private set; }

        [EcfProperty("liquid")]
        public bool? Liquid { get; private set; }

        [EcfProperty("collidable")]
        public bool? Collidable { get; private set; }

        [EcfProperty("lightopacity")]
        public int? LightOpacity { get; private set; }

        [EcfProperty("fertile_level")]
        public int? FertileLevel { get; private set; }

        [EcfProperty("stability_glue")]
        public int? StabilityGlue { get; private set; }

        [EcfProperty]
        public PropertyDecorator<float>? Mass { get; private set; }

        [EcfProperty]
        public PropertyDecorator<float>? Hardness { get; private set; }

        [EcfProperty]
        public int? ExplosionHardness {  get; private set; }

        [EcfProperty]
        public PropertyDecorator<int>? HitPoints {  get; private set; }

        [EcfProperty("O2Content")]
        public PropertyDecorator<int>? OxygenContent { get; private set; }

        [EcfProperty("plant")]
        public bool? Plant { get; private set; }


        public Material(IEcfObject obj) : base(obj)
        {
        }

        public static IEnumerable<Material> ReadFile(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentNullException(nameof(filePath));

            if (!File.Exists(filePath))
                throw new FileNotFoundException("ECF file not found", filePath);

            var ecf = new EcfFile(filePath);
            return ecf.ParseObjects().Select(obj => new Material(obj));
        }
    }
}
