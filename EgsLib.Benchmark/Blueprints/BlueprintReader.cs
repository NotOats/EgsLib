using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using EgsLib.Blueprints;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace EgsLib.Benchmark.Blueprints
{
    //[MemoryDiagnoser]
    [SimpleJob(RuntimeMoniker.Net481)]
    [SimpleJob(RuntimeMoniker.Net70)]
    public class BlueprintReader
    {
        private static string ProgramFolder =>
            Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)
            ?? throw new Exception("Failed to find current program folder");

        private readonly List<string> _blueprintFiles;

        public BlueprintReader()
        {
            var searchPath = Path.Combine(ProgramFolder, @"Resources\Blueprints");

            _blueprintFiles = Directory
                .EnumerateFiles(searchPath, "*.epb")
                .ToList();
        }

        [Benchmark]
        public List<Blueprint> ReadBlueprints()
        {
            return _blueprintFiles
                .Select(f => new Blueprint(f))
                .ToList();
        }
    }
}
