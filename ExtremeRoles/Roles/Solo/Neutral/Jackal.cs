﻿using System;
using System.Collections.Generic;

using UnityEngine;
using AmongUs.GameOptions;

using ExtremeRoles.GameMode;
using ExtremeRoles.Module;
using ExtremeRoles.Helper;
using ExtremeRoles.Resources;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Module.Ability;
using ExtremeRoles.Module.Ability.Behavior.Interface;

using ExtremeRoles.Module.CustomOption.Factory;

using ExtremeRoles.Module.CustomOption.Interfaces;

namespace ExtremeRoles.Roles.Solo.Neutral;

public sealed class Jackal : SingleRoleBase, IRoleAutoBuildAbility, IRoleSpecialReset
{
    public enum JackalOption
    {
        SidekickLimitNum,
        RangeSidekickTarget,
        CanLoverSidekick,
        ForceReplaceLover,

        UpgradeSidekickNum,

        CanSetImpostorToSidekick,
        CanSeeImpostorToSidekickImpostor,
        SidekickJackalCanMakeSidekick,

		SidekickUseSabotage = 10,
		SidekickUseVent,
		SidekickCanKill,

		SidekickHasOtherKillCool,
		SidekickKillCoolDown,
		SidekickHasOtherKillRange,
		SidekickKillRange,

		SidekickHasOtherVision,
		SidekickVision,
		SidekickApplyEnvironmentVisionEffect,
	}

    public List<byte> SidekickPlayerId;

    public ExtremeAbilityButton Button
    {
        get => this.createSidekick;
        set
        {
            this.createSidekick = value;
        }
    }

    public int NumAbility = 0;
    public int CurRecursion = 0;
    public int SidekickRecursionLimit = 0;

    public bool SidekickJackalCanMakeSidekick = false;
    public bool ForceReplaceLover = false;
    public bool CanSetImpostorToSidekick = false;
    public bool CanSeeImpostorToSidekickImpostor = false;

    public SidekickOptionHolder SidekickOption;

    public PlayerControl Target;

    private ExtremeAbilityButton createSidekick;

    private bool canLoverSidekick;
    private int numUpgradeSidekick = 0;
    private int createSidekickRange = 0;

    public struct SidekickOptionHolder
    {
        public bool CanKill = false;
        public bool UseSabotage = false;
        public bool UseVent = false;

        public bool HasOtherVision = false;
        public float Vision = 0f;
        public bool ApplyEnvironmentVisionEffect = false;

        public bool HasOtherKillCool = false;
        public float KillCool = 0f;

        public bool HasOtherKillRange = false;
        public int KillRange = 0;

        public SidekickOptionHolder(
            in AutoParentSetOptionCategoryFactory factory)
        {
            string roleName = ExtremeRoleId.Sidekick.ToString();

			factory.CreateBoolOption(JackalOption.SidekickUseSabotage, true);
			factory.CreateBoolOption(JackalOption.SidekickUseVent, true);


			var sidekickKillerOps = factory.CreateBoolOption(JackalOption.SidekickCanKill, false);

			var killCoolOption = factory.CreateBoolOption(
				JackalOption.SidekickHasOtherKillCool,
				false, sidekickKillerOps);
			factory.CreateFloatOption(
				JackalOption.SidekickKillCoolDown,
				30f, 1.0f, 120f, 0.5f,
				killCoolOption, format: OptionUnit.Second);

			var killRangeOption = factory.CreateBoolOption(
				JackalOption.SidekickHasOtherKillRange,
				false, sidekickKillerOps);
			factory.CreateSelectionOption(
				JackalOption.SidekickKillRange,
				OptionCreator.Range,
				killRangeOption);

			var visionOption = factory.CreateBoolOption(
				JackalOption.SidekickHasOtherVision, false);

			factory.CreateFloatOption(
				JackalOption.SidekickVision,
				2f, 0.25f, 5f, 0.25f,
				visionOption, format: OptionUnit.Multiplier);

			factory.CreateBoolOption(
				JackalOption.SidekickApplyEnvironmentVisionEffect,
				false, visionOption);
        }

