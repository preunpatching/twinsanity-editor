using System.Collections.Generic;
using System.IO;

namespace Twinsanity.Items
{
    public class TwinsPSF
    {
        public List<TwinsPTC> FontPages = new List<TwinsPTC>();
        public List<TwinsVector4> Vectors = new List<TwinsVector4>();
        public int UnkInt;

        public void Load(BinaryReader reader, int size)
        {
            int pages = reader.ReadInt32();
            for (int i = 0; i < pages; ++i)
            {
                TwinsPTC page = new TwinsPTC();
                page.Load(reader, 0);
                FontPages.Add(page);
            }
            int vecAmt = reader.ReadInt32();
            UnkInt = reader.ReadInt32();
            for (int i = 0; i < vecAmt; ++i)
            {
                TwinsVector4 vec = new TwinsVector4();
                vec.Load(reader, 16);
                Vectors.Add(vec);
            }
        }

        public void Save(BinaryWriter writer)
        {
            writer.Write(FontPages.Count);
            foreach (TwinsPTC page in FontPages)
            {
                page.Save(writer);
            }
            writer.Write(Vectors.Count);
            writer.Write(UnkInt);
            foreach (TwinsVector4 v in Vectors)
            {
                v.Save(writer);
            }
        }
    }
}
