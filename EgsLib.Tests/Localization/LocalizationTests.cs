using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EgsLib;

namespace EgsLib.Tests.Localization
{
    public class LocalizationTests : IClassFixture<LocalizationFixture>
    {
        private LocalizationFixture Fixture { get; }

        public LocalizationTests(LocalizationFixture fixture)
        {
            Fixture = fixture;
        }


        [Theory]
        [InlineData("English", "ConcreteDestroyedBlocks", "Concrete Blocks (Damaged)")]
        [InlineData("English", "HullArmoredFull", "Hardened Steel Block")]
        [InlineData("English", "bkiShieldCapacitor", "A secondary capacitor dedicated to supplying reserve energy to a shield generator; increasing shield strength at the cost of shield charge speed.")]
        [InlineData("Deutsch", "ConcreteDestroyedBlocks", "Betonblöcke - zerstört")]
        [InlineData("Français", "ConcreteDestroyedBlocks", "Blocs béton - détruits")]
        public void TestReadValues(string language, string key, string expected)
        {
            var loc = Fixture.Localization;
            var result = loc.Localize(key, language);

            Assert.NotNull(result);
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("Deutsch", "PaxPurgatory")]
        [InlineData("Deutsch", "MedicalIngredients")]
        [InlineData("Deutsch", "ColonyLargeFood")]
        public void TestReadEmptyValues(string language, string key)
        {
            var loc = Fixture.Localization;
            var result = loc.Localize(key, language);

            Assert.NotNull(result);
            Assert.Equal(key, result);
        }
    }
}
