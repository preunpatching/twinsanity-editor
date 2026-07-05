using System;
using System.Collections.Generic;
using Twinsanity;

namespace TwinsanityEditor.Controllers
{
    internal class CodeModelController : ItemController
    {
        public new CodeModel Data { get; set; }

        public CodeModelController(MainForm topform, CodeModel item) : base(topform, item)
        {
            Data = item;
        }

        protected override void GenText()
        {
            List<string> text = new List<string>
            {
                $"ID: {Data.ID}",
                $"Size: {Data.Size}",
                $"Header: 0x{Data.Header:X}",
                $"AgentLab additions: {Data.ArraySize}"
            };
            for (int i = 0; i < Data.ArraySize; ++i)
            {
                text.Add($"Addition {i} - Script ID: {(DefaultEnums.ScriptID)Data.scriptIds[i]}");
                CodeModel.AgentLabAdditions agentLabAddition = Data.agentLabAdditionsList[i];
                text.Add($"\tCommands amount: {agentLabAddition.scriptCommandsAmount}");
                if (agentLabAddition.scriptCommandsAmount > 0)
                {
                    Script.MainScript.ScriptCommand command = agentLabAddition.scriptCommand;
                    do
                    {
                        if (Enum.IsDefined(typeof(DefaultEnums.CommandID), command.VTableIndex))
                        {
                            text.Add($"\t{(DefaultEnums.CommandID)command.VTableIndex}: {command.VTableIndex}");
                        }
                        else
                        {
                            text.Add($"\t{command.VTableIndex}");
                        }
                        command = command.nextCommand;
                    } while (command != null);
                }
            }
            text.Add($"Unk AgentLab addition");
            Script.MainScript.ScriptCommand cmd = Data.scriptCommand;
            do
            {
                if (Enum.IsDefined(typeof(DefaultEnums.CommandID), cmd.VTableIndex))
                {
                    text.Add($"{(DefaultEnums.CommandID)cmd.VTableIndex}: {cmd.VTableIndex}");
                }
                else
                {
                    text.Add($"{cmd.VTableIndex}");
                }
                cmd = cmd.nextCommand;
            } while (cmd != null);
            TextPrev = text.ToArray();
        }

        protected override string GetName()
        {
            return $"CustomAgent [ID {Data.ID}]";
        }
    }
}