        public void ApplyOption(in IOptionLoader loader)
        {
            var curOption = GameOptionsManager.Instance.CurrentGameOptions;

            this.UseSabotage = loader.GetValue<JackalOption, bool>(
                JackalOption.SidekickUseSabotage);
            this.UseVent = loader.GetValue<JackalOption, bool>(
                JackalOption.SidekickUseVent);

            this.CanKill = loader.GetValue<JackalOption, bool>(
                JackalOption.SidekickCanKill);

            this.HasOtherKillCool = loader.GetValue<JackalOption, bool>(
                JackalOption.SidekickHasOtherKillCool);
            this.KillCool = Player.DefaultKillCoolTime;
            if (this.HasOtherKillCool)
            {
                this.KillCool = loader.GetValue<JackalOption, float>(
                    JackalOption.SidekickKillCoolDown);
            }

            this.HasOtherKillRange = loader.GetValue<JackalOption, bool>(
                JackalOption.SidekickHasOtherKillRange);
            this.KillRange = curOption.GetInt(Int32OptionNames.KillDistance);
            if (this.HasOtherKillRange)
            {
                this.KillRange = loader.GetValue<JackalOption, int>(
                    JackalOption.SidekickKillRange);
            }

			this.HasOtherVision = loader.GetValue<JackalOption, bool>(
				JackalOption.SidekickHasOtherVision);
            this.Vision = curOption.GetFloat(FloatOptionNames.CrewLightMod);
            this.ApplyEnvironmentVisionEffect = false;
            if (this.HasOtherVision)
            {
                this.Vision = loader.GetValue<JackalOption, float>(
                    JackalOption.SidekickVision);
                this.ApplyEnvironmentVisionEffect = loader.GetValue<JackalOption, bool>(
                    JackalOption.SidekickApplyEnvironmentVisionEffect);
            }
        }

        public SidekickOptionHolder Clone()
        {
            return (SidekickOptionHolder)this.MemberwiseClone();
        }
    }

    public Jackal() : base(
        ExtremeRoleId.Jackal,
        ExtremeRoleType.Neutral,
        ExtremeRoleId.Jackal.ToString(),
        ColorPalette.JackalBlue,
        true, false, true, false)
    { }

    public override SingleRoleBase Clone()
    {
        var jackal = (Jackal)base.Clone();
        jackal.SidekickOption = this.SidekickOption.Clone();

        return jackal;
    }

    public static void TargetToSideKick(byte callerId, byte targetId)
    {
        PlayerControl targetPlayer = Player.GetPlayerControlById(targetId);
        SingleRoleBase targetRole = ExtremeRoleManager.GameRole[targetId];

        IRoleSpecialReset.ResetRole(targetId);

        var sourceJackal = ExtremeRoleManager.GetSafeCastedRole<Jackal>(callerId);
        if (sourceJackal == null) { return; }
        var newSidekick = new Sidekick(
            sourceJackal,
            callerId,
            targetRole.IsImpostor(),
            sourceJackal.SidekickOption);

        sourceJackal.SidekickPlayerId.Add(targetId);

        if (targetRole.Id != ExtremeRoleId.Lover)
        {
            ExtremeRoleManager.SetNewRole(targetId, newSidekick);
        }
        else
        {
            if (sourceJackal.ForceReplaceLover)
            {
                ExtremeRoleManager.SetNewRole(targetId, newSidekick);
				IRoleSpecialReset.ResetLover(targetRole, targetPlayer);
            }
            else
            {
                var sidekickOption = sourceJackal.SidekickOption;
                var lover = (Combination.Lover)targetRole;
                lover.CanHasAnotherRole = true;

                ExtremeRoleManager.SetNewAnothorRole(targetId, newSidekick);

                lover.Team = ExtremeRoleType.Neutral;
                lover.HasTask = false;
                lover.HasOtherVision = sidekickOption.HasOtherVision;
                lover.IsApplyEnvironmentVision = sidekickOption.ApplyEnvironmentVisionEffect;
                lover.Vision = sidekickOption.Vision;
                lover.KillCoolTime = sidekickOption.KillCool;
                lover.KillRange = sidekickOption.KillRange;
            }
        }

    }

    public void CreateAbility()
    {
        this.CreateAbilityCountButton(
            "Sidekick",
			Resources.UnityObjectLoader.LoadSpriteFromResources(
				ObjectPath.JackalSidekick));
    }

