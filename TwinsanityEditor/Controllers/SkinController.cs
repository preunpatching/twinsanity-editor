using OpenTK;
using System.Collections.Generic;
using System.Drawing;
using Twinsanity;

namespace TwinsanityEditor
{
    public class SkinController : ItemController
    {
        public new Skin Data { get; set; }

        public List<Vertex[]> Vertices { get; set; } = new List<Vertex[]>();
        public List<Vertex[]> TposeVertices { get; set; } = new List<Vertex[]>();
        public List<Skin.JointInfo[]> JointInfos { get; set; } = new List<Skin.JointInfo[]>();
        public List<uint[]> Indices { get; set; } = new List<uint[]>();

        public bool IsLoaded { get; private set; }

        public SkinController(MainForm topform, Skin item) : base(topform, item)
        {
            Data = item;
            AddMenu("Open mesh viewer", Menu_OpenViewer);
        }

        protected override string GetName()
        {
            return $"Skin [ID {Data.ID:X}/{Data.ID}]";
        }

        private void Menu_OpenViewer()
        {
            MainFile.OpenMeshViewer(this);
        }

        protected override void GenText()
        {
            List<string> text = new List<string>
            {
                $"ID: {Data.ID}",
                $"SubModels {Data.SubModels.Count}"
            };

            int index = 0;
            foreach (Skin.SubModel model in Data.SubModels)
            {
                text.Add($"SubModel {index}");
                text.Add($"MaterialID {model.MaterialID}/0x{model.MaterialID:X}");
                text.Add($"Vertexes {model.Vertexes.Count}");
                for (int i = 0; i < model.Vertexes.Count; ++i)
                {
                    text.Add($"Vertex #{i}:");
                    text.Add($"\tJoint index 1 {model.Vertexes[i].Joint.JointIndex1}; Weight 1 {model.Vertexes[i].Joint.Weight1}");
                    text.Add($"\tJoint index 2 {model.Vertexes[i].Joint.JointIndex2}; Weight 2 {model.Vertexes[i].Joint.Weight2}");
                    text.Add($"\tJoint index 3 {model.Vertexes[i].Joint.JointIndex3}; Weight 3 {model.Vertexes[i].Joint.Weight3}");
                }
                index++;
            }

            TextPrev = text.ToArray();
        }

        public void LoadMeshData()
        {
            Vertices.Clear();
            Indices.Clear();
            TposeVertices.Clear();
            JointInfos.Clear();

            uint refIndex = 0U;
            int offset = 0;

            bool isSpyroModel = false;
            if (DefaultHashes.Hash_Skins.ContainsKey(Data.ID) && DefaultHashes.Hash_Skins[Data.ID] == "Spyro")
            {
                isSpyroModel = true;
            }

            foreach (Skin.SubModel model in Data.SubModels)
            {
                List<Vertex> vtx = new List<Vertex>();
                List<uint> idx = new List<uint>();
                List<Skin.JointInfo> jointInfos = new List<Skin.JointInfo>();
                for (int j = 0; j < model.Vertexes.Count; ++j)
                {
                    if (j < model.Vertexes.Count - 2)
                    {
                        if (model.Vertexes[j + 2].Conn)
                        {
                            if ((offset + j) % 2 == 0)
                            {
                                idx.Add(refIndex);
                                idx.Add(refIndex + 1);
                                idx.Add(refIndex + 2);
                            }
                            else
                            {
                                idx.Add(refIndex + 1);
                                idx.Add(refIndex);
                                idx.Add(refIndex + 2);
                            }
                        }
                        ++refIndex;
                    }
                    Color col = Color.FromArgb(model.Vertexes[j].A, model.Vertexes[j].R, model.Vertexes[j].G, model.Vertexes[j].B);
                    if (isSpyroModel)
                    { // Spyro model specifically doesn't need UV flipping
                        vtx.Add(new Vertex(new Vector3(-model.Vertexes[j].X, model.Vertexes[j].Y, model.Vertexes[j].Z),
                                new Vector3(0, 0, 0), new Vector2(model.Vertexes[j].U, model.Vertexes[j].V),
                                col));
                    }
                    else
                    {
                        vtx.Add(new Vertex(new Vector3(-model.Vertexes[j].X, model.Vertexes[j].Y, model.Vertexes[j].Z),
                                new Vector3(0, 0, 0), new Vector2(model.Vertexes[j].U, 1 - model.Vertexes[j].V),
                                col));
                    }
                    jointInfos.Add(model.Vertexes[j].Joint);
                }
                //offset += model.Vertexes.Count;
                refIndex = 0;

                for (int i = 0; i < idx.Count; i += 3)
                {
                    uint n1 = idx[i];
                    uint n2 = idx[i + 1];
                    uint n3 = idx[i + 2];
                    Vertex v1 = vtx[(int)n1];
                    Vertex v2 = vtx[(int)n2];
                    Vertex v3 = vtx[(int)n3];
                    Vector3 normal = VectorFuncs.CalcNormal(v1.Pos, v2.Pos, v3.Pos);
                    v1.Nor += normal;
                    v2.Nor += normal;
                    v3.Nor += normal;
                    vtx[(int)n1] = v1;
                    vtx[(int)n2] = v2;
                    vtx[(int)n3] = v3;
                }

                Vertices.Add(vtx.ToArray());
                Indices.Add(idx.ToArray());
                TposeVertices.Add(vtx.ToArray());
                JointInfos.Add(jointInfos.ToArray());
            }

            IsLoaded = true;
        }
    }
}