using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Twinsanity;

namespace TwinsanityEditor.Utils
{
    //Because C# UInt16 and others has no fucing interface like IParseable i have to do this. Please don't kill me.
    //Toughest of choices require strongest of will.
    public class ListManipulatorUInt16
    {
        private List<ushort> list;
        private readonly ListBox listBox;
        private readonly TextBox source;
        private bool update;
        public ListManipulatorUInt16(Button addBtn, Button delBtn, Button setBtn, Button upBtn, Button downBtn, ListBox listBox, TextBox source)
        {

            this.source = source;
            this.listBox = listBox;
            if (listBox != null)
            {
                listBox.SelectedIndexChanged += UpdateSource;
            }

            if (addBtn != null)
            {
                addBtn.Click += Add;
            }

            if (delBtn != null)
            {
                delBtn.Click += Remove;
            }

            if (setBtn != null)
            {
                setBtn.Click += Set;
            }

            if (upBtn != null)
            {
                upBtn.Click += MoveUp;
            }

            if (downBtn != null)
            {
                downBtn.Click += MoveDown;
            }
        }
        public void SetSource(List<ushort> list)
        {
            this.list = list;
            update = true;
        }
        public void UpdateSource(object sender, EventArgs args)
        {
            if (update)
            {
                source.Text = list[listBox.SelectedIndex].ToString();
            }
        }
        public void PopulateList()
        {
            listBox.BeginUpdate();
            listBox.Items.Clear();
            int i = 0;
            foreach (ushort e in list)
            {
                _ = listBox.Items.Add(GenerateText(i, e));
                ++i;
            }
            listBox.EndUpdate();
        }
        private string GenerateText(int i, int e)
        {
            return $"{i:000}: {e}";
        }

        public void MoveUp(object sender, EventArgs args)
        {
            if (listBox.SelectedIndex > 0)
            {
                int index = listBox.SelectedIndex;
                ushort val1 = list[index];
                ushort val2 = list[index - 1];
                list[index - 1] = val1;
                list[index] = val2;
                DisableUpdate();
                PopulateList();
                int top = listBox.Items.Count - 1;
                --index;
                listBox.SelectedIndex = Math.Min(Math.Max(index, 0), top);
                EnableUpdate();
            }
        }
        public void MoveDown(object sender, EventArgs args)
        {
            if (listBox.SelectedIndex < listBox.Items.Count - 1)
            {
                int index = listBox.SelectedIndex;
                ushort val1 = list[index];
                ushort val2 = list[index + 1];
                list[index + 1] = val1;
                list[index] = val2;
                DisableUpdate();
                PopulateList();
                int top = listBox.Items.Count - 1;
                ++index;
                listBox.SelectedIndex = Math.Min(Math.Max(index, 0), top);
                EnableUpdate();
            }
        }
        public void Remove(object sender, EventArgs args)
        {
            if (listBox.SelectedIndex >= 0)
            {
                int index = listBox.SelectedIndex;
                list.RemoveAt(index);
                DisableUpdate();
                PopulateList();
                int top = listBox.Items.Count - 1;
                listBox.SelectedIndex = Math.Min(Math.Max(index, 0), top);
                EnableUpdate();
            }
        }
        public void Add(object sender, EventArgs args)
        {
            int index = listBox.SelectedIndex;
            if (index < 0)
            {
                index = 0;
            }
            bool success;
            success = ushort.TryParse(source.Text, out ushort val);
            if (success)
            {
                list.Insert(index, val);
                DisableUpdate();
                PopulateList();
                listBox.SelectedIndex = index;
                EnableUpdate();
            }

        }
        public void Set(object sender, EventArgs args)
        {
            if (listBox.SelectedIndex >= 0)
            {
                bool success;
                success = ushort.TryParse(source.Text, out ushort val);
                if (success)
                {
                    list[listBox.SelectedIndex] = val;
                    int index = listBox.SelectedIndex;
                    DisableUpdate();
                    listBox.Items[listBox.SelectedIndex] = GenerateText(index, val);
                    listBox.SelectedIndex = index;
                    EnableUpdate();
                }
            }
        }
        private void DisableUpdate()
        {
            update = false;
        }
        private void EnableUpdate()
        {
            update = true;
        }
    }

