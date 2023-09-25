using EgsLib.ConfigFiles;
using EgsLib.ConfigFiles.Ecf;
using System.Diagnostics;


ReadFile(@"D:\Games\SteamLibrary\steamapps\workshop\content\383120\2550354956\Content\Configuration\MaterialConfig.ecf", Material.ReadFile);
ReadFile(@"D:\Games\SteamLibrary\steamapps\workshop\content\383120\2550354956\Content\Configuration\StatusEffects.ecf", StatusEffect.ReadFile, new[] { "Name" } );
ReadFile(@"D:\Games\SteamLibrary\steamapps\workshop\content\383120\2550354956\Content\Configuration\Templates.ecf", Template.ReadFile);
ReadFile(@"D:\Games\SteamLibrary\steamapps\workshop\content\383120\2550354956\Content\Configuration\TokenConfig.ecf", Token.ReadFile);
ReadFile(@"D:\Games\SteamLibrary\steamapps\workshop\content\383120\2550354956\Content\Configuration\TraderNPCConfig.ecf", Trader.ReadFile, new[] { "Item" });
Console.WriteLine();

static T[] ReadFile<T>(string filePath, Func<string, IEnumerable<T>> read, IEnumerable<string>? ignoreMissing = null) where T : BaseConfig
{
    T[] entries;

    using (new DebugTimer(Path.GetFileName(filePath)))
    {
        entries = read(filePath).ToArray();
    }

    ignoreMissing ??= Array.Empty<string>();

    var missing = entries
        .Where(m => m.UnparsedProperties.Count != 0)
        .SelectMany(m => m.UnparsedProperties.Where(p => !ignoreMissing.Any(m => p.Key.StartsWith(m))))
        .DistinctBy(m => m.Key)
        .ToArray();

    if (missing.Length > 0)
        Console.WriteLine($"{typeof(T).Name} unread properties: {string.Join(", ", missing.Select(kvp => kvp.Key))}");

    return entries;
}


var folder = @"D:\Games\SteamLibrary\steamapps\workshop\content\383120\2550354956\Content\Configuration";

var files = Directory.EnumerateFiles(folder, "*.ecf");
var parsed = new Dictionary<string, List<IEcfObject>>();

foreach (var file in files)
{
    var sw = Stopwatch.StartNew();

    var ecf = new EcfFile(file);
    var objects = ecf.ParseObjects().ToList();

    sw.Stop();

    var name = $"{ecf.FileName}:";
    var objCount = $"{objects.Count} obj,";
    var propCount = $"{objects.SelectMany(obj => obj.Properties).Count()} props,";
    var childCount = $"{objects.SelectMany(obj => obj.Children).Count()} children,";
    var elapsed = $"{sw.ElapsedMilliseconds}ms";

    Console.WriteLine($"{name,-25} {objCount,-10} {propCount,-12} {childCount,-14} {elapsed,-4} elapsed");

    parsed.Add(ecf.FileName, objects);
}

Console.WriteLine("Finished!");

internal class DebugTimer : IDisposable
{
    private readonly Stopwatch _stopwatch;
    private readonly string _message;
    public DebugTimer(string message)
    {
        _message = message;
        _stopwatch = Stopwatch.StartNew();
    }

    public void Dispose()
    {
        _stopwatch.Stop();
        Console.WriteLine($"{_message} finished in {_stopwatch.ElapsedMilliseconds}ms");
    }
}