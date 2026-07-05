using System.Collections.Generic;
using System.IO;

namespace Twinsanity
{
    public class GameObject : TwinsItem
    {
        private int size;
        public uint UnkBitfield { get; set; }
        public List<byte> ScriptSlots { get; set; } = new List<byte>(); // Pairs;Scripts;GameObjects;UInt32s;Sounds;00;00;00 (last 3 are potentially a side effect of needing object name's length to be word aligned)
        public List<uint> UI32 { get; set; } = new List<uint>();
        public List<ushort> OGIs { get; set; } = new List<ushort>();
        public List<ushort> Anims { get; set; } = new List<ushort>();
        public List<ushort> Scripts { get; set; } = new List<ushort>();
        public List<ushort> Objects { get; set; } = new List<ushort>();
        public List<ushort> Sounds { get; set; } = new List<ushort>();
        public uint PHeader { get; set; } // Inst;Pos;Path;00
        public uint PUI32 { get; set; }
        //private int pUi321.Length = 0;
        //private int pUi322.Length = 1;
        //private int pUi323.Length = 2;
        public List<uint> instFlagsList = new List<uint>();
        public List<float> instFloatsList = new List<float>();
        public List<uint> instIntegerList = new List<uint>();
        public uint flag;
        public List<ushort> cObjects = new List<ushort>();
        public List<ushort> cOGIs = new List<ushort>();
        public List<ushort> cAnims = new List<ushort>();
        public List<ushort> cCM = new List<ushort>();
        public List<ushort> cScripts = new List<ushort>();
        public List<ushort> cUnk = new List<ushort>();
        public List<ushort> cSounds = new List<ushort>();
        public int scriptCommandsAmount;
        public List<ushort> scriptParams = new List<ushort>();
        public int scriptGameVersion = 0;
        public Script.MainScript.ScriptCommand scriptCommand = null;
        public List<Script.MainScript.ScriptCommand> scriptCommands = new List<Script.MainScript.ScriptCommand>();

        public string Name { get; set; } = "New Game Object";
        public GameObject()
        {
            while (ScriptSlots.Count < 8)
            {
                ScriptSlots.Add(0);
            }
        }
        private void UpdateSlots()
        {
            ScriptSlots[0] = (byte)OGIs.Count;
            ScriptSlots[1] = (byte)Scripts.Count;
            ScriptSlots[2] = (byte)Objects.Count;
            ScriptSlots[3] = (byte)UI32.Count;
            ScriptSlots[4] = (byte)Sounds.Count;
            ScriptSlots[5] = 0;
            ScriptSlots[6] = 0;
            ScriptSlots[7] = 0;
        }
        public override void Save(BinaryWriter writer)
        {
            long sk = writer.BaseStream.Position;
            UpdateSlots();
            writer.Write(UnkBitfield);
            for (int i = 0; i < 8; ++i)
            {
                writer.Write(ScriptSlots[i]);
            }
            writer.Write(Name.Length);
            writer.Write(Name.ToCharArray(), 0, Name.Length);
            writer.Write(UI32.Count);
            for (int i = 0; i < UI32.Count; ++i)
            {
                writer.Write(UI32[i]);
            }

            writer.Write(OGIs.Count);
            for (int i = 0; i < OGIs.Count; ++i)
            {
                writer.Write(OGIs[i]);
            }

            writer.Write(Anims.Count);
            for (int i = 0; i < Anims.Count; ++i)
            {
                writer.Write(Anims[i]);
            }

            writer.Write(Scripts.Count);
            for (int i = 0; i < Scripts.Count; ++i)
            {
                writer.Write(Scripts[i]);
            }

            writer.Write(Objects.Count);
            for (int i = 0; i < Objects.Count; ++i)
            {
                writer.Write(Objects[i]);
            }

            writer.Write(Sounds.Count);
            for (int i = 0; i < Sounds.Count; ++i)
            {
                writer.Write(Sounds[i]);
            }

            if ((UnkBitfield & 0x20000000) != 0x0)
            {
                PHeader = (uint)((byte)instFlagsList.Count
                | (instFloatsList.Count << 8)
                | (instIntegerList.Count << 16));

                writer.Write(PHeader);
                writer.Write(PUI32);
                writer.Write(instFlagsList.Count);
                for (int i = 0; i < instFlagsList.Count; ++i)
                {
                    writer.Write(instFlagsList[i]);
                }

                writer.Write(instFloatsList.Count);
                for (int i = 0; i < instFloatsList.Count; ++i)
                {
                    writer.Write(instFloatsList[i]);
                }

                writer.Write(instIntegerList.Count);
                for (int i = 0; i < instIntegerList.Count; ++i)
                {
                    writer.Write(instIntegerList[i]);
                }
            }

            if ((UnkBitfield & 0x40000000) != 0x0)
            {
                updateFlag();
                writer.Write(flag);
                if ((flag & 0x01) != 0)
                {
                    writer.Write(cObjects.Count);
                    for (int i = 0; i < cObjects.Count; ++i)
                    {
                        writer.Write(cObjects[i]);
                    }
                }
                if ((flag & 0x02) != 0)
                {
                    writer.Write(cOGIs.Count);
                    for (int i = 0; i < cOGIs.Count; ++i)
                    {
                        writer.Write(cOGIs[i]);
                    }
                }
                if ((flag & 0x04) != 0)
                {
                    writer.Write(cAnims.Count);
                    for (int i = 0; i < cAnims.Count; ++i)
                    {
                        writer.Write(cAnims[i]);
                    }
                }
                if ((flag & 0x08) != 0)
                {
                    writer.Write(cCM.Count);
                    for (int i = 0; i < cCM.Count; ++i)
                    {
                        writer.Write(cCM[i]);
                    }
                }
                if ((flag & 0x10) != 0)
                {
                    writer.Write(cScripts.Count);
                    for (int i = 0; i < cScripts.Count; ++i)
                    {
                        writer.Write(cScripts[i]);
                    }
                }
                if ((flag & 0x20) != 0)
                {
                    writer.Write(cUnk.Count);
                    for (int i = 0; i < cUnk.Count; ++i)
                    {
                        writer.Write(cUnk[i]);
                    }
                }
                if ((flag & 0x40) != 0)
                {
                    writer.Write(cSounds.Count);
                    for (int i = 0; i < cSounds.Count; ++i)
                    {
                        writer.Write(cSounds[i]);
                    }
                }
            }
            writer.Write(scriptCommandsAmount);
            scriptCommand?.Write(writer);
            size = (int)(writer.BaseStream.Position - sk);
        }

