using System.Collections.Generic;
using Twinsanity;

namespace TwinsanityEditor
{
    public class TextureController : ItemController
    {
        public new Texture Data { get; set; }

        public TextureController(MainForm topform, Texture item) : base(topform, item)
        {
            Data = item;
            if (Data.RawData != null)
            {
                AddMenu("Open viewer", Menu_OpenViewer);
            }
        }

        protected override string GetName()
        {
            return string.Format("Texture [ID {0:X8}]", Data.ID);
        }

        protected override void GenText()
        {
            List<string> text = new List<string>
            {
                string.Format("ID: {0:X8}", Data.ID),
                $"Size: {Data.Size}",
                $"Resolution: {Data.Width}x{Data.Height}",
                $"Mip levels: {Data.MipLevels}",
                $"Texture format: {Data.PixelFormat}",
                $"VRAM storage format: {Data.DestinationPixelFormat}",
                $"Texture function: {Data.TexFun}",
                $"Color component: {Data.ColorComponent}"
            };
            TextPrev = text.ToArray();
        }

        private void Menu_OpenViewer()
        {
            MainFile.OpenTextureViewer(this);
        }
    }
}
