namespace EgsLib.Tests.Localization
{
    public class LocalizationFixture
    {
        public EgsLib.Localization Localization { get; }

        public LocalizationFixture()
        {
            Localization = new EgsLib.Localization(@"Resources\Localization\Localization.csv");
        }
    }
}