        public override void Load(BinaryReader reader, int size)
        {
            scriptGameVersion = ParentType == SectionType.ScriptX ? 1 : ParentType == SectionType.ScriptDemo ? 2 : 0;

            _ = reader.BaseStream.Position;

            UnkBitfield = reader.ReadUInt32();
            for (int i = 0; i < 8; ++i)
            {
                ScriptSlots[i] = reader.ReadByte();
            }

            //Class1 = reader.ReadUInt32();
            //Class2 = reader.ReadUInt32();
            //Class3 = reader.ReadUInt32();
            int len = reader.ReadInt32();
            Name = new string(reader.ReadChars(len));

            // Read UInt32 script slots
            int cnt = reader.ReadInt32();
            UI32.Clear();
            for (int i = 0; i < cnt; ++i)
            {
                UI32.Add(reader.ReadUInt32());
            }

            // Read OGI script slots
            cnt = reader.ReadInt32();
            OGIs.Clear();
            for (int i = 0; i < cnt; ++i)
            {
                OGIs.Add(reader.ReadUInt16());
            }

            // Read Animation script slots
            cnt = reader.ReadInt32();
            Anims.Clear();
            for (int i = 0; i < cnt; ++i)
            {
                Anims.Add(reader.ReadUInt16());
            }

            // Read Script script slots
            cnt = reader.ReadInt32();
            Scripts.Clear();
            for (int i = 0; i < cnt; ++i)
            {
                Scripts.Add(reader.ReadUInt16());
            }

            // Read Object script slots
            cnt = reader.ReadInt32();
            Objects.Clear();
            for (int i = 0; i < cnt; ++i)
            {
                Objects.Add(reader.ReadUInt16());
            }

            // Read Sound script slots
            cnt = reader.ReadInt32();
            Sounds.Clear();
            for (int i = 0; i < cnt; ++i)
            {
                Sounds.Add(reader.ReadUInt16());
            }

            // Read instance properties
            if ((UnkBitfield & 0x20000000) != 0x0)
            {
                PHeader = reader.ReadUInt32();
                PUI32 = reader.ReadUInt32();

                cnt = reader.ReadInt32();
                instFlagsList.Clear();
                for (int i = 0; i < cnt; ++i)
                {
                    instFlagsList.Add(reader.ReadUInt32());
                }

                cnt = reader.ReadInt32();
                instFloatsList.Clear();
                for (int i = 0; i < cnt; ++i)
                {
                    instFloatsList.Add(reader.ReadSingle());
                }

                cnt = reader.ReadInt32();
                instIntegerList.Clear();
                for (int i = 0; i < cnt; ++i)
                {
                    instIntegerList.Add(reader.ReadUInt32());
                }
            }
            else
            {
                PHeader = 0;
                PUI32 = 0;
                instFlagsList.Clear();
                instFloatsList.Clear();
                instIntegerList.Clear();
            }
            // Read IDs needed for instance creation
            if ((UnkBitfield & 0x40000000) != 0x0)
            {
                flag = reader.ReadUInt32();
                if ((flag & 0x00000001) != 0)
                {
                    cnt = reader.ReadInt32();
                    cObjects.Clear();
                    for (int i = 0; i < cnt; ++i)
                    {
                        cObjects.Add(reader.ReadUInt16());
                    }
                }
                if ((flag & 0x00000002) != 0)
                {
                    cnt = reader.ReadInt32();
                    cOGIs.Clear();
                    for (int i = 0; i < cnt; ++i)
                    {
                        cOGIs.Add(reader.ReadUInt16());
                    }
                }
                if ((flag & 0x00000004) != 0)
                {
                    cnt = reader.ReadInt32();
                    cAnims.Clear();
                    for (int i = 0; i < cnt; ++i)
                    {
                        cAnims.Add(reader.ReadUInt16());
                    }
                }
                if ((flag & 0x00000008) != 0)
                {
                    cnt = reader.ReadInt32();
                    cCM.Clear();
                    for (int i = 0; i < cnt; ++i)
                    {
                        cCM.Add(reader.ReadUInt16());
                    }
                }
                if ((flag & 0x00000010) != 0)
                {
                    cnt = reader.ReadInt32();
                    cScripts.Clear();
                    for (int i = 0; i < cnt; ++i)
                    {
                        cScripts.Add(reader.ReadUInt16());
                    }
                }
                if ((flag & 0x00000020) != 0)
                {
                    cnt = reader.ReadInt32();
                    cUnk.Clear();
                    for (int i = 0; i < cnt; ++i)
                    {
                        cUnk.Add(reader.ReadUInt16());
                    }
                }
                if ((flag & 0x00000040) != 0)
                {
                    cnt = reader.ReadInt32();
                    cSounds.Clear();
                    for (int i = 0; i < cnt; ++i)
                    {
                        cSounds.Add(reader.ReadUInt16());
                    }
                }
            }
            scriptCommandsAmount = (int)reader.ReadUInt32();
            if (scriptCommandsAmount != 0)
            {
                scriptCommand = new Script.MainScript.ScriptCommand(reader, scriptGameVersion);
                Script.MainScript.ScriptCommand command = scriptCommand;
                do
                {
                    scriptCommands.Add(command);
                    command = command.nextCommand;
                } while (command != null);
            }
            else
            {
                scriptCommand = null;
            }
            this.size = size;
        }

