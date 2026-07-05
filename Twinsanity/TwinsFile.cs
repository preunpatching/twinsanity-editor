using System;
using System.Collections.Generic;
using System.IO;

namespace Twinsanity
{
    /// <summary>
    /// Represents a Twinsanity RM/SM file, a full pair corresponds to a complete level "chunk"
    /// </summary>
    public class TwinsFile : TwinsSection
    {
        public string FileName { get; set; }
        public string SafeFileName { get; set; }

        public new FileType Type { get; set; }
        public ConsoleType Console { get; set; }

        public void LoadFile(string fileName, FileType type)
        {
            FileName = fileName;

            byte[] buffer;
            using (FileStream br = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read, 0x10000, FileOptions.SequentialScan))
            {
                buffer = new byte[br.Length];
                _ = br.Read(buffer, 0, buffer.Length);
            }
            using (MemoryStream memoryStream = new MemoryStream(buffer))
            {
                using (BinaryReader reader = new BinaryReader(memoryStream))
                {
                    LoadFileStream(reader, fileName, type);
                }
            }
        }

        /// <summary>
        /// Load an RM/SM file.
        /// </summary>
        /// <param name="path">Path to the file to load from.</param>
        /// <param name="type">Filetype. RM2, SM2, etc.</param>
        public void LoadFileStream(BinaryReader reader, string path, FileType type)
        {
            if (!File.Exists(path))
            {
                return;
            }

            Records = new List<TwinsItem>();
            RecordIDs = new Dictionary<uint, int>();
            FileStream file = new FileStream(path, FileMode.Open, FileAccess.Read);
            Type = type;
            Console = ConsoleType.PS2;
            if (type == FileType.RMX || type == FileType.SMX)
            {
                Console = ConsoleType.XBOX;
            }

            if (type == FileType.Frontend)
            {
                TwinsSection sec = new TwinsSection
                {
                    ID = 3,
                    Type = SectionType.SE,
                };
                long sk = reader.BaseStream.Position;
                sec.Load(reader, (int)file.Length);
                reader.BaseStream.Position = sk;
                RecordIDs.Add(3, Records.Count);
                Records.Add(sec);
                reader.Close();
                return;
            }
            if ((Magic = reader.ReadUInt32()) != magic)
            {
                throw new Exception("LoadFile: Magic number is wrong.");
            }

            FileName = path;
            bool miniFix = false;
            int count;
            if (type == FileType.MonkeyBallRM || type == FileType.MonkeyBallSM)
            {
                count = reader.ReadInt16();
                uint test = reader.ReadUInt16();
                if (test != 0) // PS2 file and sections contain 0x0080 here
                {
                    miniFix = true;
                }
                else
                {
                    Console = ConsoleType.PSP;
                }
            }
            else
            {
                count = reader.ReadInt32();
            }

            _ = reader.ReadUInt32();

            List<TwinsSubInfo> SubItems = new List<TwinsSubInfo>();
            for (int i = 0; i < count; i++)
            {
                TwinsSubInfo sub = new TwinsSubInfo
                {
                    Off = reader.ReadUInt32(),
                    Size = reader.ReadInt32(),
                    ID = reader.ReadUInt32()
                };
                SubItems.Add(sub);
            }

            for (int i = 0; i < count; i++)
            {
                uint s_off = SubItems[i].Off;
                uint s_id = SubItems[i].ID;
                int s_size = SubItems[i].Size;
                reader.BaseStream.Position = s_off;

                BinaryReader secReader = reader;
                MemoryStream subMem = null;
                if (miniFix)
                {
                    int ItemSize = i != count - 1 ? (int)(SubItems[i + 1].Off - SubItems[i].Off) : (int)(reader.BaseStream.Length - SubItems[i].Off);
                    if (SubItems[i].Size != ItemSize)
                    {
                        try
                        {
                            _ = reader.ReadBytes(4); // PACK
                            byte[] outData = InteropUCL.DecompressNRV2B(reader.ReadBytes(ItemSize - 4));
                            subMem = new MemoryStream(outData);
                            secReader = new BinaryReader(subMem);
                            s_off = 0;
                        }
                        catch
                        {
                            System.Console.WriteLine($"Failed to unpack item {SubItems[i].ID} in {Type}");
                        }
                    }
                }

                switch (type)
                {
                    case FileType.DemoRM2:
                    case FileType.RMX:
                    case FileType.RM2:
                        {
                            switch (s_id)
                            {
                                case 0:
                                case 1:
                                case 2:
                                case 3:
                                case 4:
                                case 5:
                                case 6:
                                case 7:
                                case 10:
                                case 11:
                                    {
                                        TwinsSection sec = new TwinsSection() { ID = s_id, Parent = this };
                                        if (s_id <= 7)
                                        {
                                            sec.Type = type == FileType.DemoRM2 ? SectionType.InstanceDemo : SectionType.Instance;
                                        }
                                        else if (s_id == 10)
                                        {
                                            sec.Type = type == FileType.DemoRM2 ? SectionType.CodeDemo : type == FileType.RMX ? SectionType.CodeX : SectionType.Code;
                                        }
                                        else if (s_id == 11)
                                        {
                                            sec.Type = type == FileType.RMX ? SectionType.GraphicsX : type == FileType.DemoRM2 ? SectionType.GraphicsD : SectionType.Graphics;
                                        }

                                        long sk = reader.BaseStream.Position;
                                        reader.BaseStream.Position = s_off;
                                        sec.Level = 1;
                                        sec.Load(reader, s_size);
                                        reader.BaseStream.Position = sk;
                                        RecordIDs.Add(s_id, Records.Count);
                                        Records.Add(sec);
                                        break;
                                    }
                                case 9:
                                    {
                                        ColData rec = new ColData() { ID = s_id, Parent = this };
                                        long sk = reader.BaseStream.Position;
                                        reader.BaseStream.Position = s_off;
                                        rec.Load(reader, s_size);
                                        reader.BaseStream.Position = sk;
                                        RecordIDs.Add(s_id, Records.Count);
                                        Records.Add(rec);
                                        break;
                                    }
                                case 8:
                                    {
                                        ParticleData rec = new ParticleData() { ID = s_id, Parent = this };
                                        long sk = reader.BaseStream.Position;
                                        reader.BaseStream.Position = s_off;
                                        rec.Load(reader, s_size);
                                        reader.BaseStream.Position = sk;
                                        RecordIDs.Add(s_id, Records.Count);
                                        Records.Add(rec);
                                        break;
                                    }
                                default:
                                    {
                                        TwinsItem rec = new TwinsItem { ID = s_id, Parent = this };
                                        long sk = reader.BaseStream.Position;
                                        reader.BaseStream.Position = s_off;
                                        rec.Load(reader, s_size);
                                        reader.BaseStream.Position = sk;
                                        RecordIDs.Add(s_id, Records.Count);
                                        Records.Add(rec);
                                        break;
                                    }
                            }
                        }
                        break;
                    case FileType.DemoSM2:
                    case FileType.SM2:
                    case FileType.SMX:
                        {
                            switch (s_id)
                            {
                                case 6:
                                    {
                                        SectionType targetType = SectionType.Graphics;
                                        if (type == FileType.SMX)
                                        {
                                            targetType = SectionType.GraphicsX;
                                        }

                                        if (type == FileType.DemoSM2)
                                        {
                                            targetType = SectionType.GraphicsD;
                                        }

                                        TwinsSection sec = new TwinsSection
                                        {
                                            ID = s_id,
                                            Type = targetType,
                                            Level = 1,
                                            Parent = this
                                        };
                                        long sk = reader.BaseStream.Position;
                                        reader.BaseStream.Position = s_off;
                                        sec.Load(reader, s_size);
                                        reader.BaseStream.Position = sk;
                                        RecordIDs.Add(s_id, Records.Count);
                                        Records.Add(sec);
                                        break;
                                    }
                                case 5:
                                    {
                                        ChunkLinks rec = new ChunkLinks { ID = s_id, Parent = this };
                                        long sk = reader.BaseStream.Position;
                                        reader.BaseStream.Position = s_off;
                                        rec.Load(reader, s_size);
                                        reader.BaseStream.Position = sk;
                                        RecordIDs.Add(s_id, Records.Count);
                                        Records.Add(rec);
                                        break;
                                    }
                                case 0:
                                    {
                                        SceneryData rec = new SceneryData { ID = s_id, Parent = this };
                                        long sk = reader.BaseStream.Position;
                                        reader.BaseStream.Position = s_off;
                                        rec.Load(reader, s_size);
                                        reader.BaseStream.Position = sk;
                                        RecordIDs.Add(s_id, Records.Count);
                                        Records.Add(rec);
                                        break;
                                    }
                                case 4:
                                    {
                                        DynamicSceneryData rec = new DynamicSceneryData { ID = s_id, Parent = this };
                                        long sk = reader.BaseStream.Position;
                                        reader.BaseStream.Position = s_off;
                                        rec.Load(reader, s_size);
                                        reader.BaseStream.Position = sk;
                                        RecordIDs.Add(s_id, Records.Count);
                                        Records.Add(rec);
                                        break;
                                    }
                                default:
                                    {
                                        TwinsItem rec = new TwinsItem { ID = s_id, Parent = this };
                                        long sk = reader.BaseStream.Position;
                                        reader.BaseStream.Position = s_off;
                                        rec.Load(reader, s_size);
                                        reader.BaseStream.Position = sk;
                                        RecordIDs.Add(s_id, Records.Count);
                                        Records.Add(rec);
                                        break;
                                    }
                            }
                        }
                        break;
                    case FileType.MonkeyBallRM:
                        {
                            switch (s_id)
                            {
                                case 1:
                                case 2:
                                case 3:
                                case 4:
                                case 5:
                                case 6:
                                case 7:
                                case 8:
                                case 11:
                                case 12:
                                    {
                                        TwinsSection sec = new TwinsSection
                                        {
                                            ID = s_id,
                                            Parent = this,
                                            Type = s_id == 12 ? SectionType.GraphicsMB : s_id == 11 ? SectionType.CodeMB : SectionType.InstanceMB
                                        };
                                        long sk = reader.BaseStream.Position;
                                        reader.BaseStream.Position = s_off;
                                        sec.Level = 1;
                                        sec.Load(reader, s_size, miniFix);
                                        reader.BaseStream.Position = sk;
                                        RecordIDs.Add(s_id, Records.Count);
                                        Records.Add(sec);
                                        break;
                                    }
                                case 9:
                                    {
                                        ParticleData rec = new ParticleData() { ID = s_id, Parent = this };
                                        long sk = secReader.BaseStream.Position;
                                        secReader.BaseStream.Position = s_off;
                                        rec.Load(secReader, s_size, true);
                                        secReader.BaseStream.Position = sk;
                                        RecordIDs.Add(s_id, Records.Count);
                                        Records.Add(rec);
                                        break;
                                    }
                                case 10:
                                    {
                                        if (!miniFix)
                                        {
                                            ColData rec = new ColData() { ID = s_id, Parent = this };
                                            long sk = secReader.BaseStream.Position;
                                            secReader.BaseStream.Position = s_off;
                                            rec.Load(secReader, s_size);
                                            secReader.BaseStream.Position = sk;
                                            RecordIDs.Add(s_id, Records.Count);
                                            Records.Add(rec);
                                        }
                                        else
                                        {
                                            ColDataMB rec = new ColDataMB() { ID = s_id, Parent = this };
                                            long sk = secReader.BaseStream.Position;
                                            secReader.BaseStream.Position = s_off;
                                            rec.Load(secReader, s_size);
                                            secReader.BaseStream.Position = sk;
                                            RecordIDs.Add(s_id, Records.Count);
                                            Records.Add(rec);
                                        }
                                        break;
                                    }
                                default:
                                    {
                                        TwinsItem rec = new TwinsItem { ID = s_id, Parent = this };
                                        long sk = secReader.BaseStream.Position;
                                        secReader.BaseStream.Position = s_off;
                                        rec.Load(secReader, s_size);
                                        secReader.BaseStream.Position = sk;
                                        RecordIDs.Add(s_id, Records.Count);
                                        Records.Add(rec);
                                        break;
                                    }
                            }
                        }
                        break;
                    case FileType.MonkeyBallSM:
                        {
                            switch (s_id)
                            {
                                default:
                                    {
                                        TwinsItem rec = new TwinsItem { ID = s_id, Parent = this };
                                        long sk = secReader.BaseStream.Position;
                                        secReader.BaseStream.Position = s_off;
                                        rec.Load(secReader, s_size);
                                        secReader.BaseStream.Position = sk;
                                        RecordIDs.Add(s_id, Records.Count);
                                        Records.Add(rec);
                                        break;
                                    }
                                case 0:
                                    {
                                        if (miniFix)
                                        {
                                            SceneryData rec = new SceneryData { ID = s_id, IsMonkeyBall = true, Parent = this };
                                            long sk = secReader.BaseStream.Position;
                                            secReader.BaseStream.Position = s_off;
                                            rec.Load(secReader, s_size);
                                            secReader.BaseStream.Position = sk;
                                            RecordIDs.Add(s_id, Records.Count);
                                            Records.Add(rec);
                                            break;
                                        }
                                        else
                                        {
                                            TwinsItem rec = new TwinsItem { ID = s_id, Parent = this };
                                            long sk = secReader.BaseStream.Position;
                                            secReader.BaseStream.Position = s_off;
                                            rec.Load(secReader, s_size);
                                            secReader.BaseStream.Position = sk;
                                            RecordIDs.Add(s_id, Records.Count);
                                            Records.Add(rec);
                                            break;
                                        }
                                    }
                                case 8:
                                    {
                                        SectionType targetType = SectionType.SceneryMB;
                                        TwinsSection sec = new TwinsSection
                                        {
                                            ID = s_id,
                                            Type = targetType,
                                            Level = 1,
                                            Parent = this
                                        };
                                        long sk = secReader.BaseStream.Position;
                                        secReader.BaseStream.Position = s_off;
                                        sec.Load(secReader, s_size);
                                        secReader.BaseStream.Position = sk;
                                        RecordIDs.Add(s_id, Records.Count);
                                        Records.Add(sec);
                                        break;
                                    }
                                case 5:
                                    {
                                        DynamicSceneryDataMB rec = new DynamicSceneryDataMB { ID = s_id, Parent = this };
                                        long sk = secReader.BaseStream.Position;
                                        secReader.BaseStream.Position = s_off;
                                        rec.Load(secReader, s_size);
                                        secReader.BaseStream.Position = sk;
                                        RecordIDs.Add(s_id, Records.Count);
                                        Records.Add(rec);
                                        break;
                                    }
                                case 7:
                                    {
                                        // empty on PSP
                                        SectionType targetType = SectionType.GraphicsMB;
                                        TwinsSection sec = new TwinsSection
                                        {
                                            ID = s_id,
                                            Type = targetType,
                                            Level = 1,
                                            Parent = this
                                        };
                                        long sk = reader.BaseStream.Position;
                                        reader.BaseStream.Position = s_off;
                                        sec.Load(reader, s_size, miniFix);
                                        reader.BaseStream.Position = sk;
                                        RecordIDs.Add(s_id, Records.Count);
                                        Records.Add(sec);
                                        break;
                                    }
                                case 6:
                                    {
                                        ChunkLinks rec = new ChunkLinks { ID = s_id, Parent = this };
                                        long sk = secReader.BaseStream.Position;
                                        secReader.BaseStream.Position = s_off;
                                        rec.Load(secReader, s_size);
                                        secReader.BaseStream.Position = sk;
                                        RecordIDs.Add(s_id, Records.Count);
                                        Records.Add(rec);
                                        break;
                                    }
                            }
                        }
                        break;
                }

                subMem?.Close();
            }
        }

