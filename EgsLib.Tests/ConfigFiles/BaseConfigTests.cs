using EgsLib.ConfigFiles;


namespace EgsLib.Tests.ConfigFiles
{
    public class BaseConfigTests
    {
        [Theory]
        [ClassData(typeof(EcfTestData))]
        public void ObjectsHaveNoUnparsedProperties(EcfFileDetails details)
        {
            var entries = details.ReadEntries()
                .Where(x => x != null)
                .Cast<IBaseConfig>()
                .ToList();

            Assert.NotNull(entries);
            Assert.NotEmpty(entries);

            Assert.All(entries, x => Assert.Empty(x.UnparsedProperties));
        }
    }
}
