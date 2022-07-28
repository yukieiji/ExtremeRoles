using System;
using System.Runtime.InteropServices;

namespace ExtremeRoles.Module
{
    public static class DllApi
    {
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        public static extern int MessageBox(IntPtr hWnd, string text, string caption, int options);
    }
}
