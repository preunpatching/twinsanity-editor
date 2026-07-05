using System;
using System.Collections.Generic;
using System.IO;

namespace Twinsanity
{
    public class Script : TwinsItem
    {
        public Script()
        {
            script = new byte[0];
        }
        public class HeaderScript
        {
            public HeaderScript(int id)
            {
                _ = new UnkIntPairs();
                pairs = new List<UnkIntPairs>();
                UnkIntPairs pair = new UnkIntPairs
                {
                    mainScriptIndex = id + 1,
                    unkInt2 = 4294922800 // ME = ME
                };
                pairs.Add(pair);
            }
            public HeaderScript(BinaryReader reader)
            {
                uint unkIntPairs = reader.ReadUInt32();
                pairs = new List<UnkIntPairs>();
                for (int i = 0; i < unkIntPairs; i++)
                {
                    UnkIntPairs pair = new UnkIntPairs
                    {
                        mainScriptIndex = reader.ReadInt32(),
                        unkInt2 = reader.ReadUInt32()
                    };
                    pairs.Add(pair);
                }
            }

            public void Write(BinaryWriter writer)
            {
                writer.Write(pairs.Count);
                for (int i = 0; i < pairs.Count; i++)
                {
                    writer.Write(pairs[i].mainScriptIndex);
                    writer.Write(pairs[i].unkInt2);
                }
            }
            public int GetLength()
            {
                return 4 + (pairs.Count * 8);
            }
            public class UnkIntPairs
            {
                public int mainScriptIndex;
                public uint unkInt2;
                public override string ToString()
                {
                    return $"{AssignType} - Script ID: {mainScriptIndex - 1}";
                }

                public ushort ObjectID
                {
                    get => (ushort)(unkInt2 >> 0x10); set => unkInt2 = (unkInt2 & 0xffff) | ((uint)value << 0x10);
                }
                public AssignTypeID AssignType
                {
                    get => (AssignTypeID)(unkInt2 & 0xf); set => unkInt2 = (unkInt2 & 0xfffffff0) | ((uint)value & 0xf);
                }
                public AssignLocalityID AssignLocality
                {
                    get => (AssignLocalityID)((unkInt2 >> 4) & 0xf); set => unkInt2 = (unkInt2 & 0xffffff0f) | (((uint)value & 0xf) << 4);
                }
                public AssignStatusID AssignStatus
                {
                    get => (AssignStatusID)((unkInt2 >> 8) & 0xf); set => unkInt2 = (unkInt2 & 0xfffff0ff) | (((uint)value & 0xf) << 8);
                }
                public AssignPreferenceID AssignPreference
                {
                    get => (AssignPreferenceID)((unkInt2 >> 0xc) & 0xf); set => unkInt2 = (unkInt2 & 0xffff0fff) | (((uint)value & 0xf) << 0xc);
                }
            }
            public List<UnkIntPairs> pairs;


            public enum AssignTypeID
            {
                ME = 0,
                OBJECT_CHILD,
                LINKED_OBJECT,
                GLOBAL_AGENT,
                HUMAN_PLAYER,
                BACKGROUND_CHARACTER,
                ANYBODY,
                GENERATE_AGENT,
                ORIGINATOR,
            }
            public enum AssignLocalityID
            {
                NEARBY = 0,
                LOCAL,
                GLOBAL,
                ANYWHERE,
            }
            public enum AssignStatusID
            {
                IDLE = 0,
                BUSY,
                ANYSTATE,
            }
            public enum AssignPreferenceID
            {
                NEAREST = 0,
                FURTHEST,
                STRONGEST,
                WEAKEST,
                BEST_ALIGNED,
                ANYHOW,
            }
        }

        public class MainScript
        {
            public MainScript()
            {

            }
            public MainScript(BinaryReader reader, int ver)
            {
                scriptGameVersion = ver;
                int len = reader.ReadInt32();
                name = new string(reader.ReadChars(len));
                int StatesAmount = reader.ReadInt32();
                StartUnit = reader.ReadInt32();
                if (StatesAmount > 0)
                {
                    scriptState1 = new ScriptState(reader, ver);
                    ScriptState ptr = scriptState1;
                    while (null != ptr)
                    {
                        if ((ptr.bitfield & 0x1F) != 0)
                        {
                            ptr.scriptStateBody = new ScriptStateBody(reader, ver);
                        }
                        ptr = ptr.nextState;
                    }
                }
            }
            public void Write(BinaryWriter writer)
            {
                writer.Write(name.Length);
                writer.Write(name.ToCharArray());
                writer.Write(GetStatesAmount());
                writer.Write(StartUnit);
                if (scriptState1 != null)
                {
                    scriptState1.Write(writer);
                    ScriptState ptr = scriptState1;
                    while (ptr != null)
                    {
                        ptr.scriptStateBody?.Write(writer);
                        ptr = ptr.nextState;
                    }
                }
            }
            public int GetLength()
            {
                int headerSize = 4 + name.Length + 4 + 4;
                int linkedSize = (scriptState1 != null) ? scriptState1.GetLength() : 0;
                int scriptStateBodySize = 0;
                ScriptState ptr = scriptState1;
                while (ptr != null)
                {
                    if (null != ptr.scriptStateBody)
                    {
                        scriptStateBodySize += ptr.scriptStateBody.GetLength();
                    }
                    ptr = ptr.nextState;
                }
                return headerSize + linkedSize + scriptStateBodySize;
            }
            public string name { get; set; }
            public int StartUnit { get; set; }
            public ScriptState scriptState1 { get; set; }
            public int scriptGameVersion { get; set; }

