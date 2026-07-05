using System.Collections.Generic;
using System.IO;

namespace Twinsanity.Items
{
    public class TwinsPSM
    {
        public List<TwinsPTC> PTCs = new List<TwinsPTC>();

        public void Load(BinaryReader reader, int size)
        {
            long startPos = reader.BaseStream.Position;
            while (reader.BaseStream.Position < startPos + size)
            {
                TwinsPTC ptc = new TwinsPTC();
                ptc.Load(reader, 0);
                PTCs.Add(ptc);
            }
        }

        public void Save(BinaryWriter writer)
        {
            foreach (TwinsPTC ptc in PTCs)
            {
                ptc.Save(writer);
            }
        }
    }
}
