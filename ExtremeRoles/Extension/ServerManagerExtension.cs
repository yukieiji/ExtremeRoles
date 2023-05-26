namespace ExtremeRoles.Extension.Manager;

public static class ServerManagerExtension
{
    public const string FullCustomServerName = "custom";
    public const string ExROfficialServerTokyoManinName = "ExROfficialTokyo";

    public static bool IsCustomServer(this ServerManager mng)
    {
        return
            mng.CurrentRegion != null &&
            (
                mng.CurrentRegion.Name == FullCustomServerName ||
                mng.CurrentRegion.Name == ExROfficialServerTokyoManinName
            );
    }
    public static bool IsExROnlyServer(this ServerManager mng)
    {
        return
            mng.CurrentRegion != null &&
            (
                mng.CurrentRegion.Name == ExROfficialServerTokyoManinName
            );
    }
}
