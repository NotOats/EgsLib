using EgsLib.ConfigFiles;

namespace EgsLib.Tests.ConfigFiles
{
    public abstract class BaseFileFixture<T> where T : BaseConfig<T>
    {
        public IReadOnlyList<T> Objects { get; }

        protected BaseFileFixture(string file, Func<string, IEnumerable<T>> reader)
        {
            Objects = reader(file).ToArray();
        }
    }
}
