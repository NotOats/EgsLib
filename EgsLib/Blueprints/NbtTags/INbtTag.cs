namespace EgsLib.Blueprints.NbtTags
{
    public interface INbtTag
    {
        string Name { get; }
        object Value { get; }
    }
}
