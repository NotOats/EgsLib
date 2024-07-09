using EgsLib.ConfigFiles;

namespace EgsLib.Tests.ConfigFiles
{
    public class BlockGroupFixture : BaseFileFixture<BlockGroup>
    {
        public BlockGroupFixture() : base(@"Resources\Configuration\BlockGroupsConfig.ecf", BlockGroup.ReadFile)
        { }
    }

    public class BlockGroupTests : IClassFixture<BlockGroupFixture>
    {
        private BlockGroupFixture Fixture { get; }
        public BlockGroupTests(BlockGroupFixture fixture) => Fixture = fixture;

        [Fact]
        public void HasRequiredFieldsAndProperties()
        {
            foreach(var block in Fixture.Objects)
            {
                // Fields
                Assert.NotNull(block.Name);
                Assert.NotEqual(string.Empty, block.Name);

                // Properties
                Assert.True(block.MaxCount > 0, "Exepcted MaxCount to be greater than 0");

                Assert.NotNull(block.Blocks);
                Assert.True(block.Blocks.Count > 0, "Exepcted Blocks.Count to be greater than 0");
                Assert.All(block.Blocks, x =>
                {
                    Assert.NotNull(x);
                    Assert.NotEqual(string.Empty, x);
                });
            }
        }
    }
}
