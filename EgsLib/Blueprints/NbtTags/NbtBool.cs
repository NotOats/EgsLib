using System.IO;

namespace EgsLib.Blueprints.NbtTags
{
    public class NbtBool : INbtTag
    {
        public string Name { get; }
        public bool Value { get; }

        object INbtTag.Value => Value;

        public NbtBool(string name, bool value)
        {
            Name = name;
            Value = value;
        }

        public void Serialize(BinaryWriter writer)
        {
            writer.Write((byte)NbtType.Bool);
            writer.Write(Name);
            writer.Write(Value);
        }
    }
}
