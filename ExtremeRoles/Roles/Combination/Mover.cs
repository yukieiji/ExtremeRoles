using UnityEngine;

using Hazel;

using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Resources;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Performance;

namespace ExtremeRoles.Roles.Combination;

public sealed class MoverManager : FlexibleCombinationRoleManagerBase
{
    public MoverManager() : base(new Mover(), 1)
    { }

}

public sealed class Mover : 
    MultiAssignRoleBase,
    IRoleAbility,
    IRoleSpecialReset,
    IRoleUsableOverride
{
    public enum MoverRpc : byte
    {
        Move,
        Reset,
    }

    public ExtremeAbilityButton Button { get; set; }

    public bool EnableUseButton { get; private set; } = true;

    public bool EnableVentButton { get; private set; } = true;
    public bool IsKillAnimating { get; set; } = false;

    private Console targetConsole;
    private Console hasConsole;

    public Mover() : base(
        ExtremeRoleId.Mover,
        ExtremeRoleType.Crewmate,
        ExtremeRoleId.Mover.ToString(),
        Palette.CrewmateBlue,
        false, true, false, false,
        tab: OptionTab.Combination)
    { }

    public static void Ability(ref MessageReader reader)
    {
        MoverRpc rpcId = (MoverRpc)reader.ReadByte();
        byte rolePlayerId = reader.ReadByte();

        var rolePlayer = Player.GetPlayerControlById(rolePlayerId);
        var role = ExtremeRoleManager.GetSafeCastedRole<Mover>(rolePlayerId);
        if (role == null || rolePlayer == null) { return; }

        float x = reader.ReadSingle();
        float y = reader.ReadSingle();

        rolePlayer.NetTransform.SnapTo(new Vector2(x, y));

        switch (rpcId)
        {
            case MoverRpc.Move:
                int index = reader.ReadPackedInt32();
                pickUpConsole(role, rolePlayer, index);
                break;
            case MoverRpc.Reset:
                removeConsole(role, rolePlayer);
                break;
            default:
                break;
        }
    }

    private static void pickUpConsole(Mover mover, PlayerControl player, int index)
    {
        Console console = CachedShipStatus.Instance.AllConsoles[index];

        if (console is null) { return; }

        mover.EnableUseButton = false;
        mover.hasConsole = console;
        mover.hasConsole.Image.enabled = false;
        
        GameSystem.SetColliderActive(mover.hasConsole.gameObject, false);

        mover.hasConsole.transform.position = player.transform.position;
        mover.hasConsole.transform.SetParent(player.transform);
    }

    private static void removeConsole(Mover mover, PlayerControl player)
    {
        mover.EnableUseButton = true;

        if (mover.hasConsole is null) { return; }

        GameSystem.SetColliderActive(mover.hasConsole.gameObject, true);
        
        mover.hasConsole.transform.SetParent(null);
        mover.hasConsole.Image.enabled = true;
        mover.hasConsole.transform.position = player.GetTruePosition();
        mover.hasConsole = null;
    }

    public void CreateAbility()
    {
        this.CreateReclickableCountAbilityButton(
            "carry",
            Loader.CreateSpriteFromResources(
               Path.CarrierCarry),
            checkAbility: IsAbilityActive,
            abilityOff: this.CleanUp);
    }

    public bool IsAbilityActive() =>
        CachedPlayerControl.LocalPlayer.PlayerControl.moveable ||
        this.IsKillAnimating;

    public bool IsAbilityUse()
    {
        PlayerControl localPlayer = CachedPlayerControl.LocalPlayer;

        this.targetConsole = Player.GetClosestConsole(
            localPlayer, localPlayer.MaxReportDistance);

        if (this.targetConsole is null) { return false; }

        return 
            this.IsCommonUse() && 
            this.targetConsole.Image is not null &&
            GameSystem.IsValidConsole(localPlayer, this.targetConsole);
    }

    public void ResetOnMeetingEnd(GameData.PlayerInfo exiledPlayer = null)
    {
        this.IsKillAnimating = false;
    }

    public void ResetOnMeetingStart()
    {
        this.IsKillAnimating = false;
    }

    public bool UseAbility()
    {
        PlayerControl player = CachedPlayerControl.LocalPlayer;

        for (int i = 0; i < CachedShipStatus.Instance.AllConsoles.Length; ++i)
        {
            Console console = CachedShipStatus.Instance.AllConsoles[i];
            if (console != this.targetConsole) { continue; }

            using (var caller = RPCOperator.CreateCaller(
                RPCOperator.Command.MoverAbility))
            {
                caller.WriteByte((byte)MoverRpc.Move);
                caller.WriteByte(player.PlayerId);
                caller.WriteFloat(player.transform.position.x);
                caller.WriteFloat(player.transform.position.y);
                caller.WritePackedInt(i);
            }
            pickUpConsole(this, player, i);
            this.IsKillAnimating = false;
            return true;
        }
        return false;
    }

    public void CleanUp()
    {
        PlayerControl player = CachedPlayerControl.LocalPlayer;

        using (var caller = RPCOperator.CreateCaller(
            RPCOperator.Command.MoverAbility))
        {
            caller.WriteByte((byte)MoverRpc.Reset);
            caller.WriteByte(player.PlayerId);
            caller.WriteFloat(player.transform.position.x);
            caller.WriteFloat(player.transform.position.y);
        }
        removeConsole(this, player);
    }

    public override void RolePlayerKilledAction(
        PlayerControl rolePlayer, PlayerControl killerPlayer)
    {
        removeConsole(this, rolePlayer);
    }

    protected override void CreateSpecificOption(
        IOption parentOps)
    {
        this.CreateAbilityCountOption(
            parentOps, 3, 10, 30.0f);
    }

    protected override void RoleSpecificInit()
    {
        this.RoleAbilityInit();

        this.IsKillAnimating = false;
        this.EnableVentButton = true;
        this.EnableUseButton = true;
    }

    public void AllReset(PlayerControl rolePlayer)
    {
        removeConsole(this, rolePlayer);
    }
}
