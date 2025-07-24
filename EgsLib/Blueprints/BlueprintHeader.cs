using EgsLib.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace EgsLib.Blueprints
{
    public enum BlueprintType : byte
    {
        Voxel = 0,
        Base = 2,
        SmallVessel = 4,
        CapitalVessel = 8,
        HoverVessel = 16
    }

    public class BlueprintHeader
    {
        private const int MAGIC_NUMBER = 2022986309;

        private readonly string _fileName;
        
        // Lazy statistics calculation
        private bool _statisticsDirty = false;
        private bool _calculatingStatistics = false; // Recursion guard
        private Statistics _statistics;
        private BlueprintBlockData _blockDataReference;

        #region From BP File
        public int Version { get; set; }

        public BlueprintType BlueprintType { get; set; }

        public Vector3<int>? Size { get; set; } = null;

        public List<PropertyDetails> Properties { get; set; }

        /// <summary>
        /// Gets statistics with lazy calculation - only recalculates when dirty
        /// </summary>
        public Statistics Statistics 
        { 
            get
            {
                if (_statisticsDirty && _blockDataReference != null && !_calculatingStatistics)
                {
                    RecalculateStatistics();
                }
                return _statistics;
            }
            set 
            { 
                _statistics = value;
                _statisticsDirty = false; // Statistics explicitly set, no need to recalculate
            }
        }

        public Dictionary<string, int> BlockMap { get; set; }

        public Dictionary<string, DeviceGroup> DeviceGroups { get; set; }

        public int DeviceGroupVersion { get; set; }

        // Preserve original "garbage" bytes for exact serialization
        private short _propertiesGarbageBefore = 0;
        private short _propertiesGarbageAfter = 0;
        private byte _blockMapGarbageByte = 0;
        #endregion

        /// <summary>
        /// Returns the blueprint's Display Name or File Name if the Display Name property does not exist.
        /// </summary>
        public string DisplayName
        {
            get
            {
                if (GetProperty<string>(PropertyName.DisplayName, out var displayName)
                    && !string.IsNullOrEmpty(displayName))
                {
                    return displayName;
                }

                return Path.GetFileNameWithoutExtension(_fileName);
            }
        }

        public int SizeClass => Math.Max(1, (int)Math.Round(SizeClassExact));

        public float SizeClassExact
        {
            get
            {
                // TODO: Investigate missting statistics
                // RE's 'Eden_New.epb' (version 3) doesn't seem to have to have an entry for it.
                // Not sure what the correct response is for this, just use -1 for now
                if (Statistics == null)
                    return -1;

                var devices = Statistics.BlockDevices;
                var lights = Statistics.Lights;
                var triangles = Statistics.TrianglesReal;

                if (triangles == 0)
                    triangles = Statistics.Triangles;

                if (devices == -1 || lights == -1 || triangles == 1)
                    return -1;

                if (triangles == 0)
                {
                    if (devices <= 50) return 1f;
                    else if (devices <= 250) return 2f;
                    else if (devices <= 500) return 3f;
                    else if (devices <= 1000) return 4f;
                    else if (devices <= 1500) return 5f;
                    else if (devices <= 2000) return 6f;
                    else if (devices <= 2500) return 7f;
                    else if (devices <= 3000) return 8f;
                    else if (devices <= 3500) return 9f;
                    else return 10 + (devices - 3500) / 500;
                }

                return ((devices * 0.1f) + (lights * 0.05f) + (triangles * 0.00027f)) / 3f;
            }
        }

        public BlueprintHeader(string file)
        {
            if (string.IsNullOrWhiteSpace(file))
                throw new ArgumentNullException(nameof(file));

            if (!File.Exists(file))
                throw new FileNotFoundException("Blueprint file does not exist");

            _fileName = file;

            // File stream since we don't need to work with block data
            using (var fs = new FileStream(_fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (var reader = new BinaryReader(fs))
            {
                Read(reader);
            }
        }

        internal BlueprintHeader(string fileName, BinaryReader reader)
        {
            _fileName = fileName;

            Read(reader);
        }

        /// <summary>
        /// Sets a property value, removing any existing property with the same name
        /// </summary>
        public void SetProperty(PropertyName name, PropertyType type, object value, string metadata = "")
        {
            // Remove existing property if it exists
            Properties.RemoveAll(p => p.Name == name);

            // Add new property
            Properties.Add(new PropertyDetails(name, type, value, metadata));
        }

        /// <summary>
        /// Sets the blueprint's display name
        /// </summary>
        public void SetDisplayName(string displayName)
        {
            SetProperty(PropertyName.DisplayName, PropertyType.String, displayName);
        }

        /// <summary>
        /// Sets the creator player name
        /// </summary>
        public void SetCreatorName(string creatorName)
        {
            SetProperty(PropertyName.CreatorPlayerName, PropertyType.String, creatorName);
        }

        /// <summary>
        /// Sets the creator player ID (often used for Steam ID)
        /// </summary>
        public void SetCreatorPlayerId(long playerId)
        {
            SetProperty(PropertyName.CreatorPlayerId, PropertyType.Long, playerId);
        }

        /// <summary>
        /// Adds or updates a block in the block map
        /// </summary>
        public void AddToBlockMap(string blockName, int blockId)
        {
            if (BlockMap == null)
                BlockMap = new Dictionary<string, int>();
            BlockMap[blockName] = blockId;
        }

        /// <summary>
        /// Recalculates statistics based on the provided block data
        /// </summary>
        public void UpdateStatistics(BlueprintBlockData blockData)
        {
            if (blockData == null || _statistics == null)
                return;

            // Store reference for lazy calculation
            _blockDataReference = blockData;
            
            // Mark statistics as dirty instead of recalculating immediately
            MarkStatisticsDirty();
        }

        /// <summary>
        /// Marks statistics as needing recalculation (lazy evaluation)
        /// </summary>
        public void MarkStatisticsDirty()
        {
            _statisticsDirty = true;
        }

        /// <summary>
        /// Forces immediate statistics recalculation (use sparingly)
        /// </summary>
        public void ForceUpdateStatistics()
        {
            if (_blockDataReference != null)
            {
                RecalculateStatistics();
            }
        }

        /// <summary>
        /// Internal method that performs the actual statistics calculation
        /// </summary>
        private void RecalculateStatistics()
        {
            if (_blockDataReference == null || _statistics == null || _calculatingStatistics)
                return;

            // Set recursion guard
            _calculatingStatistics = true;

            try
            {
                // Count blocks by type
                var blockDistributions = new Dictionary<int, int>();
                int totalBlocks = 0;

                // Use direct size calculation to avoid any potential circular dependencies
                int blocksCount = _blockDataReference.Size.X * _blockDataReference.Size.Y * _blockDataReference.Size.Z;

                for (int i = 0; i < blocksCount; i++)
                {
                    var block = _blockDataReference.Blocks[i];
                    if (!block.IsEmpty)
                    {
                        totalBlocks++;
                        int blockId = block.BlockId;
                        if (blockDistributions.TryGetValue(blockId, out var value))
                            blockDistributions[blockId] = value + 1;
                        else
                            blockDistributions.Add(blockId, 1);
                    }
                }

                // Create new statistics with updated values
                // Note: This is a simplified approach - in reality you'd need block type information
                // to properly categorize blocks as lights, doors, devices, etc.
                _statistics = Statistics.CreateUpdated(
                    totalBlocks, // Simplified: assume all blocks are devices
                    totalBlocks,
                    totalBlocks,
                    totalBlocks * 12, // Estimated triangles
                    blockDistributions
                );

                _statisticsDirty = false;
            }
            finally
            {
                // Reset recursion guard (always execute even if exception occurs)
                _calculatingStatistics = false;
            }
        }

        public bool GetProperty<T>(PropertyName name, out T value)
        {
            var exists = GetProperty(name, typeof(T), out object propertyValue);
            if (exists && propertyValue is T converted)
            {
                value = converted;
                return true;
            }

            value = default;
            return false;
        }

        public bool GetProperty(PropertyName name, Type type, out object value)
        {
            var entry = Properties.FirstOrDefault(x => x.Name == name);
            if (entry != default)
            {
                // Easy, straight return
                if (entry.Value.GetType() == type)
                {
                    value = entry.Value;
                    return true;
                }

                // Wonky conversions for specific properties
                if (name == PropertyName.ChangedDate
                    && type == typeof(DateTime)
                    && entry.Value is long date)
                {
                    value = DateTime.FromBinary(date);
                    return true;
                }

                if (name == PropertyName.PivotPoint
                    && type == typeof(Vector3<int>)
                    && entry.Value is long packedPivotPoint)
                {
                    var x = (int)(((ulong)packedPivotPoint >> 32) & 0xFFFF) - 32768;
                    var y = (int)(((ulong)packedPivotPoint >> 16) & 0xFFFF) - 32768;
                    var z = (int)((ulong)packedPivotPoint & 0xFFFF) - 32768;

                    value = new Vector3<int>(x, y, z);
                    return true;
                }
            }

            value = default;
            return false;
        }

        public void Serialize(BinaryWriter writer)
        {
            writer.Write(MAGIC_NUMBER);
            writer.Write(Version);
            if (Version > 1)
                writer.Write((byte)BlueprintType);
            if (Version > 2)
            {
                writer.WriteIntVector3(Size ?? new Vector3<int>());
                SerializeProperties(writer);
            }
            if (Version > 3)
                Statistics.Serialize(writer);
            if (Version > 27)
            {
                if (BlockMap != null && BlockMap.Count > 0)
                {
                    writer.Write(true);
                    SerializeBlockMap(writer);
                }
                else
                    writer.Write(false);
            }
            if (Version > 10)
                SerializeDeviceGroups(writer);
        }

        private void Read(BinaryReader reader)
        {
            if (reader.ReadInt32() != 2022986309)
                throw new FormatException("Invalid blueprint: signature not found");

            Version = reader.ReadInt32();

            if (Version > 1)
            {
                BlueprintType = (BlueprintType)reader.ReadByte();
            }

            if (Version > 2)
            {
                Size = reader.ReadIntVector3();
                Properties = ReadProperties(reader);
            }

            if (Version > 3)
            {
                Statistics = new Statistics(reader, Version);
            }

            if (Version > 27)
            {
                var readable = reader.ReadBoolean();

                if (readable)
                    BlockMap = ReadBlockMap(reader);
            }

            if (Version > 10)
            {
                DeviceGroupVersion = reader.ReadByte();
                DeviceGroups = ReadDeviceGroups(reader, DeviceGroupVersion);
            }
        }

        private List<PropertyDetails> ReadProperties(BinaryReader reader)
        {
            var list = new List<PropertyDetails>();

            _propertiesGarbageBefore = reader.ReadInt16(); // Capture garbage/unknown

            var count = reader.ReadInt16();
            for (var i = 0; i < count; i++)
            {
                var name = (PropertyName)reader.ReadInt32();
                var type = (PropertyType)(reader.ReadInt32() >> 24); // 3 filler bytes + type byte
                object value = null;
                string metadata = null;

                switch (type)
                {
                    case PropertyType.String:
                        value = reader.ReadString(); break;

                    case PropertyType.Bool:
                        value = reader.ReadBoolean();
                        metadata = reader.ReadString();
                        break;

                    case PropertyType.Int:
                        value = reader.ReadInt32();
                        metadata = reader.ReadString();
                        break;

                    case PropertyType.Single:
                        value = reader.ReadSingle();
                        metadata = reader.ReadString();
                        break;

                    case PropertyType.Vector3:
                        value = reader.ReadSingleVector3();
                        metadata = reader.ReadString();
                        break;

                    case PropertyType.Long:
                        value = reader.ReadInt64();
                        metadata = reader.ReadString();
                        break;
                }

                if (value == null)
                    throw new FormatException("Property has no value, unknown type?");

                list.Add(new PropertyDetails(name, type, value, metadata));
            }

            _propertiesGarbageAfter = reader.ReadInt16(); // Capture garbage/unknown

            return list;
        }

        private Dictionary<string, int> ReadBlockMap(BinaryReader reader)
        {
            var dict = new Dictionary<string, int>();

            _blockMapGarbageByte = reader.ReadByte(); // Capture garbage/unknown

            var count = reader.ReadInt32();
            for (var i = 0; i < count; i++)
            {
                var name = reader.ReadString();
                var id = reader.ReadInt16();

                dict[name] = id;
            }

            return dict;
        }

        private static Dictionary<string, DeviceGroup> ReadDeviceGroups(BinaryReader reader, int deviceGroupVersion)
        {
            var dict = new Dictionary<string, DeviceGroup>();

            var count = reader.ReadInt16();
            for (var i = 0; i < count; i++)
            {
                var group = new DeviceGroup(reader, deviceGroupVersion);

                dict[group.Name] = group;
            }

            return dict;
        }
        private void SerializeBlockMap(BinaryWriter writer)
        {
            writer.Write(_blockMapGarbageByte);
            writer.Write(BlockMap.Count);

            foreach (var kvp in BlockMap)
            {
                writer.Write(kvp.Key);
                writer.Write((short)kvp.Value);
            }
        }

        private void SerializeDeviceGroups(BinaryWriter writer)
        {
            if (Version > 10)
            {
                writer.Write((byte)DeviceGroupVersion);
                writer.Write((short)DeviceGroups.Count);

                foreach (var kvp in DeviceGroups)
                {
                    kvp.Value.Serialize(writer, DeviceGroupVersion);
                }
            }
        }

        private void SerializeProperties(BinaryWriter writer)
        {
            writer.Write(_propertiesGarbageBefore);
            writer.Write((Int16)Properties.Count);
            foreach (var p in Properties)
            {
                p.Serialize(writer);
            }
            writer.Write(_propertiesGarbageAfter);
        }
    }
}
