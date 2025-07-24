using System.IO;

namespace EgsLib.Blueprints.NbtTags
{
    public interface INbtTag
    {
        string Name { get; }
        object Value { get; }

        void Serialize(BinaryWriter writer);
    }
}
