using System;
using System.Runtime.InteropServices;

namespace ExtremeRoles.Module;

#nullable disable
// Class(Struct) from commdlg.h
[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
public class OPENFILENAME
{
	public int lStructSize;
	public IntPtr IntPtrOwner;
	public IntPtr hInstance;
	public string stringFilter;
	public string stringCustomFilter;
	public int nMaxCustFilter;
	public int nFilterIndex;
	public string stringFile = new(new char[256]);
	public int nMaxFile = 256;
	public string stringFileTitle = new(new char[64]);
	public int nMaxFileTitle = 64;
	public string stringInitialDir;
	public string stringTitle;
	public int Flags = 1 << 3;
	public short nFileOffset;
	public short nFileExtension;
	public string stringDefExt;
	public int lCustData;
	public IntPtr lpfnHook;
	public string lpTemplateName;
	public IntPtr pvReserved;
	public int dwReserved;
	public int FlagsEx;
}
#nullable enable

public static class DllApi
{
	[DllImport("user32.dll", CharSet = CharSet.Unicode)]
	public static extern int MessageBox(IntPtr hWnd, string text, string caption, int options);

	[DllImport("Comdlg32.dll", CharSet = CharSet.Unicode, ThrowOnUnmappableChar = true, SetLastError = true)]
	public static extern bool GetOpenFileName([In][Out] OPENFILENAME ofn);

	[DllImport("Comdlg32.dll", CharSet = CharSet.Unicode, ThrowOnUnmappableChar = true, SetLastError = true)]
	public static extern bool GetSaveFileName([In][Out] OPENFILENAME ofn);
}
