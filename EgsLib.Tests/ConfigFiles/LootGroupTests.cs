using EgsLib.ConfigFiles;


namespace EgsLib.Tests.ConfigFiles
{
    public class LootGroupFixture : BaseFileFixture<LootGroup>
    {
        public LootGroupFixture() : base(@"Resources\Configuration\LootGroups.ecf", LootGroup.ReadFile)
        { }
    }

    public class LootGroupTests : IClassFixture<LootGroupFixture>
    {
        private LootGroupFixture Fixture { get; }

        public LootGroupTests(LootGroupFixture fixture) => Fixture = fixture;

        [Fact]
        public void HasRequiredFields()
        {
            foreach (var group in Fixture.Objects)
            {
                Assert.NotNull(group.Name);
                Assert.NotEqual(string.Empty, group.Name);
            }
        }

        [Theory]
        [MemberData(nameof(LootGroupTestData))]
        public void LootGroupHasRequiredProperties(string name, Range<int> count, 
            Dictionary<string, string> itemCounts, Dictionary<string, float?> itemWeights)
        {
            var group = Fixture.Objects.FirstOrDefault(x => x.Name == name);
            Assert.NotNull(group);

            Assert.NotNull(group.Name);
            Assert.NotEqual(string.Empty, group.Name);
            Assert.Equal(count, group.Count);

            foreach (var itemCount in itemCounts)
            {
                Assert.Contains(group.Items, x => x.Name == itemCount.Key && x.Count == itemCount.Value);
            }

            foreach (var itemWeight in itemWeights)
            {
                Assert.Contains(group.Items, x => x.Name == itemWeight.Key && x.Weight == itemWeight.Value);
            }
        }

        // Name, Count, ItemCount, ItemWeight
        public static IEnumerable<object[]> LootGroupTestData()
        {
            yield return new object[]
            {
                "EscapePodEasy",
                new Range<int>(1, int.MaxValue),
                new Dictionary<string, string>
                {
                    { "EmergencyRations", "1" },
                    { "WaterBottle", "5" },
                    { "RadarSuitT1", "1" },
                    { "SurvivalTent", "1" }
                },
                new Dictionary<string, float?>
                {
                    { "EmergencyRations", null },
                    { "WaterBottle", null },
                    { "RadarSuitT1", null },
                    { "SurvivalTent", null }
                }
            };

            yield return new object[]
            {
                "Keycards",
                new Range<int>(1, 1),
                new Dictionary<string, string>
                {
                    { "KeyCardCommon", "1" },
                    { "KeyCardScience", "1" },
                    { "KeyCardExploration", "1" },
                    { "KeyCardSecurity", "1" },
                    { "KeyCardMilitary", "1" }
                },
                new Dictionary<string, float?>
                {
                    { "KeyCardCommon", 0.75f },
                    { "KeyCardScience", 0.3f },
                    { "KeyCardExploration", 0.3f },
                    { "KeyCardSecurity", 0.2f },
                    { "KeyCardMilitary", 0.1f }
                }
            };
        }
    }
}
