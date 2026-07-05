using System.Collections.Generic;
using Twinsanity;

namespace TwinsanityEditor
{
    public class TextureXController : ItemController
    {
        public new TextureX Data { get; set; }

        public TextureXController(MainForm topform, TextureX item) : base(topform, item)
        {
            Data = item;
            if (Data.RawData != null)
            {
                AddMenu("View texture", Menu_OpenViewer);
            }
        }

        protected override string GetName()
        {
            return string.Format("TextureX [ID {0:X8}]", Data.ID);
        }

        protected override void GenText()
        {
            List<string> text = new List<string>
            {
                string.Format("ID: {0:X8}", Data.ID),
                $"Size: {Data.Size}",
                $"Image Size: {Data.Width}x{Data.Height}",
                $"Mip levels: {Data.MipLevels}",
                //text.Add($"Texture format: {Data.PixelFormat}");
                //text.Add($"GS destination format: {Data.DestinationPixelFormat}");
                //text.Add($"Texture function: {Data.TexFun}");
                //text.Add($"Color component : {Data.ColorComponent}");
                $"Texture buffer width(in words): {Data.TextureBufferWidth}"
            };
            TextPrev = text.ToArray();
        }

        private void Menu_OpenViewer()
        {
            MainFile.OpenTextureViewer(this);
        }
    }
}
