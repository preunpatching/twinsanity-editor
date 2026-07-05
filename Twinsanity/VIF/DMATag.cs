using System.IO;

namespace Twinsanity.VIF
{
    public class DMATag
    {
        public ushort QWC;
        public byte PCE;
        public byte ID;
        public byte IRQ;
        public uint ADDR;
        public byte SPR;
        public ulong Extra;
        public void Read(BinaryReader reader)
        {
            ulong low = reader.ReadUInt64();
            QWC = (ushort)(low & 0xFFFF);
            PCE = (byte)((low >> 26) & 0b11);
            ID = (byte)((low >> 28) & 0b111);
            IRQ = (byte)((low >> 31) & 0b1);
            ADDR = (uint)((low >> 32) & 0x7FFFFFFF);
            SPR = (byte)((low >> 63) & 0b1);
            Extra = reader.ReadUInt64();
        }

        public void Write(BinaryWriter writer)
        {
            ulong low = QWC;
            low |= (ulong)PCE << 26;
            low |= (ulong)ID << 28;
            low |= (ulong)IRQ << 31;
            low |= (ulong)ADDR << 32;
            low |= (ulong)SPR << 63;
            writer.Write(low);
            writer.Write(Extra);
        }
    }
}
