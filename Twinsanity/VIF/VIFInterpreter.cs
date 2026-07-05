using System;
using System.Collections.Generic;
using System.IO;

namespace Twinsanity.VIF
{
    public class VIFInterpreter
    {
        public uint[] VIFn_R = { 0, 0, 0, 0 };
        public uint[] VIFn_C = { 0, 0, 0, 0 };
        public uint VIFn_CYCLE;
        public uint VIFn_MASK;
        public uint VIFn_MODE;
        public uint VIFn_ITOP;
        public uint VIFn_ITOPS;
        public uint VIF1_BASE;
        public uint VIF1_OFST;
        public uint VIF1_TOP;
        public uint VIF1_TOPS;
        public uint VIFn_MARK;
        public uint VIFn_NUM;
        public uint VIFn_CODE;

        private readonly List<List<Vector4>> VUMem = new List<List<Vector4>>();
        private readonly List<GIFTag> GifBuffer = new List<GIFTag>();
        private readonly List<uint> tmpStack = new List<uint>();
        private readonly List<List<ushort>> AddressOuput = new List<List<ushort>>();

        // Wrapper function for generating Interpreter instances
        public static VIFInterpreter InterpretCode(BinaryReader reader)
        {
            DMATag tag = new DMATag();
            tag.Read(reader);
            using (MemoryStream mem = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(mem))
            {
                // Transfer tag's extra data and its QWC data to VIF
                writer.Write(tag.Extra);
                writer.Write(reader.ReadBytes(tag.QWC * 0x10));
                mem.Position = 0;
                using (BinaryReader vifReader = new BinaryReader(mem))
                {
                    VIFInterpreter vifCode = new VIFInterpreter();
                    vifCode.Execute(vifReader);
                    return vifCode;
                }
            }
        }

        // Wrapper function for generating Interpreter instances using pure bytecode
        public static VIFInterpreter InterpretCode(byte[] code)
        {
            using (MemoryStream codeStr = new MemoryStream(code))
            {
                using (BinaryReader codeReader = new BinaryReader(codeStr))
                {
                    return InterpretCode(codeReader);
                }
            }

        }

        public List<List<Vector4>> GetMem()
        {
            return VUMem;
        }

        public List<List<ushort>> GetAddressOutput()
        {
            return AddressOuput;
        }

        public List<GIFTag> GetGifMem()
        {
            return GifBuffer;
        }

