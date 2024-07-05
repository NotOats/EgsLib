using EgsLib.Blueprints.NbtTags;
using EgsLib.Extensions;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;

namespace EgsLib.Blueprints
{
    public class BlueprintBlockData : IDisposable
    {
        private static readonly ArrayPool<Block> BlockPool = ArrayPool<Block>.Create();

        private readonly int _version;

        private readonly Block[] _blocks;
        private readonly Dictionary<Vector3<int>, NbtList> _entities = new Dictionary<Vector3<int>, NbtList>();
        private readonly Dictionary<Vector3<int>, int> _lockCodes = new Dictionary<Vector3<int>, int>();
        private readonly List<NbtList> _signalSources = new List<NbtList>();
        private readonly Dictionary<string, IReadOnlyList<NbtList>> _signalReceivers = new Dictionary<string, IReadOnlyList<NbtList>>();
        private readonly List<NbtList> _circuits = new List<NbtList>();
        private readonly List<string> _shortcutNames = new List<string>();

        public Vector3<int> Size { get; }

        public IReadOnlyList<Block> Blocks => _blocks;
        public int BlocksSize => checked(Size.X * Size.Y * Size.Z);

        public IReadOnlyDictionary<Vector3<int>, NbtList> Entities => _entities;
        public IReadOnlyDictionary<Vector3<int>, int> LockCodes => _lockCodes;
        public IReadOnlyList<NbtList> SignalSources => _signalSources;
        public IReadOnlyDictionary<string, IReadOnlyList<NbtList>> SignalReceivers => _signalReceivers;
        public IReadOnlyList<NbtList> Circuits => _circuits;
        public IReadOnlyList<string> ShortcutNames => _shortcutNames;

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

        private void ReadBlockDamage(BinaryReader reader)
        {
            if (_version <= 11)
                return;

            PackedArray.ReadData(reader, BlocksSize, (i, br) => _blocks[i].Damage = br.ReadUInt16());
        }

        private void ReadDensity(BinaryReader reader)
        {
            if (reader.ReadBoolean())
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

                for(var i = 0; i < BlocksSize; i++)
                {
                    _blocks[i].Density = bytes[i];
                }
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

        private void ReadSymbols(BinaryReader reader)
        {
            if (_version < 7)
                return;

            PackedArray.ReadData(reader, BlocksSize, (i, br) => _blocks[i].Symbol = br.ReadInt32());
            PackedArray.ReadData(reader, BlocksSize, 
                (i, br) => _blocks[i].SymbolRotation = _version >= 8 ? br.ReadInt32() : br.ReadInt16());
        }

        private void ReadEntities(BinaryReader reader)
        {
            if (_version <= 10)
                return;

            var count = reader.ReadUInt16();
#if NETSTANDARD2_1
            _entities.EnsureCapacity(count);
#endif

            for(var i = 0; i < count; i++)
            {
                var location = reader.ReadIntVector3Packed();
                var tags = new NbtList(reader);

                _entities.Add(location, tags);
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
                if(_version >= 17)
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

                for(var j = 0; j < count; j++)
                {
                    list[j] = new NbtList(reader);
                }

                _signalReceivers.Add(name, list);
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
        }
    }
}
