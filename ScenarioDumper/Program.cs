using EgsLib.Blueprints;
using EgsLib.ConfigFiles.Ecf;
using ScenarioDumper.Converters;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;

var input = @"D:\Games\SteamLibrary\steamapps\workshop\content\383120\2550354956";
var output = @"./output/";
if (!Directory.Exists(output))
    Directory.CreateDirectory(output);

Console.WriteLine("+ Exporting configuration");
DumpConfiguration(Path.Join(input, @"Content\Configuration"), Path.Join(output, "configuration"));

Console.WriteLine("+ Exporting prefabs");
DumpPrefabs(Path.Join(input, @"Prefabs"), Path.Join(output, "prefabs"));

Console.WriteLine("+ Finished!");

static void DumpConfiguration(string inputFolder, string outputFolder)
{
    if (!Directory.Exists(outputFolder))
        Directory.CreateDirectory(outputFolder);

    var files = Directory.EnumerateFiles(inputFolder, "*.ecf");

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

        var outputFile = Path.Join(outputFolder, $"{Path.GetFileNameWithoutExtension(ecf.FileName)}.json");

        using var fs = new FileStream(outputFile, FileMode.Create);
        JsonSerializer.Serialize(fs, objects);
    }
}


static void DumpPrefabs(string inputFolder, string outputFolder)
{
    if (!Directory.Exists(outputFolder))
        Directory.CreateDirectory(outputFolder);

    var files = Directory.EnumerateFiles(inputFolder, "*.epb");
    var count = 0;
    var options = new JsonSerializerOptions
    {
        Converters =
        {
            new JsonStringEnumConverter(),
            new Vector3Converter()
        }
    };

    var sw = Stopwatch.StartNew();

    Parallel.ForEach(files, file =>
    {
        Interlocked.Increment(ref count);

        var bp = new Blueprint(file);

        var obj = new
        {
            bp.FileName,
            bp.Header,
            BlockData = new
            {
                bp.BlockData?.Entities,
                bp.BlockData?.LockCodes,
                bp.BlockData?.SignalSources,
                bp.BlockData?.SignalReceivers,
                bp.BlockData?.Circuits,
                bp.BlockData?.ShortcutNames
            }
        };

        var outputFile = Path.Join(outputFolder, Path.ChangeExtension(Path.GetFileName(file), "json"));

        using var fs = new FileStream(outputFile, FileMode.Create);
        JsonSerializer.Serialize(fs, obj, options);
    });
    
    sw.Stop();

    Console.WriteLine($"Exported {count:n0} blueprints in {sw.ElapsedMilliseconds:n0}ms");
}