        public void Merge(TwinsFile package)
        {
            TwinsSection importObjects = package.GetItem<TwinsSection>(10).GetItem<TwinsSection>(0);
            TwinsSection existingObjects = GetItem<TwinsSection>(10).GetItem<TwinsSection>(0);
            foreach (GameObject importObject in importObjects.Records)
            {
                if (existingObjects.HasItem(importObject.ID))
                {
                    continue;
                }
                importObject.FillPackage(package, this);
            }
        }

        public void MergeXbox(TwinsFile package)
        {
            TwinsSection importObjects = package.GetItem<TwinsSection>(10).GetItem<TwinsSection>(0);
            TwinsSection existingObjects = GetItem<TwinsSection>(10).GetItem<TwinsSection>(0);
            foreach (GameObject importObject in importObjects.Records)
            {
                if (existingObjects.HasItem(importObject.ID))
                {
                    continue;
                }
                importObject.FillPackageXbox(package, this);
            }
        }

        public void MergeDemo(TwinsFile package)
        {
            TwinsSection importObjects = package.GetItem<TwinsSection>(10).GetItem<TwinsSection>(0);
            TwinsSection existingObjects = GetItem<TwinsSection>(10).GetItem<TwinsSection>(0);
            foreach (GameObjectDemo importObject in importObjects.Records)
            {
                if (existingObjects.HasItem(importObject.ID))
                {
                    continue;
                }
                importObject.FillPackage(package, this);
            }
        }