            public class SupportType1
            {
                public SupportType1()
                {
                    bytes = new List<byte>();
                    floats = new List<float>();
                    unkByte1 = 0;
                    unkByte2 = 0;
                    unkUShort1 = 6;
                    unkInt1 = 0x200000; // HasValidData true
                }
                public SupportType1(BinaryReader reader)
                {
                    bytes = new List<byte>();
                    floats = new List<float>();
                    _unkByte1 = reader.ReadByte();
                    _unkByte2 = reader.ReadByte();
                    unkUShort1 = reader.ReadUInt16();
                    unkInt1 = reader.ReadUInt32();
                    long BeforeFloats, AfterBytes;
                    BeforeFloats = reader.BaseStream.Position;
                    for (int i = 0; i < unkByte2; ++i)
                    {
                        floats.Add(reader.ReadSingle());
                    }
                    for (int i = 0; i < unkByte1; ++i)
                    {
                        bytes.Add(reader.ReadByte());
                    }
                    AfterBytes = reader.BaseStream.Position;
                    if (bytes.Count > 0 && bytes[0] < 128) // int SELECTOR/SYNC_INDEX
                    {
                        reader.BaseStream.Position = BeforeFloats + (4 * bytes[0]);
                        floats[bytes[0]] = reader.ReadUInt32();
                        reader.BaseStream.Position = AfterBytes;
                    }
                    if (bytes.Count > 1 && bytes[1] < 128) // int KEY_INDEX/FOCUS_DATA
                    {
                        reader.BaseStream.Position = BeforeFloats + (4 * bytes[1]);
                        floats[bytes[1]] = reader.ReadUInt32();
                        reader.BaseStream.Position = AfterBytes;
                    }
                    // bytes[21] ptr SYNC_UNIT may also need this?
                    if (bytes.Count > 22 && bytes[22] < 128) // int JOINT_INDEX
                    {
                        reader.BaseStream.Position = BeforeFloats + (4 * bytes[22]);
                        floats[bytes[22]] = reader.ReadUInt32();
                        reader.BaseStream.Position = AfterBytes;
                    }
                }
                public void Write(BinaryWriter writer)
                {
                    writer.Write(unkByte1);
                    writer.Write(unkByte2);
                    writer.Write(unkUShort1);
                    writer.Write(unkInt1);
                    for (int i = 0; i < floats.Count; i++)
                    {
                        bool found = false;
                        for (int b = 0; b < bytes.Count; b++)
                        {
                            if (bytes[b] == i)
                            {
                                found = true;
                                if (b == 0 || b == 1 || b == 22)
                                {
                                    writer.Write((uint)floats[i]);
                                }
                                else
                                {
                                    writer.Write(floats[i]);
                                }
                            }
                        }
                        if (!found)
                        {
                            writer.Write(floats[i]);
                        }
                    }
                    /*
                    foreach (Single f in floats)
                    {
                        writer.Write(f);
                    }
                    */
                    foreach (byte b in bytes)
                    {
                        writer.Write(b);
                    }
                }
                public int GetLength()
                {
                    return 8 + (floats.Count * 4) + bytes.Count;
                }
                private byte _unkByte1;
                public byte unkByte1
                {
                    get => _unkByte1;
                    set
                    {
                        _unkByte1 = value;
                        while (_unkByte1 > bytes.Count)
                        {
                            bytes.Add(0);
                        }
                        while (_unkByte1 < bytes.Count)
                        {
                            bytes.RemoveAt(bytes.Count - 1);
                        }
                    }
                }
                private byte _unkByte2;
                public byte unkByte2
                {
                    get => _unkByte2;
                    set
                    {
                        _unkByte2 = value;
                        while (_unkByte2 > floats.Count)
                        {
                            floats.Add(0);
                        }
                        while (_unkByte2 < floats.Count)
                        {
                            floats.RemoveAt(floats.Count - 1);
                        }
                    }
                }
                public ushort unkUShort1 { get; set; } // Version, always 6
                private uint unkInt1 { get; set; }
                public List<byte> bytes { get; set; }
                public List<float> floats { get; set; }


                public SpaceType Space
                {
                    get => (SpaceType)(unkInt1 & 7); set => unkInt1 = (unkInt1 & 0xfffffff8) | ((uint)value & 7);
                }
                public MotionType Motion
                {
                    get => (MotionType)((unkInt1 >> 3) & 0xf); set => unkInt1 = (unkInt1 & 0xffffff87) | (((uint)value & 0xf) << 3);
                }
                public ContinuousRotate ContRotate
                {
                    get => (ContinuousRotate)((unkInt1 >> 7) & 0xf); set => unkInt1 = (unkInt1 & 0xfffff87f) | (((uint)value & 0xf) << 7);
                }
                public AccelFunction AccelFunc
                {
                    get => (AccelFunction)((unkInt1 >> 0xb) & 3); set => unkInt1 = (unkInt1 & 0xffffe7ff) | (((uint)value & 3) << 0xb);
                }
                public bool Translates
                {
                    get => ((byte)((unkInt1 >> 0xd) & 1)) != 0; set => unkInt1 = (unkInt1 & 0xffffdfff) | ((uint)(value ? 1 : 0) << 0xd);
                }
                public bool Rotates
                {
                    get => ((byte)((unkInt1 >> 0xe) & 1)) != 0; set => unkInt1 = (unkInt1 & 0xffffbfff) | ((uint)(value ? 1 : 0) << 0xe);
                }
                public bool TranslationContinues
                {
                    get => ((byte)((unkInt1 >> 0xf) & 1)) != 0;
                    set
                    {
                        //
                    }
                }
                public bool TracksDestination
                {
                    get => ((byte)((unkInt1 >> 0x10) & 1)) != 0; set => unkInt1 = (unkInt1 & 0xfffeffff) | ((uint)(value ? 1 : 0) << 0x10);
                }
                public bool InterpolatesAngles
                {
                    get => ((byte)((unkInt1 >> 0x11) & 1)) != 0; set => unkInt1 = (unkInt1 & 0xfffdffff) | ((uint)(value ? 1 : 0) << 0x11);
                }
                public bool YawFaces
                {
                    get => ((byte)((unkInt1 >> 0x12) & 1)) != 0; set => unkInt1 = (unkInt1 & 0xfffbffff) | ((uint)(value ? 1 : 0) << 0x12);
                }
                public bool PitchFaces
                {
                    get => ((byte)((unkInt1 >> 0x13) & 1)) != 0; set => unkInt1 = (unkInt1 & 0xfff7ffff) | ((uint)(value ? 1 : 0) << 0x13);
                }
                public bool OrientsPredicts
                {
                    get => ((byte)((unkInt1 >> 0x14) & 1)) != 0; set => unkInt1 = (unkInt1 & 0xffefffff) | ((uint)(value ? 1 : 0) << 0x14);
                }
                public bool HasValidData // should always be true
                {
                    get => ((byte)((unkInt1 >> 0x15) & 1)) != 0; set => unkInt1 = (unkInt1 & 0xffdfffff) | ((uint)(value ? 1 : 0) << 0x15);
                }
                public bool KeyIsLocal
                {
                    get => ((byte)((unkInt1 >> 0x16) & 1)) != 0; set => unkInt1 = (unkInt1 & 0xffbfffff) | ((uint)(value ? 1 : 0) << 0x16);
                }
                public bool UsesRotator
                {
                    get => ((byte)((unkInt1 >> 0x17) & 1)) != 0; set => unkInt1 = (unkInt1 & 0xff7fffff) | ((uint)(value ? 1 : 0) << 0x17);
                }
                public bool UsesInterpolator
                {
                    get => ((byte)((unkInt1 >> 0x18) & 1)) != 0; set => unkInt1 = (unkInt1 & 0xfeffffff) | ((uint)(value ? 1 : 0) << 0x18);
                }
                public bool UsesPhysics
                {
                    get => ((byte)((unkInt1 >> 0x19) & 1)) != 0; set => unkInt1 = (unkInt1 & 0xfdffffff) | ((uint)(value ? 1 : 0) << 0x19);
                }
                public bool ContRotatesInWorldSpace
                {
                    get => ((byte)((unkInt1 >> 0x1a) & 1)) != 0; set => unkInt1 = (unkInt1 & 0xfbffffff) | ((uint)(value ? 1 : 0) << 0x1a);
                }
                public NaturalAxes Axes
                {
                    get => (NaturalAxes)((unkInt1 >> 0x1b) & 7); set => unkInt1 = (unkInt1 & 0xc7ffffff) | (((uint)value & 0x7) << 0x1b);
                }
                public bool Stalls
                {
                    get => ((byte)((unkInt1 >> 0x1f) & 1)) != 0; set => unkInt1 = (unkInt1 & 0x7fffffff) | ((uint)(value ? 1 : 0) << 0x1f);
                }

