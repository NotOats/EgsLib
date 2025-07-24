namespace EgsLib.Blueprints
{
    public struct Block
    {
        #region From BP File
        public uint Data { get; set; }

        public ushort Damage { get; set; }

        public byte Density { get; set; }

        public int Color { get; set; }

        public long Texture { get; set; }

        public byte TextureRotation { get; set; }

        public int Symbol { get; set; }

        public int SymbolRotation { get; set; }
        #endregion

        // Packed block data info sourced from https://github.com/ApanLoon/EmpyrionStuff
        public int BlockId => (int)(Data & 0x7FF);
        public int Rotation => (int)((Data >> 11) & 0x1F);
        public byte Variant => (byte)((Data >> 25) & 0x7F);

        /// <summary>
        /// Creates a new block with the specified ID, rotation, and variant
        /// </summary>
        public static Block Create(int blockId, int rotation = 0, byte variant = 0, byte density = 255)
        {
            return new Block
            {
                Data = (uint)(blockId | (rotation << 11) | (variant << 25)),
                Density = density
            };
        }

        /// <summary>
        /// Updates the block's ID, rotation, and variant while preserving other properties
        /// </summary>
        public void SetBlockData(int blockId, int rotation = 0, byte variant = 0)
        {
            Data = (uint)(blockId | (rotation << 11) | (variant << 25));
        }

        /// <summary>
        /// Checks if this block is empty (no block placed)
        /// </summary>
        public bool IsEmpty => Data == 0;

        /// <summary>
        /// Clears the block (makes it empty)
        /// </summary>
        public void Clear()
        {
            Data = 0;
            Damage = 0;
            Density = 0;
            Color = 0;
            Texture = 0;
            TextureRotation = 0;
            Symbol = 0;
            SymbolRotation = 0;
        }
    }
}
