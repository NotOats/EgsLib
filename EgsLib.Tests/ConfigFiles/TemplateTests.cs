using EgsLib.ConfigFiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EgsLib.Tests.ConfigFiles
{
    public class TemplateFixture : BaseFileFixture<Template>
    {
        public TemplateFixture() : base(@"Resources\Configuration\Templates.ecf", Template.ReadFile)
        { }
    }

    public class TemplateTests : IClassFixture<TemplateFixture>
    {
        private TemplateFixture Fixture { get; }

        public TemplateTests(TemplateFixture fixture) => Fixture = fixture;

        [Fact]
        public void HasRequiredFields()
        {
            foreach (var template in Fixture.Objects)
            {
                Assert.NotNull(template.Name);
                Assert.NotEqual(string.Empty, template.Name);
            }
        }

        [Theory]
        [InlineData("Graphite", true, 5, "RockDust", 20)]
        [InlineData("ContainerHarvestControllerLarge", null, 4, "Electronics", 4)]
        [InlineData("LargeCargoContainer", null, 4, "SteelPlate", 10)]
        [InlineData("PlasmaSword", null, 20, "EnergyMatrix", 1)]
        public void TemplateHasRequiredProperties(string name, bool? baseItem, int craftTime, string inputItem, int inputCount)
        {
            var template = Fixture.Objects.FirstOrDefault(x => x.Name == name);
            Assert.NotNull(template);

            Assert.Equal(name, template.Name);
            Assert.Equal(baseItem, template.BaseItem);
            Assert.Equal(craftTime, template.CraftTime);
            Assert.Contains(template.Inputs, x => x.Key == inputItem && x.Value == inputCount);
        }
    }
}
