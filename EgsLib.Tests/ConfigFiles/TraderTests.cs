using EgsLib.ConfigFiles;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EgsLib.Tests.ConfigFiles
{
    public class TraderFixture : BaseFileFixture<Trader>
    {
        public TraderFixture() : base(@"Resources\Configuration\TraderNPCConfig.ecf", Trader.ReadFile)
        { }
    }

    public class TraderTests : IClassFixture<TraderFixture>
    {
        private TraderFixture Fixture { get; }

        public TraderTests(TraderFixture fixture) => Fixture = fixture;

        [Fact]
        public void HasRequiredFields()
        {
            foreach (var trader in Fixture.Objects)
            {
                Assert.NotNull(trader.Name);
                Assert.NotEqual(string.Empty, trader.Name);

                // Properties but should always be there
                Assert.NotNull(trader.SellingText);
                Assert.NotEqual(string.Empty, trader.SellingText);

                Assert.NotNull(trader.SellingGoods);
            }
        }

        [Theory]
        [InlineData("TraderDefault", "trwFood", new[] { "CannedVegetables", "CannedMeat" }, new[] { "CannedVegetables", "CannedMeat" })]
        [InlineData("QuantumSTAR", "trwSpecial", new[] { "AutoMinerCore", "CPUExtenderSVT2" }, new[] { "AutoMinerCore", "Computer" })]
        [InlineData("The Rusty Barrel", "trwFood", new[] { "Coffee", "MeatBurger" }, new[] { "Liquors", "Flour" })]
        [InlineData("Eden_ColonistsMilitary", "", new[] { "Bandages", "Shotgun", "Token:6416" }, new[] { "Bandages", "Shotgun" })]
        public void TraderRequiredProperties(string name, string sellingGoods, string[] selling, string[] buying)
        {
            var trader = Fixture.Objects.FirstOrDefault(x => x.Name == name);
            Assert.NotNull(trader);

            Assert.Equal(name, trader.Name);
            Assert.Equal(sellingGoods, trader.SellingGoods);
            
            foreach (var item in selling)
            {
                Assert.Contains(trader.Sells, x => x.Name == item);
            }

            foreach (var item in buying)
            {
                Assert.Contains(trader.Buys, x => x.Name == item);
            }
        }
    }

}
