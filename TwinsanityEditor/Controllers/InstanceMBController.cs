using System;
using Twinsanity;

namespace TwinsanityEditor
{
    public class InstanceMBController : ItemController
    {
        public new InstanceMB Data { get; set; }

        public InstanceMBController(MainForm topform, InstanceMB item) : base(topform, item)
        {
            Data = item;
            AddMenu("Open editor", Menu_OpenEditor);
        }

        protected override string GetName()
        {
            string obj_name = string.Empty; //MainFile.GetObjectName(Data.ObjectID);
            //obj_name = Utils.TextUtils.TruncateObjectName(obj_name, Data.ObjectID, "*", "");
            if (Enum.IsDefined(typeof(DefaultEnums.ObjectID_MB), Data.ObjectID))
            {
                obj_name = (DefaultEnums.ObjectID_MB)Data.ObjectID + "";
            }

            return obj_name != string.Empty ? $"{obj_name} Instance [ID {Data.ID}]" : $"Instance [ID {Data.ID}]";
        }

        protected override void GenText()
        {
            //obj_name = Utils.TextUtils.TruncateObjectName(obj_name, Data.ObjectID, "", " (Not in Objects)");
            string obj_name = Enum.IsDefined(typeof(DefaultEnums.ObjectID_MB), Data.ObjectID) ? (DefaultEnums.ObjectID_MB)Data.ObjectID + "" : "Unknown";

            TextPrev = new string[6];//12 + Data.InstanceIDs.Count + Data.PositionIDs.Count + Data.PathIDs.Count + Data.UnkI321.Count + Data.UnkI322.Count + Data.UnkI323.Count];
            TextPrev[0] = $"ID: {Data.ID}";
            TextPrev[1] = $"Size: {Data.Size}";
            TextPrev[2] = $"Object ID {Data.ObjectID} - {(obj_name != string.Empty ? obj_name : string.Empty)}";
            TextPrev[3] = Data.ScriptID == -1 ? $"Script ID None" : $"Script ID {Data.ScriptID}";
            TextPrev[4] = $"Position ({Data.Pos.X}, {Data.Pos.Y}, {Data.Pos.Z}, {Data.Pos.W})";
            TextPrev[5] = $"Rotation ({Data.RotX} | {Data.RotY} | {Data.RotZ})";

            //TextPrev[5] = $"Instances: {Data.InstanceIDs.Count}";
            //for (int i = 0; i < Data.InstanceIDs.Count; ++i)
            //    TextPrev[6 + i] = Data.InstanceIDs[i].ToString();

            //TextPrev[6 + Data.InstanceIDs.Count] = $"Positions: {Data.PositionIDs.Count}";
            //for (int i = 0; i < Data.PositionIDs.Count; ++i)
            //    TextPrev[7 + Data.InstanceIDs.Count + i] = Data.PositionIDs[i].ToString();

            //TextPrev[7 + Data.InstanceIDs.Count + Data.PositionIDs.Count] = $"Paths: {Data.PathIDs.Count}";
            //for (int i = 0; i < Data.PathIDs.Count; ++i)
            //    TextPrev[8 + Data.InstanceIDs.Count + Data.PositionIDs.Count + i] = Data.PathIDs[i].ToString();

            //TextPrev[8 + Data.InstanceIDs.Count + Data.PositionIDs.Count + Data.PathIDs.Count] = $"Properties: {Convert.ToString(Data.UnkI32, 16).ToUpper()}";

            //TextPrev[9 + Data.InstanceIDs.Count + Data.PositionIDs.Count + Data.PathIDs.Count] = $"Integers: {Data.UnkI321.Count}";
            //for (int i = 0; i < Data.UnkI321.Count; ++i)
            //    TextPrev[10 + Data.InstanceIDs.Count + Data.PositionIDs.Count + Data.PathIDs.Count + i] = Data.UnkI321[i].ToString();

            //TextPrev[10 + Data.InstanceIDs.Count + Data.PositionIDs.Count + Data.PathIDs.Count + Data.UnkI321.Count] = $"Floats: {Data.UnkI322.Count}";
            //for (int i = 0; i < Data.UnkI322.Count; ++i)
            //    TextPrev[11 + Data.InstanceIDs.Count + Data.PositionIDs.Count + Data.PathIDs.Count + Data.UnkI321.Count + i] = Data.UnkI322[i].ToString();

            //TextPrev[11 + Data.InstanceIDs.Count + Data.PositionIDs.Count + Data.PathIDs.Count + Data.UnkI321.Count + Data.UnkI322.Count] = $"Integers: {Data.UnkI323.Count}";
            //for (int i = 0; i < Data.UnkI323.Count; ++i)
            //    TextPrev[12 + Data.InstanceIDs.Count + Data.PositionIDs.Count + Data.PathIDs.Count + Data.UnkI321.Count + Data.UnkI322.Count + i] = Data.UnkI323[i].ToString();
        }

        private void Menu_OpenEditor()
        {
            MainFile.OpenEditor((SectionController)Node.Parent.Tag);
        }
    }
}
