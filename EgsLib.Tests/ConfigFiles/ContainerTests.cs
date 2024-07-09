using EgsLib.ConfigFiles;

namespace EgsLib.Tests.ConfigFiles
{
    public class ContainerFixture : BaseFileFixture<Container>
    {
        public ContainerFixture() : base(@"Resources\Configuration\Containers.ecf", Container.ReadFile)
        { }
    }

    public class ContainerTests : IClassFixture<ContainerFixture>
    {
        private ContainerFixture Fixture { get; }
        public ContainerTests(ContainerFixture fixture) => Fixture = fixture;

        [Fact]
        public void HasRequiredFieldsAndProperties()
        {
            foreach (var container in Fixture.Objects)
            {
                Assert.True(container.Id > 0, "Id is expected to be greater than zero");

                Assert.True(container.Count.Minimum > 0, "Count.Minimum is expcted to be greater than zero");
                Assert.True(container.Count.Maximum > 0, "Count.Maximum is expcted to be greater than zero");
                Assert.True(container.Count.IsValid(), "Count is expected to be valid");

                // Only continue if we managed to parse the entry
                // Currently the parser breaks on some non-uniform entries found in the stock config
                if (container.UnparsedChildren.Count != 0)
                    return;

                var itemCount = container.Items != null ? container.Items.Count : 0;
                var groupCount = container.Groups != null ? container.Groups.Count : 0;

                Assert.False(itemCount == 0 && groupCount == 0, "Expected itemCount or groupCount to be greater than 0");
                Assert.True(container.WeightMax > 0, "Expected WeightMax to not be zero");
            }
        }

        [Theory]
        [InlineData(41, new[]{ 2, 3 }, new[]{ "RTG", "PlasmaCannonAlien", "WeaponUpgradesUltra", "Eden_UltraRare" })]
        [InlineData(600, new[]{ 4, 4 }, new[]{ "Eden_DroneSalvageProcessor", "Eden_DroneSwarmT1", "Eden_ComponentsAdvancedL" })]
        [InlineData(713, new[]{ 1, 1 }, new[]{ "Token" })]
        public void ContainerHasCorrectValues(int id, int[] rawCount, string[] items)
        {
            var container = Fixture.Objects.FirstOrDefault(x => x.Id == id);
            Assert.NotNull(container);

            var count = new Range<int>(rawCount[0], rawCount[1]);
            Assert.Equal(count, container.Count);

            foreach(var item in items)
            {
                var hasItem = container.Items.Any(x => x.Name == item);
                var hasGroup = container.Groups.Any(x => x.Name == item);

                Assert.True(hasItem || hasGroup, "Expcted hasItem or hasGroup to be true");
            }
        }

        [Theory]
        [InlineData(604, 6400)]
        [InlineData(653, 6558)]
        [InlineData(713, 6613)]
        [InlineData(736, 6636)]
        public void ContainerHasCorrentToken(int id, int token)
        {
            var container = Fixture.Objects.FirstOrDefault(x => x.Id == id);
            Assert.NotNull(container);

            Assert.Contains(container.Items, x => x.TokenId == token);
        }
    }
}
