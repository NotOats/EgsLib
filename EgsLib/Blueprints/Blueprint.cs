using ICSharpCode.SharpZipLib.Zip;
using System;
using System.IO;
using System.Linq;

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

        public BlueprintHeader Header { get; set; }

        public BlueprintBlockData BlockData { get; set; }

        /// <summary>
        /// Additional data that appears after the block data (possibly checksum or terrain data)
        /// </summary>
        public byte[] TrailingData { get; set; } = new byte[0];

        /// <summary>
        /// Byte and boolean that appear after the ZIP length (as read by the game)
        /// </summary>
        private byte _zipByte;
        private bool _zipBoolean;

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

            // Initialize lazy statistics system with block data reference
            InitializeLazyStatistics();
        }

        #region Helper Methods
        
        /// <summary>
        /// Initializes the lazy statistics system by setting up the block data reference
        /// </summary>
        private void InitializeLazyStatistics()
        {
            // Set up block data reference for lazy statistics calculation
            Header.UpdateStatistics(BlockData);
        }

        /// <summary>
        /// Adds a block with the specified ID at the given position
        /// </summary>
        public void AddBlock(Vector3<int> position, int blockId, int rotation = 0, int color = 0)
        {
            // Add the block to block data (this may expand the blueprint size)
            BlockData.AddBlock(position, blockId, rotation, density: 255);

            // Update header size if block data was expanded
            Header.Size = BlockData.Size;

            // Set color if specified
            if (color != 0)
            {
                BlockData.UpdateBlock(position, color: color);
            }

            // Update block map if needed
            UpdateBlockMapForBlock(blockId);

            // Mark statistics as dirty (lazy calculation)
            Header.MarkStatisticsDirty();
        }

        /// <summary>
        /// Removes a block at the specified position
        /// </summary>
        public void RemoveBlock(Vector3<int> position)
        {
            BlockData.RemoveBlock(position);
            
            // Mark statistics as dirty (lazy calculation)
            Header.MarkStatisticsDirty();
        }

        /// <summary>
        /// Sets the blueprint's display name
        /// </summary>
        public void SetDisplayName(string displayName)
        {
            Header.SetDisplayName(displayName);
        }

        /// <summary>
        /// Sets the creator player name
        /// </summary>
        public void SetCreatorName(string creatorName)
        {
            Header.SetCreatorName(creatorName);
        }

        /// <summary>
        /// Sets the creator player ID (often used for Steam ID)
        /// </summary>
        public void SetCreatorPlayerId(long playerId)
        {
            Header.SetCreatorPlayerId(playerId);
        }

        /// <summary>
        /// Gets a block at the specified position
        /// </summary>
        public Block GetBlock(Vector3<int> position)
        {
            return BlockData.GetBlock(position);
        }

        /// <summary>
        /// Updates properties of a block at the specified position
        /// </summary>
        public void UpdateBlock(Vector3<int> position, int? color = null, long? texture = null,
            byte? textureRotation = null, ushort? damage = null, byte? density = null,
            int? symbol = null, int? symbolRotation = null)
        {
            BlockData.UpdateBlock(position, color, texture, textureRotation, damage, density, symbol, symbolRotation);
        }

        /// <summary>
        /// Saves the blueprint to the specified file path
        /// </summary>
        public void SaveTo(string filePath)
        {
            using (var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            using (var writer = new BinaryWriter(fs))
            {
                Serialize(writer);
            }
        }

        #endregion

        #region Private Helper Methods

        private void UpdateBlockMapForBlock(int blockId)
        {
            string blockName = $"Block_{blockId}";
            Header.AddToBlockMap(blockName, blockId);
        }

        #endregion

        public void Serialize(BinaryWriter bw)
        {
            // Ensure statistics are up-to-date before serialization
            Header.ForceUpdateStatistics();
            
            Header.Serialize(bw);
            SerializeBlockData(bw);
            SerializeTrailingData(bw);
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
            int length;
            if (Header.Version > 22)
            {
                length = reader.ReadInt32();
                _zipByte = reader.ReadByte();      // Read the byte
                _zipBoolean = reader.ReadBoolean(); // Read the boolean
            }
            else
            {
                length = (int)(reader.BaseStream.Length - reader.BaseStream.Position);
            }

            var bytes = reader.ReadBytes(length);

            // Older versions are missing the zip header because ???
            bytes[0] = (byte)'P';
            bytes[1] = (byte)'K';

            var compressed = new MemoryStream(bytes, writable: false);
            using (var zipFile = new ZipFile(compressed, leaveOpen: false))
            {
                var entry = zipFile.Cast<ZipEntry>()
                    .FirstOrDefault(e => e.IsFile && e.Name == "0");

                // TODO: Better error handling for missing block data, low priority since this shouldn't be possible
                if (entry == null)
                    return null;

                Stream stream = null;
                BinaryReader zipReader = null;

                try
                {
                    stream = zipFile.GetInputStream(entry);
                    zipReader = new BinaryReader(stream);

                    return new BlueprintBlockData(zipReader, Header);
                }
                catch (ZipException)
                {
                    // Thrown on malformed zip entry which seems to be an issue with some files
                    // Notably: CV_New, HV_New, SV_New
                    return null;
                }
                finally
                {
                    zipReader?.Dispose();
                    stream?.Dispose();
                }
            }
        }

        private void SerializeBlockData(BinaryWriter writer)
        {
            // Serialize block data to byte[]
            byte[] blockDataBytes;
            using (var blockDataStream = new MemoryStream())
            using (var blockDataWriter = new BinaryWriter(blockDataStream))
            {
                BlockData.Serialize(blockDataWriter);
                blockDataWriter.Flush();
                blockDataBytes = blockDataStream.ToArray();
            }

            // Create a ZIP archive in memory with one entry "0"
            byte[] compressedBytes;
            using (var compressedStream = new MemoryStream())
            {
                using (var zipFile = ZipFile.Create(compressedStream))
                {
                    zipFile.BeginUpdate();
                    zipFile.Add(new ByteArrayDataSource(blockDataBytes), "0");
                    zipFile.CommitUpdate();
                }
                compressedBytes = compressedStream.ToArray();
            }

            if (Header.Version > 22)
            {
                // Write length + 2 "garbage" bytes
                var length = compressedBytes.Length;
                if (length <= 0)
                {
                    throw new InvalidOperationException($"Invalid compressed bytes length: {length}");
                }
                writer.Write(length);
                writer.Write(_zipByte);    // Write the byte 
                writer.Write(_zipBoolean); // Write the boolean
                writer.Write(compressedBytes); // Full ZIP archive
            }
            else
            {
                // Remove the first 2 bytes ("PK") for legacy versions
                var dataWithoutPK = new byte[compressedBytes.Length - 2];
                Array.Copy(compressedBytes, 2, dataWithoutPK, 0, dataWithoutPK.Length);
                writer.Write(dataWithoutPK);
            }
        }

        private void SerializeTrailingData(BinaryWriter writer)
        {
            if (TrailingData.Length > 0)
            {
                writer.Write(TrailingData);
            }
        }


        private void ReadTerrainData(BinaryReader reader)
        {
            // Capture any remaining data in the stream (could be checksum, terrain data, etc.)
            var remainingBytes = reader.BaseStream.Length - reader.BaseStream.Position;
            if (remainingBytes > 0)
            {
                TrailingData = reader.ReadBytes((int)remainingBytes);
            }
            else
            {
                TrailingData = new byte[0];
            }
        }
    }

    public class ByteArrayDataSource : IStaticDataSource
    {
        private readonly byte[] _data;

        public ByteArrayDataSource(byte[] data)
        {
            _data = data;
        }

        public Stream GetSource()
        {
            return new MemoryStream(_data, writable: false);
        }
    }
}
