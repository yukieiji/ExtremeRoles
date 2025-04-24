
#nullable enable

namespace ExtremeRoles.Extension.Manager;

public static class IRegionInfoExtension
{
	public const string FullCustomServerName = "custom";
	public const string ExROfficialServerTokyoManinName = "ExROfficialTokyo";

	public static bool IsCustomServer(this IRegionInfo? info)
	{
		return
			info != null &&
			(
				info.Name == FullCustomServerName ||
				info.Name == ExROfficialServerTokyoManinName
			);
	}
	public static bool IsExROnlyServer(this IRegionInfo? info)
	{
		return
			info != null &&
			(
				info.Name == ExROfficialServerTokyoManinName
			);
	}
}