        private void Execute(BinaryReader reader)
        {
            while (reader.BaseStream.Position < reader.BaseStream.Length)
            {
                VIFCode vif = new VIFCode();
                vif.Read(reader);
                if (vif.isUnpack())
                {
                    byte cmd = (byte)vif.OP;
                    byte vn = (byte)((cmd & 0b1100) >> 2);
                    byte vl = (byte)((cmd & 0b0011) >> 0);
                    _ = (byte)((cmd & 0b10000) >> 4);
                    byte amount = vif.Amount;
                    ushort addr = (ushort)(vif.Immediate & 0b111111111);
                    byte usn = (byte)(vif.Immediate & 0b0100000000000000);
                    _ = (byte)(vif.Immediate & 0b1000000000000000);
                    byte WL = (byte)((VIFn_CYCLE >> 8) & 0xFF);
                    byte CL = (byte)((VIFn_CYCLE >> 0) & 0xFF);
                    uint dimensions = (uint)(vn + 1);
                    bool fill = WL > CL;
                    uint packet_length;
                    //Console.WriteLine($"Total cycle specifier {CL}");
                    //Console.WriteLine($"Write cycle specifier {WL}");
                    if (!fill)
                    {
                        uint a = (uint)(32 >> vl);
                        uint b = dimensions;
                        float c = a * b * amount;
                        float d = c / 32.0f;
                        float e = (float)Math.Ceiling(d);
                        uint f = (uint)e;
                        packet_length = 1 + f;
                    }
                    else
                    {
                        uint n = (uint)((CL * (amount / WL)) + ((amount % WL) > CL ? CL : (amount % WL)));
                        uint a = (uint)(32 >> vl);
                        uint b = dimensions;
                        float c = a * b * n;
                        float d = c / 32.0f;
                        float e = (float)Math.Ceiling(d);
                        uint f = (uint)e;
                        packet_length = 1 + f;
                    }
                    //Console.WriteLine($"VU memory address 0x{addr:x}");
                    if (AddressOuput.Count == 0)
                    {
                        AddressOuput.Add(new List<ushort>());
                    }
                    AddressOuput[AddressOuput.Count - 1].Add(addr);
                    PackFormat fmt = (PackFormat)(vl | (vn << 2));
                    List<Vector4> vectors = new List<Vector4>(new Vector4[1024]);
                    tmpStack.Clear();
                    for (int i = 0; i < packet_length - 1; ++i)
                    {
                        tmpStack.Add(reader.ReadUInt32());
                    }
                    Unpack(tmpStack, vectors, fmt, amount, usn, false, 1, 1, 0);
                    VUMem.Add(vectors);
                    //Console.WriteLine($"UNPACK {((int)packet_length - 1) * 4} bytes into {amount} 128bit vectors using {fmt} format");
                }
                else
                {
                    //Console.WriteLine(vif.OP.ToString());
                    switch (vif.OP)
                    {
                        case VIFCodeEnum.NOP:
                            // Skip, no operation
                            break;
                        case VIFCodeEnum.STCYCL:
                            VIFn_CYCLE = vif.Immediate;
                            break;
                        case VIFCodeEnum.OFFSET:
                            VIF1_OFST = (uint)(vif.Immediate & 0b1111111111);
                            break;
                        case VIFCodeEnum.BASE:
                            VIF1_BASE = (uint)(vif.Immediate & 0b1111111111);
                            break;
                        case VIFCodeEnum.ITOP:
                            VIFn_ITOPS = (uint)(vif.Immediate & 0b1111111111);
                            break;
                        case VIFCodeEnum.STMOD:
                            VIFn_MODE = (uint)(vif.Immediate & 0b11);
                            break;
                        case VIFCodeEnum.MSKPATH3:
                            //throw new NotImplementedException();
                            break;
                        case VIFCodeEnum.MARK:
                            VIFn_MARK = vif.Immediate;
                            break;
                        case VIFCodeEnum.FLUSHE:

                            break;
                        case VIFCodeEnum.FLUSH:

                            break;
                        case VIFCodeEnum.FLUSHA:

                            break;
                        case VIFCodeEnum.MSCAL:
                            //throw new NotImplementedException();
                            AddressOuput.Add(new List<ushort>());
                            break;
                        case VIFCodeEnum.MSCNT:
                            //throw new NotImplementedException();
                            break;
                        case VIFCodeEnum.MSCALF:
                            //throw new NotImplementedException();
                            break;
                        case VIFCodeEnum.STMASK:
                            VIFn_MASK = reader.ReadUInt32();
                            break;
                        case VIFCodeEnum.STROW:
                            VIFn_R[0] = reader.ReadUInt32();
                            VIFn_R[1] = reader.ReadUInt32();
                            VIFn_R[2] = reader.ReadUInt32();
                            VIFn_R[3] = reader.ReadUInt32();
                            break;
                        case VIFCodeEnum.STCOL:
                            VIFn_C[0] = reader.ReadUInt32();
                            VIFn_C[1] = reader.ReadUInt32();
                            VIFn_C[2] = reader.ReadUInt32();
                            VIFn_C[3] = reader.ReadUInt32();
                            break;
                        case VIFCodeEnum.MPG:
                            //throw new NotImplementedException();
                            break;
                        case VIFCodeEnum.DIRECT:
                            _ = (uint)((vif.Immediate == 0) ? 65536 * 16 : vif.Immediate * 16);
                            GifBuffer.Clear();
                            int len = 0;
                            bool flag;
                            do
                            {
                                GIFTag tag = new GIFTag();
                                tag.Read(reader);
                                GifBuffer.Add(tag);
                                flag = tag.EOP != 1;
                                int tagLen = tag.GetLength();
                                len += tagLen;
                            } while (flag);
                            break;
                        case VIFCodeEnum.DIRECTHL:

                            break;
                    }
                }
            }
        }

