using System.Drawing;
using System.IO;

namespace EgsLib.Blueprints.NbtTags
{
    public class NbtColor : INbtTag
    {
        public string Name { get; }
        public Color Value { get; }

        object INbtTag.Value => Value;

        public NbtColor(string name, byte r, byte g, byte b, byte a)
        {
            Name = name;

            // Look into custom Colorf, this tag isn't really used right now
            Value = Color.FromArgb(a, r, g, b);
        }

        public void Serialize(BinaryWriter writer)
        {
            writer.Write((byte)NbtType.Color);
            writer.Write(Name);
            writer.Write(Value.R);
            writer.Write(Value.G);
            writer.Write(Value.B);
            writer.Write(Value.A);
        }
    }
}
