using System.Collections;

using UnityEngine;
using Hazel;

using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Resources;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Module.CustomMonoBehaviour;
using ExtremeRoles.Performance;



using BepInEx.Unity.IL2CPP.Utils;
using ExtremeRoles.Module.Ability;


using ExtremeRoles.Module.CustomOption.Factory;

namespace ExtremeRoles.Roles.Solo.Crewmate;

public sealed class TimeMasterRole : SingleRoleBase, IRoleAutoBuildAbility
{
    public enum TimeMasterOption
    {
        RewindTime
    }

    public enum TimeMasterOps : byte
    {
        ShieldOff,
        ShieldOn,
        RewindTime,
        ResetMeeting,
    }

    public ExtremeAbilityButton Button
    {
        get => this.timeShieldButton;
        set
        {
            this.timeShieldButton = value;
        }
    }
    private ExtremeAbilityButton timeShieldButton;

    private TimeMasterStatusModel status;
    private static TimeMasterHistory history;
    public override IStatusModel? Status => status;

    public TimeMaster() : base(
		RoleCore.BuildCrewmate(
			ExtremeRoleId.TimeMaster,
			ColorPalette.TimeMasterBlue),
        false, true, false, false)
    {
    }

    public static void Ability(ref MessageReader reader)
    {
        byte tmPlayerId = reader.ReadByte();
        TimeMasterOps ops = (TimeMasterOps)reader.ReadByte();
        var timeMaster = ExtremeRoleManager.GetSafeCastedRole<TimeMasterRole>(tmPlayerId);
        if (timeMaster == null) { return; }

        switch (ops)
        {
            case TimeMasterOps.ShieldOff:
                shieldOff(tmPlayerId);
                break;
            case TimeMasterOps.ShieldOn:
                shieldOn(tmPlayerId);
                break;
            case TimeMasterOps.RewindTime:
                ((TimeMasterAbilityHandler)timeMaster.AbilityClass!).StartRewind(tmPlayerId);
                break;
            case TimeMasterOps.ResetMeeting:
                resetMeeting(tmPlayerId);
                break;
            default:
                break;
        }
    }

    public static void ResetHistory()
    {
        history = null;
    }

    private static void shieldOn(byte playerId)
    {
        var timeMaster = ExtremeRoleManager.GetSafeCastedRole<TimeMasterRole>(playerId);

        if (timeMaster != null)
        {
            timeMaster.status.isShieldOn = true;
        }
    }

    private static void shieldOff(byte playerId)
    {
        var timeMaster = ExtremeRoleManager.GetSafeCastedRole<TimeMasterRole>(playerId);

        if (timeMaster != null)
        {
            timeMaster.status.isShieldOn = false;
        }
    }
    private static void resetMeeting(byte playerId)
    {
        var timeMaster = ExtremeRoleManager.GetSafeCastedRole<TimeMasterRole>(playerId);

        if (timeMaster == null) { return; }

        // ヒストリーのコルーチン処理を止める
        history.StopAllCoroutines();

        timeMaster.status.isShieldOn = false;
        timeMaster.status.isRewindTime = false;
        if (timeMaster.status.rewindScreen != null)
        {
            timeMaster.status.rewindScreen.enabled = false;
        }

        // ヒストリーブロック解除
        history.BlockAddHistory = false;

		if (MeetingHud.Instance != null)
		{
			// 会議開始後リウィンドのコルーチンが止まるまでポジションがバグるので
			// ここでポジションを上書きする => TMが発動してなくても通るが問題なし
			// それ以外でコードを追加してもいいが最も被害が少ない変更がここ
			ShipStatus.Instance.SpawnPlayer(
				PlayerControl.LocalPlayer,
				GameData.Instance.PlayerCount, false);
		}

        PlayerControl.LocalPlayer.moveable = true;
    }

    public void CleanUp()
    {
        PlayerControl localPlayer = PlayerControl.LocalPlayer;

        using (var caller = RPCOperator.CreateCaller(
            RPCOperator.Command.TimeMasterAbility))
        {
            caller.WriteByte(localPlayer.PlayerId);
            caller.WriteByte((byte)TimeMasterOps.ShieldOff);
        }
        shieldOff(localPlayer.PlayerId);
    }

    public void CreateAbility()
    {
        this.CreateNormalActivatingAbilityButton(
            "timeShield",
			Resources.UnityObjectLoader.LoadSpriteFromResources(
			   ObjectPath.TimeMasterTimeShield),
            abilityOff: this.CleanUp);
        this.Button.SetLabelToCrewmate();
    }

    public bool UseAbility()
    {
        PlayerControl localPlayer = PlayerControl.LocalPlayer;

        using (var caller = RPCOperator.CreateCaller(
            RPCOperator.Command.TimeMasterAbility))
        {
            caller.WriteByte(localPlayer.PlayerId);
            caller.WriteByte((byte)TimeMasterOps.ShieldOn);
        }
        shieldOn(localPlayer.PlayerId);

        return true;
    }

    public bool IsAbilityUse() => IRoleAbility.IsCommonUse();

    public void ResetOnMeetingStart()
    {

        PlayerControl localPlayer = PlayerControl.LocalPlayer;
        using (var caller = RPCOperator.CreateCaller(
            RPCOperator.Command.TimeMasterAbility))
        {
            caller.WriteByte(localPlayer.PlayerId);
            caller.WriteByte((byte)TimeMasterOps.ResetMeeting);
        }
        resetMeeting(localPlayer.PlayerId);
    }

    public void ResetOnMeetingEnd(NetworkedPlayerInfo exiledPlayer = null)
    {
        return;
    }

    public bool TryKilledFrom(
        PlayerControl rolePlayer, PlayerControl fromPlayer)
    {
        if (this.isRewindTime) { return false; }

        if (this.isShieldOn)
        {
            using (var caller = RPCOperator.CreateCaller(
                RPCOperator.Command.TimeMasterAbility))
            {
                caller.WriteByte(rolePlayer.PlayerId);
                caller.WriteByte((byte)TimeMasterOps.RewindTime);
            }
            startRewind(rolePlayer.PlayerId);

            return false;
        }

        return true;
    }

    protected override void CreateSpecificOption(
        AutoParentSetOptionCategoryFactory factory)
    {
        IRoleAbility.CreateCommonAbilityOption(
            factory, 3.0f);

        factory.CreateFloatOption(
            TimeMasterOption.RewindTime,
            5.0f, 1.0f, 60.0f, 0.5f,
            format: OptionUnit.Second);
    }

    protected override void RoleSpecificInit()
    {
        status = new TimeMasterStatusModel();
        AbilityClass = new TimeMasterAbilityHandler(status);

        if (history != null || PlayerControl.LocalPlayer == null) { return; }

        history = PlayerControl.LocalPlayer.gameObject.AddComponent<
            TimeMasterHistory>();
        history.Initialize(
            this.Loader.GetValue<TimeMasterOption, float>(
                TimeMasterOption.RewindTime));
    }
}
