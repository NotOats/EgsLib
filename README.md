### EGS Lib
A useful library for interacting with Empyrion Galactic Survival game files.

#### Features
- Read blueprint (epb) files
- Read scenario configuration (ecf) files
- Parse playfield yaml into strongly typed .net objects

#### Examples

##### Dumping blueprints to JSON files
```cs
var inputFolder = "Foo";
var outputFolder = "Bar";
var files = Directory.EnumerateFiles(inputFolder, "*.epb");
var options = new JsonSerializerOptions
{
    Converters =
    {
        new JsonStringEnumConverter(),
        new Vector3Converter() // Custom Vector<> json type converter
    }
};

Parallel.ForEach(files, file =>
{
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
```

##### Dumping scenario configuration to JSON files
```cs
var inputFolder = "Foo";
var outputFolder = "Bar";
var files = Directory.EnumerateFiles(inputFolder, "*.ecf");

Parallel.ForEach(files, file =>
{
    // Strongly typed per file classes are also available
    var ecf = new EcfFile(file);
    var objects = ecf.ParseObjects().ToList();

    var outputFile = Path.Join(outputFolder, $"{Path.GetFileNameWithoutExtension(ecf.FileName)}.json");

    using var fs = new FileStream(outputFile, FileMode.Create);
    JsonSerializer.Serialize(fs, objects);
});
```