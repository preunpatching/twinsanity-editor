using System.Collections.Generic;
using System.IO;

namespace Twinsanity
{
    public class Material : TwinsItem
    {
        public string Name { get; set; } = "Unnamed\0";
        public ulong Header { get; set; } = 2;
        public int Unknown { get; set; } = 2;
        public List<TwinsShader> Shaders = new List<TwinsShader>();

        public override void Save(BinaryWriter writer)
        {
            writer.Write(Header);
            writer.Write(Unknown);
            writer.Write(Name.Length);
            writer.Write(Name.ToCharArray());
            writer.Write(Shaders.Count);
            foreach (TwinsShader shd in Shaders)
            {
                shd.Write(writer);
            }
        }

        public override void Load(BinaryReader reader, int size)
        {
            bool isMB = false; //Parent.Parent.Type == SectionType.GraphicsMB;
            Header = reader.ReadUInt64();
            Unknown = reader.ReadInt32();
            int nameLen = reader.ReadInt32();
            Name = new string(reader.ReadChars(nameLen));
            int shdCnt = reader.ReadInt32();
            Shaders.Clear();
            for (int i = 0; i < shdCnt; ++i)
            {
                TwinsShader shd = new TwinsShader();
                shd.Read(reader, 0, false, isMB);
                Shaders.Add(shd);
            }
        }

        protected override int GetSize()
        {
            int shdLen = 0;
            foreach (TwinsShader shd in Shaders)
            {
                shdLen += shd.GetLength();
            }
            return 20 + Name.Length + shdLen;
        }

        internal void FillPackage(TwinsFile source, TwinsFile destination)
        {
            TwinsSection sourceTextures = source.GetItem<TwinsSection>(11).GetItem<TwinsSection>(0);
            TwinsSection destinationTextures = destination.GetItem<TwinsSection>(11).GetItem<TwinsSection>(0);
            foreach (TwinsShader shader in Shaders)
            {
                uint textureId = shader.TextureId;
                if (destinationTextures.HasItem(textureId))
                {
                    continue;
                }
                Texture linkedTexture = sourceTextures.GetItem<Texture>(textureId);
                destinationTextures.AddItem(textureId, linkedTexture);
            }
        }

        internal void FillPackageXbox(TwinsFile source, TwinsFile destination)
        {
            TwinsSection sourceTextures = source.GetItem<TwinsSection>(11).GetItem<TwinsSection>(0);
            TwinsSection destinationTextures = destination.GetItem<TwinsSection>(11).GetItem<TwinsSection>(0);
            foreach (TwinsShader shader in Shaders)
            {
                uint textureId = shader.TextureId;
                if (destinationTextures.HasItem(textureId))
                {
                    continue;
                }
                TextureX linkedTexture = sourceTextures.GetItem<TextureX>(textureId);
                destinationTextures.AddItem(textureId, linkedTexture);
            }
        }
    }
}
