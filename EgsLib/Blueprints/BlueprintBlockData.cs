using EgsLib.Blueprints.NbtTags;
using EgsLib.Extensions;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace EgsLib.Blueprints
{
    public class BlueprintBlockData : IDisposable
    {
        private static readonly ArrayPool<Block> BlockPool = ArrayPool<Block>.Create();

        private readonly int _version;

        private Block[] _blocks;
        private Dictionary<Vector3<int>, NbtList> _entities = new Dictionary<Vector3<int>, NbtList>();
        private Dictionary<Vector3<int>, int> _lockCodes = new Dictionary<Vector3<int>, int>();
        private List<NbtList> _signalSources = new List<NbtList>();
        private Dictionary<string, IReadOnlyList<NbtList>> _signalReceivers = new Dictionary<string, IReadOnlyList<NbtList>>();
        private List<NbtList> _circuits = new List<NbtList>();
        private List<string> _shortcutNames = new List<string>();

        /// <summary>
        /// Raw bytes for all remaining data after shortcut names (blueprint parts, snap points, color palette, etc.)
        /// This is captured during read and written back during serialize as a stopgap until we implement proper parsing
        /// </summary>
        private byte[] _remainingDataBytes = new byte[0];

        public Vector3<int> Size { get; set; }

        /// <summary>
        /// Note: Use BlocksSize instead of Blocks.Count.
        /// Blocks.Count may be longer than the actual data
        /// </summary>
        public IReadOnlyList<Block> Blocks => _blocks;
        public int BlocksSize => checked(Size.X * Size.Y * Size.Z);

        public IReadOnlyDictionary<Vector3<int>, NbtList> Entities => _entities;
        public IReadOnlyDictionary<Vector3<int>, int> LockCodes => _lockCodes;
        public IReadOnlyList<NbtList> SignalSources => _signalSources;
        public IReadOnlyDictionary<string, IReadOnlyList<NbtList>> SignalReceivers => _signalReceivers;
        public IReadOnlyList<NbtList> Circuits => _circuits;
        public IReadOnlyList<string> ShortcutNames => _shortcutNames;

        /// <summary>
        /// I don't really know what to call this but it looks like when this is set all blocks get the same density value.
        /// </summary>
        private bool _singleDensityFlag = false;

        internal BlueprintBlockData(BinaryReader reader, BlueprintHeader header)
        {
            Size = ReadSize(reader, header);

            _blocks = BlockPool.Rent(BlocksSize);
            Array.Clear(_blocks, 0, BlocksSize);

            _version = header.Version;

            ReadBlockData(reader);
            ReadBlockDamage(reader);
            ReadDensity(reader);
            ReadColorAndTextures(reader);
            ReadSymbols(reader);
            ReadEntities(reader);
            ReadLockCodes(reader);
            ReadSignals(reader);
            ReadLogicCircuits(reader);
            ReadShortcutNames(reader);

            // Capture remaining data that EgsLib doesn't parse yet:
            // - Blueprint parts section
            // - Snap point match table section  
            // - Block color palette section
            
            // Read all remaining bytes (can't use stream.Length inside ZIP archive)
            var remainingBytes = new List<byte>();
            try
            {
                while (true)
                {
                    remainingBytes.Add(reader.ReadByte());
                }
            }
            catch (EndOfStreamException)
            {
                // Expected when we reach end of stream
            }
            
            _remainingDataBytes = remainingBytes.ToArray();
        }

        /// <summary>
        /// Gets a block at the specified 3D coordinate
        /// </summary>
        public Block GetBlock(Vector3<int> position)
        {
            if (!IsValidPosition(position))
                return new Block();

            int index = CoordinateToIndex(position);
            return _blocks[index];
        }

        /// <summary>
        /// Sets a block at the specified 3D coordinate
        /// </summary>
        public void SetBlock(Vector3<int> position, Block block)
        {
            if (!IsValidPosition(position))
                throw new ArgumentOutOfRangeException(nameof(position), 
                    $"Position {position} is outside blueprint bounds {Size}");

            int index = CoordinateToIndex(position);
            _blocks[index] = block;
        }

        /// <summary>
        /// Adds a block with the specified ID at the given position
        /// </summary>
        public void AddBlock(Vector3<int> position, int blockId, int rotation = 0, byte variant = 0, byte density = 255)
        {
            // Expand blueprint boundaries if the position is outside current bounds
            ExpandBlueprintIfNeeded(position);
            
            var block = Block.Create(blockId, rotation, variant, density);
            SetBlock(position, block);
        }

        /// <summary>
        /// Expands the blueprint size if the given position is outside current bounds
        /// </summary>
        private void ExpandBlueprintIfNeeded(Vector3<int> position)
        {
            // Check if position is already within bounds
            if (IsValidPosition(position))
                return;

            // Calculate new bounds that include the position
            var newMin = new Vector3<int>(
                Math.Min(0, position.X),
                Math.Min(0, position.Y), 
                Math.Min(0, position.Z)
            );
            
            var newMax = new Vector3<int>(
                Math.Max(Size.X - 1, position.X),
                Math.Max(Size.Y - 1, position.Y),
                Math.Max(Size.Z - 1, position.Z)
            );

            var newSize = new Vector3<int>(
                newMax.X - newMin.X + 1,
                newMax.Y - newMin.Y + 1,
                newMax.Z - newMin.Z + 1
            );

            // No expansion needed if size hasn't changed
            if (newSize.X == Size.X && newSize.Y == Size.Y && newSize.Z == Size.Z)
                return;

            // Calculate offset needed for existing blocks
            var offset = new Vector3<int>(-newMin.X, -newMin.Y, -newMin.Z);

            // CRITICAL FIX: Preserve the original size for coordinate calculations
            var originalSize = Size;
            var originalBlocksSize = originalSize.X * originalSize.Y * originalSize.Z;

            // Create new larger block array
            var newBlocksSize = newSize.X * newSize.Y * newSize.Z;
            var newBlocks = BlockPool.Rent(newBlocksSize);
            Array.Clear(newBlocks, 0, newBlocksSize);

            for (int i = 0; i < originalBlocksSize; i++)
            {
                if (!_blocks[i].IsEmpty)
                {
                    // Convert from 1D index to 3D position using original size
                    var oldPos = IndexToCoordinate(i, originalSize);
                    
                    // Apply offset if expanding into negative space
                    var newPos = new Vector3<int>(
                        oldPos.X + offset.X,
                        oldPos.Y + offset.Y, 
                        oldPos.Z + offset.Z
                    );
                    
                    // Convert back to 1D index using new size
                    var newIndex = CoordinateToIndex(newPos, newSize);
                    newBlocks[newIndex] = _blocks[i];
                }
            }

            
            if (offset.X != 0 || offset.Y != 0 || offset.Z != 0)
            {
                UpdateCollectionPositions(offset);
            }

            // Replace old block array with new one
            BlockPool.Return(_blocks, clearArray: true);
            _blocks = newBlocks;
            Size = newSize;
        }

        /// <summary>
        /// Updates positions in entities and lock codes collections when blueprint is expanded
        /// </summary>
        private void UpdateCollectionPositions(Vector3<int> offset)
        {
            // Update entities
            if (_entities.Count > 0)
            {
                var updatedEntities = new Dictionary<Vector3<int>, NbtList>();
                foreach (var entity in _entities)
                {
                    var newPos = new Vector3<int>(
                        entity.Key.X + offset.X,
                        entity.Key.Y + offset.Y,
                        entity.Key.Z + offset.Z
                    );
                    updatedEntities[newPos] = entity.Value;
                }
                _entities = updatedEntities;
            }

            // Update lock codes
            if (_lockCodes.Count > 0)
            {
                var updatedLockCodes = new Dictionary<Vector3<int>, int>();
                foreach (var lockCode in _lockCodes)
                {
                    var newPos = new Vector3<int>(
                        lockCode.Key.X + offset.X,
                        lockCode.Key.Y + offset.Y,
                        lockCode.Key.Z + offset.Z
                    );
                    updatedLockCodes[newPos] = lockCode.Value;
                }
                _lockCodes = updatedLockCodes;
            }
        }

        /// <summary>
        /// Removes a block at the specified position (makes it empty)
        /// </summary>
        public void RemoveBlock(Vector3<int> position)
        {
            if (!IsValidPosition(position))
                return;

            int index = CoordinateToIndex(position);
            _blocks[index].Clear();
        }

        /// <summary>
        /// Updates block properties at the specified position
        /// </summary>
        public void UpdateBlock(Vector3<int> position, int? color = null, long? texture = null, 
            byte? textureRotation = null, ushort? damage = null, byte? density = null,
            int? symbol = null, int? symbolRotation = null)
        {
            if (!IsValidPosition(position))
                return;

            int index = CoordinateToIndex(position);
            
            if (color.HasValue) _blocks[index].Color = color.Value;
            if (texture.HasValue) _blocks[index].Texture = texture.Value;
            if (textureRotation.HasValue) _blocks[index].TextureRotation = textureRotation.Value;
            if (damage.HasValue) _blocks[index].Damage = damage.Value;
            if (density.HasValue) _blocks[index].Density = density.Value;
            if (symbol.HasValue) _blocks[index].Symbol = symbol.Value;
            if (symbolRotation.HasValue) _blocks[index].SymbolRotation = symbolRotation.Value;
        }

        /// <summary>
        /// Adds an entity at the specified position
        /// </summary>
        public void AddEntity(Vector3<int> position, NbtList entityData)
        {
            _entities[position] = entityData;
        }

        /// <summary>
        /// Removes an entity at the specified position
        /// </summary>
        public void RemoveEntity(Vector3<int> position)
        {
            _entities.Remove(position);
        }

        /// <summary>
        /// Adds a lock code at the specified position
        /// </summary>
        public void AddLockCode(Vector3<int> position, int lockCode)
        {
            _lockCodes[position] = lockCode;
        }

        /// <summary>
        /// Removes a lock code at the specified position
        /// </summary>
        public void RemoveLockCode(Vector3<int> position)
        {
            _lockCodes.Remove(position);
        }

        /// <summary>
        /// Converts 3D coordinate to 1D array index
        /// </summary>
        private int CoordinateToIndex(Vector3<int> position)
        {
            return position.X + position.Y * Size.X + position.Z * Size.X * Size.Y;
        }

        /// <summary>
        /// Converts 1D array index to 3D coordinate
        /// </summary>
        private Vector3<int> IndexToCoordinate(int index)
        {
            int x = index % Size.X;
            int y = (index / Size.X) % Size.Y;
            int z = index / (Size.X * Size.Y);
            return new Vector3<int>(x, y, z);
        }

        /// <summary>
        /// Converts 1D array index to 3D coordinate using specified size
        /// </summary>
        private Vector3<int> IndexToCoordinate(int index, Vector3<int> size)
        {
            int x = index % size.X;
            int y = (index / size.X) % size.Y;
            int z = index / (size.X * size.Y);
            return new Vector3<int>(x, y, z);
        }

        /// <summary>
        /// Converts 3D coordinate to 1D array index using specified size
        /// </summary>
        private int CoordinateToIndex(Vector3<int> position, Vector3<int> size)
        {
            return position.X + position.Y * size.X + position.Z * size.X * size.Y;
        }

        /// <summary>
        /// Checks if the position is within blueprint bounds
        /// </summary>
        private bool IsValidPosition(Vector3<int> position)
        {
            return position.X >= 0 && position.X < Size.X &&
                   position.Y >= 0 && position.Y < Size.Y &&
                   position.Z >= 0 && position.Z < Size.Z;
        }

        public void Serialize(BinaryWriter writer)
        {
            if (_version <= 2)
                writer.WriteIntVector3(Size);
            
            SerializeBlockData(writer);
            SerializeBlockDamage(writer);
            SerializeDensity(writer);
            SerializeColorAndTextures(writer);
            SerializeSymbols(writer);
            SerializeEntities(writer);
            SerializeLockCodes(writer);
            SerializeSignals(writer);
            SerializeLogicCircuits(writer);
            SerializeShortcutNames(writer);
            
            // Write back the captured remaining data (blueprint parts, snap points, color palette, etc.)
            if (_remainingDataBytes.Length > 0)
            {
                writer.Write(_remainingDataBytes);
            }
        }

        public void Dispose()
        {
            BlockPool.Return(_blocks, clearArray: true);
        }

        private Vector3<int> ReadSize(BinaryReader reader, BlueprintHeader header)
        {
            // Read size or pull from header (new file version)
            if (header.Version <= 2)
                return reader.ReadIntVector3();

            if (!header.Size.HasValue)
                throw new FormatException("Header does not have a Size when Version specifies it should");

            return header.Size.Value;
        }

        private void ReadBlockData(BinaryReader reader)
        {
            if (_version < 6)
            {
                for (var i = 0; i < BlocksSize; i++)
                {
                    _blocks[i].Data = reader.ReadUInt32();
                }
            }
            else
            {
                PackedArray.ReadData(reader, BlocksSize, (i, br) =>
                {
                    _blocks[i].Data = br.ReadUInt32();
                });
            }
        }

        private void SerializeBlockData(BinaryWriter writer)
        {
            if (_version < 6)
            {
                for (var i = 0; i < BlocksSize; i++)
                {
                    writer.Write(_blocks[i].Data);
                }
            }
            else
            {
                PackedArray.SerializeData(writer, BlocksSize,
                    i => _blocks[i].Data != 0,
                    (i, w) => w.Write(_blocks[i].Data));
            }
        }

        private void ReadBlockDamage(BinaryReader reader)
        {
            if (_version <= 11)
                return;

            PackedArray.ReadData(reader, BlocksSize, (i, br) => _blocks[i].Damage = br.ReadUInt16());
        }

        private void SerializeBlockDamage(BinaryWriter writer)
        {
            if (_version <= 11)
                return;

            PackedArray.SerializeData(writer, BlocksSize,
                i => _blocks[i].Data != 0 && _blocks[i].Damage != 0,
                (i, w) => w.Write(_blocks[i].Damage));
        }

        private void ReadDensity(BinaryReader reader)
        {
            _singleDensityFlag = reader.ReadBoolean();
            if (_singleDensityFlag)
            {
                var value = reader.ReadByte();
                for (var i = 0; i < BlocksSize; i++)
                {
                    _blocks[i].Density = value;
                }
            }
            else
            {
                var bytes = reader.ReadBytes(BlocksSize);
                if (bytes.Length != BlocksSize)
                    throw new FormatException("Density array is not the correct length");

                for (var i = 0; i < BlocksSize; i++)
                {
                    _blocks[i].Density = bytes[i];
                }
            }
        }

        private void SerializeDensity(BinaryWriter writer)
        {
            writer.Write(_singleDensityFlag);
            if (_singleDensityFlag)
                writer.Write(_blocks.Length > 0 ? _blocks[0].Density : (byte)0);
            else
                for (var i = 0; i < BlocksSize; i++) // Use BlocksSize instead of _blocks.Length!
                {
                    writer.Write(_blocks[i].Density);
                }
        }

        private void ReadColorAndTextures(BinaryReader reader)
        {
            if (_version < 6)
                return;

            PackedArray.ReadData(reader, BlocksSize, (i, br) => _blocks[i].Color = br.ReadInt32());
            PackedArray.ReadData(reader, BlocksSize, (i, br) => _blocks[i].Texture = br.ReadInt64());

            if (_version < 20)
                return;

            PackedArray.ReadData(reader, BlocksSize, (i, br) => _blocks[i].TextureRotation = br.ReadByte());
        }

        private void SerializeColorAndTextures(BinaryWriter writer)
        {
            if (_version < 6)
                return;

            PackedArray.SerializeData(writer, BlocksSize,
                i => _blocks[i].Data != 0 && _blocks[i].Color != 0,
                (i, w) => w.Write(_blocks[i].Color));

            PackedArray.SerializeData(writer, BlocksSize,
                i => _blocks[i].Data != 0 && _blocks[i].Texture != 0,
                (i, w) => w.Write(_blocks[i].Texture));

            if (_version < 20)
                return;

            PackedArray.SerializeData(writer, BlocksSize,
                i => _blocks[i].Data != 0 && _blocks[i].TextureRotation != 0,
                (i, w) => w.Write(_blocks[i].TextureRotation));
        }

        private void ReadSymbols(BinaryReader reader)
        {
            if (_version < 7)
                return;

            PackedArray.ReadData(reader, BlocksSize, (i, br) => _blocks[i].Symbol = br.ReadInt32());
            PackedArray.ReadData(reader, BlocksSize,
                (i, br) => _blocks[i].SymbolRotation = _version >= 8 ? br.ReadInt32() : br.ReadInt16());
        }

        private void SerializeSymbols(BinaryWriter writer)
        {
            if (_version < 7)
                return;

            PackedArray.SerializeData(writer, BlocksSize,
                i => _blocks[i].Data != 0 && _blocks[i].Symbol != 0,
                (i, w) => w.Write(_blocks[i].Symbol));

            PackedArray.SerializeData(writer, BlocksSize,
                i => _blocks[i].Data != 0 && _blocks[i].SymbolRotation != 0,
                (i, w) => w.Write(_version >= 8 ? (int)_blocks[i].SymbolRotation : (short)_blocks[i].SymbolRotation));
        }

        private void ReadEntities(BinaryReader reader)
        {
            if (_version <= 10)
                return;

            var count = reader.ReadUInt16();
#if NETSTANDARD2_1
            _entities.EnsureCapacity(count);
#endif

            for (var i = 0; i < count; i++)
            {
                var location = reader.ReadIntVector3Packed();
                var tags = new NbtList(reader);

                _entities.Add(location, tags);
            }
        }

        private void SerializeEntities(BinaryWriter writer)
        {
            if (_version <= 10)
                return;

            writer.Write((ushort)_entities.Count);
            foreach (var entity in _entities.OrderBy(x => x.Key.X).ThenBy(x => x.Key.Y).ThenBy(x => x.Key.Z))
            {
                writer.WriteIntVector3Packed(entity.Key);
                entity.Value.Serialize(writer);
            }
        }

        private void ReadLockCodes(BinaryReader reader)
        {
            if (_version <= 11)
                return;

            var count = reader.ReadUInt16();
#if NETSTANDARD2_1
            _lockCodes.EnsureCapacity(count);
#endif

            for (var i = 0; i < count; i++)
            {
                var location = reader.ReadIntVector3Packed();
                var value = _version > 24 ? reader.ReadInt32() : reader.ReadInt16();

                _lockCodes.Add(location, value);
            }
        }

        private void SerializeLockCodes(BinaryWriter writer)
        {
            if (_version <= 11)
                return;

            writer.Write((ushort)_lockCodes.Count);
            foreach (var lockCode in _lockCodes.OrderBy(x => x.Key.X).ThenBy(x => x.Key.Y).ThenBy(x => x.Key.Z))
            {
                writer.WriteIntVector3Packed(lockCode.Key);
                if (_version > 24)
                    writer.Write(lockCode.Value);
                else
                    writer.Write((short)lockCode.Value);
            }
        }

        private void ReadSignals(BinaryReader reader)
        {
            if (_version < 14)
                return;

            var signalcount = reader.ReadUInt16();
#if NETSTANDARD2_1
            if (_signalSources.Capacity < signalcount)
                _signalSources.Capacity = signalcount;
#endif

            for (var i = 0; i < signalcount; i++)
            {
                if (_version >= 17)
                {
                    var nbt = new NbtList(reader);
                    _signalSources.Add(nbt);
                }
                else
                {
                    var location = reader.ReadIntVector3Packed();
                    var name = reader.ReadString();

                    // TODO: Convert this into an NBT collection some how?
                }
            }

            var receiverCount = reader.ReadUInt16();
#if NETSTANDARD2_1
            _signalReceivers.EnsureCapacity(receiverCount);
#endif
            for (var i = 0; i < receiverCount; i++)
            {
                var name = reader.ReadString();
                var count = reader.ReadUInt16();
                var list = new NbtList[count];

                for (var j = 0; j < count; j++)
                {
                    list[j] = new NbtList(reader);
                }

                _signalReceivers.Add(name, list);
            }
        }

        private void SerializeSignals(BinaryWriter writer)
        {
            if (_version < 14)
                return;

            writer.Write((ushort)_signalSources.Count);
            foreach (var signal in _signalSources)
            {
                signal.Serialize(writer);
            }

            writer.Write((ushort)_signalReceivers.Count);
            foreach (var receiver in _signalReceivers.OrderBy(x => x.Key))
            {
                writer.Write(receiver.Key);
                writer.Write((ushort)receiver.Value.Count);
                foreach (var nbt in receiver.Value)
                {
                    nbt.Serialize(writer);
                }
            }
        }

        private void ReadLogicCircuits(BinaryReader reader)
        {
            if (_version < 15)
                return;

            var count = reader.ReadUInt16();
#if NETSTANDARD2_1
            if (_circuits.Capacity < count)
                _circuits.Capacity = count;
#endif

            for (var i = 0; i < count; i++)
            {
                var nbt = new NbtList(reader);
                _circuits.Add(nbt);
            }
        }

        private void SerializeLogicCircuits(BinaryWriter writer)
        {
            if (_version < 15)
                return;

            writer.Write((ushort)_circuits.Count);
            foreach (var circuit in _circuits)
            {
                circuit.Serialize(writer);
            }
        }

        private void ReadShortcutNames(BinaryReader reader)
        {
            if (_version <= 15)
                return;

            var count = reader.ReadUInt16();
#if NETSTANDARD2_1
            if (_shortcutNames.Capacity < count)
                _shortcutNames.Capacity = count;
#endif

            for (var i = 0; i < count; i++)
            {
                var name = reader.ReadString();
                _shortcutNames.Add(name);
            }

            if (_version >= 17)
                return;

            // Older data, possibly signal names?
            var count2 = reader.ReadUInt16();
            for (var i = 0; i < count2; i++)
            {
                reader.ReadString();
            }
        }

        private void SerializeShortcutNames(BinaryWriter writer)
        {
            if (_version <= 15)
                return;

            writer.Write((ushort)_shortcutNames.Count);
            foreach (var name in _shortcutNames)
            {
                writer.Write(name);
            }
        }

        private class PackedArray
        {
            private readonly byte[] _data;

            private int _dataIndex = 0;
            private int _bitOffset = 8;

            private byte _entry;

            public bool ReadFlag
            {
                get
                {
                    if (_bitOffset > 7)
                    {
                        var index = _dataIndex++;
                        _entry = _data[index];
                        _bitOffset = 0;
                    }

                    return (_entry & 1 << _bitOffset++) != 0;
                }
            }

            public PackedArray(BinaryReader reader)
            {
                var length = reader.ReadInt32();
                _data = reader.ReadBytes(length);
            }

            public PackedArray(List<int> data, int totalCount)
            {
                int byteCount = (totalCount + 7) / 8;
                _data = new byte[byteCount];

                foreach (int index in data)
                {
                    int byteIndex = index / 8;
                    int bitIndex = index % 8;
                    _data[byteIndex] |= (byte)(1 << bitIndex);
                }
            }

            public void Serialize(BinaryWriter writer)
            {
                writer.Write(_data.Length);
                writer.Write(_data);
            }

            public static void ReadData(BinaryReader reader, int count, Action<int, BinaryReader> handler)
            {
                var flags = new PackedArray(reader);
                for (var i = 0; i < count; i++)
                {
                    if (!flags.ReadFlag)
                        continue;

                    handler(i, reader);
                }
            }

            public static void SerializeData(BinaryWriter writer, int count, Func<int, bool> hasData, Action<int, BinaryWriter> writeData)
            {
                var blocksWithData = new List<int>();
                for (int i = 0; i < count; i++)
                {
                    if (hasData(i))
                        blocksWithData.Add(i);
                }

                var packedArray = new PackedArray(blocksWithData, count);
                packedArray.Serialize(writer);

                foreach (int blockIndex in blocksWithData)
                {
                    writeData(blockIndex, writer);
                }
            }
        }
    }
}
