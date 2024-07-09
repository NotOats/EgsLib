using EgsLib.ConfigFiles;
using System.Diagnostics;

namespace EgsLib.Tests.ConfigFiles
{
    public class BlockFixture : BaseFileFixture<Block>
    {
        public BlockFixture() : base(@"Resources\Configuration\BlocksConfig.ecf", Block.ReadFile)
        { }
    }

    public class BlockTests : IClassFixture<BlockFixture>
    {
        private BlockFixture Fixture { get; }
        public BlockTests(BlockFixture fixture) => Fixture = fixture;


        [Fact]
        public void HasRequiredFields()
        {
            foreach(var block in Fixture.Objects)
            {
                Assert.NotNull(block.Name);
                Assert.NotEqual(string.Empty, block.Name);
            }
        }

        [Theory]
        [InlineData("Air", 0, "air")]
        [InlineData("CrystalStraight", 894, "rock")]
        [InlineData("WeaponSVBombLauncherT2", -1, "WeaponSmall")]
        public void BlockHasCorrectValues(string name, int id, string material)
        {
            var entry = Fixture.Objects.FirstOrDefault(x => x.Name == name);
            
            Assert.NotNull(entry);

            // Some blocks have auto generated Ids, these are save specific.
            if (id != -1)
                Assert.Equal(id, entry.Id);

            Assert.Equal(material, entry.Material);
        }
    }
}
