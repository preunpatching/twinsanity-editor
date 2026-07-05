using System;
using System.Collections.Generic;
using System.IO;

namespace Twinsanity.VIF
{
    public class GIFTag
    {
        public ushort NLOOP { get; set; }
        public byte EOP { get; set; }
        public byte PRE { get; set; }
        public ushort PRIM { get; set; }
        public GIFModeEnum FLG { get; set; }
        public byte NREG { get; set; }
        public REGSEnum[] REGS { get; set; }
        public List<RegOutput> Data { get; set; }
        private ulong Q = 0x3F800000;
        public void Read(BinaryReader reader)
        {
            ulong low = reader.ReadUInt64();
            NLOOP = (ushort)((low & 0b111111111111111) >> 0);
            EOP = (byte)((low & ((ulong)0b1 << 15)) >> 15);
            PRE = (byte)((low & ((ulong)0b1 << 46)) >> 46);
            PRIM = (ushort)((low & ((ulong)0b11111111111 << 47)) >> 47);
            FLG = (GIFModeEnum)((low & (((ulong)0b11) << 58)) >> 58);
            NREG = (byte)((low & ((ulong)0b1111 << 60)) >> 60);
            NREG = (NREG == 0) ? (byte)16 : NREG;
            REGS = new REGSEnum[16];
            ulong high = reader.ReadUInt64();
            for (int i = 0; i < 16; ++i)
            {
                REGS[i] = (REGSEnum)(high & 0b1111);
                high >>= 4;
            }
            Data = new List<RegOutput>();
            if (PRE == 1)
            {
                RegOutput prim = new RegOutput
                {
                    REG = REGSEnum.PRIM,
                    Output = PRIM
                };
                Data.Add(prim);
            }
            switch (FLG)
            {
                case GIFModeEnum.IMAGE:
                    for (int i = 0; i < NLOOP; ++i)
                    {
                        Interpret(reader, REGSEnum.HWREG, Data);
                    }
                    break;
                case GIFModeEnum.REGLIST:
                    for (int i = 0; i < NLOOP; ++i)
                    {
                        for (int j = 0; j < NREG; ++j)
                        {
                            Interpret(reader, REGS[j], Data);
                        }
                    }
                    break;
                case GIFModeEnum.PACKED:
                    for (int i = 0; i < NLOOP; ++i)
                    {
                        for (int j = 0; j < NREG; ++j)
                        {
                            Interpret(reader, REGS[j], Data);
                        }
                    }
                    break;
            }

        }
        private void Interpret(BinaryReader reader, REGSEnum REG, List<RegOutput> list)
        {
            RegOutput output = new RegOutput();
            ulong low, high;
            switch (FLG)
            {
                case GIFModeEnum.PACKED:
                    high = reader.ReadUInt64();
                    low = reader.ReadUInt64();
                    output.REG = REG;
                    switch (REG)
                    {
                        case REGSEnum.RGBAQ:
                            ulong r = BitUtils.GetBits(low, 8, 0);
                            ulong g = BitUtils.GetBits(low, 8, 23);
                            ulong b = BitUtils.GetBits(low, 8, 46);
                            ulong a = BitUtils.GetBits(low, 8, 69);
                            output.Output = BitUtils.SetBits(BitUtils.SetBits(BitUtils.SetBits(BitUtils.SetBits(r, g, 8), b, 16), a, 24), Q, 32);
                            break;
                        case REGSEnum.ST:
                            ulong s = BitUtils.GetBits(low, 32, 0);
                            ulong t = BitUtils.GetBits(low, 32, 32);
                            ulong q = BitUtils.GetBits(high, 32, 0);
                            Q = q;
                            output.Output = BitUtils.SetBits(s, t, 32);
                            break;
                        case REGSEnum.UV:
                            ulong v = BitUtils.GetBits(low, 14, 0);
                            ulong u = BitUtils.GetBits(low, 14, 32);
                            output.Output = BitUtils.SetBits(v, u, 16);
                            break;
                        case REGSEnum.XYZF2:
                            {
                                ulong x = BitUtils.GetBits(low, 16, 0);
                                ulong y = BitUtils.GetBits(low, 16, 32);
                                ulong z = BitUtils.GetBits(high, 24, 4);
                                ulong f = BitUtils.GetBits(high, 8, 36);
                                ulong adc = BitUtils.GetBits(high, 1, 47);
                                output.REG = adc == 0 ? REGSEnum.XYZF2 : REGSEnum.XYZF3;
                                output.Output = BitUtils.SetBits(BitUtils.SetBits(BitUtils.SetBits(x, y, 16), z, 32), f, 56);
                            }
                            break;
                        case REGSEnum.XYZ2:
                            {
                                ulong x = BitUtils.GetBits(low, 16, 0);
                                ulong y = BitUtils.GetBits(low, 16, 32);
                                ulong z = BitUtils.GetBits(high, 32, 0);
                                ulong adc = BitUtils.GetBits(high, 1, 47);
                                output.REG = adc == 0 ? REGSEnum.XYZ2 : REGSEnum.XYZ3;
                                output.Output = BitUtils.SetBits(BitUtils.SetBits(x, y, 16), z, 32);
                            }

                            break;
                        case REGSEnum.FOG:
                            {
                                ulong f = BitUtils.GetBits(high, 8, 36);
                                output.Output = BitUtils.SetBits(0, f, 56);
                            }
                            break;
                        case REGSEnum.ApD:
                            ulong Data = high;
                            ulong Addr = BitUtils.GetBits(low, 7, 0);
                            output.Output = Data;
                            output.Address = Addr;
                            break;
                        case REGSEnum.TEX0_1:
                        case REGSEnum.TEX1_1:
                        case REGSEnum.CLAMP_1:
                        case REGSEnum.CLAMP_2:
                        case REGSEnum.XYZF3:
                        case REGSEnum.XYZ3:
                            output.Output = low;
                            break;
                        case REGSEnum.NOP:
                        default:
                            break;
                    }
                    list.Add(output);
                    break;
                case GIFModeEnum.REGLIST:
                    output.Output = reader.ReadUInt64();
                    output.REG = REG;
                    list.Add(output);
                    break;
                case GIFModeEnum.IMAGE:
                    RegOutput output1 = new RegOutput();
                    RegOutput output2 = new RegOutput();
                    high = reader.ReadUInt64();
                    output2.REG = REGSEnum.HWREG;
                    output2.Output = high;
                    low = reader.ReadUInt64();
                    output1.REG = REGSEnum.HWREG;
                    output1.Output = low;
                    list.Add(output1);
                    list.Add(output2);
                    break;
                case GIFModeEnum.DISABLE:
                    // Nothing
                    break;
            }
        }
        public void Write(BinaryWriter writer)
        {
            ulong low = 0;
            low |= ((ulong)NLOOP & 0b111111111111111) << 0;
            low |= ((ulong)EOP & 0b1) << 15;
            low |= ((ulong)PRE & 0b1) << 46;
            low |= ((ulong)PRIM & 0b11111111111) << 47;
            low |= ((ulong)FLG & 0b11) << 58;
            low |= ((ulong)NREG & 0b1111) << 60;
            writer.Write(low);
            ulong high = 0;
            for (int i = 0; i < 16; ++i)
            {
                high |= (ulong)REGS[REGS.Length - i - 1] & 0b1111;
                if (i != 15)
                {
                    high <<= 4;
                }
            }
            writer.Write(high);
            switch (FLG)
            {
                case GIFModeEnum.PACKED:
                    // Twinsanity textures only use A+D with PACKED, so we can safely ignore all the other writes
                    for (int i = 0; i < Data.Count; ++i)
                    {
                        switch (Data[i].REG)
                        {
                            case REGSEnum.ApD:
                                writer.Write(Data[i].Output);
                                writer.Write(Data[i].Address);
                                break;
                        }
                    }
                    break;
                case GIFModeEnum.REGLIST:
                    for (int i = 0; i < Data.Count; ++i)
                    {
                        writer.Write(Data[i].Output);
                    }
                    break;
                case GIFModeEnum.IMAGE:
                    for (int i = 0; i < Data.Count; i += 2)
                    {
                        writer.Write(Data[i + 1].Output);
                        writer.Write(Data[i].Output);
                    }
                    break;
                case GIFModeEnum.DISABLE:
                    // Nothing
                    break;
            }
        }
        public int GetLength()
        {
            switch (FLG)
            {
                case GIFModeEnum.PACKED:
                    return NREG * NLOOP; // QWORD
                case GIFModeEnum.REGLIST:
                    return NREG * NLOOP; // DWORD
                case GIFModeEnum.IMAGE:
                    return NLOOP; // QWORD
                case GIFModeEnum.DISABLE:
                    return 0;
            }
            return 0;
        }
    }
    public enum GIFModeEnum
    {
        PACKED = 0b00,
        REGLIST = 0b01,
        IMAGE = 0b10,
        DISABLE = 0b11
    }
    public enum REGSEnum
    {
        PRIM = 0x00,
        RGBAQ = 0x01,
        ST = 0x02,
        UV = 0x03,
        XYZF2 = 0x04,
        XYZ2 = 0x05,
        TEX0_1 = 0x06,
        TEX1_1 = 0x07,
        CLAMP_1 = 0x08,
        CLAMP_2 = 0x09,
        FOG = 0x0a,
        RESERVED = 0x0b,
        XYZF3 = 0x0c,
        XYZ3 = 0x0d,
        ApD = 0x0e,
        NOP = 0x0f,
        HWREG = 0xff
    }
    public class RegOutput
    {
        private ulong _Output;
        public ulong Output
        {
            get => _Output;
            set
            {
                _Output = value;
                switch (REG)
                {
                    case REGSEnum.RGBAQ:
                        R = (byte)BitUtils.GetBits(_Output, 8, 0);
                        G = (byte)BitUtils.GetBits(_Output, 8, 8);
                        B = (byte)BitUtils.GetBits(_Output, 8, 16);
                        A = (byte)BitUtils.GetBits(_Output, 8, 24);
                        Q = BitConverter.ToSingle(BitConverter.GetBytes((uint)BitUtils.GetBits(_Output, 32, 32)), 0);
                        break;
                    case REGSEnum.ST:
                        S = BitConverter.ToSingle(BitConverter.GetBytes((uint)BitUtils.GetBits(_Output, 32, 0)), 0);
                        T = BitConverter.ToSingle(BitConverter.GetBytes((uint)BitUtils.GetBits(_Output, 32, 32)), 0);
                        break;
                    case REGSEnum.UV:
                        ulong v = BitUtils.GetBits(_Output, 14, 0);
                        ulong v_int = BitUtils.GetBits(v, 10, 4);
                        ulong v_fract = BitUtils.GetBits(v, 4, 0);
                        ulong u = BitUtils.GetBits(_Output, 14, 16);
                        ulong u_int = BitUtils.GetBits(u, 10, 4);
                        ulong u_fract = BitUtils.GetBits(u, 4, 0);
                        U = BitUtils.FixedToSingle(u_int, u_fract, 4);
                        V = BitUtils.FixedToSingle(v_int, v_fract, 4);
                        break;
                    case REGSEnum.XYZF3:
                    case REGSEnum.XYZF2:
                        {
                            Z = (uint)BitUtils.GetBits(_Output, 24, 32);
                            ulong x = BitUtils.GetBits(_Output, 16, 0);
                            ulong x_int = BitUtils.GetBits(x, 12, 4);
                            ulong x_fract = BitUtils.GetBits(x, 4, 0);
                            ulong y = BitUtils.GetBits(_Output, 16, 16);
                            ulong y_int = BitUtils.GetBits(y, 12, 4);
                            ulong y_fract = BitUtils.GetBits(y, 4, 0);
                            X = BitUtils.FixedToSingle(x_int, x_fract, 4);
                            Y = BitUtils.FixedToSingle(y_int, y_fract, 4);
                            F = (byte)BitUtils.GetBits(_Output, 8, 56);
                        }
                        break;
                    case REGSEnum.XYZ3:
                    case REGSEnum.XYZ2:
                        {
                            Z = (uint)(_Output >> 32);
                            ulong x = BitUtils.GetBits(_Output, 16, 0);
                            ulong x_int = BitUtils.GetBits(x, 12, 4);
                            ulong x_fract = BitUtils.GetBits(x, 4, 0);
                            ulong y = BitUtils.GetBits(_Output, 16, 16);
                            ulong y_int = BitUtils.GetBits(y, 12, 4);
                            ulong y_fract = BitUtils.GetBits(y, 4, 0);
                            X = BitUtils.FixedToSingle(x_int, x_fract, 4);
                            Y = BitUtils.FixedToSingle(y_int, y_fract, 4);
                        }
                        break;
                    case REGSEnum.FOG:
                        {
                            F = (byte)(_Output >> 56);
                        }
                        break;
                }
            }
        }
        public REGSEnum REG { get; set; }
        public ulong Address { get; set; }
        public ushort PRIM { get; set; }
        public byte A { get; set; }
        public byte R { get; set; }
        public byte G { get; set; }
        public byte B { get; set; }
        public float Q { get; set; }
        public float T { get; set; }
        public float S { get; set; }
        public float V { get; set; }
        public float U { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
        public uint Z { get; set; }
        public byte F { get; set; }
    }
    public static class BitUtils
    {
        public static float[] Fracts;

        static BitUtils()
        {
            Fracts = new float[64];
            for (int i = 0; i < 64; ++i)
            {
                Fracts[i] = 1.0f / ((float)Math.Pow(2, i));
            }
        }
        public static ulong GetBits(ulong src, byte len, byte offset)
        {
            ulong mask = 0;
            for (int i = 0; i < len; ++i)
            {
                mask = (mask << 1) | 1;
            }
            return (src & ((ulong)0b11111111 << offset)) >> offset;
        }
        public static ulong SetBits(ulong src, ulong val, byte offset)
        {
            return src | (val << offset);
        }
        public static float FixedToSingle(ulong I, ulong F, byte fractLength)
        {
            float result = I;
            for (int i = 1; i <= fractLength; ++i)
            {
                float fract = Fracts[fractLength - i];
                if ((F & 0b1) != 0)
                {
                    result += fract;
                }
                F >>= 1;
            }
            return result;
        }
    }
}
