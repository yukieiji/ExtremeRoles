using System.Linq;

using UnityEngine;

using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Resources;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Performance;
using Hazel;

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

    private Console targetConsole;
    private Console hasConsole;

    public Mover() : base(
        ExtremeRoleId.Mover,
        ExtremeRoleType.Crewmate,
        ExtremeRoleId.Mover.ToString(),
        Palette.CrewmateBlue,
        false, true, false, false)
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
                int id = reader.ReadPackedInt32();
                pickUpConsole(role, rolePlayer, id);
                break;
            case MoverRpc.Reset:
                removeConsole(role);
                break;
            default:
                break;
        }
    }

    private static void pickUpConsole(Mover mover, PlayerControl player, int id)
    {
        Console console = CachedShipStatus.Instance.AllConsoles.ToList().Find(
            x => x.ConsoleId == id);

        if (console is null) { return; }

        mover.EnableVentButton = false;
        mover.EnableUseButton = false;

        mover.hasConsole = console;
        mover.hasConsole.Image.enabled = false;
        mover.hasConsole.transform.position = player.transform.position;
        mover.hasConsole.transform.SetParent(player.transform);
    }

    private static void removeConsole(Mover mover)
    {
        mover.EnableVentButton = true;
        mover.EnableUseButton = true;

        if (mover.hasConsole is null) { return; }

        mover.hasConsole.transform.SetParent(null);
        mover.hasConsole.Image.enabled = true;
        mover.hasConsole = null;
    }

    public void CreateAbility()
    {
        this.CreateReclickableAbilityButton(
            "carry",
            Loader.CreateSpriteFromResources(
               Path.CarrierCarry),
            checkAbility: IsAbilityActive,
            abilityOff: this.CleanUp);
    }

    public bool IsAbilityActive() =>
        CachedPlayerControl.LocalPlayer.PlayerControl.moveable;

    public bool IsAbilityUse()
    {
        PlayerControl localPlayer = CachedPlayerControl.LocalPlayer;

        this.targetConsole = Player.GetClosestConsole(
            localPlayer, localPlayer.MaxReportDistance);

        return this.IsCommonUse() && this.targetConsole is not null;
    }

    public void ResetOnMeetingEnd(GameData.PlayerInfo exiledPlayer = null)
    {
        return;
    }

    public void ResetOnMeetingStart()
    {
        return;
    }

    public bool UseAbility()
    {
        PlayerControl player = CachedPlayerControl.LocalPlayer;
        int id = this.targetConsole.ConsoleId;

        using (var caller = RPCOperator.CreateCaller(
            RPCOperator.Command.SlimeAbility))
        {
            caller.WriteByte((byte)MoverRpc.Move);
            caller.WriteByte(player.PlayerId);
            caller.WriteFloat(player.transform.position.x);
            caller.WriteFloat(player.transform.position.y);
            caller.WritePackedInt(id);
        }
        pickUpConsole(this, player, id);
        return true;
    }

    public void CleanUp()
    {
        PlayerControl player = CachedPlayerControl.LocalPlayer;

        using (var caller = RPCOperator.CreateCaller(
            RPCOperator.Command.SlimeAbility))
        {
            caller.WriteByte((byte)MoverRpc.Reset);
            caller.WriteByte(player.PlayerId);
            caller.WriteFloat(player.transform.position.x);
            caller.WriteFloat(player.transform.position.y);
        }
        removeConsole(this);
    }

    public override void RolePlayerKilledAction(
        PlayerControl rolePlayer, PlayerControl killerPlayer)
    {
        removeConsole(this);
    }

    protected override void CreateSpecificOption(
        IOption parentOps)
    {
        this.CreateCommonAbilityOption(
            parentOps, 30.0f);
    }

    protected override void RoleSpecificInit()
    {
        this.RoleAbilityInit();

        this.EnableVentButton = true;
        this.EnableUseButton = true;
    }

    public void AllReset(PlayerControl rolePlayer)
    {
        removeConsole(this);
    }
}