                public enum SpaceType
                {
                    WORLD_SPACE = 0,
                    /*
                    INITIAL_SPACE,
                    CURRENT_SPACE,
                    INITIAL_SPACE2,
                    INITIAL_POS,
                    CURRENT_POS,
                    STORED_SPACE,
                    */
                    INITIAL_SPACE,
                    CURRENT_SPACE,
                    TARGET_SPACE,
                    PARENT_SPACE, // or CHASE_SPACE
                    INITIAL_POS,
                    CURRENT_POS,
                    STORED_SPACE,
                }
                public enum MotionType
                {
                    NO_MOTION = 0,
                    CONSTANT_VEL,
                    ACCELERATED,
                    SPRING,
                    PROJECTILE,
                    LINEAR_INTERP,
                    SMOOTH_PATH,
                    FACE_DEST_ONLY,
                    DRIVE,
                    GROUND_CHASE,
                    AIR_CHASE,
                }
                public enum ContinuousRotate
                {
                    NO_CONT_ROTATION = 0,
                    NUM_FULL_ROTS,
                    RADS_PER_SECOND, // Or degrees?
                    NATURAL_ROLL,
                }
                public enum NaturalAxes
                {
                    NO_NATURAL = 0,
                    X_NATURAL,
                    Y_NATURAL,
                    Z_NATURAL,
                    ALL_NATURAL,
                }
                public enum AccelFunction
                {
                    NO_ACCEL = 0,
                    CONSTANT_ACCEL,
                    SMOOTH_CURVE,
                }
            }
            public class ScriptStateBody
            {
                public ScriptStateBody(int ver)
                {
                    scriptGameVersion = ver;
                    bitfield = 0;
                    scriptStateListIndex = 0;
                    condition = null;
                    command = null;
                    nextScriptStateBody = null;
                }
                public ScriptStateBody(BinaryReader reader, int ver)
                {
                    scriptGameVersion = ver;
                    bitfield = reader.ReadInt32();
                    if ((bitfield & 0x400) != 0)
                    {
                        scriptStateListIndex = reader.ReadInt32();
                    }
                    if ((bitfield & 0x200) != 0)
                    {
                        condition = new ScriptCondition(reader);
                    }
                    if ((bitfield & 0xFF) != 0)
                    {
                        command = new ScriptCommand(reader, scriptGameVersion);
                    }
                    if ((bitfield & 0x800) != 0)
                    {
                        nextScriptStateBody = new ScriptStateBody(reader, ver);
                    }
                }
                public void Write(BinaryWriter writer)
                {
                    writer.Write(bitfield);
                    if ((bitfield & 0x400) != 0)
                    {
                        writer.Write(scriptStateListIndex);
                    }
                    if ((bitfield & 0x200) != 0)
                    {
                        condition.Write(writer);
                    }
                    if ((bitfield & 0xFF) != 0)
                    {
                        command.Write(writer);
                    }
                    if ((bitfield & 0x800) != 0)
                    {
                        nextScriptStateBody.Write(writer);
                    }
                }
                public int GetLength()
                {
                    return 4 + (((bitfield & 0x400) != 0) ? 4 : 0)
                        + (((bitfield & 0x200) != 0) ? condition.GetLength() : 0)
                        + (((bitfield & 0xFF) != 0) ? command.GetLength() : 0)
                        + (((bitfield & 0x800) != 0) ? nextScriptStateBody.GetLength() : 0);
                }
                public int bitfield { get; set; }
                public int scriptStateListIndex { get; set; }
                public ScriptCondition condition { get; set; }
                public ScriptCommand command { get; set; }
                public ScriptStateBody nextScriptStateBody { get; set; }
                public int scriptGameVersion { get; set; }
                public bool isBitFieldValid()
                {
                    if (((bitfield & 0x200) == 0) && (condition != null))
                    {
                        return false;
                    }
                    if (((bitfield & 0x200) != 0) && (condition == null))
                    {
                        return false;
                    }
                    return ((bitfield & 0xFF) != 0 || command == null) && ((bitfield & 0xFF) == 0 || command != null) && ((bitfield & 0x800) != 0 || nextScriptStateBody == null) && ((bitfield & 0x800) == 0 || nextScriptStateBody != null);
                }
                public bool IsEnabled
                {
                    get => (bitfield & 0x400) != 0; set => bitfield = value ? (short)(bitfield | 0x400) : (short)(bitfield & ~0x400);
                }
                public byte commandCount
                {
                    get => (byte)(bitfield & 0xFF); set => bitfield = (int)(bitfield & 0xFFFFFF00) | (value & 0xFF);
                }
                public bool CreateCondition()
                {
                    if (condition == null)
                    {
                        condition = new ScriptCondition();
                        bitfield = (short)(bitfield | 0x200);
                        return true;
                    }
                    else
                    {
                        return false;
                    }

                }
                public bool DeleteCondition()
                {
                    if (condition != null)
                    {
                        condition = null;
                        bitfield = (short)(bitfield & ~0x200);
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                public bool AddCommand(int position)
                {
                    if (position > commandCount || position < 0)
                    {
                        return false;
                    }
                    if (commandCount == 0)
                    {
                        command = new ScriptCommand(scriptGameVersion);
                    }
                    else if (position == commandCount)
                    {
                        ScriptCommand ptr = command;
                        while (ptr.nextCommand != null)
                        {
                            ptr = ptr.nextCommand;
                        }
                        ptr.internalIndex |= 0x1000000;
                        ptr.nextCommand = new ScriptCommand(scriptGameVersion);
                    }
                    else
                    {
                        int pos = 0;
                        ScriptCommand prevPtr = null;
                        ScriptCommand ptr = command;
                        ScriptCommand newCommand = new ScriptCommand(scriptGameVersion);
                        while (pos < position)
                        {
                            prevPtr = ptr;
                            ptr = ptr.nextCommand;
                            ++pos;
                        }
                        if (prevPtr != null)
                        {
                            prevPtr.nextCommand = newCommand;
                            prevPtr.nextCommand.nextCommand = ptr;
                        }
                        else
                        {
                            newCommand.nextCommand = command;
                            command = newCommand;
                        }

                        if (newCommand.nextCommand != null)
                        {
                            newCommand.internalIndex |= 0x1000000;
                        }
                    }
                    ++commandCount;
                    return true;
                }
                public bool DeleteCommand(int position)
                {
                    if (position >= commandCount || position < 0)
                    {
                        return false;
                    }
                    if (position == 0)
                    {
                        command = command.nextCommand;
                    }
                    else
                    {
                        int pos = 0;
                        ScriptCommand prevPtr = null;
                        ScriptCommand ptr = command;
                        while (pos < position)
                        {
                            prevPtr = ptr;
                            ptr = ptr.nextCommand;
                            ++pos;
                        }
                        prevPtr.nextCommand = ptr.nextCommand;
                        if (prevPtr.nextCommand == null)
                        {
                            prevPtr.internalIndex &= ~0x1000000;
                        }
                    }
                    --commandCount;
                    return true;
                }
            }
            public class ScriptCondition
            {
                public ScriptCondition()
                {
                    unkInt1 = 0;
                    Interval = 0.0f;
                    Threshold = 0.5f;
                    ThresholdInverse = 2.0f;
                }
                public ScriptCondition(BinaryReader reader)
                {
                    unkInt1 = reader.ReadInt32();
                    vTableAddress = 0x0;
                    Interval = reader.ReadSingle();
                    Threshold = reader.ReadSingle();
                    ThresholdInverse = reader.ReadSingle();
                }
                public void Write(BinaryWriter writer)
                {
                    writer.Write(unkInt1);
                    writer.Write(Interval);
                    writer.Write(Threshold);
                    writer.Write(ThresholdInverse);
                }
                public int GetLength()
                {
                    return 16;
                }
                public int unkInt1 { get; set; }
                public int vTableAddress { get; set; }
                public float Interval { get; set; }
                public float Threshold { get; set; }
                public float ThresholdInverse { get; set; }
                public ushort VTableIndex
                {
                    get => (ushort)(unkInt1 & 0xffff); set => unkInt1 = (int)(unkInt1 & 0xffff0000) | (value & 0xffff);
                }
                public ushort Parameter
                {
                    get => (ushort)((unkInt1 & 0xffff0000) >> 17); set => unkInt1 = (unkInt1 & 0x1ffff) | (int)((value << 17) & 0xfffe0000);
                }
                public bool NotGate
                {
                    get => (unkInt1 & 0x10000) != 0; set => unkInt1 = (int)(unkInt1 & 0xfffeffff) | (Convert.ToInt32(value) << 16);
                }
            }
            public class ScriptCommand
            {
                public ScriptCommand(int ver)
                {
                    scriptGameVersion = ver;
                    internalIndex = 0;
                    arguments = new List<uint>();
                }
                public ScriptCommand(BinaryReader reader, int ver)
                {
                    scriptGameVersion = ver;
                    internalIndex = reader.ReadInt32();
                    arguments = new List<uint>();
                    length = GetCommandSize(internalIndex & 0x0000FFFF, ver);
                    if (length - 0xC > 0x0)
                    {
                        int sz = (length - 0xC) / 4;
                        for (int i = 0; i < sz; ++i)
                        {
                            arguments.Add(reader.ReadUInt32());
                        }
                    }
                    if ((internalIndex & 0x1000000) != 0)
                    {
                        nextCommand = new ScriptCommand(reader, scriptGameVersion);
                        int flag = (length != 0) ? 1 : 0;
                        unkUInt = (uint)(((ulong)unkUInt & 0xFEFFFFFF) | ((ulong)flag << 0x18));
                    }
                    else
                    {
                        unkUInt &= 0xFEFFFFFF;
                    }
                    if (!isValidBits())
                    {
                        Console.WriteLine("Command " + (internalIndex & 0xffff) + ": Invalid bits, check command size mapper");
                    }
                }
                public void Write(BinaryWriter writer)
                {
                    writer.Write(internalIndex);
                    if (null != arguments)
                    {
                        foreach (uint arg in arguments)
                        {
                            writer.Write(arg);
                        }
                    }
                    if ((internalIndex & 0x1000000) != 0)
                    {
                        nextCommand.Write(writer);
                    }
                }
                public int GetLength()
                {
                    return 4 + ((arguments != null) ? arguments.Count * 4 : 0) + (((internalIndex & 0x1000000) != 0) ? nextCommand.GetLength() : 0);
                }
                public uint unkUInt { get; set; }
                public int vTableAddress;
                private void UpdateArguments()
                {
                    int sz = GetExpectedSize() / 4;
                    while (sz > arguments.Count)
                    {
                        arguments.Add(0);
                    }
                    while (sz < arguments.Count)
                    {
                        arguments.RemoveAt(arguments.Count - 1);
                    }
                }
                public int internalIndex { get; set; }
                public int length { get; set; }
                public List<uint> arguments { get; set; }
                public ScriptCommand nextCommand { get; set; }
                public int scriptGameVersion { get; set; }
                public static int ScriptCommandTableSize => CommandSizeMapper_PS2.Length;

                public ushort VTableIndex
                {
                    get => (ushort)(internalIndex & 0xffff);
                    set
                    {
                        internalIndex = (int)(internalIndex & 0xffff0000) | (value & 0xffff);
                        UpdateArguments();
                    }
                }
                public ushort UnkShort
                {
                    get => (ushort)((internalIndex & 0xffff0000) >> 16); set => internalIndex = (internalIndex & 0xffff) | (int)((value << 16) & 0xffff0000);
                }

                public bool isValidBits()
                {
                    if (((internalIndex & 0x1000000) != 0) && nextCommand == null)
                    {
                        return false;
                    }
                    return ((internalIndex & 0x1000000) != 0 || nextCommand == null) && (arguments != null || GetCommandSize(internalIndex & 0xffff, scriptGameVersion) <= 0) && (arguments == null || GetCommandSize(internalIndex & 0xffff, scriptGameVersion) != 0) && (arguments == null || arguments.Count * 4 == GetExpectedSize());
                }
                public int GetExpectedSize()
                {
                    int sz = GetCommandSize(internalIndex & 0xffff, scriptGameVersion);
                    return sz - 0xC > 0 ? sz - 0xC : 0;
                }
                public static int GetCommandSize(int index, int ver)
                {
                    if (index < 0 || index >= CommandSizeMapper_PS2.Length)
                    {
                        return 0;
                    }
                    switch (ver)
                    {
                        default:
                        case 0:
                            return CommandSizeMapper_PS2[index];
                        case 1:
                            return CommandSizeMapper_Xbox[index];
                        case 2:
                            return CommandSizeMapper_Demo[index];
                    }
                }
                private static readonly int[] CommandSizeMapper_PS2 = {
                        0x0C, 0x80, 0x0C, 0x20, 0x10, 0x0C, 0x00, 0x0C, 0x30, 0x24, 0x30, 0x48, 0x94, 0x0C, 0x10, 0x10, 0x10, 0x10, 0x10, 0x10, 0x00, 0x00, 0x00, 0x10, 0x10, 0x00, 0x00, 0x10, 0x10, 0x20, 0x00, 0x10,
                        0x00, 0x10, 0x10, 0x0C, 0x0C, 0x00, 0x00, 0x0C, 0x14, 0x00, 0x10, 0x00, 0x50, 0x10, 0x00, 0x30, 0x30, 0x30, 0x0C, 0x20, 0x0C, 0x0C, 0x1C, 0x40, 0x14, 0x10, 0x00, 0x10, 0x60, 0x0C, 0x20, 0x0C,
                        0x30, 0x1C, 0x0C, 0x10, 0x14, 0x18, 0x00, 0x0C, 0x50, 0x00, 0x10, 0x10, 0x30, 0x0C, 0x14, 0x10, 0x50, 0x0C, 0x94, 0x94, 0x0C, 0x10, 0x28, 0x1C, 0x20, 0x10, 0x10, 0x10, 0x10, 0x10, 0x30, 0x10,
                        0xC0, 0x0C, 0x0C, 0x10, 0x10, 0x10, 0x10, 0x10, 0x10, 0x20, 0x10, 0x00, 0x60, 0x20, 0x0C, 0x0C, 0x30, 0x1C, 0x0C, 0x0C, 0x0C, 0x14, 0x14, 0x0C, 0x0C, 0x14, 0x10, 0x0C, 0x10, 0x20, 0x0C, 0x10,
                        0x0C, 0x0C, 0x1C, 0x0C, 0x10, 0x0C, 0x0C, 0x0C, 0x14, 0x14, 0x14, 0x10, 0x10, 0x10, 0x10, 0x0C, 0x0C, 0x10, 0x10, 0x0C, 0x1C, 0x14, 0x18, 0x0C, 0x1C, 0x20, 0x10, 0x10, 0x10, 0x10, 0x98, 0x0C,
                        0x0C, 0x0C, 0x14, 0x10, 0x18, 0x40, 0x10, 0x10, 0x30, 0x14, 0x18, 0x14, 0x10, 0x10, 0x0C, 0x0C, 0x14, 0x30, 0x30, 0x30, 0x14, 0x0C, 0x0C, 0x10, 0x10, 0x14, 0x0C, 0x1C, 0x24, 0x20, 0x24, 0x10,
                        0x10, 0x30, 0x14, 0x0C, 0x0C, 0x30, 0x18, 0x20, 0x18, 0x18, 0x0C, 0x10, 0x2C, 0x14, 0x0C, 0x0C, 0x10, 0x10, 0x10, 0x0C, 0x0C, 0x0C, 0x10, 0x18, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                        0x14, 0x10, 0x40, 0x10, 0x10, 0x0C, 0x14, 0x0C, 0x0C, 0x14, 0x0C, 0x3C, 0x18, 0x40, 0x2C, 0x10, 0x10, 0x10, 0x20, 0x0C, 0x10, 0x10, 0x14, 0x0C, 0x10, 0x10, 0x0C, 0x1C, 0x24, 0x80, 0x0C, 0x24,
                        0x30, 0x48, 0x40, 0x00, 0x30, 0x50, 0x10, 0x0C, 0x0C, 0x10, 0x0C, 0x18, 0x0C, 0x40, 0x10, 0x18, 0x0C, 0x10, 0x0C, 0x40, 0x40, 0x40, 0x0C, 0x0C, 0x00, 0x10, 0x10, 0x10, 0x00, 0x18, 0x54, 0x14,
                        0x10, 0x1C, 0x10, 0x10, 0x20, 0x10, 0x4C, 0x54, 0x0C, 0x10, 0x10, 0x10, 0x0C, 0x10, 0x10, 0x3C, 0x10, 0x10, 0x14, 0x18, 0x18, 0x10, 0x0C, 0x0C, 0x0C, 0x0C, 0x24, 0x28, 0x0C, 0x10, 0x0C, 0x0C,
                        0x0C, 0x0C, 0x0C, 0x0C, 0x0C, 0x0C, 0x10, 0x10, 0x10, 0x18, 0x0C, 0x10, 0x10, 0x0C, 0x10, 0x0C, 0x0C, 0x14, 0x0C, 0x0C, 0x14, 0x18, 0x10, 0x10, 0x10, 0x18, 0x14, 0x00, 0x10, 0x0C, 0x18, 0x10,
                        0x0C, 0x24, 0x24, 0x24, 0x24, 0x10, 0x00, 0x14, 0x10, 0x0C, 0x10, 0x10, 0x0C, 0x24, 0x0C, 0x28, 0x0C, 0x24, 0x28, 0x10, 0x10, 0x68, 0x0C, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, };

                private static readonly int[] CommandSizeMapper_Xbox = {
                        0x0C, 0x80, 0x0C, 0x20, 0x10, 0x0C, 0x00, 0x0C, 0x40, 0x24, 0x30, 0x48, 0x94, 0x0C, 0x10, 0x10, 0x10, 0x10, 0x10, 0x10, 0x00, 0x00, 0x00, 0x10, 0x10, 0x00, 0x00, 0x10, 0x10, 0x20, 0x00, 0x10,
                        0x00, 0x10, 0x10, 0x0C, 0x0C, 0x00, 0x00, 0x0C, 0x14, 0x00, 0x10, 0x00, 0x50, 0x10, 0x00, 0x30, 0x40, 0x30, 0x0C, 0x20, 0x0C, 0x0C, 0x20, 0x40, 0x14, 0x10, 0x00, 0x10, 0x60, 0x0C, 0x20, 0x0C,
                        0x30, 0x20, 0x0C, 0x10, 0x14, 0x18, 0x30, 0x0C, 0x50, 0x00, 0x10, 0x10, 0x30, 0x0C, 0x14, 0x10, 0x50, 0x0C, 0x94, 0x94, 0x0C, 0x10, 0x28, 0x1C, 0x20, 0x10, 0x10, 0x10, 0x10, 0x10, 0x30, 0x10,
                        0xC0, 0x0C, 0x0C, 0x10, 0x10, 0x10, 0x10, 0x10, 0x10, 0x20, 0x10, 0x00, 0x60, 0x20, 0x0C, 0x0C, 0x40, 0x1C, 0x0C, 0x0C, 0x0C, 0x14, 0x14, 0x0C, 0x0C, 0x14, 0x10, 0x0C, 0x10, 0x20, 0x0C, 0x10,
                        0x0C, 0x0C, 0x1C, 0x0C, 0x10, 0x0C, 0x0C, 0x0C, 0x14, 0x14, 0x14, 0x10, 0x10, 0x10, 0x10, 0x0C, 0x0C, 0x10, 0x10, 0x0C, 0x1C, 0x14, 0x18, 0x0C, 0x1C, 0x20, 0x10, 0x10, 0x10, 0x10, 0x98, 0x0C,
                        0x0C, 0x0C, 0x14, 0x14, 0x18, 0x40, 0x10, 0x10, 0x30, 0x14, 0x18, 0x14, 0x14, 0x10, 0x0C, 0x0C, 0x14, 0x30, 0x30, 0x30, 0x14, 0x0C, 0x0C, 0x10, 0x14, 0x14, 0x0C, 0x1C, 0x24, 0x20, 0x24, 0x10,
                        0x10, 0x30, 0x14, 0x0C, 0x0C, 0x30, 0x18, 0x20, 0x18, 0x18, 0x0C, 0x10, 0x2C, 0x14, 0x0C, 0x0C, 0x10, 0x10, 0x10, 0x0C, 0x0C, 0x0C, 0x10, 0x18, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                        0x14, 0x10, 0x40, 0x10, 0x10, 0x0C, 0x14, 0x0C, 0x0C, 0x14, 0x0C, 0x3C, 0x1C, 0x40, 0x2C, 0x10, 0x10, 0x10, 0x20, 0x0C, 0x10, 0x10, 0x14, 0x0C, 0x10, 0x10, 0x0C, 0x1C, 0x24, 0x80, 0x0C, 0x24,
                        0x30, 0x48, 0x40, 0x00, 0x30, 0x50, 0x10, 0x0C, 0x0C, 0x10, 0x0C, 0x18, 0x0C, 0x40, 0x10, 0x18, 0x0C, 0x10, 0x0C, 0x40, 0x40, 0x40, 0x0C, 0x0C, 0x00, 0x10, 0x10, 0x10, 0x94, 0x18, 0x54, 0x14,
                        0x10, 0x1C, 0x10, 0x10, 0x20, 0x10, 0x4C, 0x54, 0x0C, 0x10, 0x10, 0x10, 0x0C, 0x10, 0x10, 0x3C, 0x10, 0x10, 0x14, 0x18, 0x18, 0x10, 0x0C, 0x0C, 0x0C, 0x0C, 0x24, 0x28, 0x0C, 0x10, 0x0C, 0x0C,
                        0x0C, 0x0C, 0x0C, 0x0C, 0x0C, 0x0C, 0x10, 0x10, 0x10, 0x18, 0x0C, 0x10, 0x10, 0x0C, 0x10, 0x0C, 0x0C, 0x14, 0x0C, 0x0C, 0x14, 0x18, 0x10, 0x10, 0x10, 0x18, 0x14, 0x00, 0x10, 0x0C, 0x18, 0x10,
                        0x0C, 0x24, 0x24, 0x24, 0x24, 0x10, 0x10, 0x14, 0x10, 0x0C, 0x10, 0x10, 0x0C, 0x24, 0x0C, 0x28, 0x0C, 0x24, 0x28, 0x10, 0x10, 0x68, 0x0C, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, };

                private static readonly int[] CommandSizeMapper_Demo = {
                        0x0C, 0x80, 0x0C, 0x20, 0x10, 0x0C, 0x00, 0x0C, 0x30, 0x24, 0x30, 0x44, 0x94, 0x0C, 0x10, 0x10, 0x10, 0x10, 0x10, 0x10, 0x00, 0x00, 0x00, 0x10, 0x10, 0x00, 0x00, 0x10, 0x10, 0x20, 0x00, 0x10,
                        0x00, 0x10, 0x10, 0x0C, 0x0C, 0x00, 0x00, 0x0C, 0x14, 0x00, 0x10, 0x00, 0x50, 0x10, 0x00, 0x30, 0x30, 0x30, 0x0C, 0x20, 0x0C, 0x0C, 0x1C, 0x40, 0x14, 0x10, 0x00, 0x10, 0x50, 0x0C, 0x20, 0x0C,
                        0x30, 0x1C, 0x0C, 0x10, 0x14, 0x14, 0x00, 0x0C, 0x50, 0x00, 0x10, 0x10, 0x30, 0x0C, 0x14, 0x10, 0x50, 0x0C, 0x94, 0x94, 0x0C, 0x10, 0x28, 0x1C, 0x18, 0x10, 0x10, 0x10, 0x10, 0x10, 0x30, 0x10,
                        0xC0, 0x0C, 0x0C, 0x10, 0x10, 0x10, 0x10, 0x10, 0x10, 0x20, 0x10, 0x00, 0x50, 0x20, 0x0C, 0x0C, 0x30, 0x1C, 0x0C, 0x0C, 0x0C, 0x14, 0x14, 0x0C, 0x0C, 0x14, 0x10, 0x0C, 0x10, 0x20, 0x0C, 0x10,
                        0x0C, 0x0C, 0x1C, 0x0C, 0x10, 0x0C, 0x0C, 0x0C, 0x14, 0x14, 0x14, 0x10, 0x10, 0x10, 0x10, 0x0C, 0x0C, 0x10, 0x10, 0x0C, 0x1C, 0x14, 0x18, 0x0C, 0x1C, 0x20, 0x10, 0x10, 0x10, 0x10, 0x10, 0x0C,
                        0x0C, 0x0C, 0x14, 0x10, 0x18, 0x40, 0x10, 0x10, 0x30, 0x14, 0x18, 0x14, 0x10, 0x10, 0x0C, 0x0C, 0x10, 0x30, 0x30, 0x30, 0x14, 0x0C, 0x0C, 0x10, 0x10, 0x14, 0x0C, 0x1C, 0x24, 0x20, 0x24, 0x10,
                        0x10, 0x30, 0x14, 0x0C, 0x0C, 0x30, 0x18, 0x20, 0x18, 0x18, 0x0C, 0x10, 0x2C, 0x14, 0x0C, 0x0C, 0x10, 0x10, 0x10, 0x0C, 0x0C, 0x0C, 0x10, 0x18, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                        0x18, 0x10, 0x40, 0x10, 0x10, 0x0C, 0x14, 0x0C, 0x0C, 0x14, 0x0C, 0x3C, 0x18, 0x40, 0x2C, 0x10, 0x10, 0x10, 0x20, 0x0C, 0x10, 0x10, 0x14, 0x0C, 0x10, 0x10, 0x0C, 0x1C, 0x24, 0x80, 0x0C, 0x24,
                        0x30, 0x44, 0x40, 0x00, 0x30, 0x50, 0x10, 0x0C, 0x0C, 0x10, 0x0C, 0x18, 0x0C, 0x40, 0x10, 0x18, 0x0C, 0x10, 0x0C, 0x40, 0x40, 0x40, 0x0C, 0x0C, 0x00, 0x10, 0x10, 0x10, 0x00, 0x18, 0x54, 0x14,
                        0x10, 0x1C, 0x10, 0x10, 0x20, 0x10, 0x4C, 0x54, 0x0C, 0x10, 0x10, 0x10, 0x0C, 0x10, 0x10, 0x34, 0x10, 0x10, 0x14, 0x14, 0x14, 0x10, 0x0C, 0x0C, 0x0C, 0x0C, 0x24, 0x28, 0x0C, 0x10, 0x0C, 0x0C,
                        0x0C, 0x0C, 0x0C, 0x0C, 0x0C, 0x0C, 0x10, 0x10, 0x10, 0x18, 0x0C, 0x10, 0x10, 0x0C, 0x10, 0x0C, 0x0C, 0x14, 0x0C, 0x0C, 0x14, 0x18, 0x10, 0x10, 0x10, 0x18, 0x14, 0x00, 0x10, 0x0C, 0x18, 0x10,
                        0x0C, 0x24, 0x24, 0x24, 0x24, 0x10, 0x00, 0x14, 0x10, 0x0C, 0x10, 0x10, 0x0C, 0x24, 0x0C, 0x28, 0x0C, 0x24, 0x28, 0x10, 0x10, 0x68, 0x0C, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, };
            }
            public class ScriptState
            {
                public ScriptState(int ver)
                {
                    scriptGameVersion = ver;
                    bitfield = 0;
                    scriptIndexOrSlot = -1;
                    type1 = null;
                    scriptStateBody = null;
                    nextState = null;
                    scriptStateBodyCount = 0;
                }
                public ScriptState(BinaryReader reader, int ver)
                {
                    scriptGameVersion = ver;
                    bitfield = reader.ReadInt16();
                    scriptIndexOrSlot = reader.ReadInt16();
                    if ((bitfield & 0x4000) != 0)
                    {
                        type1 = new SupportType1(reader);
                    }
                    if ((bitfield & 0x8000) != 0)
                    {
                        nextState = new ScriptState(reader, scriptGameVersion);
                    }
                }
                public void Write(BinaryWriter writer)
                {
                    writer.Write(bitfield);
                    writer.Write(scriptIndexOrSlot);
                    if ((bitfield & 0x4000) != 0)
                    {
                        type1.Write(writer);
                    }
                    if ((bitfield & 0x8000) != 0)
                    {
                        nextState.Write(writer);
                    }
                }
                public int GetLength()
                {
                    return 4 + (((bitfield & 0x4000) != 0) ? type1.GetLength() : 0) + (((bitfield & 0x8000) != 0) ? nextState.GetLength() : 0);
                }
                public short bitfield { get; set; }
                private short scriptStateBodyCount
                {
                    get => (short)(((ushort)bitfield) & 0x1F); set => bitfield = (short)((((ushort)bitfield) & 0xFFE0) | (value & 0x1F));
                }
                public short scriptIndexOrSlot { get; set; }
                public bool IsSlot
                {
                    get => (bitfield & 0x1000) != 0; set => bitfield = value ? (short)(bitfield | 0x1000) : (short)(bitfield & ~0x1000);
                }
                public SupportType1 type1 { get; set; }
                public ScriptStateBody scriptStateBody { get; set; }
                public ScriptState nextState { get; set; }
                public int scriptGameVersion { get; set; }
                public bool isValidBits()
                {
                    if (((bitfield & 0x4000) != 0) && type1 == null)
                    {
                        return false;
                    }
                    if (((bitfield & 0x4000) == 0) && type1 != null)
                    {
                        return false;
                    }
                    return ((bitfield & 0x8000) == 0 || nextState != null) && ((bitfield & 0x8000) != 0 || nextState == null) && ((bitfield & 0x1F) == 0 || scriptStateBody != null) && ((bitfield & 0x1F) != 0 || scriptStateBody == null);
                }
                public bool CreateType1()
                {
                    if (type1 == null)
                    {
                        type1 = new SupportType1();
                        bitfield = (short)(bitfield | 0x4000);
                        return true;
                    }
                    else
                    {
                        return false;
                    }

                }
                public bool DeleteType1()
                {
                    if (type1 != null)
                    {
                        type1 = null;
                        bitfield = (short)(bitfield & ~0x4000);
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                public bool AddScriptStateBody(int position)
                {
                    if (position > scriptStateBodyCount || position < 0)
                    {
                        return false;
                    }
                    if (scriptStateBodyCount == 0)
                    {
                        scriptStateBody = new ScriptStateBody(scriptGameVersion);
                    }
                    else if (position == scriptStateBodyCount)
                    {
                        ScriptStateBody ptr = scriptStateBody;
                        while (ptr.nextScriptStateBody != null)
                        {
                            ptr = ptr.nextScriptStateBody;
                        }
                        ptr.bitfield = (short)(ptr.bitfield | 0x800);
                        ptr.nextScriptStateBody = new ScriptStateBody(scriptGameVersion);
                    }
                    else
                    {
                        int pos = 0;
                        ScriptStateBody prevPtr = null;
                        ScriptStateBody ptr = scriptStateBody;
                        ScriptStateBody newType2 = new ScriptStateBody(scriptGameVersion);
                        while (pos < position)
                        {
                            prevPtr = ptr;
                            ptr = ptr.nextScriptStateBody;
                            ++pos;
                        }
                        if (prevPtr != null)
                        {
                            prevPtr.nextScriptStateBody = newType2;
                            prevPtr.nextScriptStateBody.nextScriptStateBody = ptr;
                        }
                        else
                        {
                            newType2.nextScriptStateBody = scriptStateBody;
                            scriptStateBody = newType2;
                        }

                        if (newType2.nextScriptStateBody != null)
                        {
                            newType2.bitfield |= 0x800;
                        }
                    }
                    ++scriptStateBodyCount;
                    return true;
                }
                public bool DeleteScriptStateBody(int position)
                {
                    if (position >= scriptStateBodyCount || position < 0)
                    {
                        return false;
                    }
                    if (position == 0)
                    {
                        scriptStateBody = scriptStateBody.nextScriptStateBody;
                    }
                    else
                    {
                        int pos = 0;
                        ScriptStateBody prevPtr = null;
                        ScriptStateBody ptr = scriptStateBody;
                        while (pos < position)
                        {
                            prevPtr = ptr;
                            ptr = ptr.nextScriptStateBody;
                            ++pos;
                        }
                        prevPtr.nextScriptStateBody = ptr.nextScriptStateBody;
                        if (prevPtr.nextScriptStateBody == null)
                        {
                            prevPtr.bitfield &= ~0x800;
                        }
                    }
                    --scriptStateBodyCount;
                    return true;
                }
            }

            public bool DeleteLinkedScript(int position)
            {
                if (position >= GetStatesAmount() || position < 0)
                {
                    return false;
                }
                if (position == 0)
                {
                    scriptState1 = scriptState1.nextState;
                }
                else
                {
                    int pos = 0;
                    ScriptState prevPtr = null;
                    ScriptState ptr = scriptState1;
                    while (pos < position)
                    {
                        prevPtr = ptr;
                        ptr = ptr.nextState;
                        ++pos;
                    }
                    prevPtr.nextState = ptr.nextState;
                    if (prevPtr.nextState == null)
                    {
                        prevPtr.bitfield = (short)(prevPtr.bitfield & ~0x8000);
                    }
                }
                return true;
            }
            public bool AddLinkedScript(int position)
            {
                int states = GetStatesAmount();
                if (position > states || position < 0)
                {
                    return false;
                }
                if (scriptState1 == null)
                {
                    scriptState1 = new ScriptState(scriptGameVersion);
                }
                else if (position == states)
                {
                    ScriptState ptr = scriptState1;
                    while (ptr.nextState != null)
                    {
                        ptr = ptr.nextState;
                    }
                    ptr.bitfield = (short)(ptr.bitfield | 0x8000);
                    ptr.nextState = new ScriptState(scriptGameVersion);
                }
                else
                {
                    int pos = 0;
                    ScriptState prevPtr = null;
                    ScriptState ptr = scriptState1;
                    ScriptState newState = new ScriptState(scriptGameVersion);
                    while (pos < position)
                    {
                        prevPtr = ptr;
                        ptr = ptr.nextState;
                        ++pos;
                    }
                    if (prevPtr != null)
                    {
                        prevPtr.nextState = newState;
                        prevPtr.nextState.nextState = ptr;
                    }
                    else
                    {
                        newState.nextState = scriptState1;
                        scriptState1 = newState;
                    }

                    if (newState.nextState != null)
                    {
                        newState.bitfield = (short)(newState.bitfield | 0x8000);
                    }
                }
                return true;
            }
            public int GetStatesAmount()
            {
                int iter = 0;
                ScriptState state = scriptState1;
                while (state != null)
                {
                    iter++;
                    state = state.nextState;
                }
                return iter;
            }
        }
        public string Name
        {
            get => Main != null
                    ? Main.name
                    : Header != null && Header.pairs.Count > 0
                        ? $"B.Starter Priority {mask} for Script {Header.pairs[0].mainScriptIndex - 1:0000}"
                        : $"B.Starter Priority {mask}";
            set
            {
                if (Main != null)
                {
                    Main.name = value;
                }
            }
        }

        private ushort id;
        public byte mask; // priority value in HeaderScript
        public byte flag;
        public HeaderScript Header { get; set; }
        public MainScript Main { get; set; }

        public byte[] script;
        public byte[] data;
        public int scriptGameVersion;

        public override void Save(BinaryWriter writer)
        {
            id = (ushort)ID;
            writer.Write(id);
            writer.Write(mask);
            writer.Write(flag);
            if (data != null && data.Length > 0)
            {
                writer.Write(data);
                return;
            }
            if (flag == 0)
            {
                Main.Write(writer);
            }
            else
            {
                Header.Write(writer);
            }
            writer.Write(script);
        }
        public override void Load(BinaryReader reader, int size)
        {
            scriptGameVersion = ParentType == SectionType.ScriptX ? 1 : ParentType == SectionType.ScriptDemo ? 2 : ParentType == SectionType.ScriptMB ? 3 : 0;
            long sk = reader.BaseStream.Position;
            id = reader.ReadUInt16();
            mask = reader.ReadByte();
            flag = reader.ReadByte();
            long datapos = reader.BaseStream.Position;
            if (flag == 0)
            {
                Main = new MainScript(reader, scriptGameVersion);
            }
            else
            {
                Header = new HeaderScript(reader);
            }
            try
            {
                script = reader.ReadBytes(size - (int)(reader.BaseStream.Position - sk));
                if (Main != null && script != null && script.Length > 0)
                {
                    Console.WriteLine("Script has leftovers (check command size mapper): " + Main.name);
                }
            }
            catch
            {
                if (flag == 0 && Main != null)
                {
                    Console.WriteLine("Failed to load script: " + Main.name);
                }
                script = null;
                reader.BaseStream.Position = datapos;
                data = reader.ReadBytes(size - 4);
            }
        }
        protected override int GetSize()
        {
            if (flag != 0)
            {
                return Header.GetLength() + 4 + script.Length;
            }
            else
            {
                if (data != null && data.Length > 0)
                {
                    return 4 + data.Length;
                }

                _ = Main.GetLength();
                _ = script.Length;
                return Main.GetLength() + 4 + script.Length;
            }

        }
    }
}