    public class ListManipulatorUInt32
    {
        private List<uint> list;
        private readonly ListBox listBox;
        private readonly TextBox source;
        private bool update;
        public ListManipulatorUInt32(Button addBtn, Button delBtn, Button setBtn, Button upBtn, Button downBtn, ListBox listBox, TextBox source)
        {

            this.source = source;
            this.listBox = listBox;
            if (listBox != null)
            {
                listBox.SelectedIndexChanged += UpdateSource;
            }

            if (addBtn != null)
            {
                addBtn.Click += Add;
            }

            if (delBtn != null)
            {
                delBtn.Click += Remove;
            }

            if (setBtn != null)
            {
                setBtn.Click += Set;
            }

            if (upBtn != null)
            {
                upBtn.Click += MoveUp;
            }

            if (downBtn != null)
            {
                downBtn.Click += MoveDown;
            }
        }
        public void SetSource(List<uint> list)
        {
            this.list = list;
            update = true;
        }
        public void UpdateSource(object sender, EventArgs args)
        {
            if (update)
            {
                source.Text = list[listBox.SelectedIndex].ToString();
            }
        }
        public void PopulateList()
        {
            listBox.BeginUpdate();
            listBox.Items.Clear();
            int i = 0;
            foreach (uint e in list)
            {
                _ = listBox.Items.Add(GenerateText(i, e));
                ++i;
            }
            listBox.EndUpdate();
        }
        private string GenerateText(int i, uint e)
        {
            return $"{i:000}: {e}";
        }

        public void MoveUp(object sender, EventArgs args)
        {
            if (listBox.SelectedIndex > 0)
            {
                int index = listBox.SelectedIndex;
                uint val1 = list[index];
                uint val2 = list[index - 1];
                list[index - 1] = val1;
                list[index] = val2;
                DisableUpdate();
                PopulateList();
                int top = listBox.Items.Count - 1;
                --index;
                listBox.SelectedIndex = Math.Min(Math.Max(index, 0), top);
                EnableUpdate();
            }
        }
        public void MoveDown(object sender, EventArgs args)
        {
            if (listBox.SelectedIndex < listBox.Items.Count - 1)
            {
                int index = listBox.SelectedIndex;
                uint val1 = list[index];
                uint val2 = list[index + 1];
                list[index + 1] = val1;
                list[index] = val2;
                DisableUpdate();
                PopulateList();
                int top = listBox.Items.Count - 1;
                ++index;
                listBox.SelectedIndex = Math.Min(Math.Max(index, 0), top);
                EnableUpdate();
            }
        }
        public void Remove(object sender, EventArgs args)
        {
            if (listBox.SelectedIndex >= 0)
            {
                int index = listBox.SelectedIndex;
                list.RemoveAt(index);
                DisableUpdate();
                PopulateList();
                int top = listBox.Items.Count - 1;
                listBox.SelectedIndex = Math.Min(Math.Max(index, 0), top);
                EnableUpdate();
            }
        }
        public void Add(object sender, EventArgs args)
        {
            int index = listBox.SelectedIndex;
            if (index < 0)
            {
                index = 0;
            }
            bool success;
            success = uint.TryParse(source.Text, out uint val);
            if (success)
            {
                list.Insert(index, val);
                DisableUpdate();
                PopulateList();
                listBox.SelectedIndex = index;
                EnableUpdate();
            }

        }
        public void Set(object sender, EventArgs args)
        {
            if (listBox.SelectedIndex >= 0)
            {
                bool success;
                success = uint.TryParse(source.Text, out uint val);
                if (success)
                {
                    list[listBox.SelectedIndex] = val;
                    int index = listBox.SelectedIndex;
                    DisableUpdate();
                    listBox.Items[listBox.SelectedIndex] = GenerateText(index, val);
                    listBox.SelectedIndex = index;
                    EnableUpdate();
                }
            }
        }
        private void DisableUpdate()
        {
            update = false;
        }
        private void EnableUpdate()
        {
            update = true;
        }
    }
    public class ListManipulatorSingle
    {
        private List<float> list;
        private readonly ListBox listBox;
        private readonly TextBox source;
        private bool update;
        public ListManipulatorSingle(Button addBtn, Button delBtn, Button setBtn, Button upBtn, Button downBtn, ListBox listBox, TextBox source)
        {

            this.source = source;
            this.listBox = listBox;
            if (listBox != null)
            {
                listBox.SelectedIndexChanged += UpdateSource;
            }

            if (addBtn != null)
            {
                addBtn.Click += Add;
            }

            if (delBtn != null)
            {
                delBtn.Click += Remove;
            }

            if (setBtn != null)
            {
                setBtn.Click += Set;
            }

            if (upBtn != null)
            {
                upBtn.Click += MoveUp;
            }

            if (downBtn != null)
            {
                downBtn.Click += MoveDown;
            }
        }
        public void SetSource(List<float> list)
        {
            this.list = list;
            update = true;
        }
        public void UpdateSource(object sender, EventArgs args)
        {
            if (update)
            {
                source.Text = list[listBox.SelectedIndex].ToString();
            }
        }
        public void PopulateList()
        {
            listBox.BeginUpdate();
            listBox.Items.Clear();
            int i = 0;
            foreach (float e in list)
            {
                _ = listBox.Items.Add(GenerateText(i, e));
                ++i;
            }
            listBox.EndUpdate();
        }
        private string GenerateText(int i, float e)
        {
            return $"{i:000}: {e}";
        }

