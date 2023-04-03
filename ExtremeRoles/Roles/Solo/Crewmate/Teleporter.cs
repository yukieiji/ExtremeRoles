using UnityEngine;

using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Module.CustomMonoBehaviour;
using ExtremeRoles.Resources;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Performance;
using ExtremeRoles.Performance.Il2Cpp;

namespace ExtremeRoles.Roles.Solo.Crewmate;

public sealed class Teleporter : SingleRoleBase, IRoleAbility
{
    public ExtremeAbilityButton Button { get; set; }

    private PortalFirst portal;

    public Teleporter() : base(
        ExtremeRoleId.Maintainer,
        ExtremeRoleType.Crewmate,
        ExtremeRoleId.Maintainer.ToString(),
        ColorPalette.MaintainerBlue,
        false, true, false, false)
    { }

    public static void SetPortal(byte teleporterPlayerId, Vector2 pos)
    {
        Teleporter teleporter = ExtremeRoleManager.GetSafeCastedRole<Teleporter>(
            teleporterPlayerId);

        GameObject obj = new GameObject("portal");
        obj.transform.position = new Vector3(pos.x, pos.y, pos.y / 1000.0f);

        if (ExtremeRolesPlugin.Compat.IsModMap)
        {
            ExtremeRolesPlugin.Compat.ModMap.AddCustomComponent(
                obj, Compat.Interface.CustomMonoBehaviourType.MovableFloorBehaviour);
        }

        if (teleporter.portal == null)
        {
            teleporter.portal = obj.AddComponent<PortalFirst>();
        }
        else
        {
            PortalSecond potal = obj.AddComponent<PortalSecond>();
            PortalBase.Link(potal, teleporter.portal);
            teleporter.portal = null;
        }
    }

    public void CreateAbility()
    {
        this.CreateAbilityCountButton(
            "maintenance",
            Loader.CreateSpriteFromResources(
                Path.MaintainerRepair));
        this.Button.SetLabelToCrewmate();
    }

    public bool UseAbility()
    {
        PlayerControl localPlayer = CachedPlayerControl.LocalPlayer;

        SetPortal(localPlayer.PlayerId, localPlayer.GetTruePosition());
        return true;
    }

    public bool IsAbilityUse() => this.IsCommonUse();

    public void ResetOnMeetingStart()
    {
        return;
    }

    public void ResetOnMeetingEnd(GameData.PlayerInfo exiledPlayer = null)
    {
        return;
    }

    protected override void CreateSpecificOption(
        IOption parentOps)
    {
        this.CreateAbilityCountOption(
            parentOps, 2, 10);
    }

    protected override void RoleSpecificInit()
    {
        this.RoleAbilityInit();
    }
}
