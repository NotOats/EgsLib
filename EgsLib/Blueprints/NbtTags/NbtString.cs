using System.IO;

namespace EgsLib.Blueprints.NbtTags
{
    public class NbtString : INbtTag
    {
        public string Name { get; }
        public string Value { get; }

        object INbtTag.Value => Value;

        public NbtString(string name, string value)
        {
            Name = name;
            Value = value;
        }

        public void Serialize(BinaryWriter writer)
        {
            writer.Write((byte)NbtType.String);
            writer.Write(Name);
            writer.Write(Value);
        }
    }
}
