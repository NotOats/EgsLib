using EgsLib.Blueprints;

namespace EgsLib.Tests.Blueprints
{
    public class BlueprintReaderTests
    {
        [Theory]
        [ClassData(typeof(BlueprintTestData))]
        public void BlueprintParsesHeaderCorrectly(BlueprintDetails details)
        {
            var blueprint = new Blueprint(details.File);

            Assert.NotNull(blueprint);

            // Header
            Assert.Equal(details.Size, blueprint.Header.Size);
            Assert.Equal(details.SizeClass, blueprint.Header.SizeClass);

            // Statistics
            Assert.Equal(details.BlockCount, blueprint.Header.Statistics.BlockSolids);
            Assert.Equal(details.DeviceCount, blueprint.Header.Statistics.BlockDevices);
            Assert.Equal(details.TriangleCount, blueprint.Header.Statistics.TrianglesReal);

            // Properties
            Assert.True(blueprint.Header.GetProperty<string>(PropertyName.CreatorPlayerName, out var creatorPlayerName));
            Assert.False(string.IsNullOrEmpty(creatorPlayerName));
            
            Assert.True(blueprint.Header.GetProperty<DateTime>(PropertyName.ChangedDate, out var changedDate));
            Assert.True(changedDate < DateTime.Now);
        }

        [Theory]
        [ClassData(typeof(BlueprintTestData))]
        public void BlueprintParsesBlockDataCorrectly(BlueprintDetails details)
        {
            var blueprint = new Blueprint(details.File);

            var blocks = blueprint.BlockData.Blocks.Where(b => b.BlockId != 0).ToList();

            Assert.Equal(details.BlockCount, blocks.Count);
            Assert.All(blocks, b => Assert.Contains(blueprint.Header.BlockMap, kvp => kvp.Value == b.BlockId));
        }
    }
}
