
using System.Collections.Generic;

using AmongUs.GameOptions;

using UnityEngine;

using ExtremeRoles.Extension.Manager;
using ExtremeRoles.GameMode;
using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Module.Ability;
using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.CustomOption.Factory.OptionBuilder;
using ExtremeRoles.Module.SystemType;
using ExtremeRoles.Resources;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;


namespace ExtremeRoles.Roles.Solo.Neutral.Queen;

public sealed class QueenRole :
    SingleRoleBase,
    IRoleAutoBuildAbility,
    IRoleSpecialReset,
    IRoleMurderPlayerHook,
    IRoleUpdate
{
    public const string RoleShowTag = "<b>Ⓠ</b>";

    public enum QueenOption
    {
        Range,
        CanUseVent,
        ServantKillKillCoolReduceRate,
        ServantTaskKillCoolReduceRate,
        ServantTaskCompKillCoolReduceRate,
        ServantSelfKillCool,
		ServantSucideWithQueenWhenHasKill
    }

    public ExtremeAbilityButton Button
    {
        get => createServant;
        set
        {
            createServant = value;
        }
    }

    public PlayerControl Target;
    public float ServantSelfKillCool;
    private ExtremeAbilityButton createServant;
    private float range;
    private float killKillCoolReduceRate;
    private float taskKillCoolReduceRate;
    private float taskCompKillCoolReduceRate;
    private Dictionary<byte, float> servantTaskGage;
    private HashSet<byte> taskCompServant;
    private HashSet<byte> servantPlayerId;
	private bool servantSucideWithQueenWhenHasKill;

	public QueenRole() : base(
		RoleCore.BuildNeutral(
			ExtremeRoleId.Queen,
			ColorPalette.QueenWhite),
        true, false, false, false)
    { }

    public static void TargetToServant(
        byte rolePlayerId, byte targetPlayerId)
    {

        QueenRole queen = ExtremeRoleManager.GetSafeCastedRole<QueenRole>(rolePlayerId);

        if (queen == null) { return; }

        var targetPlayer = Player.GetPlayerControlById(targetPlayerId);
        var targetRole = ExtremeRoleManager.GameRole[targetPlayerId];

        IParentChainStatus.PurgeParent(targetPlayerId);
        resetTargetAnotherRole(targetRole, targetPlayerId, targetPlayer);
        replaceVanilaRole(targetRole, targetPlayer);
        resetAbility(targetRole, targetPlayerId);

        ServantRole servant = new ServantRole(
            rolePlayerId, queen, targetRole);

		var core = targetRole.Core;
        if (targetPlayerId == PlayerControl.LocalPlayer.PlayerId)
        {
            Player.ResetTarget();
            servant.SelfKillAbility(queen.ServantSelfKillCool);
            if (core.Team != ExtremeRoleType.Neutral)
            {
                servant.Button.HotKey = KeyCode.C;
            }
            HudManager.Instance.ReGridButtons();
        }

        if (core.Team != ExtremeRoleType.Neutral)
        {
			core.Team = ExtremeRoleType.Neutral;

            if (targetRole is VanillaRoleWrapper vanillaRole)
            {
                vanillaRole.AnotherRole = null;
                vanillaRole.CanHasAnotherRole = false;
                vanillaRole.CanCallMeeting = true;
                vanillaRole.CanUseAdmin = true;
                vanillaRole.CanUseSecurity = true;
                vanillaRole.CanUseVital = true;

                servant.CanHasAnotherRole = true;
                servant.SetAnotherRole(vanillaRole);

                ExtremeRoleManager.SetNewRole(targetPlayerId, servant);
            }
            else if (targetRole is MultiAssignRoleBase multiAssignRole)
            {
                multiAssignRole.AnotherRole = null;

                multiAssignRole.CanHasAnotherRole = true;
                servant.CanHasAnotherRole = false;

                ExtremeRoleManager.SetNewAnothorRole(targetPlayerId, servant);
            }
            else
            {

                servant.CanHasAnotherRole = true;
                servant.SetAnotherRole(targetRole);

                ExtremeRoleManager.SetNewRole(targetPlayerId, servant);
            }
        }
        else
        {
            servant.CanHasAnotherRole = false;
            resetRole(targetRole, targetPlayerId, targetPlayer);
            ExtremeRoleManager.SetNewRole(targetPlayerId, servant);
        }
        queen.AddServantPlayer(targetPlayerId);
    }

    private static void resetTargetAnotherRole(
        SingleRoleBase targetRole,
        byte targetPlayerId,
        PlayerControl targetPlayer)
    {
        var multiAssignRole = targetRole as MultiAssignRoleBase;
        if (multiAssignRole == null) { return; }

        if (PlayerControl.LocalPlayer.PlayerId == targetPlayerId)
        {
            if (multiAssignRole.AnotherRole is IRoleAbility abilityRole)
            {
                abilityRole.Button.OnMeetingStart();
            }
            if (multiAssignRole.AnotherRole is IRoleResetMeeting meetingResetRole)
            {
                meetingResetRole.ResetOnMeetingStart();
            }
        }

        if (multiAssignRole.AnotherRole is IRoleSpecialReset specialResetRole)
        {
            specialResetRole.AllReset(targetPlayer);
        }

    }
    private static void replaceVanilaRole(
        SingleRoleBase targetRole,
        PlayerControl targetPlayer)
    {
        if (targetRole is MultiAssignRoleBase multiAssignRole &&
			multiAssignRole.AnotherRole is VanillaRoleWrapper)
        {
			RoleManager.Instance.SetRole(
				targetPlayer, RoleTypes.Crewmate);
			return;
		}

        switch (targetPlayer.Data.Role.Role)
        {
            case RoleTypes.Crewmate:
            case RoleTypes.Impostor:
				RoleManager.Instance.SetRole(
					targetPlayer, RoleTypes.Crewmate);
				break;
        }
    }
    private static void resetRole(
        SingleRoleBase targetRole,
        byte targetPlayerId,
        PlayerControl targetPlayer)
    {
        if (PlayerControl.LocalPlayer.PlayerId == targetPlayerId)
        {
            if (targetRole is IRoleAbility abilityRole)
            {
                abilityRole.Button.OnMeetingStart();
            }
            if (targetRole is IRoleResetMeeting meetingResetRole)
            {
                meetingResetRole.ResetOnMeetingStart();
            }
        }

        if (targetRole is IRoleSpecialReset specialResetRole)
        {
            specialResetRole.AllReset(targetPlayer);
        }
    }

    private static void resetAbility(
        SingleRoleBase targetRole,
        byte targetPlayerId)
    {
        // 会議開始と終了の処理を呼び出すことで能力を使用可能な状態でリセット
        if (PlayerControl.LocalPlayer.PlayerId == targetPlayerId)
        {
            if (targetRole is IRoleAbility abilityRole)
            {
                abilityRole.Button.OnMeetingStart();
                abilityRole.Button.OnMeetingEnd();
            }
            if (targetRole is IRoleResetMeeting meetingResetRole)
            {
                meetingResetRole.ResetOnMeetingStart();
                meetingResetRole.ResetOnMeetingEnd();
            }
        }
    }

    public void AddServantPlayer(byte servantPlayerId)
    {
        this.servantPlayerId.Add(servantPlayerId);
    }

    public void RemoveServantPlayer(byte servantPlayerId)
    {
        this.servantPlayerId.Remove(servantPlayerId);
    }

    public void HookMuderPlayer(
        PlayerControl source, PlayerControl target)
    {
        if (source.PlayerId != target.PlayerId &&
            servantPlayerId.Contains(source.PlayerId))
        {

            float killcool = PlayerControl.LocalPlayer.killTimer;
            if (killcool > 0.0f)
            {
                PlayerControl.LocalPlayer.killTimer = killcool * killKillCoolReduceRate;
            }
        }
    }

    public void Update(PlayerControl rolePlayer)
    {
        if (!GameProgressSystem.IsTaskPhase ||
			rolePlayer.Data.Tasks.Count == 0)
		{
			return;
		}

        foreach (byte playerId in servantPlayerId)
        {
            var player = Player.GetPlayerControlById(playerId);
            if (!player) { continue; }

            float gage = Player.GetPlayerTaskGage(player);
            if (!servantTaskGage.ContainsKey(playerId))
            {
                servantTaskGage.Add(playerId, 0.0f);
            }
            float prevGage = servantTaskGage[playerId];
            servantTaskGage[playerId] = gage;

            float killcool = PlayerControl.LocalPlayer.killTimer;
            if (gage > prevGage && killcool > 0.0f)
            {
                PlayerControl.LocalPlayer.killTimer = killcool * taskKillCoolReduceRate;
            }
            if (gage >= 1.0f && !taskCompServant.Contains(playerId))
            {
                taskCompServant.Add(playerId);
                if (!HasOtherKillCool)
                {
                    KillCoolTime = Player.DefaultKillCoolTime;
                }
                HasOtherKillCool = true;
                KillCoolTime = KillCoolTime * taskCompKillCoolReduceRate;
            }
        }
    }


    public void AllReset(PlayerControl rolePlayer)
    {
        foreach (byte playerId in servantPlayerId)
        {
            var player = Player.GetPlayerControlById(playerId);

            if (player == null ||
				player.Data.IsDead ||
                player.Data.Disconnected ||
				isNotSucideServant(playerId)) { continue; }

            RPCOperator.UncheckedMurderPlayer(
                playerId, playerId,
                byte.MaxValue);
        }
    }

    public void CreateAbility()
    {
        this.CreateAbilityCountButton(
            "queenCharm", UnityObjectLoader.LoadSpriteFromResources(
				ObjectPath.QueenCharm));
    }

    public bool UseAbility()
    {
		ExtremeRoleManager.RpcReplaceRole(
			PlayerControl.LocalPlayer.PlayerId, this.Target.PlayerId,
			ExtremeRoleManager.ReplaceOperation.CreateServant);
        return true;
    }

    public bool IsAbilityUse()
    {
        Target = Player.GetClosestPlayerInRange(
            PlayerControl.LocalPlayer,
            this, range);

        return Target != null && IRoleAbility.IsCommonUse();
    }

    public void ResetOnMeetingStart()
    {
        return;
    }

    public void ResetOnMeetingEnd(NetworkedPlayerInfo exiledPlayer = null)
    {
        return;
    }

    public override void ExiledAction(PlayerControl rolePlayer)
    {
        foreach (byte playerId in servantPlayerId)
        {
            PlayerControl player = Player.GetPlayerControlById(playerId);

            if (player == null ||
				player.Data.IsDead ||
				player.Data.Disconnected ||
				isNotSucideServant(playerId)) { continue; }

            player.Exiled();
        }
    }
    public override Color GetTargetRoleSeeColor(
        SingleRoleBase targetRole,
        byte targetPlayerId)
    {

        if (targetRole.Core.Id == ExtremeRoleId.Servant &&
            IsSameControlId(targetRole) &&
            servantPlayerId.Contains(targetPlayerId))
        {
            return ColorPalette.QueenWhite;
        }

        return base.GetTargetRoleSeeColor(targetRole, targetPlayerId);
    }

    public override string GetRoleTag() => RoleShowTag;

    public override string GetRolePlayerNameTag(
        SingleRoleBase targetRole, byte targetPlayerId)
    {

        if (servantPlayerId.Contains(targetPlayerId))
        {
            return Design.ColoredString(
                ColorPalette.QueenWhite,
                $" {RoleShowTag}");
        }

        return base.GetRolePlayerNameTag(targetRole, targetPlayerId);
    }

    public override void RolePlayerKilledAction(
        PlayerControl rolePlayer, PlayerControl killerPlayer)
    {
        foreach (byte playerId in servantPlayerId)
        {
            PlayerControl player = Player.GetPlayerControlById(playerId);

            if (player == null ||
				player.Data.IsDead ||
                player.Data.Disconnected ||
				isNotSucideServant(playerId)) { continue; }

            RPCOperator.UncheckedMurderPlayer(
                playerId, playerId,
                byte.MaxValue);
        }
    }

    public override bool IsSameTeam(SingleRoleBase targetRole)
    {
        if (isSameQueenTeam(targetRole))
        {
            if (ExtremeGameModeManager.Instance.ShipOption.IsSameNeutralSameWin)
            {
                return true;
            }
            else
            {
                return IsSameControlId(targetRole);
            }
        }
        else
        {
            return base.IsSameTeam(targetRole);
        }
    }

    protected override void CreateSpecificOption(OptionCategoryScope<AutoParentSetBuilder> categoryScope)
    {
        var factory = categoryScope.Builder;
        factory.CreateBoolOption(
            QueenOption.CanUseVent,
            false);

        IRoleAbility.CreateAbilityCountOption(
            factory, 1, 3);

        factory.CreateFloatOption(
            QueenOption.Range,
            1.0f, 0.5f, 2.6f, 0.1f);
        factory.CreateIntOption(
            QueenOption.ServantKillKillCoolReduceRate,
            40, 0, 85, 1,
            format:OptionUnit.Percentage);
        factory.CreateIntOption(
            QueenOption.ServantTaskKillCoolReduceRate,
            75, 0, 99, 1,
            format: OptionUnit.Percentage);
        factory.CreateIntOption(
            QueenOption.ServantTaskCompKillCoolReduceRate,
            30, 0, 75, 1,
            format: OptionUnit.Percentage);
        factory.CreateFloatOption(
            QueenOption.ServantSelfKillCool,
            30.0f, 0.5f, 60.0f, 0.5f,
            format: OptionUnit.Second);
		factory.CreateBoolOption(
			QueenOption.ServantSucideWithQueenWhenHasKill,
			true);
    }

    protected override void RoleSpecificInit()
    {
		var cate = Loader;

        range = cate.GetValue<QueenOption, float>(QueenOption.Range);
        UseVent = cate.GetValue<QueenOption, bool>(
            QueenOption.CanUseVent);
        ServantSelfKillCool = cate.GetValue<QueenOption, float>(
            QueenOption.ServantSelfKillCool);
        killKillCoolReduceRate = 1.0f - cate.GetValue<QueenOption, int>(
            QueenOption.ServantKillKillCoolReduceRate) / 100.0f;
        taskKillCoolReduceRate = 1.0f - cate.GetValue<QueenOption, int>(
            QueenOption.ServantTaskKillCoolReduceRate) / 100.0f;
        taskCompKillCoolReduceRate = 1.0f - cate.GetValue<QueenOption, int>(
            QueenOption.ServantTaskCompKillCoolReduceRate) / 100.0f;
		servantSucideWithQueenWhenHasKill = cate.GetValue<QueenOption, bool>(
			QueenOption.ServantSucideWithQueenWhenHasKill);

		servantTaskGage = new Dictionary<byte, float>();
        servantPlayerId = new HashSet<byte>();
        taskCompServant = new HashSet<byte>();
    }

    private bool isSameQueenTeam(SingleRoleBase targetRole)
    {
		var id = targetRole.Core.Id;
		return id == Core.Id || id is ExtremeRoleId.Servant;
    }
	private bool isNotSucideServant(byte playerId)
		=>
		!servantSucideWithQueenWhenHasKill &&
		ExtremeRoleManager.TryGetSafeCastedRole<ServantRole>(playerId, out var servant) &&
		servant.CanKill && !servant.IsSpecialKill;
}
