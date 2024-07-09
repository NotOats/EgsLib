using EgsLib.ConfigFiles;


namespace EgsLib.Tests.ConfigFiles
{
    public class ItemFixture : BaseFileFixture<Item>
    {
        public ItemFixture() : base(@"Resources\Configuration\ItemsConfig.ecf", Item.ReadFile)
        { }
    }

    public class ItemTests : IClassFixture<ItemFixture>
    {
        private ItemFixture Fixture { get; }

        public ItemTests(ItemFixture fixture) => Fixture = fixture;

        [Fact]
        public void HasRequiredFields()
        {
            foreach (var item in Fixture.Objects)
            {
                Assert.True(item.Id > 0, "Expected item.Id to be greater than 0");

                Assert.NotNull(item.Name);
                Assert.NotEqual(string.Empty, item.Name);
            }
        }

        [Theory]
        [InlineData(90, "RespiratorCharge", "Components")]
        [InlineData(3122, "Eden_Cryotorch", "Weapons/Items")]
        [InlineData(3142, "Eden_TurretBolterBA_Ammo", "Weapons/Items")]
        public void ItemHasCorrectValues(int id, string name, string category)
        {
            var def = Fixture.Objects.FirstOrDefault(x => x.Name == name);
            Assert.NotNull(def);

            Assert.Equal(id, def.Id);
            Assert.Equal(name, def.Name);
            Assert.Equal(category, def.Category);
        }
    }
}
