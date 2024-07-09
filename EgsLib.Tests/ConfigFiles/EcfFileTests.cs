using EgsLib.ConfigFiles;
using EgsLib.ConfigFiles.Ecf;

namespace EgsLib.Tests.ConfigFiles
{
    public class EcfFileFixture
    {
        private static readonly Dictionary<string, Func<string, IEnumerable<object>>> _files = new()
        {
            { @"Resources\Configuration\BlocksConfig.ecf", Block.ReadFile },
            { @"Resources\Configuration\BlockGroupsConfig.ecf", BlockGroup.ReadFile },
            { @"Resources\Configuration\Containers.ecf", Container.ReadFile },
            { @"Resources\Configuration\Dialogues.ecf", Dialogue.ReadFile },
            { @"Resources\Configuration\GalaxyConfig.ecf", Galaxy.ReadFile },
            { @"Resources\Configuration\GlobalDefsConfig.ecf", GlobalDef.ReadFile },
            { @"Resources\Configuration\ItemsConfig.ecf", Item.ReadFile },
            { @"Resources\Configuration\LootGroups.ecf", LootGroup.ReadFile },
            { @"Resources\Configuration\MaterialConfig.ecf", Material.ReadFile },
            { @"Resources\Configuration\StatusEffects.ecf", StatusEffect.ReadFile },
            { @"Resources\Configuration\Templates.ecf", Template.ReadFile },
            { @"Resources\Configuration\TokenConfig.ecf", Token.ReadFile },
            { @"Resources\Configuration\TraderNPCConfig.ecf", Trader.ReadFile }
        };

        public IReadOnlyList<object> Entries { get; }
        public IReadOnlyList<string> Files { get; }

        public EcfFileFixture()
        {
            Entries = _files.SelectMany(x => x.Value(x.Key)).ToArray();
            Files = _files.Select(x => x.Key).ToArray();
        }
    }

    public class EcfFileTests : IClassFixture<EcfFileFixture>
    {
        private EcfFileFixture Fixture { get; }
        public EcfFileTests(EcfFileFixture fixture) => Fixture = fixture;

        [Fact]
        public void ObjectsHaveNoUnparsedProperties()
        {
            var entries = Fixture.Entries
                .Cast<IBaseConfig>()
                .Where(x => x != null)
                .ToList();

            Assert.NotNull(entries);
            Assert.NotEmpty(entries);

            Assert.All(entries, x => Assert.Empty(x.UnparsedProperties));
        }

        [Fact]
        public void EcfFileParsesCorrectly()
        {
            foreach (var file in Fixture.Files)
            {
                var ecfFile = new EcfFile(file);
                var objects = ecfFile.ParseObjects();

                Assert.NotNull(objects);
                Assert.NotEmpty(objects);

                Assert.All(objects, x =>
                {
                    Assert.False(string.IsNullOrWhiteSpace(x.Type));

                    // Objects must have fields
                    Assert.NotNull(x.Fields);
                    Assert.NotEmpty(x.Fields);

                    // Everything else is optional but shouldn't be null
                    Assert.NotNull(x.Properties);
                    Assert.NotNull(x.Children);
                });
            }
        }
    }
}
