using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace TwinsanityEditor
{
    public static class ControlExt
    {
        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int wMsg, bool wParam, int lParam);

        private const int WM_SETREDRAW = 11;

        public static void SuspendDrawing(this Control c)
        {
            _ = SendMessage(c.Handle, WM_SETREDRAW, false, 0);
        }

        public static void ResumeDrawing(this Control c)
        {
            _ = SendMessage(c.Handle, WM_SETREDRAW, true, 0);
            c.Refresh();
        }
    }
}