        public void MoveUp(object sender, EventArgs args)
        {
            if (listBox.SelectedIndex > 0)
            {
                int index = listBox.SelectedIndex;
                float val1 = list[index];
                float val2 = list[index - 1];
                list[index - 1] = val1;
                list[index] = val2;
                DisableUpdate();
                PopulateList();
                int top = listBox.Items.Count - 1;
                --index;
                listBox.SelectedIndex = Math.Min(Math.Max(index, 0), top);
                EnableUpdate();
            }
        }
        public void MoveDown(object sender, EventArgs args)
        {
            if (listBox.SelectedIndex < listBox.Items.Count - 1)
            {
                int index = listBox.SelectedIndex;
                float val1 = list[index];
                float val2 = list[index + 1];
                list[index + 1] = val1;
                list[index] = val2;
                DisableUpdate();
                PopulateList();
                int top = listBox.Items.Count - 1;
                ++index;
                listBox.SelectedIndex = Math.Min(Math.Max(index, 0), top);
                EnableUpdate();
            }
        }
        public void Remove(object sender, EventArgs args)
        {
            if (listBox.SelectedIndex >= 0)
            {
                int index = listBox.SelectedIndex;
                list.RemoveAt(index);
                DisableUpdate();
                PopulateList();
                int top = listBox.Items.Count - 1;
                listBox.SelectedIndex = Math.Min(Math.Max(index, 0), top);
                EnableUpdate();
            }
        }
        public void Add(object sender, EventArgs args)
        {
            int index = listBox.SelectedIndex;
            if (index < 0)
            {
                index = 0;
            }
            bool success;
            success = float.TryParse(source.Text, out float val);
            if (success)
            {
                list.Insert(index, val);
                DisableUpdate();
                PopulateList();
                listBox.SelectedIndex = index;
                EnableUpdate();
            }

        }
        public void Set(object sender, EventArgs args)
        {
            if (listBox.SelectedIndex >= 0)
            {
                bool success;
                success = float.TryParse(source.Text, out float val);
                if (success)
                {
                    list[listBox.SelectedIndex] = val;
                    int index = listBox.SelectedIndex;
                    DisableUpdate();
                    listBox.Items[listBox.SelectedIndex] = GenerateText(index, val);
                    listBox.SelectedIndex = index;
                    EnableUpdate();
                }
            }
        }
        private void DisableUpdate()
        {
            update = false;
        }
        private void EnableUpdate()
        {
            update = true;
        }
    }
    public class ListManipulatorByte
    {
        private List<byte> list;
        private readonly ListBox listBox;
        private readonly TextBox source;
        private bool update;
        public ListManipulatorByte(Button addBtn, Button delBtn, Button setBtn, Button upBtn, Button downBtn, ListBox listBox, TextBox source)
        {

            this.source = source;
            this.listBox = listBox;
            if (listBox != null)
            {
                listBox.SelectedIndexChanged += UpdateSource;
            }

            if (addBtn != null)
            {
                addBtn.Click += Add;
            }

            if (delBtn != null)
            {
                delBtn.Click += Remove;
            }

            if (setBtn != null)
            {
                setBtn.Click += Set;
            }

            if (upBtn != null)
            {
                upBtn.Click += MoveUp;
            }

            if (downBtn != null)
            {
                downBtn.Click += MoveDown;
            }
        }
        public void SetSource(List<byte> list)
        {
            this.list = list;
            update = true;
        }
        public void UpdateSource(object sender, EventArgs args)
        {
            if (update)
            {
                source.Text = list[listBox.SelectedIndex].ToString();
            }
        }
        public void PopulateList()
        {
            listBox.BeginUpdate();
            listBox.Items.Clear();
            int i = 0;
            foreach (byte e in list)
            {
                _ = listBox.Items.Add(GenerateText(i, e));
                ++i;
            }
            listBox.EndUpdate();
        }
        private string GenerateText(int i, byte e)
        {
            return $"{i:000}: {e}";
        }

