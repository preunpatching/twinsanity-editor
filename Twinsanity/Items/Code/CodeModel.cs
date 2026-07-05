using System;
using System.Collections.Generic;
using System.IO;

namespace Twinsanity
{
    public class CodeModel : TwinsItem
    {
        public uint Header;
        public class AgentLabAdditions
        {
            public int scriptCommandsAmount;
            public Script.MainScript.ScriptCommand scriptCommand = null;

            public AgentLabAdditions(int ver)
            {
                scriptCommand = new Script.MainScript.ScriptCommand(ver);
            }
        }
        public List<AgentLabAdditions> agentLabAdditionsList = new List<AgentLabAdditions>();
        public List<ushort> scriptIds = new List<ushort>();
        public Script.MainScript.ScriptCommand scriptCommand = null;
        public int scriptGameVersion;
        private uint arraySize;
        public uint ArraySize
        {
            set
            {
                if (value >= 255)
                {
                    throw new Exception("Array can't be bigger than the max number in a byte");
                }
                Header = (Header & 0xFF00FFFF) | (value << 16);
                arraySize = value;
            }

            get => arraySize;
        }

        protected override int GetSize()
        {
            int totalSize = 4; // Header
            foreach (AgentLabAdditions agentLabAddition in agentLabAdditionsList)
            {
                totalSize += 4;
                if (agentLabAddition.scriptCommandsAmount > 0)
                {
                    totalSize += agentLabAddition.scriptCommand.GetLength();
                }
                totalSize += 2;
            }
            totalSize += scriptCommand.GetLength();
            return totalSize;
        }

        public override void Save(BinaryWriter writer)
        {
            _ = writer.BaseStream.Position;
            writer.Write(Header);
            for (int i = 0; i < arraySize; ++i)
            {
                writer.Write(agentLabAdditionsList[i].scriptCommandsAmount);
                if (agentLabAdditionsList[i].scriptCommandsAmount > 0)
                {
                    agentLabAdditionsList[i].scriptCommand.Write(writer);
                }
                writer.Write(scriptIds[i]);
            }
            scriptCommand.Write(writer);
        }

        public override void Load(BinaryReader reader, int size)
        {
            scriptGameVersion = ParentType == SectionType.CustomAgentX ? 1 : ParentType == SectionType.CustomAgentDemo ? 2 : 0;

            Header = reader.ReadUInt32();
            arraySize = (Header >> 16) & 0xFF;
            for (int i = 0; i < arraySize; ++i)
            {
                AgentLabAdditions agentLabAddition = new AgentLabAdditions(scriptGameVersion)
                {
                    scriptCommandsAmount = reader.ReadInt32()
                };
                if (agentLabAddition.scriptCommandsAmount > 0)
                {
                    agentLabAddition.scriptCommand = new Script.MainScript.ScriptCommand(reader, scriptGameVersion);
                }
                agentLabAdditionsList.Add(agentLabAddition);
                scriptIds.Add(reader.ReadUInt16());
            }
            scriptCommand = new Script.MainScript.ScriptCommand(reader, scriptGameVersion);
        }
    }
}
