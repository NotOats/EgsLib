using System.IO;

namespace EgsLib.Blueprints.NbtTags
{
    public class NbtInt32 : INbtTag
    {
        public string Name { get; }
        public int Value { get; }

        object INbtTag.Value => Value;

        public NbtInt32(string name, int value)
        {
            Name = name;
            Value = value;
        }

        public void Serialize(BinaryWriter writer)
        {
            writer.Write((byte)NbtType.Int32);
            writer.Write(Name);
            writer.Write(Value);
        }
    }
}
