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
    }
}
