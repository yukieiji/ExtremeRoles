﻿using System;
using System.Collections.Generic;

using UnityEngine;

using AmongUs.GameOptions;

using ExtremeRoles.Extension.Manager;
using ExtremeRoles.GameMode;
using ExtremeRoles.Module;
using ExtremeRoles.Module.ExtremeShipStatus;
using ExtremeRoles.Helper;
using ExtremeRoles.Resources;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Roles.Solo.Crewmate;
using ExtremeRoles.Performance;
using ExtremeRoles.Module.Ability;
using ExtremeRoles.Module.Ability.Factory;

using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.CustomOption.Interfaces;

namespace ExtremeRoles.Roles.Solo.Neutral;

public sealed class Queen :
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
        get => this.createServant;
        set
        {
            this.createServant = value;
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

	public Queen() : base(
        ExtremeRoleId.Queen,
        ExtremeRoleType.Neutral,
        ExtremeRoleId.Queen.ToString(),
        ColorPalette.QueenWhite,
        true, false, false, false)
    { }

    public static void TargetToServant(
        byte rolePlayerId, byte targetPlayerId)
    {

        Queen queen = ExtremeRoleManager.GetSafeCastedRole<Queen>(rolePlayerId);

        if (queen == null) { return; }

        var targetPlayer = Player.GetPlayerControlById(targetPlayerId);
        var targetRole = ExtremeRoleManager.GameRole[targetPlayerId];

        IRoleHasParent.PurgeParent(targetPlayerId);
        resetTargetAnotherRole(targetRole, targetPlayerId, targetPlayer);
        replaceVanilaRole(targetRole, targetPlayer);
        resetAbility(targetRole, targetPlayerId);

        Servant servant = new Servant(
            rolePlayerId, queen, targetRole);

        if (targetPlayerId == PlayerControl.LocalPlayer.PlayerId)
        {
            Player.ResetTarget();
            servant.SelfKillAbility(queen.ServantSelfKillCool);
            if (targetRole.Team != ExtremeRoleType.Neutral)
            {
                servant.Button.HotKey = KeyCode.C;
            }
            HudManager.Instance.ReGridButtons();
        }

        if (targetRole.Team != ExtremeRoleType.Neutral)
        {
            targetRole.Team = ExtremeRoleType.Neutral;

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
            this.servantPlayerId.Contains(source.PlayerId))
        {

            float killcool = PlayerControl.LocalPlayer.killTimer;
            if (killcool > 0.0f)
            {
                PlayerControl.LocalPlayer.killTimer = killcool * this.killKillCoolReduceRate;
            }
        }
    }

    public void Update(PlayerControl rolePlayer)
    {
        if (!rolePlayer ||
            rolePlayer.Data.Tasks.Count == 0 ||
            !GameData.Instance ||
            !ShipStatus.Instance ||
            !ShipStatus.Instance.enabled) { return; }

        foreach (byte playerId in this.servantPlayerId)
        {
            var player = Player.GetPlayerControlById(playerId);
            if (!player) { continue; }

            float gage = Player.GetPlayerTaskGage(player);
            if (!this.servantTaskGage.ContainsKey(playerId))
            {
                this.servantTaskGage.Add(playerId, 0.0f);
            }
            float prevGage = this.servantTaskGage[playerId];
            this.servantTaskGage[playerId] = gage;

            float killcool = PlayerControl.LocalPlayer.killTimer;
            if (gage > prevGage && killcool > 0.0f)
            {
                PlayerControl.LocalPlayer.killTimer = killcool * this.taskKillCoolReduceRate;
            }
            if (gage >= 1.0f && !this.taskCompServant.Contains(playerId))
            {
                this.taskCompServant.Add(playerId);
                if (!this.HasOtherKillCool)
                {
                    this.KillCoolTime = Player.DefaultKillCoolTime;
                }
                this.HasOtherKillCool = true;
                this.KillCoolTime = this.KillCoolTime * this.taskCompKillCoolReduceRate;
            }
        }
    }


    public void AllReset(PlayerControl rolePlayer)
    {
        foreach (byte playerId in this.servantPlayerId)
        {
            var player = Player.GetPlayerControlById(playerId);

            if (player == null ||
				player.Data.IsDead ||
                player.Data.Disconnected ||
				this.isNotSucideServant(playerId)) { continue; }

            RPCOperator.UncheckedMurderPlayer(
                playerId, playerId,
                byte.MaxValue);
        }
    }

    public void CreateAbility()
    {
        this.CreateAbilityCountButton(
            "queenCharm", Resources.UnityObjectLoader.LoadSpriteFromResources(
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
        this.Target = Player.GetClosestPlayerInRange(
            PlayerControl.LocalPlayer,
            this, this.range);

        return this.Target != null && IRoleAbility.IsCommonUse();
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
        foreach (byte playerId in this.servantPlayerId)
        {
            PlayerControl player = Player.GetPlayerControlById(playerId);

            if (player == null ||
				player.Data.IsDead ||
				player.Data.Disconnected ||
				this.isNotSucideServant(playerId)) { continue; }

            player.Exiled();
        }
    }
    public override Color GetTargetRoleSeeColor(
        SingleRoleBase targetRole,
        byte targetPlayerId)
    {

        if (targetRole.Id == ExtremeRoleId.Servant &&
            this.IsSameControlId(targetRole) &&
            this.servantPlayerId.Contains(targetPlayerId))
        {
            return ColorPalette.QueenWhite;
        }

        return base.GetTargetRoleSeeColor(targetRole, targetPlayerId);
    }

    public override string GetRoleTag() => RoleShowTag;

    public override string GetRolePlayerNameTag(
        SingleRoleBase targetRole, byte targetPlayerId)
    {

        if (this.servantPlayerId.Contains(targetPlayerId))
        {
            return Helper.Design.ColoedString(
                ColorPalette.QueenWhite,
                $" {RoleShowTag}");
        }

        return base.GetRolePlayerNameTag(targetRole, targetPlayerId);
    }

    public override void RolePlayerKilledAction(
        PlayerControl rolePlayer, PlayerControl killerPlayer)
    {
        foreach (byte playerId in this.servantPlayerId)
        {
            PlayerControl player = Player.GetPlayerControlById(playerId);

            if (player == null ||
				player.Data.IsDead ||
                player.Data.Disconnected ||
				this.isNotSucideServant(playerId)) { continue; }

            RPCOperator.UncheckedMurderPlayer(
                playerId, playerId,
                byte.MaxValue);
        }
    }

    public override bool IsSameTeam(SingleRoleBase targetRole)
    {
        if (this.isSameQueenTeam(targetRole))
        {
            if (ExtremeGameModeManager.Instance.ShipOption.IsSameNeutralSameWin)
            {
                return true;
            }
            else
            {
                return this.IsSameControlId(targetRole);
            }
        }
        else
        {
            return base.IsSameTeam(targetRole);
        }
    }

    protected override void CreateSpecificOption(AutoParentSetOptionCategoryFactory factory)
    {
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
		var cate = this.Loader;

        this.range = cate.GetValue<QueenOption, float>(QueenOption.Range);
        this.UseVent = cate.GetValue<QueenOption, bool>(
            QueenOption.CanUseVent);
        this.ServantSelfKillCool = cate.GetValue<QueenOption, float>(
            QueenOption.ServantSelfKillCool);
        this.killKillCoolReduceRate = 1.0f - (cate.GetValue<QueenOption, int>(
            QueenOption.ServantKillKillCoolReduceRate) / 100.0f);
        this.taskKillCoolReduceRate = 1.0f - (cate.GetValue<QueenOption, int>(
            QueenOption.ServantTaskKillCoolReduceRate) / 100.0f);
        this.taskCompKillCoolReduceRate = 1.0f - (cate.GetValue<QueenOption, int>(
            QueenOption.ServantTaskCompKillCoolReduceRate) / 100.0f);
		this.servantSucideWithQueenWhenHasKill = cate.GetValue<QueenOption, bool>(
			QueenOption.ServantSucideWithQueenWhenHasKill);

		this.servantTaskGage = new Dictionary<byte, float>();
        this.servantPlayerId = new HashSet<byte>();
        this.taskCompServant = new HashSet<byte>();
    }

    private bool isSameQueenTeam(SingleRoleBase targetRole)
    {
        return ((targetRole.Id == this.Id) || (targetRole.Id == ExtremeRoleId.Servant));
    }
	private bool isNotSucideServant(byte playerId)
		=>
		!this.servantSucideWithQueenWhenHasKill &&
		ExtremeRoleManager.TryGetSafeCastedRole<Servant>(playerId, out var servant) &&
		servant.CanKill && !servant.IsSpecialKill;
}

public sealed class Servant :
    MultiAssignRoleBase,
    IRoleAutoBuildAbility,
    IRoleMurderPlayerHook,
    IRoleHasParent
{
    public byte Parent => this.queenPlayerId;

    public ExtremeAbilityButton Button
    {
        get => this.selfKillButton;
        set
        {
            this.selfKillButton = value;
        }
    }

    private ExtremeAbilityButton selfKillButton;

    private byte queenPlayerId;
    private SpriteRenderer killFlash;
    private Queen queen;

	public override IOptionLoader Loader { get; }
	public bool IsSpecialKill { get; }

    public Servant(
        byte queenPlayerId,
        Queen queen,
        SingleRoleBase baseRole) :
        base(
            ExtremeRoleId.Servant,
            ExtremeRoleType.Neutral,
            ExtremeRoleId.Servant.ToString(),
            ColorPalette.QueenWhite,
            baseRole.CanKill,
            !baseRole.IsImpostor() ? true : baseRole.HasTask,
            baseRole.UseVent,
            baseRole.UseSabotage)
    {
		this.Loader = queen.Loader;
        this.SetControlId(queen.GameControlId);
        this.queenPlayerId = queenPlayerId;
        this.queen = queen;
        this.FakeImposter = baseRole.Team == ExtremeRoleType.Impostor;

		var id = baseRole.Id;

		this.IsSpecialKill = id is
			ExtremeRoleId.Fencer or ExtremeRoleId.Sheriff;

		this.CanKill = id switch
		{
			ExtremeRoleId.Fencer => false,
			ExtremeRoleId.Yandere or ExtremeRoleId.Hero => true,
			_ => this.CanKill,
		};

		if (baseRole.IsImpostor())
		{
			this.HasOtherVision = true;
		}
		else
		{
			this.HasOtherVision = baseRole.HasOtherVision;
		}
        this.Vision = baseRole.Vision;
        this.IsApplyEnvironmentVision = baseRole.IsApplyEnvironmentVision;

        this.HasOtherKillCool = baseRole.HasOtherKillCool;
        this.KillCoolTime = baseRole.KillCoolTime;
        this.HasOtherKillRange = baseRole.HasOtherKillRange;
        this.KillRange = baseRole.KillRange;
    }

    public void SelfKillAbility(float coolTime)
    {
        this.Button = RoleAbilityFactory.CreateReusableAbility(
            "selfKill",
			Resources.UnityObjectLoader.LoadSpriteFromResources(
				ObjectPath.SucideSprite),
            this.IsAbilityUse,
            this.UseAbility);
        this.Button.Behavior.SetCoolTime(coolTime);
        this.Button.OnMeetingEnd();
    }

    public void HookMuderPlayer(
        PlayerControl source, PlayerControl target)
    {

        if (MeetingHud.Instance ||
            source.PlayerId == target.PlayerId ||
            ExtremeRoleManager.GameRole[source.PlayerId] == this) { return; }

        var hudManager = HudManager.Instance;

        if (this.killFlash == null)
        {
            this.killFlash = UnityEngine.Object.Instantiate(
                 hudManager.FullScreen,
                 hudManager.transform);
            this.killFlash.transform.localPosition = new Vector3(0f, 0f, 20f);
            this.killFlash.gameObject.SetActive(true);
        }

        Color32 color = new Color(0f, 0.8f, 0f);

        if (source.PlayerId == this.queenPlayerId)
        {
            color = this.NameColor;
        }

        this.killFlash.enabled = true;

        hudManager.StartCoroutine(
            Effects.Lerp(1.0f, new Action<float>((p) =>
            {
                if (this.killFlash == null) { return; }
                if (p < 0.5)
                {
                    this.killFlash.color = new Color(color.r, color.g, color.b, Mathf.Clamp01(p * 2 * 0.75f));

                }
                else
                {
                    this.killFlash.color = new Color(color.r, color.g, color.b, Mathf.Clamp01((1 - p) * 2 * 0.75f));
                }
                if (p == 1f)
                {
                    this.killFlash.enabled = false;
                }
            }))
        );
    }

    public void CreateAbility()
    {
        throw new Exception("Don't call this class method!!");
    }

    public bool IsAbilityUse() => IRoleAbility.IsCommonUse();

    public void ResetOnMeetingEnd(NetworkedPlayerInfo exiledPlayer = null)
    {
        return;
    }

    public void ResetOnMeetingStart()
    {
        if (this.killFlash != null)
        {
            this.killFlash.enabled = false;
        }
    }

    public bool UseAbility()
    {
        byte playerId = PlayerControl.LocalPlayer.PlayerId;
        Player.RpcUncheckMurderPlayer(
            playerId, playerId, byte.MaxValue);

        return true;
    }

    public void RemoveParent(byte rolePlayerId)
    {
        this.queen.RemoveServantPlayer(rolePlayerId);
    }

    public override void OverrideAnotherRoleSetting()
    {
        var queenPlayer = GameData.Instance.GetPlayerById(Parent);

        if (this.AnotherRole is Resurrecter resurrecter &&
            (queenPlayer == null || queenPlayer.IsDead || queenPlayer.Disconnected))
        {
            Resurrecter.UseResurrect(resurrecter);
        }
    }

    public override bool TryRolePlayerKillTo(
        PlayerControl rolePlayer, PlayerControl targetPlayer)
    {
        if (targetPlayer.PlayerId == this.queenPlayerId)
        {
            if (this.AnotherRole?.Id == ExtremeRoleId.Sheriff)
            {

                Player.RpcUncheckMurderPlayer(
                    rolePlayer.PlayerId,
                    rolePlayer.PlayerId,
                    byte.MaxValue);

                ExtremeRolesPlugin.ShipState.RpcReplaceDeadReason(
                    rolePlayer.PlayerId, ExtremeShipStatus.PlayerStatus.MissShot);
            }
            return false;
        }

        return base.TryRolePlayerKillTo(rolePlayer, targetPlayer);
    }

    public override string GetFullDescription()
    {
        var queen = Player.GetPlayerControlById(this.queenPlayerId);
        string fullDesc = base.GetFullDescription();

        if (queen == null ||
            queen.Data == null)
		{
			return fullDesc;
		}

        return string.Format(
            fullDesc, queen.Data.PlayerName);
    }

    public override Color GetTargetRoleSeeColor(
        SingleRoleBase targetRole,
        byte targetPlayerId)
    {

        if (targetPlayerId == this.queenPlayerId)
        {
            return ColorPalette.QueenWhite;
        }
        return base.GetTargetRoleSeeColor(targetRole, targetPlayerId);
    }

    public override string GetRoleTag() => Queen.RoleShowTag;

    public override string GetRolePlayerNameTag(
        SingleRoleBase targetRole, byte targetPlayerId)
    {

        if (targetPlayerId == this.queenPlayerId)
        {
            return Helper.Design.ColoedString(
                ColorPalette.QueenWhite,
                $" {Queen.RoleShowTag}");
        }

        return base.GetRolePlayerNameTag(targetRole, targetPlayerId);
    }

    protected override void CreateSpecificOption(
        AutoParentSetOptionCategoryFactory factory)
    {
        throw new Exception("Don't call this class method!!");
    }

    protected override void RoleSpecificInit()
    {
        throw new Exception("Don't call this class method!!");
    }
}
