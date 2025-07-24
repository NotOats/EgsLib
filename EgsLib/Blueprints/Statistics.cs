using System;
using System.Collections.Generic;
using System.IO;

namespace EgsLib.Blueprints
{
    public class Statistics
    {
        public const float V26_MAGIC = 4711.08f;

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

        public int Version { get; }
        public float Magic { get; }
        public float ExtraMagic { get; }

        internal Statistics(BinaryReader reader, int version)
        {
            Version = version;

            Lights = reader.ReadInt32();
            Doors = reader.ReadInt32();
            BlockDevices = reader.ReadInt32();
            BlockModels = reader.ReadInt32();
            BlockSolids = reader.ReadInt32();

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
                Teleporters = reader.ReadInt32();
                SpawnPointsUnlocked = reader.ReadInt32();
                SpawnPointsLocked = reader.ReadInt32();
            }

            BlockDistributions = ReadBlockDistributions(reader, version);

            if (version == 26)
            {
                // Comment: No idea what the hell happened here
                Magic = reader.ReadSingle();
                if (Magic == V26_MAGIC)
                {
                    ExtraMagic = reader.ReadByte();
                    ArtilleryAttack = reader.ReadSingle();
                    ArtilleryDefense = reader.ReadSingle();
                    InfantryAttack = reader.ReadSingle();
                    InfantryDefense = reader.ReadSingle();
                }
                else
                {
                    ArtilleryAttack = Magic;
                    ArtilleryDefense = reader.ReadSingle();
                }
            }
            else if (version > 26)
            {
                ArtilleryAttack = reader.ReadSingle();
                ArtilleryDefense = reader.ReadSingle();
                InfantryAttack = reader.ReadSingle();
                InfantryDefense = reader.ReadSingle();
            }

            if (version > 28)
            {
                AdminCore = reader.ReadBoolean();
                KeepContainers = reader.ReadBoolean();
            }
        }

        /// <summary>
        /// Private constructor for creating updated statistics instances
        /// </summary>
        private Statistics(Statistics original, int? blockDevices = null, int? blockModels = null, 
            int? blockSolids = null, int? triangles = null, Dictionary<int, int> blockDistributions = null)
        {
            Version = original.Version;
            Magic = original.Magic;
            ExtraMagic = original.ExtraMagic;

            Lights = original.Lights;
            Doors = original.Doors;
            BlockDevices = blockDevices ?? original.BlockDevices;
            BlockModels = blockModels ?? original.BlockModels;
            BlockSolids = blockSolids ?? original.BlockSolids;
            Triangles = triangles ?? original.Triangles;
            TrianglesReal = triangles ?? original.TrianglesReal;
            Teleporters = original.Teleporters;
            SpawnPointsUnlocked = original.SpawnPointsUnlocked;
            SpawnPointsLocked = original.SpawnPointsLocked;
            BlockDistributions = blockDistributions ?? original.BlockDistributions;
            ArtilleryAttack = original.ArtilleryAttack;
            ArtilleryDefense = original.ArtilleryDefense;
            InfantryAttack = original.InfantryAttack;
            InfantryDefense = original.InfantryDefense;
            AdminCore = original.AdminCore;
            KeepContainers = original.KeepContainers;
        }

        /// <summary>
        /// Creates a new Statistics instance with updated values
        /// </summary>
        public Statistics CreateUpdated(int blockDevices, int blockModels, int blockSolids, 
            int triangles, Dictionary<int, int> blockDistributions)
        {
            return new Statistics(this, blockDevices, blockModels, blockSolids, triangles, blockDistributions);
        }

        public void Serialize(BinaryWriter writer)
        {
            writer.Write(Lights);
            writer.Write(Doors);
            writer.Write(BlockDevices);
            writer.Write(BlockModels);
            writer.Write(BlockSolids);

            if (Version > 12)
                writer.Write(Triangles);

            if (Version > 17)
                writer.Write(TrianglesReal);

            if (Version > 23)
            {
                writer.Write(Teleporters);
                writer.Write(SpawnPointsUnlocked);
                writer.Write(SpawnPointsLocked);
            }

            SerializeBlockDistributions(writer);

            if (Version == 26)
            {
                writer.Write(Magic);
                if (Magic == V26_MAGIC)
                {
                    writer.Write(ExtraMagic);
                    writer.Write(ArtilleryAttack);
                    writer.Write(ArtilleryDefense);
                    writer.Write(InfantryAttack);
                    writer.Write(InfantryDefense);
                }
                else
                    writer.Write(ArtilleryDefense);
            }
            else if (Version > 26)
            {
                writer.Write(ArtilleryAttack);
                writer.Write(ArtilleryDefense);
                writer.Write(InfantryAttack);
                writer.Write(InfantryDefense);
            }

            if (Version > 28)
            {
                writer.Write(AdminCore);
                writer.Write(KeepContainers);
            }
        }

        private void SerializeBlockDistributions(BinaryWriter writer)
        {
            writer.Write((Int16)BlockDistributions.Count);
            foreach (var kvp in BlockDistributions)
            {
                if (Version > 13)
                    writer.Write((Int16)kvp.Key);
                else
                    writer.Write(kvp.Key);
                writer.Write(kvp.Value);
            }
        }

        private static Dictionary<int, int> ReadBlockDistributions(BinaryReader reader, int version)
        {
            var dict = new Dictionary<int, int>();

            var count = reader.ReadInt16();
            for (int i = 0; i < count; i++)
            {
                var key = version > 13 ? reader.ReadInt16() : reader.ReadInt32();
                var value = reader.ReadInt32();

                dict[key] = value;
            }

            return dict;
        }
    }
}
