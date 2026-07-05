using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using Twinsanity;

namespace TwinsanityEditor
{
    public class RigidModelController : ItemController
    {
        public new RigidModel Data { get; set; }

        public RigidModelController(MainForm topform, RigidModel item) : base(topform, item)
        {
            Data = item;
            AddMenu("Export mesh to PLY", Menu_ExportPLY);
            AddMenu("Open model viewer", Menu_OpenViewer);
        }

        protected override string GetName()
        {
            return string.Format("Rigid Model [ID {0:X8}/{0}]", Data.ID);
        }

        protected override void GenText()
        {
            List<string> text = new List<string>
            {
                string.Format("ID: {0:X8}", Data.ID),
                $"Size: {Data.Size}",
                $"Header: {Data.Header} MaterialCount: {Data.MaterialIDs.Length}"
            };
            for (int i = 0; i < Data.MaterialIDs.Length; ++i)
            {
                text.Add($"#{i}: 0x{Data.MaterialIDs[i]:X8}:{MainFile.GetMaterialName(Data.MaterialIDs[i])}");
            }

            text.Add(string.Format("Model: {0:X8}", Data.MeshID));
            TextPrev = text.ToArray();
        }

        private void Menu_ExportPLY()
        {
            if (MessageBox.Show("PLY export is experimental, material and texture information will not be exported. Continue anyway?", "Export Warning", MessageBoxButtons.YesNo) == DialogResult.No)
            {
                return;
            }

            SaveFileDialog sfd = new SaveFileDialog { Filter = "PLY files (*.ply)|*.ply", FileName = GetName() };
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                File.WriteAllBytes(sfd.FileName, Data.Parent.Parent.GetItem<TwinsSection>(2).GetItem<Model>(Data.MeshID).ToPLY());
            }
        }

        private void Menu_OpenViewer()
        {
            MainFile.OpenModelViewer(this);
        }
    }
}