        private void SEXT(ref uint n)
        {
            n = ((n & 0x8000) != 0) ? n | 0xFFFF0000 : n;
        }

        private void SEXT8(ref uint n)
        {
            n = ((n & 0x80) != 0) ? n | 0xFFFFFF00 : n;
        }

        private void Unpack(List<uint> src, List<Vector4> dst, PackFormat fmt, byte amount, byte unsigned, bool fill, byte write, byte cycle, ushort addr)
        {
            int srcIdx = 0;
            switch (fmt)
            {
                case PackFormat.S_32:
                    for (int i = 0; i < amount; ++i)
                    {
                        Vector4 v = new Vector4();
                        v.SetBinaryX(src[i] + (IsInOffsetMode() ? VIFn_R[0] : 0));
                        v.SetBinaryY(src[i] + (IsInOffsetMode() ? VIFn_R[1] : 0));
                        v.SetBinaryZ(src[i] + (IsInOffsetMode() ? VIFn_R[2] : 0));
                        v.SetBinaryW(src[i] + (IsInOffsetMode() ? VIFn_R[3] : 0));

                        Fill(dst, v, i, fill, write, cycle, ref addr);
                    }
                    break;
                case PackFormat.S_16:
                    for (int i = 0; i < amount; ++i)
                    {
                        Vector4 v1 = new Vector4();
                        uint mask = (i % 2 == 0) ? 0x0000FFFF : 0xFFFF0000;
                        uint w1 = src[srcIdx] & mask;
                        if (i % 2 != 0)
                        {
                            w1 >>= 16;
                            srcIdx++;
                        }
                        if (unsigned == 0)
                        {
                            SEXT(ref w1);
                        }
                        v1.SetBinaryX(w1 + (IsInOffsetMode() ? VIFn_R[0] : 0));
                        v1.SetBinaryY(w1 + (IsInOffsetMode() ? VIFn_R[1] : 0));
                        v1.SetBinaryZ(w1 + (IsInOffsetMode() ? VIFn_R[2] : 0));
                        v1.SetBinaryW(w1 + (IsInOffsetMode() ? VIFn_R[3] : 0));
                        Fill(dst, v1, i, fill, write, cycle, ref addr);
                    }
                    break;
                case PackFormat.S_8:
                    for (int i = 0; i < amount; ++i)
                    {
                        Vector4 v1 = new Vector4();
                        uint mask = 0x000000FF;
                        int shift = 0;
                        switch (i % 4)
                        {
                            case 1:
                                mask = 0x0000FF00;
                                shift = 8;
                                break;
                            case 2:
                                mask = 0x00FF0000;
                                shift = 16;
                                break;
                            case 3:
                                mask = 0xFF000000;
                                shift = 24;
                                break;
                        }
                        uint w1 = (src[srcIdx] & mask) >> shift;
                        if (unsigned == 0)
                        {
                            SEXT8(ref w1);
                        }
                        v1.SetBinaryX(w1 + (IsInOffsetMode() ? VIFn_R[0] : 0));
                        v1.SetBinaryY(w1 + (IsInOffsetMode() ? VIFn_R[1] : 0));
                        v1.SetBinaryZ(w1 + (IsInOffsetMode() ? VIFn_R[2] : 0));
                        v1.SetBinaryW(w1 + (IsInOffsetMode() ? VIFn_R[3] : 0));
                        Fill(dst, v1, i, fill, write, cycle, ref addr);
                        if (i % 4 == 3)
                        {
                            srcIdx++;
                        }
                    }
                    break;
                case PackFormat.V2_32:
                    for (int i = 0; i < src.Count / 2; ++i)
                    {
                        Vector4 v = new Vector4();
                        v.SetBinaryX(src[(i * 2) + 0] + (IsInOffsetMode() ? VIFn_R[0] : 0));
                        v.SetBinaryY(src[(i * 2) + 1] + (IsInOffsetMode() ? VIFn_R[1] : 0));
                        Fill(dst, v, i, fill, write, cycle, ref addr);
                    }
                    break;
                case PackFormat.V2_16:
                    for (int i = 0; i < src.Count; ++i)
                    {
                        Vector4 v = new Vector4();
                        uint w1 = src[i] & 0x0000FFFF;
                        uint w2 = (src[i] & 0xFFFF0000) >> 16;
                        if (unsigned == 0)
                        {
                            SEXT(ref w1);
                            SEXT(ref w2);
                        }
                        v.SetBinaryX(w1 + (IsInOffsetMode() ? VIFn_R[0] : 0));
                        v.SetBinaryY(w2 + (IsInOffsetMode() ? VIFn_R[1] : 0));
                        Fill(dst, v, i, fill, write, cycle, ref addr);
                    }
                    break;
                case PackFormat.V2_8:
                    for (int i = 0; i < amount; ++i)
                    {
                        Vector4 v1 = new Vector4();
                        uint[] mask = { 0x000000FF, 0x0000FF00 };
                        int[] shift = { 0, 8 };
                        if (i % 2 != 0)
                        {
                            mask[0] = 0x00FF0000;
                            mask[1] = 0xFF000000;
                            shift[0] = 16;
                            shift[1] = 24;
                        }
                        uint w1 = (byte)((src[srcIdx] & mask[0]) >> shift[0]);
                        uint w2 = (byte)((src[srcIdx] & mask[1]) >> shift[1]);
                        if (unsigned == 0)
                        {
                            SEXT8(ref w1);
                            SEXT8(ref w2);
                        }

                        v1.SetBinaryX(w1 + (IsInOffsetMode() ? VIFn_R[0] : 0));
                        v1.SetBinaryY(w2 + (IsInOffsetMode() ? VIFn_R[1] : 0));
                        Fill(dst, v1, i, fill, write, cycle, ref addr);
                        if (i % 2 != 0)
                        {
                            srcIdx++;
                        }
                    }
                    break;
                case PackFormat.V3_32:
                    for (int i = 0; i < src.Count / 3; ++i)
                    {
                        Vector4 v = new Vector4();
                        v.SetBinaryX(src[(i * 3) + 0] + (IsInOffsetMode() ? VIFn_R[0] : 0));
                        v.SetBinaryY(src[(i * 3) + 1] + (IsInOffsetMode() ? VIFn_R[1] : 0));
                        v.SetBinaryZ(src[(i * 3) + 2] + (IsInOffsetMode() ? VIFn_R[2] : 0));
                        Fill(dst, v, i, fill, write, cycle, ref addr);
                    }
                    break;
                case PackFormat.V3_16:
                    for (int i = 0; i < amount; ++i)
                    {
                        uint[] mask = { 0x0000FFFF, 0xFFFF0000 };
                        int[] shift = { 0, 16 };
                        if (i % 2 != 0)
                        {
                            mask[0] = 0xFFFF0000;
                            mask[1] = 0x0000FFFF;
                            shift[0] = 16;
                            shift[1] = 0;
                        }
                        Vector4 v1 = new Vector4();
                        uint w1 = (src[srcIdx] & mask[0]) >> shift[0];
                        if (i % 2 != 0)
                        {
                            srcIdx++;
                        }
                        uint w2 = (src[srcIdx] & mask[1]) >> shift[1];
                        if (i % 2 == 0)
                        {
                            srcIdx++;
                        }
                        uint w3 = (src[srcIdx] & mask[0]) >> shift[0];
                        if (i % 2 != 0)
                        {
                            srcIdx++;
                        }
                        if (unsigned == 0)
                        {
                            SEXT(ref w1);
                            SEXT(ref w2);
                            SEXT(ref w3);
                        }
                        v1.SetBinaryX(w1 + (IsInOffsetMode() ? VIFn_R[0] : 0));
                        v1.SetBinaryY(w2 + (IsInOffsetMode() ? VIFn_R[1] : 0));
                        v1.SetBinaryZ(w3 + (IsInOffsetMode() ? VIFn_R[2] : 0));
                        Fill(dst, v1, i, fill, write, cycle, ref addr);
                    }
                    break;
                case PackFormat.V3_8:
                    for (int i = 0; i < amount; ++i)
                    {
                        Vector4 v1 = new Vector4();
                        // Is this scuffed? This is 100% scuffed
                        uint[] mask = { 0x000000FF, 0x0000FF00, 0x00FF0000 };
                        int[] shift = { 0, 8, 16 };
                        bool[] incIdx = { false, false, false };
                        switch (i % 4)
                        {
                            case 1:
                                mask[0] = 0xFF000000;
                                mask[1] = 0x000000FF;
                                mask[2] = 0x0000FF00;
                                shift[0] = 24;
                                shift[1] = 0;
                                shift[2] = 8;
                                incIdx[0] = true;
                                break;
                            case 2:
                                mask[0] = 0x00FF0000;
                                mask[1] = 0xFF000000;
                                mask[2] = 0x000000FF;
                                shift[0] = 16;
                                shift[1] = 24;
                                shift[2] = 0;
                                incIdx[1] = true;
                                break;
                            case 3:
                                mask[0] = 0x0000FF00;
                                mask[1] = 0x00FF0000;
                                mask[2] = 0xFF000000;
                                shift[0] = 8;
                                shift[1] = 16;
                                shift[2] = 24;
                                incIdx[2] = true;
                                break;
                        }
                        uint w1 = (byte)((src[srcIdx] & mask[0]) >> shift[0]);
                        if (incIdx[0])
                        {
                            srcIdx++;
                        }

                        uint w2 = (byte)((src[srcIdx] & mask[1]) >> shift[1]);
                        if (incIdx[1])
                        {
                            srcIdx++;
                        }

                        uint w3 = (byte)((src[srcIdx] & mask[2]) >> shift[2]);
                        if (incIdx[2])
                        {
                            srcIdx++;
                        }

                        if (unsigned == 0)
                        {
                            SEXT8(ref w1);
                            SEXT8(ref w2);
                            SEXT8(ref w3);
                        }
                        v1.SetBinaryX(w1 + (IsInOffsetMode() ? VIFn_R[0] : 0));
                        v1.SetBinaryY(w2 + (IsInOffsetMode() ? VIFn_R[1] : 0));
                        v1.SetBinaryZ(w3 + (IsInOffsetMode() ? VIFn_R[2] : 0));
                        Fill(dst, v1, i, fill, write, cycle, ref addr);
                    }
                    break;
                case PackFormat.V4_32:
                    for (int i = 0; i < src.Count / 4; ++i)
                    {
                        Vector4 v = new Vector4();
                        v.SetBinaryX(src[(i * 4) + 0] + (IsInOffsetMode() ? VIFn_R[0] : 0));
                        v.SetBinaryY(src[(i * 4) + 1] + (IsInOffsetMode() ? VIFn_R[1] : 0));
                        v.SetBinaryZ(src[(i * 4) + 2] + (IsInOffsetMode() ? VIFn_R[2] : 0));
                        v.SetBinaryW(src[(i * 4) + 3] + (IsInOffsetMode() ? VIFn_R[3] : 0));
                        Fill(dst, v, i, fill, write, cycle, ref addr);
                    }
                    break;
                case PackFormat.V4_16:
                    for (int i = 0; i < src.Count / 2; ++i)
                    {
                        Vector4 v = new Vector4();
                        uint w1 = src[i * 2] & 0x0000FFFF;
                        uint w2 = (src[i * 2] & 0xFFFF0000) >> 16;
                        uint w3 = src[(i * 2) + 1] & 0x0000FFFF;
                        uint w4 = (src[(i * 2) + 1] & 0xFFFF0000) >> 16;
                        if (unsigned == 0)
                        {
                            SEXT(ref w1);
                            SEXT(ref w2);
                            SEXT(ref w3);
                            SEXT(ref w4);
                        }
                        v.SetBinaryX(w1 + (IsInOffsetMode() ? VIFn_R[0] : 0));
                        v.SetBinaryY(w2 + (IsInOffsetMode() ? VIFn_R[1] : 0));
                        v.SetBinaryZ(w3 + (IsInOffsetMode() ? VIFn_R[2] : 0));
                        v.SetBinaryW(w4 + (IsInOffsetMode() ? VIFn_R[3] : 0));
                        Fill(dst, v, i, fill, write, cycle, ref addr);
                    }
                    break;
                case PackFormat.V4_8:
                    for (int i = 0; i < src.Count; ++i)
                    {
                        Vector4 v = new Vector4();
                        uint w1 = (byte)(src[i] & 0x000000FF);
                        uint w2 = (byte)((src[i] & 0x0000FF00) >> 8);
                        uint w3 = (byte)((src[i] & 0x00FF0000) >> 16);
                        uint w4 = (byte)((src[i] & 0xFF000000) >> 24);
                        if (unsigned == 0)
                        {
                            SEXT8(ref w1);
                            SEXT8(ref w2);
                            SEXT8(ref w3);
                            SEXT8(ref w4);
                        }
                        v.SetBinaryX(w1 + (IsInOffsetMode() ? VIFn_R[0] : 0));
                        v.SetBinaryY(w2 + (IsInOffsetMode() ? VIFn_R[1] : 0));
                        v.SetBinaryZ(w3 + (IsInOffsetMode() ? VIFn_R[2] : 0));
                        v.SetBinaryW(w4 + (IsInOffsetMode() ? VIFn_R[3] : 0));
                        Fill(dst, v, i, fill, write, cycle, ref addr);
                    }
                    break;
                case PackFormat.V4_5:
                    for (int i = 0; i < amount; ++i)
                    {
                        uint mask = (i % 2 == 0) ? 0x0000FFFF : 0xFFFF0000;
                        uint rgba1 = src[srcIdx] & mask;
                        if (i % 2 != 0)
                        {
                            rgba1 >>= 16;
                            srcIdx++;
                        }
                        Color c1 = new Color
                        {
                            R = (byte)(rgba1 & (0b11111 << 3)),
                            G = (byte)((rgba1 & (0b11111 << 5)) >> 5 << 3),
                            B = (byte)((rgba1 & (0b11111 << 10)) >> 10 << 3),
                            A = (byte)((rgba1 & (0b1 << 15)) >> 15 << 7)
                        };
                        Fill(dst, c1.GetVector(), i, fill, write, cycle, ref addr);
                    }
                    break;
            }
        }

