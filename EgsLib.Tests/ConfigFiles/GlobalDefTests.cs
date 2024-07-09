using EgsLib.ConfigFiles;


namespace EgsLib.Tests.ConfigFiles
{
    public class GlobalDefFixture : BaseFileFixture<GlobalDef>
    {
        public GlobalDefFixture() : base(@"Resources\Configuration\GlobalDefsConfig.ecf", GlobalDef.ReadFile)
        { }
    }

    public class GlobalDefTests : IClassFixture<GlobalDefFixture>
    {
        private GlobalDefFixture Fixture { get; }

        public GlobalDefTests(GlobalDefFixture fixture) => Fixture = fixture;

        [Fact]
        public void HasRequiredFields()
        {
            foreach (var def in Fixture.Objects)
            {
                Assert.NotNull(def.Name);
                Assert.NotEqual(string.Empty, def.Name);
            }
        }

        [Theory]
        [InlineData("Eden_Shipment8k", 48000f, "Kilogram")]
        [InlineData("Eden_Shipment320k", 192000f, "Kilogram")]
        public void GlobalDefHasCorrectValues(string name, float mass, string formatter)
        {
            var def = Fixture.Objects.FirstOrDefault(x => x.Name == name);
            Assert.NotNull(def);

            Assert.NotNull(def.Mass);
            Assert.Equal(mass, def.Mass?.Value);
            Assert.Equal(formatter, def.Mass?.Formatter);
        }
    }
}
