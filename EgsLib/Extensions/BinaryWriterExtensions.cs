using System.IO;

namespace EgsLib.Extensions
{
    public static class BinaryWriterExtensions
    {
        public static void WriteIntVector3(this BinaryWriter writer, Vector3<int> vector)
        {
            writer.Write(vector.X);
            writer.Write(vector.Y);
            writer.Write(vector.Z);
        }

        public static void WriteIntVector3Packed(this BinaryWriter writer, Vector3<int> vector)
        {
            var x = (uint)((vector.X + 2048) & 4095) << 20;
            var y = (uint)(vector.Y & 255) << 12;
            var z = (uint)((vector.Z + 2048) & 4095);

            var packed = x | y | z;
            writer.Write(packed);
        }

        public static void WriteSingleVector3(this BinaryWriter writer, Vector3<float> vector)
        {
            writer.Write(vector.X);
            writer.Write(vector.Y);
            writer.Write(vector.Z);
        }
    }
}
