using EgsLib.ConfigFiles;

var folder = @"D:\Games\SteamLibrary\steamapps\workshop\content\383120\2550354956\Content\Configuration";

/*
var files = new[]
{
    "Dialogues.ecf",
    "TraderNPCConfig.ecf"
};*/

var files = Directory.EnumerateFiles(folder, "*.ecf");
var parsed = new Dictionary<string, GenericEcfFile>();

foreach(var file in files)
{
    var ecf = new GenericEcfFile(file);

    Console.WriteLine($"{ecf.FileName}: {ecf.Objects.Count} objects");

    parsed.Add(ecf.FileName, ecf);
}

Console.WriteLine("");