        private void Pack(List<Vector4> src, List<uint> dst, PackFormat fmt)
        {
            uint resUInt = 0;
            switch (fmt)
            {
                case PackFormat.S_32:
                    for (int i = 0; i < src.Count; ++i)
                    {
                        dst.Add(src[i].GetBinaryX());
                    }
                    break;
                case PackFormat.S_16:
                    for (int i = 0; i < src.Count; ++i)
                    {
                        uint vecPart = src[i].GetBinaryX() & 0xFFFF;
                        if (i % 2 != 0)
                        {
                            resUInt |= vecPart << 16;
                            dst.Add(resUInt);
                            resUInt = 0; // Reset bits
                        }
                        else
                        {
                            resUInt |= vecPart;
                            // Add last vector with padding 0 bits
                            if (i == src.Count - 1)
                            {
                                dst.Add(resUInt);
                            }
                        }
                    }
                    break;
                case PackFormat.S_8:
                    for (int i = 0; i < src.Count; ++i)
                    {
                        uint vecPart = src[i].GetBinaryX() & 0xFF;
                        switch (i % 4)
                        {
                            case 0:
                                resUInt |= vecPart;
                                break;
                            case 1:
                                resUInt |= vecPart << 8;
                                break;
                            case 2:
                                resUInt |= vecPart << 16;
                                break;
                            case 3:
                                resUInt |= vecPart << 24;
                                dst.Add(resUInt);
                                resUInt = 0; // Reset bits
                                break;
                        }
                        // Add last vector with padding 0 bits
                        if (i == src.Count - 1 && i % 4 != 3)
                        {
                            dst.Add(resUInt);
                        }
                    }
                    break;
                case PackFormat.V2_32:
                    for (int i = 0; i < src.Count; ++i)
                    {
                        dst.Add(src[i].GetBinaryX());
                        dst.Add(src[i].GetBinaryY());
                    }
                    break;
                case PackFormat.V2_16:
                    for (int i = 0; i < src.Count; ++i)
                    {
                        resUInt |= src[i].GetBinaryX() & 0xFFFF;
                        resUInt |= (src[i].GetBinaryY() & 0xFFFF) << 16;
                        dst.Add(resUInt);
                        resUInt = 0; // Reset bits
                    }
                    break;
                case PackFormat.V2_8:
                    for (int i = 0; i < src.Count; ++i)
                    {
                        uint vecX = src[i].GetBinaryX() & 0xFF;
                        uint vecY = src[i].GetBinaryY() & 0xFF;
                        if (i % 2 != 0)
                        {
                            resUInt |= vecX << 16;
                            resUInt |= vecY << 24;
                            dst.Add(resUInt);
                            resUInt = 0; // Reset bits
                        }
                        else
                        {
                            resUInt |= vecX;
                            resUInt |= vecY << 8;
                            // Add last vector with padding 0 bits
                            if (i == src.Count - 1)
                            {
                                dst.Add(resUInt);
                            }
                        }
                    }
                    break;
                case PackFormat.V3_32:
                    for (int i = 0; i < src.Count; ++i)
                    {
                        dst.Add(src[i].GetBinaryX());
                        dst.Add(src[i].GetBinaryY());
                        dst.Add(src[i].GetBinaryZ());
                    }
                    break;
                case PackFormat.V3_16:
                    for (int i = 0; i < src.Count; ++i)
                    {
                        uint vecX = src[i].GetBinaryX() & 0xFFFF;
                        uint vecY = src[i].GetBinaryY() & 0xFFFF;
                        uint vecZ = src[i].GetBinaryZ() & 0xFFFF;
                        if (i % 2 != 0)
                        {
                            resUInt |= vecX << 16;
                            dst.Add(resUInt);
                            resUInt = 0; // Reset bits
                            resUInt |= vecY;
                            resUInt |= vecZ << 16;
                            dst.Add(resUInt);
                            resUInt = 0; // Reset bits
                        }
                        else
                        {
                            resUInt |= vecX;
                            resUInt |= vecY << 16;
                            dst.Add(resUInt);
                            resUInt = 0; // Reset bits
                            resUInt |= vecZ;
                            // Add last vector with padding 0 bits
                            if (i == src.Count - 1)
                            {
                                dst.Add(resUInt);
                            }
                        }
                    }
                    break;
                case PackFormat.V3_8:
                    for (int i = 0; i < src.Count; ++i)
                    {
                        uint vecX = src[i].GetBinaryX() & 0xFF;
                        uint vecY = src[i].GetBinaryY() & 0xFF;
                        uint vecZ = src[i].GetBinaryZ() & 0xFF;
                        switch (i % 4)
                        {
                            case 0:
                                resUInt |= vecX;
                                resUInt |= vecY << 8;
                                resUInt |= vecZ << 16;
                                break;
                            case 1:
                                resUInt |= vecX << 24;
                                dst.Add(resUInt);
                                resUInt = 0; // Reset bits
                                resUInt |= vecY;
                                resUInt |= vecZ << 8;
                                break;
                            case 2:
                                resUInt |= vecX << 16;
                                resUInt |= vecY << 24;
                                dst.Add(resUInt);
                                resUInt = 0; // Reset bits
                                resUInt |= vecZ;
                                break;
                            case 3:
                                resUInt |= vecX << 8;
                                resUInt |= vecY << 16;
                                resUInt |= vecZ << 24;
                                dst.Add(resUInt);
                                resUInt = 0; // Reset bits
                                break;
                        }
                        // Add last vector with padding 0 bits
                        if (i == src.Count - 1 && i % 4 != 3)
                        {
                            dst.Add(resUInt);
                        }
                    }
                    break;
                case PackFormat.V4_32:
                    for (int i = 0; i < src.Count; ++i)
                    {
                        dst.Add(src[i].GetBinaryX());
                        dst.Add(src[i].GetBinaryY());
                        dst.Add(src[i].GetBinaryZ());
                        dst.Add(src[i].GetBinaryW());
                    }
                    break;
                case PackFormat.V4_16:
                    for (int i = 0; i < src.Count; ++i)
                    {
                        uint vecX = src[i].GetBinaryX() & 0xFFFF;
                        uint vecY = src[i].GetBinaryY() & 0xFFFF;
                        uint vecZ = src[i].GetBinaryZ() & 0xFFFF;
                        uint vecW = src[i].GetBinaryW() & 0xFFFF;
                        resUInt |= vecX;
                        resUInt |= vecY << 16;
                        dst.Add(resUInt);
                        resUInt = 0; // Reset bits
                        resUInt |= vecZ;
                        resUInt |= vecW << 16;
                        dst.Add(resUInt);
                        resUInt = 0; // Reset bits
                    }
                    break;
                case PackFormat.V4_8:
                    for (int i = 0; i < src.Count; ++i)
                    {
                        uint vecX = src[i].GetBinaryX() & 0xFF;
                        uint vecY = src[i].GetBinaryY() & 0xFF;
                        uint vecZ = src[i].GetBinaryZ() & 0xFF;
                        uint vecW = src[i].GetBinaryW() & 0xFF;
                        resUInt |= vecX;
                        resUInt |= vecY << 8;
                        resUInt |= vecZ << 16;
                        resUInt |= vecW << 24;
                        dst.Add(resUInt);
                        resUInt = 0; // Reset bits
                    }
                    break;
                case PackFormat.V4_5:
                    for (int i = 0; i < src.Count; ++i)
                    {
                        Color c = src[i].GetColor();
                        uint r = ((uint)c.R >> 3) & 0b11111;
                        uint g = ((uint)c.G >> 3) & 0b11111;
                        uint b = ((uint)c.B >> 3) & 0b11111;
                        uint a = ((uint)c.A >> 7) & 0b1;
                        if (i % 2 != 0)
                        {
                            resUInt |= r << 16;
                            resUInt |= g << 21;
                            resUInt |= b << 26;
                            resUInt |= a << 31;
                            dst.Add(resUInt);
                            resUInt = 0; // Reset bits
                        }
                        else
                        {
                            resUInt |= r;
                            resUInt |= g << 5;
                            resUInt |= b << 10;
                            resUInt |= a << 15;
                            // Add last vector with padding 0 bits
                            if (i == src.Count - 1)
                            {
                                dst.Add(resUInt);
                            }
                        }
                    }
                    break;
            }
        }