        public void MoveUp(object sender, EventArgs args)
        {
            if (listBox.SelectedIndex > 0)
            {
                int index = listBox.SelectedIndex;
                byte val1 = list[index];
                byte val2 = list[index - 1];
                list[index - 1] = val1;
                list[index] = val2;
                DisableUpdate();
                PopulateList();
                int top = listBox.Items.Count - 1;
                --index;
                listBox.SelectedIndex = Math.Min(Math.Max(index, 0), top);
                EnableUpdate();
            }
        }
        public void MoveDown(object sender, EventArgs args)
        {
            if (listBox.SelectedIndex < listBox.Items.Count - 1)
            {
                int index = listBox.SelectedIndex;
                byte val1 = list[index];
                byte val2 = list[index + 1];
                list[index + 1] = val1;
                list[index] = val2;
                DisableUpdate();
                PopulateList();
                int top = listBox.Items.Count - 1;
                ++index;
                listBox.SelectedIndex = Math.Min(Math.Max(index, 0), top);
                EnableUpdate();
            }
        }
        public void Remove(object sender, EventArgs args)
        {
            if (listBox.SelectedIndex >= 0)
            {
                int index = listBox.SelectedIndex;
                list.RemoveAt(index);
                DisableUpdate();
                PopulateList();
                int top = listBox.Items.Count - 1;
                listBox.SelectedIndex = Math.Min(Math.Max(index, 0), top);
                EnableUpdate();
            }
        }
        public void Add(object sender, EventArgs args)
        {
            int index = listBox.SelectedIndex;
            if (index < 0)
            {
                index = 0;
            }
            bool success;
            success = byte.TryParse(source.Text, out byte val);
            if (success)
            {
                list.Insert(index, val);
                DisableUpdate();
                PopulateList();
                listBox.SelectedIndex = index;
                EnableUpdate();
            }

        }
        public void Set(object sender, EventArgs args)
        {
            if (listBox.SelectedIndex >= 0)
            {
                bool success;
                success = byte.TryParse(source.Text, out byte val);
                if (success)
                {
                    list[listBox.SelectedIndex] = val;
                    int index = listBox.SelectedIndex;
                    DisableUpdate();
                    listBox.Items[listBox.SelectedIndex] = GenerateText(index, val);
                    listBox.SelectedIndex = index;
                    EnableUpdate();
                }
            }
        }
        private void DisableUpdate()
        {
            update = false;
        }
        private void EnableUpdate()
        {
            update = true;
        }
    }


    public class ListManipulatorEvent
    {
        private List<uint> list;
        private readonly ListBox listBox;
        private readonly TextBox source;
        private readonly TextBox source_caller;
        private readonly TextBox source_script;
        private readonly TextBox source_argument;
        private bool update;
        public ListManipulatorEvent(Button addBtn, Button delBtn, Button setBtn, Button upBtn, Button downBtn, ListBox listBox, TextBox source, TextBox caller, TextBox script, TextBox argument)
        {

            this.source = source;
            this.listBox = listBox;
            source_caller = caller;
            source_script = script;
            source_argument = argument;

            if (listBox != null)
            {
                listBox.SelectedIndexChanged += UpdateSource;
            }

            if (addBtn != null)
            {
                addBtn.Click += Add;
            }

            if (delBtn != null)
            {
                delBtn.Click += Remove;
            }

            if (setBtn != null)
            {
                setBtn.Click += Set;
            }

            if (upBtn != null)
            {
                upBtn.Click += MoveUp;
            }

            if (downBtn != null)
            {
                downBtn.Click += MoveDown;
            }
        }
        public void SetSource(List<uint> list)
        {
            this.list = list;
            update = true;
        }
        public void UpdateSource(object sender, EventArgs args)
        {
            if (update)
            {
                uint val = list[listBox.SelectedIndex];
                source.Text = val.ToString();
                ushort script = (ushort)((val >> 0xA) & 0x3FFF);
                ushort arg = (ushort)(val & 0x3FF);
                ushort caller = (ushort)((val >> 0x18) & 0x1);
                source_caller.Text = caller.ToString();
                source_script.Text = script.ToString();
                source_argument.Text = arg.ToString();
            }
        }
        public void PopulateList()
        {
            listBox.BeginUpdate();
            listBox.Items.Clear();
            int i = 0;
            foreach (uint e in list)
            {
                _ = listBox.Items.Add(GenerateText(i));
                ++i;
            }
            listBox.EndUpdate();
        }
        private string GenerateText(int i)
        {
            uint val = list[i];
            ushort script = (ushort)((val >> 0xA) & 0x3FFF);
            ushort arg = (ushort)(val & 0x3FF);
            ushort caller = (ushort)((val >> 0x18) & 0x1);
            string scrTxt = script.ToString();

            if (Enum.IsDefined(typeof(DefaultEnums.ScriptID), script))
            {
                scrTxt += " " + (DefaultEnums.ScriptID)script;
            }

            return $"{i:000}: Arg {arg}: Script {scrTxt} (Channel {caller})";
        }

