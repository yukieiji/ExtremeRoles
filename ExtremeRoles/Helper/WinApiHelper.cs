using ExtremeRoles.Module;
using System.Runtime.InteropServices;

#nullable enable

namespace ExtremeRoles.Helper;

public sealed record FileInfo(string? FilePath, string? FileName);

public sealed class WinApiHelper
{
	public static FileInfo? OpenFile(
		string filter = "",
		string title = "",
		string defaultDir = "",
		int? defaultFilterIdx = null)
	{
		var ofn = create(filter, title, defaultDir, defaultFilterIdx);

		bool result = DllApi.GetOpenFileName(ofn);

		return result ? new FileInfo(ofn.stringFile, ofn.stringFileTitle) : null;
	}

	public static FileInfo? SaveFile(
		string filter = "",
		string title = "",
		string defaultDir = "",
		int? defaultFilterIdx = null)
	{
		var ofn = create(filter, title, defaultDir, defaultFilterIdx);

		bool result = DllApi.GetSaveFileName(ofn);

		return result ? new FileInfo(ofn.stringFile, ofn.stringFileTitle) : null;
	}

	private static OPENFILENAME create(
		string filter = "",
		string title = "",
		string defaultDir = "",
		int? defaultFilterIdx = null)
	{
		var ofn = new OPENFILENAME();
		ofn.stringFilter = filter;
		ofn.stringTitle = title;
		ofn.stringInitialDir = defaultDir;
		if (defaultFilterIdx.HasValue)
		{
			ofn.nFilterIndex = defaultFilterIdx.Value;
		}
		ofn.lStructSize = Marshal.SizeOf(ofn);
		return ofn;
	}
}
