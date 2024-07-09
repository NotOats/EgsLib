using EgsLib.ConfigFiles;

namespace EgsLib.Tests.ConfigFiles
{
    public class MaterialFixture : BaseFileFixture<Material>
    {
        public MaterialFixture() : base(@"Resources\Configuration\MaterialConfig.ecf", Material.ReadFile)
        { }
    }

    public class MaterialTests : IClassFixture<MaterialFixture>
    {
        private MaterialFixture Fixture { get; }

        public MaterialTests(MaterialFixture fixture) => Fixture = fixture;

        [Fact]
        public void HasRequiredFields()
        {
            foreach (var material in Fixture.Objects)
            {
                Assert.NotNull(material.Name);
                Assert.NotEqual(string.Empty, material.Name);
            }
        }

        [Theory]
        [InlineData("air", null, null, false)]
        [InlineData("water", "water", true, false)]
        [InlineData("grass", "grass", null, null)]
        public void MaterialHasRequiredProperties(string name, string? stepsound, bool? liquid, bool? collidable)
        {
            var material = Fixture.Objects.FirstOrDefault(x => x.Name == name);
            Assert.NotNull(material);

            Assert.Equal(name, material.Name);
            Assert.Equal(stepsound, material.StepSound);
            Assert.Equal(liquid, material.Liquid);
            Assert.Equal(collidable, material.Collidable);
        }
    }
}
