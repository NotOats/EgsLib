using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace EgsLib.Blueprints.NbtTags
{
    public class NbtList : INbtTag, IReadOnlyList<INbtTag>
    {
        public string Name => "Root";

        public IReadOnlyList<INbtTag> Value { get; }

        object INbtTag.Value => Value;

        public int Count => Value.Count;

        public INbtTag this[int index] => Value[index];

        public NbtList(BinaryReader reader)
        {
            reader.ReadByte(); // Unknown/garbage

            var count = reader.ReadUInt16();
            var tags = new INbtTag[count];

            for (var i = 0; i < count; i++)
            {
                var type = (NbtType)reader.ReadByte();
                var name = reader.ReadString();
                INbtTag tag = null;

                switch (type)
                {
                    case NbtType.Int32:
                        tag = new NbtInt32(name, reader.ReadInt32()); break;
                    case NbtType.String:
                        tag = new NbtString(name, reader.ReadString()); break;
                    case NbtType.Bool:
                        tag = new NbtBool(name, reader.ReadBoolean()); break;
                    case NbtType.Single:
                        tag = new NbtSingle(name, reader.ReadSingle()); break;
                    case NbtType.Color:
                        tag = new NbtColor(name, 
                            reader.ReadByte(), reader.ReadByte(), 
                            reader.ReadByte(), reader.ReadByte());
                        break;
                }

                tags[i] = tag ?? throw new NotSupportedException("NbtTag type not supported");
            }

            Value = tags;
        }

        public IEnumerator<INbtTag> GetEnumerator() => Value.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => Value.GetEnumerator();

        private enum NbtType : byte
        {
            Int32 = 0,
            String = 1,
            Bool = 2,
            Single = 3,

            Color = 5
        }

    }
}
