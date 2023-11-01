namespace EgsLib.Blueprints
{
    public class Block
    {
        #region From BP File
        public uint Data { get; internal set; }

        public ushort Damage { get; internal set; }

        public byte Density { get; internal set; }

        public int Color {  get; internal set; }

        public long Texture { get; internal set; }

        public byte TextureRotation { get; internal set; }

        public int Symbol { get; internal set; }

        public int SymbolRotation { get; internal set; }
        #endregion

        // Packed block data info sourced from https://github.com/ApanLoon/EmpyrionStuff
        public int BlockId => (int)(Data & 0x7FF);
        public int Rotation => (int)((Data >> 11) & 0x1F);
        public byte Variant => (byte)((Data >> 25) & 0x7F);
    }
}
