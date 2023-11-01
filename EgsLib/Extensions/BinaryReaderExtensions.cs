using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Text;

namespace EgsLib.Extensions
{
    internal static class BinaryReaderExtensions
    {
        public static Vector3<int> ReadIntVector3(this BinaryReader reader)
        {
            var x = reader.ReadInt32();
            var y = reader.ReadInt32();
            var z = reader.ReadInt32();

            return new Vector3<int>(x, y, z);
        }

        public static Vector3<int> ReadIntVector3Packed(this BinaryReader reader)
        {
            var value = reader.ReadUInt32();
            var x = (value >> 20 & 4095) - 2048;
            var y = value >> 12 & 255;
            var z = (value & 4095) - 2048;

            return new Vector3<int>((int)x, (int)y, (int)z);
        }


        public static Vector3<float> ReadSingleVector3(this BinaryReader reader)
        {
            var x = reader.ReadSingle();
            var y = reader.ReadSingle();
            var z = reader.ReadSingle();

            return new Vector3<float>(x, y, z);
        }
    }
}