    public override Color GetTargetRoleSeeColor(
        SingleRoleBase targetRole,
        byte targetPlayerId)
    {

        if (((
                targetRole.Id == ExtremeRoleId.Sidekick &&
                this.SidekickPlayerId.Contains(targetPlayerId)) ||
            (
                targetRole.Id == ExtremeRoleId.Jackal
            )) && this.IsSameControlId(targetRole))
        {
            return ColorPalette.JackalBlue;
        }

        return base.GetTargetRoleSeeColor(targetRole, targetPlayerId);
    }

    public override string GetFullDescription()
    {
        string baseDesc = base.GetFullDescription();

        if (SidekickPlayerId.Count != 0)
        {
            baseDesc = $"{baseDesc}\n{Tr.GetString("curSidekick")}:";

            foreach (byte playerId in SidekickPlayerId)
            {
                string playerName = Player.GetPlayerControlById(playerId).Data.PlayerName;
                baseDesc += $"{playerName},";
            }
        }

        return baseDesc;
    }

    public override bool IsSameTeam(SingleRoleBase targetRole)
    {
        if (this.isSameJackalTeam(targetRole))
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

    public override void ExiledAction(
        PlayerControl rolePlayer)
    {
        SidekickToJackal(rolePlayer);
    }

    public override void RolePlayerKilledAction(
        PlayerControl rolePlayer, PlayerControl killerPlayer)
    {
        SidekickToJackal(rolePlayer);
    }

    public bool IsAbilityUse()
    {
		if (!GameSystem.TryGetKillDistance(out var range))
		{
			return false;
		}


        this.Target = Player.GetClosestPlayerInRange(
            PlayerControl.LocalPlayer,
            this, range[Mathf.Clamp(this.createSidekickRange, 0, 2)]);

        return this.Target != null && IRoleAbility.IsCommonUse();
    }

    public bool UseAbility()
    {
        byte targetPlayerId = this.Target.PlayerId;
        if (!(
				isImpostorAndSetTarget(targetPlayerId) &&
				isLoverAndSetTarget(targetPlayerId)
			))
		{
			return false;
		}
		ExtremeRoleManager.RpcReplaceRole(
			PlayerControl.LocalPlayer.PlayerId, targetPlayerId,
			ExtremeRoleManager.ReplaceOperation.ForceReplaceToSidekick);

        return true;
    }

    public void ResetOnMeetingStart()
    {
        return;
    }

    public void ResetOnMeetingEnd(NetworkedPlayerInfo exiledPlayer = null)
    {
        return;
    }

    public void SidekickToJackal(PlayerControl rolePlayer)
    {

        if (this.SidekickPlayerId.Count == 0) { return; }

        int numUpgrade = this.SidekickPlayerId.Count >= this.numUpgradeSidekick ?
            this.numUpgradeSidekick : this.SidekickPlayerId.Count;

        List<byte> updateSideKick = new List<byte>();

        for (int i = 0; i < numUpgrade; ++i)
        {
            int useIndex = UnityEngine.Random.Range(0, this.SidekickPlayerId.Count);
            byte targetPlayerId = this.SidekickPlayerId[useIndex];
            this.SidekickPlayerId.Remove(targetPlayerId);

            updateSideKick.Add(targetPlayerId);

        }
        foreach (byte playerId in updateSideKick)
        {
			if (ExtremeRolesPlugin.ShipState.DeadPlayerInfo.ContainsKey(playerId))
			{
				continue;
			}
			ExtremeRoleManager.RoleReplace(
				rolePlayer.PlayerId, playerId,
				ExtremeRoleManager.ReplaceOperation.SidekickToJackal);
        }
    }

    public void AllReset(PlayerControl rolePlayer)
    {
        this.SidekickToJackal(rolePlayer);
    }

    protected override void CreateSpecificOption(
        AutoParentSetOptionCategoryFactory factory)
    {
        // JackalOption
        this.createJackalOption(factory);

		// SideKickOption
		factory.OptionPrefix = string.Empty;
        this.SidekickOption = new SidekickOptionHolder(factory);
    }

    protected override void RoleSpecificInit()
    {
        this.CurRecursion = 0;
        this.SidekickPlayerId = new List<byte>();

        var cate = this.Loader;

        this.SidekickRecursionLimit = cate.GetValue<JackalOption, int>(
            JackalOption.SidekickLimitNum);

        this.canLoverSidekick = cate.GetValue<JackalOption, bool>(
            JackalOption.CanLoverSidekick);

        this.ForceReplaceLover = cate.GetValue<JackalOption, bool>(
            JackalOption.ForceReplaceLover);

        this.CanSetImpostorToSidekick = cate.GetValue<JackalOption, bool>(
            JackalOption.CanSetImpostorToSidekick);
        this.CanSeeImpostorToSidekickImpostor = cate.GetValue<JackalOption, bool>(
            JackalOption.CanSeeImpostorToSidekickImpostor);

        this.SidekickJackalCanMakeSidekick = cate.GetValue<JackalOption, bool>(
            JackalOption.SidekickJackalCanMakeSidekick);

        this.createSidekickRange = cate.GetValue<JackalOption, int>(
            JackalOption.RangeSidekickTarget);

        this.numUpgradeSidekick = cate.GetValue<JackalOption, int>(
            JackalOption.UpgradeSidekickNum);

        this.SidekickOption.ApplyOption(cate);
    }

    private void createJackalOption(AutoParentSetOptionCategoryFactory factory)
    {

        IRoleAbility.CreateAbilityCountOption(
			factory, 1, GameSystem.VanillaMaxPlayerNum - 1);

        factory.CreateSelectionOption(
            JackalOption.RangeSidekickTarget,
            OptionCreator.Range);

        var loverSkOpt = factory.CreateBoolOption(
            JackalOption.CanLoverSidekick,
            true);

        factory.CreateBoolOption(
            JackalOption.ForceReplaceLover,
            true, loverSkOpt,
            invert: true);

        factory.CreateIntOption(
            JackalOption.UpgradeSidekickNum,
            1, 1, GameSystem.VanillaMaxPlayerNum - 1, 1);

        var sidekickMakeSidekickOps = factory.CreateBoolOption(
            JackalOption.SidekickJackalCanMakeSidekick,
            false);

        factory.CreateIntOption(
            JackalOption.SidekickLimitNum,
            1, 1, GameSystem.VanillaMaxPlayerNum / 2, 1,
            sidekickMakeSidekickOps);

        factory.CreateBoolOption(
            JackalOption.CanSetImpostorToSidekick, false);

        factory.CreateBoolOption(
            JackalOption.CanSeeImpostorToSidekickImpostor, false);

    }

    private bool isImpostorAndSetTarget(
        byte playerId)
    {
        if (ExtremeRoleManager.GameRole[playerId].IsImpostor())
        {
            return this.CanSetImpostorToSidekick;
        }
        return true;
    }

    private bool isLoverAndSetTarget(byte playerId)
    {
        if (ExtremeRoleManager.GameRole[playerId].Id == ExtremeRoleId.Lover)
        {
            return this.canLoverSidekick;
        }
        return true;
    }

    private bool isSameJackalTeam(SingleRoleBase targetRole)
    {
        return ((targetRole.Id == this.Id) || (targetRole.Id == ExtremeRoleId.Sidekick));
    }
}

public sealed class Sidekick : SingleRoleBase, IRoleUpdate, IRoleHasParent
{
    public byte Parent => this.jackalPlayerId;

