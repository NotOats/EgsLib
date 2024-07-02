using EgsLib.Blueprints;
using EgsLib.ConfigFiles.Ecf;
using EgsLib.Playfields;
using EgsLib.Playfields.Files;
using Mono.Options;
using ScenarioDumper.Converters;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;

var input = string.Empty;
var output = "./output";

var options = new OptionSet
{
    { "i|input=", "input scenario path", i => input = i },
    { "o|output=", "optional output folder path", o => output = o}
};

try
{
    options.Parse(args);
} 
catch(OptionException e)
{
    Console.WriteLine(e.Message);
    return;
}

if (string.IsNullOrEmpty(input) || !Directory.Exists(input))
{
    Console.WriteLine("Error: input folder does not exist");
    return;
}

if (!Directory.Exists(output))
    Directory.CreateDirectory(output);

Console.WriteLine("+ Exporting configuration");
DumpConfiguration(Path.Join(input, @"Content\Configuration"), Path.Join(output, "configuration"));

Console.WriteLine("+ Exporting prefabs");
DumpPrefabs(Path.Join(input, @"Prefabs"), Path.Join(output, "prefabs"));

Console.WriteLine("+ Exporting playfields");
DumpPlayfields(Path.Join(input, @"Playfields"), Path.Join(output, "playfields"));

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

static void DumpPlayfields(string inputFolder, string outputFolder)
{
    if (!Directory.Exists(outputFolder))
        Directory.CreateDirectory(outputFolder);

    var directories = Directory.EnumerateDirectories(inputFolder);
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

    Parallel.ForEach(directories, directory =>
    {
        Interlocked.Increment(ref count);

        static object ParseFile(IPlayfieldFile file)
        {
            object contents = file;

            if (file is SpaceDynamic spaceDynamic)
                contents = spaceDynamic.Contents;
            else if (file is PlayfieldDynamic playfieldDynamic)
                contents = playfieldDynamic.Contents;
            else if (file is PlayfieldStatic playfieldStatic)
                contents = playfieldStatic.Contents;
            else if (file is PlayfieldObsoleteFormat obsolete)
                contents = obsolete.Contents;
            else if (file is GenericPlayfieldFile generic)
                contents = generic.Properties;

            return new
            {
                file.Name,
                Contents = contents
            };
        }

        var playfield = new Playfield(directory);
        var obj = new
        {
            playfield.Folder,
            playfield.PlayfieldType,
            Files = playfield.Files.Select(ParseFile)
        };

        var outputFile = Path.Join(outputFolder, Path.ChangeExtension(Path.GetFileName(directory), "json"));

        using var fs = new FileStream(outputFile, FileMode.Create);
        JsonSerializer.Serialize(fs, obj, options);
    });

    sw.Stop();

    Console.WriteLine($"Exported {count:n0} playfields in {sw.ElapsedMilliseconds:n0}ms");
}