using System.IO;

namespace Twinsanity
{
    /// <summary>
    /// Represents a generic item in the chunk tree
    /// </summary>
    public class TwinsItem
    {
        protected virtual int GetSize()
        {
            return Data == null ? -1 : Data.Length;
        }

        public virtual void Save(BinaryWriter writer)
        {
            writer.Write(Data);
        }

        public virtual void Load(BinaryReader reader, int size)
        {
            Data = reader.ReadBytes(size);
        }

        public byte[] Data { get; set; }
        public uint ID { get; set; }
        public int Size => GetSize();
        public TwinsSection Parent { get; set; }
        public SectionType ParentType { get; set; }
    }
}
