using AmongUs.GameOptions;
using ExtremeRoles.GameMode;
using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Module.Ability;
using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.CustomOption.Implemented;
using ExtremeRoles.Module.CustomOption.Interfaces;
using ExtremeRoles.Resources;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using System;
using System.Collections.Generic;
using UnityEngine;


#nullable enable

namespace ExtremeRoles.Roles.Solo.Neutral.Jackal;

public sealed class JackalRole : SingleRoleBase, IRoleAutoBuildAbility, IRoleSpecialReset
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

    public List<byte> SidekickPlayerId = [];

    public ExtremeAbilityButton? Button { get; set; }

    public int NumAbility = 0;
    public int CurRecursion = 0;
    public int SidekickRecursionLimit = 0;

    public bool SidekickJackalCanMakeSidekick = false;
    public bool ForceReplaceLover = false;
    public bool CanSetImpostorToSidekick = false;
    public bool CanSeeImpostorToSidekickImpostor = false;

    public SidekickOptionHolder SidekickOption;

    public PlayerControl? Target;

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
			var rangeOptActive = new ParentActive(killRangeOption);
			factory.CreateSelectionOption(
				JackalOption.SidekickKillRange,
				OptionCreator.Range, rangeOptActive);

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

            UseSabotage = loader.GetValue<JackalOption, bool>(
                JackalOption.SidekickUseSabotage);
            UseVent = loader.GetValue<JackalOption, bool>(
                JackalOption.SidekickUseVent);

            CanKill = loader.GetValue<JackalOption, bool>(
                JackalOption.SidekickCanKill);

            HasOtherKillCool = loader.GetValue<JackalOption, bool>(
                JackalOption.SidekickHasOtherKillCool);
            KillCool = Player.DefaultKillCoolTime;
            if (HasOtherKillCool)
            {
                KillCool = loader.GetValue<JackalOption, float>(
                    JackalOption.SidekickKillCoolDown);
            }

            HasOtherKillRange = loader.GetValue<JackalOption, bool>(
                JackalOption.SidekickHasOtherKillRange);
            KillRange = curOption.GetInt(Int32OptionNames.KillDistance);
            if (HasOtherKillRange)
            {
                KillRange = loader.GetValue<JackalOption, int>(
                    JackalOption.SidekickKillRange);
            }

			HasOtherVision = loader.GetValue<JackalOption, bool>(
				JackalOption.SidekickHasOtherVision);
            Vision = curOption.GetFloat(FloatOptionNames.CrewLightMod);
            ApplyEnvironmentVisionEffect = false;
            if (HasOtherVision)
            {
                Vision = loader.GetValue<JackalOption, float>(
                    JackalOption.SidekickVision);
                ApplyEnvironmentVisionEffect = loader.GetValue<JackalOption, bool>(
                    JackalOption.SidekickApplyEnvironmentVisionEffect);
            }
        }

        public SidekickOptionHolder Clone()
        {
            return (SidekickOptionHolder)MemberwiseClone();
        }
    }

    public JackalRole() : base(
		RoleCore.BuildNeutral(
			ExtremeRoleId.Jackal,
			ColorPalette.JackalBlue),
        true, false, true, false)
    { }

    public override SingleRoleBase Clone()
    {
        var jackal = (JackalRole)base.Clone();
        jackal.SidekickOption = SidekickOption.Clone();

        return jackal;
    }

    public static void TargetToSideKick(byte callerId, byte targetId)
    {
        PlayerControl targetPlayer = Player.GetPlayerControlById(targetId);
        SingleRoleBase targetRole = ExtremeRoleManager.GameRole[targetId];

        IRoleSpecialReset.ResetRole(targetId);

        var sourceJackal = ExtremeRoleManager.GetSafeCastedRole<JackalRole>(callerId);
        if (sourceJackal == null) { return; }
        var newSidekick = new SidekickRole(
            sourceJackal,
            callerId,
            targetRole.IsImpostor(),
            sourceJackal.SidekickOption);

        sourceJackal.SidekickPlayerId.Add(targetId);

        if (targetRole.Core.Id != ExtremeRoleId.Lover)
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

                lover.Core.Team = ExtremeRoleType.Neutral;
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
			UnityObjectLoader.LoadSpriteFromResources(
				ObjectPath.JackalSidekick));
    }

    public override Color GetTargetRoleSeeColor(
        SingleRoleBase targetRole,
        byte targetPlayerId)
    {
		var id = targetRole.Core.Id;

        if ((
                id is ExtremeRoleId.Sidekick &&
                SidekickPlayerId.Contains(targetPlayerId) ||

                id is ExtremeRoleId.Jackal
            ) && IsSameControlId(targetRole))
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
        if (isSameJackalTeam(targetRole))
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


        Target = Player.GetClosestPlayerInRange(
            PlayerControl.LocalPlayer,
            this, range[Mathf.Clamp(createSidekickRange, 0, 2)]);

        return Target != null && IRoleAbility.IsCommonUse();
    }

    public bool UseAbility()
    {
		if (this.Target == null)
		{
			return false;
		}

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

    public void ResetOnMeetingEnd(NetworkedPlayerInfo? exiledPlayer = null)
    {
        return;
    }

    public void SidekickToJackal(PlayerControl rolePlayer)
    {

        if (SidekickPlayerId.Count == 0) { return; }

        int numUpgrade = SidekickPlayerId.Count >= numUpgradeSidekick ?
            numUpgradeSidekick : SidekickPlayerId.Count;

        List<byte> updateSideKick = new List<byte>();

        for (int i = 0; i < numUpgrade; ++i)
        {
            int useIndex = UnityEngine.Random.Range(0, SidekickPlayerId.Count);
            byte targetPlayerId = SidekickPlayerId[useIndex];
            SidekickPlayerId.Remove(targetPlayerId);

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
        SidekickToJackal(rolePlayer);
    }

    protected override void CreateSpecificOption(
        AutoParentSetOptionCategoryFactory factory)
    {
        // JackalOption
        createJackalOption(factory);

		// SideKickOption
		factory.OptionPrefix = string.Empty;
        SidekickOption = new SidekickOptionHolder(factory);
    }

    protected override void RoleSpecificInit()
    {
        CurRecursion = 0;
        SidekickPlayerId = new List<byte>();

        var cate = Loader;

        SidekickRecursionLimit = cate.GetValue<JackalOption, int>(
            JackalOption.SidekickLimitNum);

        canLoverSidekick = cate.GetValue<JackalOption, bool>(
            JackalOption.CanLoverSidekick);

        ForceReplaceLover = cate.GetValue<JackalOption, bool>(
            JackalOption.ForceReplaceLover);

        CanSetImpostorToSidekick = cate.GetValue<JackalOption, bool>(
            JackalOption.CanSetImpostorToSidekick);
        CanSeeImpostorToSidekickImpostor = cate.GetValue<JackalOption, bool>(
            JackalOption.CanSeeImpostorToSidekickImpostor);

        SidekickJackalCanMakeSidekick = cate.GetValue<JackalOption, bool>(
            JackalOption.SidekickJackalCanMakeSidekick);

        createSidekickRange = cate.GetValue<JackalOption, int>(
            JackalOption.RangeSidekickTarget);

        numUpgradeSidekick = cate.GetValue<JackalOption, int>(
            JackalOption.UpgradeSidekickNum);

        SidekickOption.ApplyOption(cate);
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
            return CanSetImpostorToSidekick;
        }
        return true;
    }

    private bool isLoverAndSetTarget(byte playerId)
    {
        if (ExtremeRoleManager.GameRole[playerId].Core.Id == ExtremeRoleId.Lover)
        {
            return canLoverSidekick;
        }
        return true;
    }

    private bool isSameJackalTeam(SingleRoleBase targetRole)
    {
		var id = targetRole.Core.Id;
		return id == Core.Id || id is ExtremeRoleId.Sidekick;
    }
}
