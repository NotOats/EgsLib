using EgsLib.ConfigFiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EgsLib.Tests.ConfigFiles
{
    public class StatusEffectFixture : BaseFileFixture<StatusEffect>
    {
        public StatusEffectFixture() : base(@"Resources\Configuration\StatusEffects.ecf", StatusEffect.ReadFile)
        { }
    }

    public class StatusEffectTests : IClassFixture<StatusEffectFixture>
    {
        private StatusEffectFixture Fixture { get; }

        public StatusEffectTests(StatusEffectFixture fixture) => Fixture = fixture;

        [Fact]
        public void HasRequiredFields()
        {
            foreach (var effect in Fixture.Objects)
            {
                Assert.NotNull(effect.Name);
                Assert.NotEqual(string.Empty, effect.Name);
            }
        }

        [Theory]
        [InlineData("OpenWound", 60, null, null)]
        [InlineData("DermalBurn", 180, true, "InfectedWound,0.25")]
        [InlineData("RadiationPoisoning", 600, null, null)]
        public void StatusEffectHasRequiredProperties(string name, int duration, bool? nextIsWorse, string? evolves)
        {
            var effect = Fixture.Objects.FirstOrDefault(x => x.Name == name);
            Assert.NotNull(effect);

            Assert.Equal(name, effect.Name);
            Assert.Equal(duration, effect.Duration);
            Assert.Equal(nextIsWorse, effect.NextIsWorse);
            Assert.Equal(evolves, effect.Evolves);
        }
    }
}
