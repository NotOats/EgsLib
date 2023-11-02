using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using EgsLib.ConfigFiles;
using EgsLib.ConfigFiles.Ecf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace EgsLib.Benchmark.ConfigFiles
{
    [SimpleJob(RuntimeMoniker.Net481)]
    [SimpleJob(RuntimeMoniker.Net70)]
    public class EcfFilePerformance
    {
        private static string ProgramFolder =>
            Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)
            ?? throw new Exception("Failed to find current program folder");

        private readonly List<string> _configFiles;

        private readonly Dictionary<string, Func<string, IEnumerable<object>>> _files = new Dictionary<string, Func<string, IEnumerable<object>>>()
        {
            { @"Resources\Configuration\Dialogues.ecf", Dialogue.ReadFile },
            { @"Resources\Configuration\GalaxyConfig.ecf", Galaxy.ReadFile },
            { @"Resources\Configuration\GlobalDefsConfig.ecf", GlobalDef.ReadFile },
            { @"Resources\Configuration\ItemsConfig.ecf", Item.ReadFile },
            { @"Resources\Configuration\LootGroups.ecf", LootGroup.ReadFile },
            { @"Resources\Configuration\MaterialConfig.ecf", Material.ReadFile },
            { @"Resources\Configuration\StatusEffects.ecf", StatusEffect.ReadFile },
            { @"Resources\Configuration\Templates.ecf", Template.ReadFile },
            { @"Resources\Configuration\TokenConfig.ecf", Token.ReadFile },
            { @"Resources\Configuration\TraderNPCConfig.ecf", Trader.ReadFile }
        };

        public EcfFilePerformance()
        {
            var searchPath = Path.Combine(ProgramFolder, @"Resources\Configuration");

            _configFiles = Directory
                .EnumerateFiles(searchPath, "*.ecf")
                .ToList();
        }

        [Benchmark]
        public List<IEcfObject> ReadAllEcfFiles()
        {
            return _configFiles
                .Select(x => new EcfFile(x))
                .SelectMany(x => x.ParseObjects())
                .ToList();
        }

        [Benchmark]
        public List<IEcfObject> ReadSelectEcfFiles()
        {
            return _files
                .Select(x => new EcfFile(x.Key))
                .SelectMany(x => x.ParseObjects())
                .ToList();
        }

        [Benchmark]
        public List<object> ReadParsedEcfFiles()
        {
            return _files
                .SelectMany(x => x.Value(x.Key))
                .ToList();
        }
    }
}