        private void Fill(List<Vector4> dst, Vector4 vec, int index, bool fill, byte wl, byte cl, ref ushort addr)
        {
            // Fill writing
            if (fill)
            {
                bool doFill = (index + 1) % cl == 0;
                dst[addr++] = vec;
                if (doFill)
                {
                    Vector4 fillVec = new Vector4();
                    fillVec.SetBinaryX(VIFn_R[0]);
                    fillVec.SetBinaryY(VIFn_R[1]);
                    fillVec.SetBinaryZ(VIFn_R[2]);
                    fillVec.SetBinaryW(VIFn_R[3]);
                    for (int i = 0; i < wl - cl; i++)
                    {
                        dst[addr++] = fillVec;
                    }
                }
                return;
            }
            // Skip writing
            _ = new Vector4();
            int skipAmt = cl - wl;
            dst[addr++] = vec;
            if (wl != 0)
            {
                bool doSkip = (index + 1) % wl == 0;
                if (doSkip)
                {
                    addr += (ushort)skipAmt;
                }
            }
        }

        private bool IsInOffsetMode()
        {
            return (VIFn_MODE & 0b01) == 1;
        }
    }
    public enum PackFormat
    {
        S_32 = 0b0000,
        S_16 = 0b0001,
        S_8 = 0b0010,
        V2_32 = 0b0100,
        V2_16 = 0b0101,
        V2_8 = 0b0110,
        V3_32 = 0b1000,
        V3_16 = 0b1001,
        V3_8 = 0b1010,
        V4_32 = 0b1100,
        V4_16 = 0b1101,
        V4_8 = 0b1110,
        V4_5 = 0b1111,
    }
}
