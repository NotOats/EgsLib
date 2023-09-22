using EgsLib.ConfigFiles;
using System.Diagnostics;

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