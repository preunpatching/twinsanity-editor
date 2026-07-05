using System.Collections.Generic;
using Twinsanity;

namespace TwinsanityEditor
{
    public class DynamicSceneryDataMBController : ItemController
    {
        public new DynamicSceneryDataMB Data { get; set; }

        public DynamicSceneryDataMBController(MainForm topform, DynamicSceneryDataMB item) : base(topform, item)
        {
            Data = item;
        }

        protected override string GetName()
        {
            return $"Dynamic Scenery Data [ID {Data.ID}]";
        }

        protected override void GenText()
        {
            List<string> text = new List<string>
            {
                $"ID: {Data.ID}",
                $"Size: {Data.Size}",
                $"Model Count: {Data.Models.Count}"
            };

            TextPrev = text.ToArray();
        }
    }
}