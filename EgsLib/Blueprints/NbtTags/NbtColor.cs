using System.Drawing;

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
    }
}
