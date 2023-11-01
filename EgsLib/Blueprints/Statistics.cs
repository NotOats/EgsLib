using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace EgsLib.Blueprints
{
    public class Statistics
    {
        public int Lights { get; }
        public int Doors { get; }
        public int BlockDevices { get; }
        public int BlockModels { get; }
        public int BlockSolids { get; }

        public int Triangles { get; }

        public int TrianglesReal { get; }

        public int Teleporters { get; }
        public int SpawnPointsUnlocked { get; }
        public int SpawnPointsLocked { get; }

        public IReadOnlyDictionary<int, int> BlockDistributions { get; }
        
        public float ArtilleryAttack { get; }
        public float ArtilleryDefense { get; }
        public float InfantryAttack { get; }
        public float InfantryDefense { get; }

        public bool AdminCore { get; } = false;
        public bool KeepContainers { get; } = false;

        internal Statistics(BinaryReader reader, int version)
        {
            Lights       = reader.ReadInt32();
            Doors        = reader.ReadInt32();
            BlockDevices = reader.ReadInt32();
            BlockModels  = reader.ReadInt32();
            BlockSolids  = reader.ReadInt32();

            if (version > 12)
            {
                Triangles = reader.ReadInt32();
            }

            if (version > 17)
            {
                TrianglesReal = reader.ReadInt32();
            }

            if (version > 23)
            {
                Teleporters         = reader.ReadInt32();
                SpawnPointsUnlocked = reader.ReadInt32();
                SpawnPointsLocked   = reader.ReadInt32();
            }

            BlockDistributions = ReadBlockDistributions(reader, version);

            if(version == 26)
            {
                // Comment: No idea what the hell happened here
                float magic = reader.ReadSingle();
                if(magic == 4711.08)
                {
                    reader.ReadByte();
                    ArtilleryAttack  = reader.ReadSingle();
                    ArtilleryDefense = reader.ReadSingle();
                    InfantryAttack   = reader.ReadSingle();
                    InfantryDefense  = reader.ReadSingle();
                }
                else
                {
                    ArtilleryAttack = magic;
                    ArtilleryDefense = reader.ReadSingle();
                }
            }
            else if (version > 26)
            {
                ArtilleryAttack  = reader.ReadSingle();
                ArtilleryDefense = reader.ReadSingle();
                InfantryAttack   = reader.ReadSingle();
                InfantryDefense  = reader.ReadSingle();
            }

            if (version > 28)
            {
                AdminCore = reader.ReadBoolean();
                KeepContainers = reader.ReadBoolean();
            }
        }

        private static Dictionary<int, int> ReadBlockDistributions(BinaryReader reader, int version)
        {
            var dict = new Dictionary<int, int>();

            var count = reader.ReadInt16();
            for(int i = 0; i < count; i++)
            {
                var key = version > 13 ? reader.ReadInt16() : reader.ReadInt32();
                var value = reader.ReadInt32();

                dict[key] = value;
            }

            return dict;
        }
    }
}
