using EgsLib.Extensions;
using System;
using System.Collections.Generic;
using System.IO;

namespace EgsLib.Blueprints
{
    public class DeviceGroup
    {
        public string Name { get; }
        public IReadOnlyList<DeviceDetails> Devices { get; }

        public bool Unknown1 { get; }
        public bool Unknown2 { get; }
        public byte Unknown3 { get; }

        public DeviceGroup(BinaryReader reader, int version)
        {
            Name     = reader.ReadString();
            Unknown1 = reader.ReadBoolean();
            Unknown2 = version > 4 ? reader.ReadBoolean() : true;
            Unknown3 = version > 3 ? reader.ReadByte() : byte.MaxValue; // Shortcut? Labeled this on https://github.com/ApanLoon/EmpyrionStuff/

            Devices = ReadDevices(reader, version);
        }

        private static List<DeviceDetails> ReadDevices(BinaryReader reader, int version)
        {
            var list = new List<DeviceDetails>();

            var count = reader.ReadInt16();
            for (var i = 0; i < count; i++)
            {
                var location = version > 3 ? reader.ReadIntVector3Packed() : reader.ReadIntVector3();
                var name     = version > 1 ? reader.ReadString() : null;

                list.Add(new DeviceDetails(location, name));
            }

            return list;
        }
    }

    public readonly struct DeviceDetails : IEquatable<DeviceDetails>
    {
        public Vector3<int> Location { get; }
        public string CustomName { get; }

        public DeviceDetails(Vector3<int> location, string customName)
        {
            Location = location;
            CustomName = customName;
        }

        public override string ToString()
        {
            return Location.ToString() + (!string.IsNullOrEmpty(CustomName) ? $" (Name: {CustomName ?? "N/A"})" : string.Empty);
        }

        public override bool Equals(object obj)
        {
            return obj is DeviceDetails details &&
                   Location.Equals(details.Location) &&
                   CustomName == details.CustomName;
        }

        public bool Equals(DeviceDetails other)
        {
            return Location.Equals(other.Location) &&
                   CustomName == other.CustomName;
        }

        public override int GetHashCode()
        {
            int hashCode = -23149117;
            hashCode = hashCode * -1521134295 + Location.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(CustomName);
            return hashCode;
        }

        public static bool operator ==(DeviceDetails left, DeviceDetails right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(DeviceDetails left, DeviceDetails right)
        {
            return !(left == right);
        }
    }
}