    private byte jackalPlayerId;
    private Jackal jackal;
    private int recursion = 0;
    private bool sidekickJackalCanMakeSidekick = false;

	public override IOptionLoader Loader => jackal.Loader;

	public Sidekick(
        Jackal jackal,
        byte jackalPlayerId,
        bool isImpostor,
        Jackal.SidekickOptionHolder option) : base(
            ExtremeRoleId.Sidekick,
            ExtremeRoleType.Neutral,
            ExtremeRoleId.Sidekick.ToString(),
            ColorPalette.JackalBlue,
            option.CanKill, false,
            option.UseVent, option.UseSabotage)
    {
        this.jackal = jackal;
        this.jackalPlayerId = jackalPlayerId;
        this.SetControlId(jackal.GameControlId);

        this.HasOtherKillCool = option.HasOtherKillCool;
        this.KillCoolTime = option.KillCool;
        this.HasOtherKillRange = option.HasOtherKillRange;
        this.KillRange = option.KillRange;

        this.HasOtherVision = option.HasOtherVision;
        this.Vision = option.Vision;
        this.IsApplyEnvironmentVision = option.ApplyEnvironmentVisionEffect;

        this.FakeImposter = jackal.CanSeeImpostorToSidekickImpostor && isImpostor;

        this.recursion = jackal.CurRecursion;
        this.sidekickJackalCanMakeSidekick = jackal.SidekickJackalCanMakeSidekick;
    }