        public void FillPackage(TwinsFile source, TwinsFile destination)
        {
            TwinsSection sourceObjects = source.GetItem<TwinsSection>(10).GetItem<TwinsSection>(0);
            TwinsSection destinationObjects = destination.GetItem<TwinsSection>(10).GetItem<TwinsSection>(0);
            TwinsSection sourceScripts = source.GetItem<TwinsSection>(10).GetItem<TwinsSection>(1);
            TwinsSection destinationScripts = destination.GetItem<TwinsSection>(10).GetItem<TwinsSection>(1);
            TwinsSection sourceAnimations = source.GetItem<TwinsSection>(10).GetItem<TwinsSection>(2);
            TwinsSection destinationAnimations = destination.GetItem<TwinsSection>(10).GetItem<TwinsSection>(2);
            TwinsSection sourceOGIs = source.GetItem<TwinsSection>(10).GetItem<TwinsSection>(3);
            TwinsSection destinationOGIs = destination.GetItem<TwinsSection>(10).GetItem<TwinsSection>(3);
            TwinsSection sourceCMs = source.GetItem<TwinsSection>(10).GetItem<TwinsSection>(4);
            TwinsSection destinationCMs = destination.GetItem<TwinsSection>(10).GetItem<TwinsSection>(4);
            TwinsSection sourceSounds = source.GetItem<TwinsSection>(10).GetItem<TwinsSection>(6);
            TwinsSection destinationSounds = destination.GetItem<TwinsSection>(10).GetItem<TwinsSection>(6);
            TwinsSection sourceEngSounds = source.GetItem<TwinsSection>(10).GetItem<TwinsSection>(7);
            TwinsSection destinationEngSounds = destination.GetItem<TwinsSection>(10).GetItem<TwinsSection>(7);
            TwinsSection sourceFreSounds = source.GetItem<TwinsSection>(10).GetItem<TwinsSection>(8);
            TwinsSection destinationFreSounds = destination.GetItem<TwinsSection>(10).GetItem<TwinsSection>(8);
            TwinsSection sourceGerSounds = source.GetItem<TwinsSection>(10).GetItem<TwinsSection>(9);
            TwinsSection destinationGerSounds = destination.GetItem<TwinsSection>(10).GetItem<TwinsSection>(9);
            TwinsSection sourceSpaSounds = source.GetItem<TwinsSection>(10).GetItem<TwinsSection>(10);
            TwinsSection destinationSpaSounds = destination.GetItem<TwinsSection>(10).GetItem<TwinsSection>(10);
            TwinsSection sourceItaSounds = source.GetItem<TwinsSection>(10).GetItem<TwinsSection>(11);
            TwinsSection destinationItaSounds = destination.GetItem<TwinsSection>(10).GetItem<TwinsSection>(11);
            TwinsSection sourceJpnSounds = source.GetItem<TwinsSection>(10).GetItem<TwinsSection>(12);
            TwinsSection destinationJpnSounds = destination.GetItem<TwinsSection>(10).GetItem<TwinsSection>(12);
            foreach (ushort animId in cAnims)
            {
                if (destinationAnimations.HasItem(animId))
                {
                    continue;
                }
                Animation linkedAnimation = sourceAnimations.GetItem<Animation>(animId);
                destinationAnimations.AddItem(animId, linkedAnimation);
            }
            foreach (ushort cmID in cCM)
            {
                if (destinationCMs.HasItem(cmID))
                {
                    continue;
                }
                CodeModel linkedCM = sourceCMs.GetItem<CodeModel>(cmID);
                destinationCMs.AddItem(cmID, linkedCM);
            }
            foreach (ushort ogiID in cOGIs)
            {
                if (destinationOGIs.HasItem(ogiID))
                {
                    continue;
                }
                GraphicsInfo linkedOGI = sourceOGIs.GetItem<GraphicsInfo>(ogiID);
                destinationOGIs.AddItem(ogiID, linkedOGI);
                linkedOGI.FillPackage(source, destination);
            }
            foreach (ushort scriptID in cScripts)
            {
                if (destinationScripts.HasItem(scriptID) || !sourceScripts.HasItem(scriptID))
                {
                    continue;
                }
                Script linkedScript = sourceScripts.GetItem<Script>(scriptID);
                destinationScripts.AddItem(scriptID, linkedScript);
            }
            using (MemoryStream soundStream = new MemoryStream())
            using (MemoryStream soundEngStream = new MemoryStream())
            using (MemoryStream soundFreStream = new MemoryStream())
            using (MemoryStream soundGerStream = new MemoryStream())
            using (MemoryStream soundSpaStream = new MemoryStream())
            using (MemoryStream soundItaStream = new MemoryStream())
            using (MemoryStream soundJpnStream = new MemoryStream())
            {
                BinaryWriter writerSound = new BinaryWriter(soundStream);
                writerSound.Write(destinationSounds.ExtraData);
                BinaryWriter writerEngSound = new BinaryWriter(soundEngStream);
                writerEngSound.Write(destinationEngSounds.ExtraData);
                BinaryWriter writerFreSound = new BinaryWriter(soundFreStream);
                writerFreSound.Write(destinationFreSounds.ExtraData);
                BinaryWriter writerGerSound = new BinaryWriter(soundGerStream);
                writerGerSound.Write(destinationGerSounds.ExtraData);
                BinaryWriter writerSpaSound = new BinaryWriter(soundSpaStream);
                writerSpaSound.Write(destinationSpaSounds.ExtraData);
                BinaryWriter writerItaSound = new BinaryWriter(soundItaStream);
                writerItaSound.Write(destinationItaSounds.ExtraData);
                BinaryWriter writerJpnSound = new BinaryWriter(soundJpnStream);
                writerJpnSound.Write(destinationJpnSounds.ExtraData);
                foreach (ushort soundID in cSounds)
                {
                    bool isSfx = sourceSounds.HasItem(soundID);
                    bool isVoJpn = sourceJpnSounds.HasItem(soundID);
                    bool isVoEng = sourceEngSounds.HasItem(soundID);
                    bool isVoFre = sourceFreSounds.HasItem(soundID);
                    bool isVoGer = sourceGerSounds.HasItem(soundID);
                    bool isVoSpa = sourceSpaSounds.HasItem(soundID);
                    bool isVoIta = sourceItaSounds.HasItem(soundID);
                    if (isSfx && !destinationSounds.HasItem(soundID))
                    {
                        SoundEffect linkedSound = sourceSounds.GetItem<SoundEffect>(soundID);
                        SoundEffect newSound = new SoundEffect();
                        SoundEffect.CopySoundTo(linkedSound, sourceSounds.ExtraData, newSound, writerSound);
                        destinationSounds.AddItem(soundID, newSound);
                    }
                    if (isVoEng && !destinationEngSounds.HasItem(soundID))
                    {
                        SoundEffect linkedSound = sourceEngSounds.GetItem<SoundEffect>(soundID);
                        SoundEffect newSound = new SoundEffect();
                        SoundEffect.CopySoundTo(linkedSound, sourceEngSounds.ExtraData, newSound, writerEngSound);
                        destinationEngSounds.AddItem(soundID, newSound);
                    }
                    if (isVoFre && !destinationFreSounds.HasItem(soundID))
                    {
                        SoundEffect linkedSound = sourceFreSounds.GetItem<SoundEffect>(soundID);
                        SoundEffect newSound = new SoundEffect();
                        SoundEffect.CopySoundTo(linkedSound, sourceFreSounds.ExtraData, newSound, writerFreSound);
                        destinationFreSounds.AddItem(soundID, newSound);
                    }
                    if (isVoGer && !destinationGerSounds.HasItem(soundID))
                    {
                        SoundEffect linkedSound = sourceGerSounds.GetItem<SoundEffect>(soundID);
                        SoundEffect newSound = new SoundEffect();
                        SoundEffect.CopySoundTo(linkedSound, sourceGerSounds.ExtraData, newSound, writerGerSound);
                        destinationGerSounds.AddItem(soundID, newSound);
                    }
                    if (isVoSpa && !destinationSpaSounds.HasItem(soundID))
                    {
                        SoundEffect linkedSound = sourceSpaSounds.GetItem<SoundEffect>(soundID);
                        SoundEffect newSound = new SoundEffect();
                        SoundEffect.CopySoundTo(linkedSound, sourceSpaSounds.ExtraData, newSound, writerSpaSound);
                        destinationSpaSounds.AddItem(soundID, newSound);
                    }
                    if (isVoIta && !destinationItaSounds.HasItem(soundID))
                    {
                        SoundEffect linkedSound = sourceItaSounds.GetItem<SoundEffect>(soundID);
                        SoundEffect newSound = new SoundEffect();
                        SoundEffect.CopySoundTo(linkedSound, sourceItaSounds.ExtraData, newSound, writerItaSound);
                        destinationItaSounds.AddItem(soundID, newSound);
                    }
                    if (isVoJpn && !destinationJpnSounds.HasItem(soundID))
                    {
                        SoundEffect linkedSound = sourceJpnSounds.GetItem<SoundEffect>(soundID);
                        SoundEffect newSound = new SoundEffect();
                        SoundEffect.CopySoundTo(linkedSound, sourceJpnSounds.ExtraData, newSound, writerJpnSound);
                        destinationJpnSounds.AddItem(soundID, newSound);
                    }

                }
                destinationSounds.ExtraData = soundStream.ToArray();
                destinationEngSounds.ExtraData = soundEngStream.ToArray();
                destinationFreSounds.ExtraData = soundFreStream.ToArray();
                destinationGerSounds.ExtraData = soundGerStream.ToArray();
                destinationSpaSounds.ExtraData = soundSpaStream.ToArray();
                destinationItaSounds.ExtraData = soundItaStream.ToArray();
                destinationJpnSounds.ExtraData = soundJpnStream.ToArray();
            }
            foreach (ushort objectId in cObjects)
            {
                if (destinationObjects.HasItem(objectId) || ID == objectId)
                {
                    continue;
                }
                GameObject linkedObject = sourceObjects.GetItem<GameObject>(objectId);
                linkedObject.FillPackage(source, destination);

            }
            destinationObjects.AddItem(ID, this);
        }

