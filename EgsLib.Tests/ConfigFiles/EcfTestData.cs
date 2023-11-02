using EgsLib.ConfigFiles;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EgsLib.Tests.ConfigFiles
{
    public class EcfTestData : IEnumerable<object[]>
    {
        public IEnumerator<object[]> GetEnumerator()
        {
            var files = new Dictionary<string, Func<string, IEnumerable<object>>> {
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

            foreach (var kvp in files)
            {
                yield return new object[]
                {
                    new EcfFileDetails(kvp.Key, kvp.Value)
                };
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    public class EcfFileDetails
    {
        private static string ProgramFolder =>
            Path.GetDirectoryName(Process.GetCurrentProcess()?.MainModule?.FileName)
            ?? throw new Exception("Failed to find current program folder");

        private readonly Func<string, IEnumerable<object>> _readFunc;

        public string File { get; }

        public EcfFileDetails(string file, Func<string, IEnumerable<object>> readFunc)
        {
            File = Path.Combine(ProgramFolder, file);
            _readFunc = readFunc;
        }

        public IEnumerable<object> ReadEntries()
        {
            return _readFunc(File);
        }

        public IEnumerable<T?> ReadEntries<T>() where T : BaseConfig<T>
        {
            foreach (var obj in ReadEntries())
            {
                if (obj is T casted)
                    yield return casted;
                else
                    yield return null;
            }
        }
    }
}