    public void RemoveParent(byte rolePlayerId)
    {
        jackal.SidekickPlayerId.Remove(rolePlayerId);
    }

    public override bool IsSameTeam(SingleRoleBase targetRole)
    {
        if (this.isSameJackalTeam(targetRole))
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

    public override Color GetTargetRoleSeeColor(
        SingleRoleBase targetRole,
        byte targetPlayerId)
    {
        if (targetRole.Id is ExtremeRoleId.Jackal &&
			targetPlayerId == this.jackalPlayerId)
        {
            return ColorPalette.JackalBlue;
        }
        return base.GetTargetRoleSeeColor(targetRole, targetPlayerId);
    }

    public override string GetFullDescription()
    {
        var jackal = Player.GetPlayerControlById(this.jackalPlayerId);
        string fullDesc = base.GetFullDescription();

        if (jackal == null ||
			jackal.Data == null)
		{
			return fullDesc;
		}

        return string.Format(
            fullDesc, jackal.Data.PlayerName);
    }

    public static void BecomeToJackal(byte callerId, byte targetId)
    {

        Jackal curJackal = ExtremeRoleManager.GetSafeCastedRole<Jackal>(callerId);
        if (curJackal == null) { return; }
        Jackal newJackal = (Jackal)curJackal.Clone();

        bool multiAssignTrigger = false;
        var curRole = ExtremeRoleManager.GameRole[targetId];
        var curSideKick = curRole as Sidekick;
        if (curSideKick == null)
        {
            curSideKick = (Sidekick)((MultiAssignRoleBase)curRole).AnotherRole;
            multiAssignTrigger = true;
        }

        newJackal.Initialize();
        if (targetId == PlayerControl.LocalPlayer.PlayerId)
        {
            newJackal.CreateAbility();
        }

        if ((
                !curSideKick.sidekickJackalCanMakeSidekick ||
                curSideKick.recursion >= newJackal.SidekickRecursionLimit
            ) &&
            newJackal.Button?.Behavior is ICountBehavior countBehavior)
        {
            countBehavior.SetAbilityCount(0);
        }

        newJackal.CurRecursion = curSideKick.recursion + 1;
        newJackal.SidekickPlayerId = new List<byte> (curJackal.SidekickPlayerId);
        newJackal.SetControlId(curSideKick.GameControlId);

        newJackal.SidekickPlayerId.Remove(targetId);

        if (multiAssignTrigger)
        {
            var multiAssignRole = (MultiAssignRoleBase)curRole;
            multiAssignRole.AnotherRole = null;
            multiAssignRole.CanKill = false;
            multiAssignRole.HasTask = false;
            multiAssignRole.UseSabotage = false;
            multiAssignRole.UseVent = false;

            ExtremeRoleManager.SetNewAnothorRole(targetId, newJackal);
        }
        else
        {
            ExtremeRoleManager.SetNewRole(targetId, newJackal);
        }

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

    public void Update(PlayerControl rolePlayer)
    {
        if (Player.GetPlayerControlById(this.jackalPlayerId).Data.Disconnected)
        {
            this.jackal.SidekickPlayerId.Clear();
			ExtremeRoleManager.RpcReplaceRole(
				this.jackalPlayerId, rolePlayer.PlayerId,
				ExtremeRoleManager.ReplaceOperation.SidekickToJackal);
        }
    }

    private bool isSameJackalTeam(SingleRoleBase targetRole)
    {
        return ((targetRole.Id == this.Id) || (targetRole.Id == ExtremeRoleId.Jackal));
    }
}
