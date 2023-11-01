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
    }
}
