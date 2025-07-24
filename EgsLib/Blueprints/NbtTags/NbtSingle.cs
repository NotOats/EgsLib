
using System.IO;

namespace EgsLib.Blueprints.NbtTags
{
    public class NbtSingle : INbtTag
    {
        public string Name { get; }
        public float Value { get; }

        object INbtTag.Value => Value;

        public NbtSingle(string name, float value)
        {
            Name = name;
            Value = value;
        }
        public void Serialize(BinaryWriter writer)
        {
            writer.Write((byte)NbtType.Single);
            writer.Write(Name);
            writer.Write(Value);
        }
    }
}