        public void FillExportPackageStructure(ConsoleType console = ConsoleType.PS2)
        {
            Magic = magic;
            Console = console;
            TwinsSection graphicsSection = CreateGraphicsSection();
            RecordIDs.Add(graphicsSection.ID, Records.Count);
            Records.Add(graphicsSection);
            TwinsSection codeSection = CreateCodeSection();
            RecordIDs.Add(codeSection.ID, Records.Count);
            Records.Add(codeSection);
        }

        public void FillExportPackageXboxStructure(ConsoleType console = ConsoleType.XBOX)
        {
            Magic = magic;
            Console = console;
            TwinsSection graphicsSection = CreateGraphicsXSection();
            RecordIDs.Add(graphicsSection.ID, Records.Count);
            Records.Add(graphicsSection);
            TwinsSection codeSection = CreateCodeXSection();
            RecordIDs.Add(codeSection.ID, Records.Count);
            Records.Add(codeSection);
        }

        public void FillExportPackageDemoStructure(ConsoleType console = ConsoleType.PS2)
        {
            Magic = magic;
            Console = console;
            TwinsSection graphicsSection = CreateGraphicsSection();
            RecordIDs.Add(graphicsSection.ID, Records.Count);
            Records.Add(graphicsSection);
            TwinsSection codeSection = CreateCodeDemoSection();
            RecordIDs.Add(codeSection.ID, Records.Count);
            Records.Add(codeSection);
        }