        public void FillPackageXbox(TwinsFile source, TwinsFile destination)
        {
            TwinsSection sourceObjects = source.GetItem<TwinsSection>(10).GetItem<TwinsSection>(0);
            TwinsSection destinationObjects = destination.GetItem<TwinsSection>(10).GetItem<TwinsSection>(0);
            TwinsSection sourceScripts = source.GetItem<TwinsSection>(10).GetItem<TwinsSection>(1);
            TwinsSection destinationScripts = destination.GetItem<TwinsSection>(10).GetItem<TwinsSection>(1);
            TwinsSection sourceAnimations = source.GetItem<TwinsSection>(10).GetItem<TwinsSection>(2);
            TwinsSection destinationAnimations = destination.GetItem<TwinsSection>(10).GetItem<TwinsSection>(2);
            TwinsSection sourceOGIs = source.GetItem<TwinsSection>(10).GetItem<TwinsSection>(3);
            TwinsSection destinationOGIs = destination.GetItem<TwinsSection>(10).GetItem<TwinsSection>(3);
            TwinsSection sourceCMs = source.GetItem<TwinsSection>(10).GetItem<TwinsSection>(4);
            TwinsSection destinationCMs = destination.GetItem<TwinsSection>(10).GetItem<TwinsSection>(4);
            TwinsSection sourceSounds = source.GetItem<TwinsSection>(10).GetItem<TwinsSection>(6);
            TwinsSection destinationSounds = destination.GetItem<TwinsSection>(10).GetItem<TwinsSection>(6);
            TwinsSection sourceEngSounds = source.GetItem<TwinsSection>(10).GetItem<TwinsSection>(7);
            TwinsSection destinationEngSounds = destination.GetItem<TwinsSection>(10).GetItem<TwinsSection>(7);
            TwinsSection sourceFreSounds = source.GetItem<TwinsSection>(10).GetItem<TwinsSection>(8);
            TwinsSection destinationFreSounds = destination.GetItem<TwinsSection>(10).GetItem<TwinsSection>(8);
            TwinsSection sourceGerSounds = source.GetItem<TwinsSection>(10).GetItem<TwinsSection>(9);
            TwinsSection destinationGerSounds = destination.GetItem<TwinsSection>(10).GetItem<TwinsSection>(9);
            TwinsSection sourceSpaSounds = source.GetItem<TwinsSection>(10).GetItem<TwinsSection>(10);
            TwinsSection destinationSpaSounds = destination.GetItem<TwinsSection>(10).GetItem<TwinsSection>(10);
            TwinsSection sourceItaSounds = source.GetItem<TwinsSection>(10).GetItem<TwinsSection>(11);
            TwinsSection destinationItaSounds = destination.GetItem<TwinsSection>(10).GetItem<TwinsSection>(11);
            TwinsSection sourceJpnSounds = source.GetItem<TwinsSection>(10).GetItem<TwinsSection>(12);
            TwinsSection destinationJpnSounds = destination.GetItem<TwinsSection>(10).GetItem<TwinsSection>(12);
            foreach (ushort animId in cAnims)
            {
                if (destinationAnimations.HasItem(animId))
                {
                    continue;
                }
                Animation linkedAnimation = sourceAnimations.GetItem<Animation>(animId);
                destinationAnimations.AddItem(animId, linkedAnimation);
            }
            foreach (ushort cmID in cCM)
            {
                if (destinationCMs.HasItem(cmID))
                {
                    continue;
                }
                CodeModel linkedCM = sourceCMs.GetItem<CodeModel>(cmID);
                destinationCMs.AddItem(cmID, linkedCM);
            }
            foreach (ushort ogiID in cOGIs)
            {
                if (destinationOGIs.HasItem(ogiID))
                {
                    continue;
                }
                GraphicsInfo linkedOGI = sourceOGIs.GetItem<GraphicsInfo>(ogiID);
                destinationOGIs.AddItem(ogiID, linkedOGI);
                linkedOGI.FillPackageXbox(source, destination);
            }
            foreach (ushort scriptID in cScripts)
            {
                if (destinationScripts.HasItem(scriptID) || !sourceScripts.HasItem(scriptID))
                {
                    continue;
                }
                Script linkedScript = sourceScripts.GetItem<Script>(scriptID);
                destinationScripts.AddItem(scriptID, linkedScript);
            }
            using (MemoryStream soundStream = new MemoryStream())
            using (MemoryStream soundEngStream = new MemoryStream())
            using (MemoryStream soundFreStream = new MemoryStream())
            using (MemoryStream soundGerStream = new MemoryStream())
            using (MemoryStream soundSpaStream = new MemoryStream())
            using (MemoryStream soundItaStream = new MemoryStream())
            using (MemoryStream soundJpnStream = new MemoryStream())
            {
                BinaryWriter writerSound = new BinaryWriter(soundStream);
                writerSound.Write(destinationSounds.ExtraData);
                BinaryWriter writerEngSound = new BinaryWriter(soundEngStream);
                writerEngSound.Write(destinationEngSounds.ExtraData);
                BinaryWriter writerFreSound = new BinaryWriter(soundFreStream);
                writerFreSound.Write(destinationFreSounds.ExtraData);
                BinaryWriter writerGerSound = new BinaryWriter(soundGerStream);
                writerGerSound.Write(destinationGerSounds.ExtraData);
                BinaryWriter writerSpaSound = new BinaryWriter(soundSpaStream);
                writerSpaSound.Write(destinationSpaSounds.ExtraData);
                BinaryWriter writerItaSound = new BinaryWriter(soundItaStream);
                writerItaSound.Write(destinationItaSounds.ExtraData);
                BinaryWriter writerJpnSound = new BinaryWriter(soundJpnStream);
                writerJpnSound.Write(destinationJpnSounds.ExtraData);
                foreach (ushort soundID in cSounds)
                {
                    bool isSfx = sourceSounds.HasItem(soundID);
                    bool isVoJpn = sourceJpnSounds.HasItem(soundID);
                    bool isVoEng = sourceEngSounds.HasItem(soundID);
                    bool isVoFre = sourceFreSounds.HasItem(soundID);
                    bool isVoGer = sourceGerSounds.HasItem(soundID);
                    bool isVoSpa = sourceSpaSounds.HasItem(soundID);
                    bool isVoIta = sourceItaSounds.HasItem(soundID);
                    if (isSfx && !destinationSounds.HasItem(soundID))
                    {
                        SoundEffectX linkedSound = sourceSounds.GetItem<SoundEffectX>(soundID);
                        SoundEffectX newSound = new SoundEffectX();
                        SoundEffectX.CopySoundTo(linkedSound, sourceSounds.ExtraData, newSound, writerSound);
                        destinationSounds.AddItem(soundID, newSound);
                    }
                    if (isVoEng && !destinationEngSounds.HasItem(soundID))
                    {
                        SoundEffectX linkedSound = sourceEngSounds.GetItem<SoundEffectX>(soundID);
                        SoundEffectX newSound = new SoundEffectX();
                        SoundEffectX.CopySoundTo(linkedSound, sourceEngSounds.ExtraData, newSound, writerEngSound);
                        destinationEngSounds.AddItem(soundID, newSound);
                    }
                    if (isVoFre && !destinationFreSounds.HasItem(soundID))
                    {
                        SoundEffectX linkedSound = sourceFreSounds.GetItem<SoundEffectX>(soundID);
                        SoundEffectX newSound = new SoundEffectX();
                        SoundEffectX.CopySoundTo(linkedSound, sourceFreSounds.ExtraData, newSound, writerFreSound);
                        destinationFreSounds.AddItem(soundID, newSound);
                    }
                    if (isVoGer && !destinationGerSounds.HasItem(soundID))
                    {
                        SoundEffectX linkedSound = sourceGerSounds.GetItem<SoundEffectX>(soundID);
                        SoundEffectX newSound = new SoundEffectX();
                        SoundEffectX.CopySoundTo(linkedSound, sourceGerSounds.ExtraData, newSound, writerGerSound);
                        destinationGerSounds.AddItem(soundID, newSound);
                    }
                    if (isVoSpa && !destinationSpaSounds.HasItem(soundID))
                    {
                        SoundEffectX linkedSound = sourceSpaSounds.GetItem<SoundEffectX>(soundID);
                        SoundEffectX newSound = new SoundEffectX();
                        SoundEffectX.CopySoundTo(linkedSound, sourceSpaSounds.ExtraData, newSound, writerSpaSound);
                        destinationSpaSounds.AddItem(soundID, newSound);
                    }
                    if (isVoIta && !destinationItaSounds.HasItem(soundID))
                    {
                        SoundEffectX linkedSound = sourceItaSounds.GetItem<SoundEffectX>(soundID);
                        SoundEffectX newSound = new SoundEffectX();
                        SoundEffectX.CopySoundTo(linkedSound, sourceItaSounds.ExtraData, newSound, writerItaSound);
                        destinationItaSounds.AddItem(soundID, newSound);
                    }
                    if (isVoJpn && !destinationJpnSounds.HasItem(soundID))
                    {
                        SoundEffectX linkedSound = sourceJpnSounds.GetItem<SoundEffectX>(soundID);
                        SoundEffectX newSound = new SoundEffectX();
                        SoundEffectX.CopySoundTo(linkedSound, sourceJpnSounds.ExtraData, newSound, writerJpnSound);
                        destinationJpnSounds.AddItem(soundID, newSound);
                    }

                }
                destinationSounds.ExtraData = soundStream.ToArray();
                destinationEngSounds.ExtraData = soundEngStream.ToArray();
                destinationFreSounds.ExtraData = soundFreStream.ToArray();
                destinationGerSounds.ExtraData = soundGerStream.ToArray();
                destinationSpaSounds.ExtraData = soundSpaStream.ToArray();
                destinationItaSounds.ExtraData = soundItaStream.ToArray();
                destinationJpnSounds.ExtraData = soundJpnStream.ToArray();
            }
            foreach (ushort objectId in cObjects)
            {
                if (destinationObjects.HasItem(objectId) || ID == objectId)
                {
                    continue;
                }
                GameObject linkedObject = sourceObjects.GetItem<GameObject>(objectId);
                linkedObject.FillPackage(source, destination);

            }
            destinationObjects.AddItem(ID, this);
        }