        public void MoveUp(object sender, EventArgs args)
        {
            if (listBox.SelectedIndex > 0)
            {
                int index = listBox.SelectedIndex;
                uint val1 = list[index];
                uint val2 = list[index - 1];
                list[index - 1] = val1;
                list[index] = val2;
                DisableUpdate();
                PopulateList();
                int top = listBox.Items.Count - 1;
                --index;
                listBox.SelectedIndex = Math.Min(Math.Max(index, 0), top);
                EnableUpdate();
            }
        }
        public void MoveDown(object sender, EventArgs args)
        {
            if (listBox.SelectedIndex < listBox.Items.Count - 1)
            {
                int index = listBox.SelectedIndex;
                uint val1 = list[index];
                uint val2 = list[index + 1];
                list[index + 1] = val1;
                list[index] = val2;
                DisableUpdate();
                PopulateList();
                int top = listBox.Items.Count - 1;
                ++index;
                listBox.SelectedIndex = Math.Min(Math.Max(index, 0), top);
                EnableUpdate();
            }
        }
        public void Remove(object sender, EventArgs args)
        {
            if (listBox.SelectedIndex >= 0)
            {
                int index = listBox.SelectedIndex;
                list.RemoveAt(index);
                DisableUpdate();
                PopulateList();
                int top = listBox.Items.Count - 1;
                listBox.SelectedIndex = Math.Min(Math.Max(index, 0), top);
                EnableUpdate();
            }
        }
        public void Add(object sender, EventArgs args)
        {
            int index = listBox.SelectedIndex;
            if (index < 0)
            {
                index = 0;
            }
            bool success;
            success = uint.TryParse(source.Text, out uint val);
            if (success)
            {
                list.Insert(index, val);
                DisableUpdate();
                PopulateList();
                listBox.SelectedIndex = index;
                EnableUpdate();
            }

        }
        public void Set(object sender, EventArgs args)
        {
            if (listBox.SelectedIndex >= 0)
            {
                bool success;
                _ = uint.TryParse(source.Text, out uint val);
                _ = ushort.TryParse(source_script.Text, out ushort scr);
                _ = ushort.TryParse(source_argument.Text, out ushort arg);
                success = ushort.TryParse(source_caller.Text, out ushort caller);
                if (success)
                {
                    for (int i = 0; i < 0xA + 0xE + 0x1; i++)
                    {
                        val &= ~(uint)(1 << i);
                    }
                    val |= (uint)arg & 0x3FF;
                    val |= (uint)scr << 0xA;

                    uint mask = 1 << 0x18;
                    if (caller != 0)
                    {
                        val |= mask;
                    }

                    list[listBox.SelectedIndex] = val;
                    int index = listBox.SelectedIndex;
                    DisableUpdate();

                    source.Text = val.ToString();

                    listBox.Items[listBox.SelectedIndex] = GenerateText(index);
                    listBox.SelectedIndex = index;
                    EnableUpdate();
                }
            }
        }
        private void DisableUpdate()
        {
            update = false;
        }
        private void EnableUpdate()
        {
            update = true;
        }
    }
}
