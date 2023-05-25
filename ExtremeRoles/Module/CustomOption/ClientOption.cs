using BepInEx.Configuration;

namespace ExtremeRoles.Module.CustomOption;

public sealed class ClientOption
{
    public static ClientOption Instance { get; private set; }

    public ConfigEntry<string> StreamerModeReplacementText { get; init; }
    public ConfigEntry<bool> GhostsSeeTask { get; init; }
    public ConfigEntry<bool> GhostsSeeRole { get; init; }
    public ConfigEntry<bool> GhostsSeeVote { get; init; }
    public ConfigEntry<bool> ShowRoleSummary { get; init; }
    public ConfigEntry<bool> HideNamePlate { get; init; }
    public ConfigEntry<string> Ip { get; init; }
    public ConfigEntry<ushort> Port { get; init; }

    public static void Create()
    {
        Instance = new ClientOption();
    }

    private ClientOption()
    {
        var config = ExtremeRolesPlugin.Instance.Config;

        GhostsSeeTask = config.Bind(
            "ClientOption", "GhostCanSeeRemainingTasks", true);
        GhostsSeeRole = config.Bind(
            "ClientOption", "GhostCanSeeRoles", true);
        GhostsSeeVote = config.Bind(
            "ClientOption", "GhostCanSeeVotes", true);
        ShowRoleSummary = config.Bind(
            "ClientOption", "IsShowRoleSummary", true);
        HideNamePlate = config.Bind(
            "ClientOption", "IsHideNamePlate", false);

        StreamerModeReplacementText = config.Bind(
            "ClientOption",
            "ReplacementRoomCodeText",
            "Playing with Extreme Roles");

        Ip = config.Bind(
            "ClientOption", "CustomServerIP", "127.0.0.1");
        Port = config.Bind(
            "ClientOption", "CustomServerPort", (ushort)22023);
    }
}