        protected override int GetSize()
        {
            size = 0;

            size += 4;
            size += 8;
            size += 4;
            size += Name.Length;

            size += 4;
            size += UI32.Count * 4;

            size += 4;
            size += OGIs.Count * 2;

            size += 4;
            size += Anims.Count * 2;

            size += 4;
            size += Scripts.Count * 2;

            size += 4;
            size += Objects.Count * 2;

            size += 4;
            size += Sounds.Count * 2;

            if ((UnkBitfield & 0x20000000) != 0x0)
            {
                size += 4; // PHeader
                size += 4; // UnkInt

                size += 4;
                size += instFlagsList.Count * 4;

                size += 4;
                size += instFloatsList.Count * 4;

                size += 4;
                size += instIntegerList.Count * 4;
            }
            updateFlag();
            if ((UnkBitfield & 0x40000000) != 0x0)
            {
                size += 4; // Flag

                if ((flag & 0x00000001) != 0)
                {
                    size += 4;
                    size += cObjects.Count * 2;
                }
                if ((flag & 0x00000002) != 0)
                {
                    size += 4;
                    size += cOGIs.Count * 2;
                }
                if ((flag & 0x00000004) != 0)
                {
                    size += 4;
                    size += cAnims.Count * 2;
                }
                if ((flag & 0x00000008) != 0)
                {
                    size += 4;
                    size += cCM.Count * 2;
                }
                if ((flag & 0x00000010) != 0)
                {
                    size += 4;
                    size += cScripts.Count * 2;
                }
                if ((flag & 0x00000020) != 0)
                {
                    size += 4;
                    size += cUnk.Count * 2;
                }
                if ((flag & 0x00000040) != 0)
                {
                    size += 4;
                    size += cSounds.Count * 2;
                }
            }
            size += 4; // Amount of script commands
            if (scriptCommand != null)
            {
                size += scriptCommand.GetLength();
            }
            return size;
        }
        private void updateFlag()
        {
            flag = 0;
            if (cObjects.Count > 0)
            {
                flag |= 0x01;
            }

            if (cOGIs.Count > 0)
            {
                flag |= 0x02;
            }

            if (cAnims.Count > 0)
            {
                flag |= 0x04;
            }

            if (cCM.Count > 0)
            {
                flag |= 0x08;
            }

            if (cScripts.Count > 0)
            {
                flag |= 0x10;
            }

            if (cUnk.Count > 0)
            {
                flag |= 0x20;
            }

            if (cSounds.Count > 0)
            {
                flag |= 0x40;
            }
        }
    }
}