        /// <summary>
        /// Save the file.
        /// </summary>
        /// <param name="path">File directory to save to.</param>
        public void SaveFile(string path)
        {
            FileStream file = new FileStream(path, FileMode.Create, FileAccess.Write);
            BinaryWriter writer = new BinaryWriter(file, System.Text.Encoding.ASCII);
            writer.Write(Magic);
            writer.Write(Records.Count);
            writer.Write(ContentSize);

            int sec_off = (Records.Count * 12) + 12;
            foreach (TwinsItem i in Records)
            {
                writer.Write(sec_off);
                writer.Write(i.Size);
                writer.Write(i.ID);
                sec_off += i.Size;
            }

            foreach (TwinsItem i in Records)
            {
                i.Save(writer);
            }

            writer.Close();
        }

        private int GetContentSize()
        {
            int c_size = 0;
            foreach (TwinsItem i in Records)
            {
                c_size += i.Size;
            }

            return c_size;
        }

        protected override int GetSize()
        {
            return ContentSize + (Records.Count * 12) + 12;
        }

        //NOTE: Do NOT use "First"
        public enum FileType { First = SectionType.Last, RM2, SM2, DemoRM2, DemoSM2, RMX, SMX, Frontend, MonkeyBallRM, MonkeyBallSM };

        public enum ConsoleType { First = SectionType.Last, PS2, PSP, XBOX }
    }
}
