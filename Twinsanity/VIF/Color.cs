using System.IO;

namespace Twinsanity.VIF
{
    public class Color
    {
        public Color()
        {
            A = 255;
            R = 255;
            G = 255;
            B = 255;
        }
        public Color(byte R, byte G, byte B)
        {
            A = 255;
            this.R = R;
            this.G = G;
            this.B = B;
        }
        public Color(byte R, byte G, byte B, byte A)
        {
            this.A = A;
            this.R = R;
            this.G = G;
            this.B = B;
        }
        public byte A { get; set; }
        public byte R { get; set; }
        public byte G { get; set; }
        public byte B { get; set; }
        public int GetLength()
        {
            return 4;
        }

        public void Read(BinaryReader reader, int length)
        {
            R = reader.ReadByte();
            G = reader.ReadByte();
            B = reader.ReadByte();
            A = reader.ReadByte();
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write(R);
            writer.Write(G);
            writer.Write(B);
            writer.Write(A);
        }
        public Vector4 GetVector()
        {
            Vector4 vec = new Vector4
            {
                X = R / 255.0f,
                Y = G / 255.0f,
                Z = B / 255.0f,
                W = A / 255.0f
            };
            return vec;
        }

        public uint ToARGB()
        {
            return (uint)((A << 24) | (R << 16) | (G << 8) | B);
        }
        public void FromABGR(uint val)
        {
            A = (byte)((val >> 24) & 0xFF);
            B = (byte)((val >> 16) & 0xFF);
            G = (byte)((val >> 8) & 0xFF);
            R = (byte)((val >> 0) & 0xFF);
        }
        public void ScaleAlphaUp()
        {
            A = (byte)(A << 1);
            R = (byte)(R << 1);
            G = (byte)(G << 1);
            B = (byte)(B << 1);

        }
        public void ScaleAlphaDown()
        {
            A = (byte)(A >> 1);
            R = (byte)(R >> 1);
            G = (byte)(G >> 1);
            B = (byte)(B >> 1);
        }
        public uint ToABGR()
        {
            byte a = A;
            byte b = B;
            byte g = G;
            byte r = R;
            return (uint)((a << 24) | (b << 16) | (g << 8) | (r << 0));
        }

        public override string ToString()
        {
            return $"{A} {R} {G} {B}";
        }

        public override bool Equals(object obj)
        {
            if (obj is Color)
            {
                if (obj == this)
                {
                    return true;
                }
                else
                {
                    Color other = (Color)obj;
                    return other.ToARGB() == ToARGB();
                }
            }
            else
            {
                return false;
            }
        }
        public override int GetHashCode()
        {
            return (int)ToARGB();
        }
    }
}
