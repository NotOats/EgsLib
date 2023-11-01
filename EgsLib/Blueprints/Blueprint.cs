using ICSharpCode.SharpZipLib.Zip;
using System;
using System.IO;

namespace EgsLib.Blueprints
{
    public class Blueprint
    {
        #region File Info
        public string FilePath { get; }

        public string FileName { get; }

        public long FileSize { get; }

        public DateTime FileLastWritten { get; }
        #endregion

        public BlueprintHeader Header { get; }

        public BlueprintBlockData BlockData { get; }

        public Blueprint(string file)
        {
            if (string.IsNullOrWhiteSpace(file))
                throw new ArgumentNullException(nameof(file));

            if (!File.Exists(file))
                throw new FileNotFoundException("Blueprint file does not exist");

            var fileInfo = new FileInfo(file);

            // Save file info
            FilePath = fileInfo.FullName;
            FileName = Path.GetFileNameWithoutExtension(fileInfo.Name);
            FileSize = fileInfo.Length;

            // Read file & cache LastWriteTime
            var bytes = ReadFileBytes(fileInfo, out DateTime lastWriteTime);
            FileLastWritten = lastWriteTime;

            // Parse file
            using (var ms = new MemoryStream(bytes))
            using (var reader = new BinaryReader(ms))
            {
                Header = new BlueprintHeader(FileName, reader);

                BlockData = ReadBlockData(reader);

                ReadTerrainData(reader);
            }
        }

        private static byte[] ReadFileBytes(FileInfo file, out DateTime lastWriteTime)
        {
            byte[] bytes;

            using (var fs = file.Open(FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                lastWriteTime = file.LastWriteTime;

                var fileLength = fs.Length;
                if (fileLength > int.MaxValue)
                    throw new IOException("File is too large");

                var index = 0;
                var count = (int)fileLength;
                bytes = new byte[fileLength];

                while (count > 0)
                {
                    var n = fs.Read(bytes, index, count);
                    if (n == 0)
                        throw new EndOfStreamException();

                    index += n;
                    count -= n;
                }
            }

            return bytes;
        }

        private BlueprintBlockData ReadBlockData(BinaryReader reader)
        {
            // Older versions read until the end of file while newer ones support terrain data after block data
            var length = checked((int)(reader.BaseStream.Length - reader.BaseStream.Position));
            if(Header.Version > 22)
            {
                length = reader.ReadInt32();
                reader.ReadBytes(2); // Unknown/garbage
            }

            var bytes = reader.ReadBytes(length);

            // Older versions are missing the zip header because ???
            bytes[0] = (byte)'P';
            bytes[1] = (byte)'K';

            using (var compressed = new MemoryStream(bytes, false))
            using (var zipStream = new ZipInputStream(compressed))
            {
                zipStream.GetNextEntry();

                using (var zipReader = new BinaryReader(zipStream))
                {
                    return new BlueprintBlockData(zipReader, Header);
                }
            }
        }

        private void ReadTerrainData(BinaryReader reader)
        {

        }
    }
}
