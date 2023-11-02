using EgsLib.ConfigFiles.Ecf;

namespace EgsLib.Tests.ConfigFiles
{
    public class EcfFileTests
    {
        [Theory]
        [ClassData(typeof(EcfTestData))]
        public void EcfFileParsesCorrectly(EcfFileDetails details)
        {
            var ecfFile = new EcfFile(details.File);

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
