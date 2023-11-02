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
        CapticalVessel = 8,
        HoverVessel = 16
    }

    public class BlueprintHeader
    {
        private readonly string _fileName;

        #region From BP File
        public int Version { get; private set; }

        public BlueprintType BlueprintType { get; private set; }

        public Vector3<int>? Size { get; private set; } = null;

        public IReadOnlyList<PropertyDetails> Properties { get; private set; }

        public Statistics Statistics { get; private set; }

        public IReadOnlyDictionary<string, int> BlockMap { get; private set; }

        public IReadOnlyDictionary<string, DeviceGroup> DeviceGroups { get; private set; }
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

                return _fileName;
            }
        }

        public int SizeClass => Math.Max(1, (int)Math.Round(SizeClassExact));

        public float SizeClassExact
        {
            get
            {
                var devices   = Statistics.BlockDevices;
                var lights    = Statistics.Lights;
                var triangles = Statistics.TrianglesReal;

                if(triangles == 0)
                    triangles = Statistics.Triangles;

                if (devices == -1 || lights == -1 || triangles == 1)
                    return -1;

                if(triangles == 0)
                {
                    if (devices <= 50)        return 1f;
                    else if (devices <= 250)  return 2f;
                    else if (devices <= 500)  return 3f;
                    else if (devices <= 1000) return 4f;
                    else if (devices <= 1500) return 5f;
                    else if (devices <= 2000) return 6f;
                    else if (devices <= 2500) return 7f;
                    else if (devices <= 3000) return 8f;
                    else if (devices <= 3500) return 9f;
                    else                      return 10 + (devices - 3500) / 500;
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

            _fileName = Path.GetFileNameWithoutExtension(file);

            // File stream since we don't need to work with block data
            using (var fs = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (var reader = new BinaryReader(fs))
            {
                Read(reader);
            }
        }

        internal BlueprintHeader(string fileName, BinaryReader reader)
        {
            _fileName = Path.GetFileNameWithoutExtension(fileName);

            Read(reader);
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

                if(readable)
                    BlockMap = ReadBlockMap(reader);
            }

            if (Version > 10)
            {
                DeviceGroups = ReadDeviceGroups(reader);
            }
        }

        private static List<PropertyDetails> ReadProperties(BinaryReader reader)
        {
            var list = new List<PropertyDetails>();

            reader.ReadInt16(); // Garbage/unknown

            var count = reader.ReadInt16();
            for (var i = 0; i < count; i++)
            {
                var name = (PropertyName)reader.ReadInt32();
                var type = (PropertyType)(reader.ReadInt32() >> 24); // 3 filler bytes + type byte
                object value = null;

                switch(type)
                {
                    case PropertyType.String:
                        value = reader.ReadString(); break;

                    case PropertyType.Bool:
                        value = reader.ReadBoolean();
                        reader.ReadString();
                        break;

                    case PropertyType.Int:
                        value = reader.ReadInt32();
                        reader.ReadString();
                        break;

                    case PropertyType.Single:
                        value = reader.ReadSingle();
                        reader.ReadString();
                        break;

                    case PropertyType.Vector3:
                        value = reader.ReadSingleVector3();
                        reader.ReadString(); 
                        break;

                    case PropertyType.Long:
                        value = reader.ReadInt64();
                        reader.ReadString(); 
                        break;
                }

                if (value == null)
                    throw new FormatException("Property has no value, unknown type?");

                list.Add(new PropertyDetails(name, type, value));
            }

            reader.ReadInt16(); // Garbage/unknown

            return list;
        }

        private static Dictionary<string, int> ReadBlockMap(BinaryReader reader)
        {
            var dict = new Dictionary<string, int>();

            reader.ReadByte(); // Garbage/unknown

            var count = reader.ReadInt32();
            for (var i = 0; i < count; i++)
            {
                var name = reader.ReadString();
                var id = reader.ReadInt16();

                dict[name] = id;
            }

            return dict;
        }
        
        private static Dictionary<string, DeviceGroup> ReadDeviceGroups(BinaryReader reader)
        {
            var dict = new Dictionary<string, DeviceGroup>();

            var deviceGroupVersion = reader.ReadByte();
            var count = reader.ReadInt16();

            for (var i = 0; i < count; i++)
            {
                var group = new DeviceGroup(reader, deviceGroupVersion);
                
                dict[group.Name] = group;
            }

            return dict;
        }
    }
}
