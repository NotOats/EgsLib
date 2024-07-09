using EgsLib.ConfigFiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EgsLib.Tests.ConfigFiles
{
    public class TokenFixture : BaseFileFixture<Token>
    {
        public TokenFixture() : base(@"Resources\Configuration\TokenConfig.ecf", Token.ReadFile)
        { }
    }

    public class TokenTests : IClassFixture<TokenFixture>
    {
        private TokenFixture Fixture { get; }

        public TokenTests(TokenFixture fixture) => Fixture = fixture;

        [Fact]
        public void HasRequiredFields()
        {
            foreach (var token in Fixture.Objects)
            {
                Assert.NotNull(token.Name);
                Assert.NotEqual(string.Empty, token.Name);
                Assert.True(token.Id > 0, "Expected token id to be greate than 0");

                // Properties but should always be there
                Assert.NotNull(token.Description);
                Assert.NotEqual(string.Empty, token.Description);
            }
        }

        [Theory]
        [InlineData("KeyCardRed", 1, null, null)]
        [InlineData("[c][3C00FF]Long Heavy Barrel[/c]", 22, "WeaponKitA", null)]
        [InlineData("[c][FFC100]Work Order - Solaris-AMP[/c]", 4057, "OrderA", null)]
        [InlineData("Schematic - [c][FFFFFF]Advanced Wind Turbine[-][/c]", 6416, "Eden_WindTurbineActiveT2_Recipe", false)]
        public void TokenHasRequiredProperties(string name, int id, string? icon, bool? dropOnDeath)
        {
            var token = Fixture.Objects.FirstOrDefault(x => x.Name == name);
            Assert.NotNull(token);

            Assert.Equal(name, token.Name);
            Assert.Equal(id, token.Id);
            Assert.Equal(icon, token.CustomIcon);
            Assert.Equal(dropOnDeath, token.DropOnDeath);
        }
    }

}
