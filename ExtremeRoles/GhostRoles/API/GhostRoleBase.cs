﻿using System;
using System.Collections.Generic;

using UnityEngine;

using ExtremeRoles.Helper;
using ExtremeRoles.Module.AbilityBehavior;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Performance;

using OptionFactory = ExtremeRoles.Module.CustomOption.Factorys.AutoParentSetFactory;

namespace ExtremeRoles.GhostRoles.API;

#nullable enable

public enum GhostRoleOption
{
    IsReportAbility = 40
}

public abstract class GhostRoleBase
{
	public Color Color { get; protected set; }

	public ExtremeRoleType Team { get; protected set; }
	public ExtremeGhostRoleId Id { get; protected set; }

	public int OptionIdOffset { get; protected set; }
	public int GameControlId { get; protected set; }

	public string Name { get; protected set; }
	public bool HasTask { get; protected set; }

	public Module.ExtremeAbilityButton? Button { get; protected set; }

    private OptionTab tab = OptionTab.General;
    private int controlId;

    public GhostRoleBase(
        bool hasTask,
        ExtremeRoleType team,
        ExtremeGhostRoleId id,
        string roleName,
        Color color,
        OptionTab tab = OptionTab.General)
    {
        this.HasTask = hasTask;
        this.Team = team;
        this.Id = id;
        this.Name = roleName;
        this.Color = color;

        if (tab == OptionTab.General)
        {
            switch (team)
            {
                case ExtremeRoleType.Crewmate:
                    this.tab = OptionTab.GhostCrewmate;
                    break;
                case ExtremeRoleType.Impostor:
                    this.tab = OptionTab.GhostImpostor;
                    break;
                case ExtremeRoleType.Neutral:
                    this.tab = OptionTab.GhostNeutral;
                    break;
            }
        }
        else
        {
            this.tab = tab;
        }
    }

    public virtual GhostRoleBase Clone()
    {
        GhostRoleBase copy = (GhostRoleBase)this.MemberwiseClone();
        Color baseColor = this.Color;

        copy.Color = new Color(
            baseColor.r,
            baseColor.g,
            baseColor.b,
            baseColor.a);

        return copy;
    }

    public void CreateRoleAllOption(int optionIdOffset)
    {
        this.OptionIdOffset = optionIdOffset;
        var parentOps = createOptionFactory(optionIdOffset);
        CreateSpecificOption(parentOps);
    }

    public void CreateRoleSpecificOption(
		OptionFactory factory, int optionIdOffset)
    {
        this.OptionIdOffset = optionIdOffset;
        CreateSpecificOption(factory);
    }

    public int GetRoleOptionId<T>(T option) where T : struct, IConvertible
    {
        EnumCheck(option);
        return GetRoleOptionId(Convert.ToInt32(option));
    }

    public int GetRoleOptionId(int option) => this.OptionIdOffset + option;

    public bool IsCrewmate() => this.Team == ExtremeRoleType.Crewmate;

    public bool IsImpostor() => this.Team == ExtremeRoleType.Impostor;

    public bool IsNeutral() => this.Team == ExtremeRoleType.Neutral;

    public bool IsVanillaRole() => this.Id == ExtremeGhostRoleId.VanillaRole;

    public virtual string GetColoredRoleName() => Design.ColoedString(
        this.Color, Translation.GetString(this.Name));

    public virtual string GetFullDescription() => Translation.GetString(
       $"{this.Id}FullDescription");

    public virtual string GetImportantText() =>
        Design.ColoedString(
            this.Color,
            string.Format("{0}: {1}",
                Design.ColoedString(
                    this.Color,
                    Translation.GetString(this.Name)),
                Translation.GetString(
                    $"{this.Id}ShortDescription")));

    public virtual Color GetTargetRoleSeeColor(
        byte targetPlayerId, SingleRoleBase targetRole, GhostRoleBase targetGhostRole)
    {
        var overLoader = targetRole as Roles.Solo.Impostor.OverLoader;

        if (overLoader != null)
        {
            if (overLoader.IsOverLoad)
            {
                return Palette.ImpostorRed;
            }
        }

        bool isGhostRoleImpostor = false;
        if (targetGhostRole != null)
        {
            isGhostRoleImpostor = targetGhostRole.IsImpostor();
        }

        if ((targetRole.IsImpostor() || targetRole.FakeImposter || isGhostRoleImpostor) &&
            this.IsImpostor())
        {
            return Palette.ImpostorRed;
        }

        return Color.clear;
    }

    public void SetGameControlId(int newId)
    {
        this.controlId = newId;
    }

    public void ResetOnMeetingEnd()
    {
        if (this.Button != null)
        {
            this.Button.OnMeetingEnd();
        }
        this.OnMeetingEndHook();
    }

    public void ResetOnMeetingStart()
    {
        if (this.Button != null)
        {
            this.Button.OnMeetingStart();
        }
        this.OnMeetingStartHook();
    }

    protected void ButtonInit()
    {
        if (this.Button == null) { return; }

        var allOps = OptionManager.Instance;
        this.Button.Behavior.SetCoolTime(
            allOps.GetValue<float>(this.GetRoleOptionId(RoleAbilityCommonOption.AbilityCoolTime)));

        if (allOps.TryGet<float>(
                this.GetRoleOptionId(
                    RoleAbilityCommonOption.AbilityActiveTime), out var activeTimeOtion) &&
			activeTimeOtion is not null)
        {
            this.Button.Behavior.SetActiveTime(activeTimeOtion.GetValue());
        }

        if (this.Button.Behavior is AbilityCountBehavior behavior &&
            allOps.TryGet<int>(
                this.GetRoleOptionId(
                    RoleAbilityCommonOption.AbilityCount),
                out var countOption) &&
			countOption is not null)
        {
            behavior.SetAbilityCount(countOption.GetValue());
        }
        this.Button.OnMeetingEnd();
    }

    protected bool isReportAbility() => OptionManager.Instance.GetValue<bool>(
        this.GetRoleOptionId(GhostRoleOption.IsReportAbility));

    private OptionFactory createOptionFactory(int offset)
    {
		var factory = new OptionFactory(offset, this.Name, this.tab);
		factory.CreateSelectionOption(
			RoleCommonOption.SpawnRate,
			OptionCreator.SpawnRate, null, true,
			color: this.Color);

        int spawnNum = this.IsImpostor() ? GameSystem.MaxImposterNum : GameSystem.VanillaMaxPlayerNum - 1;

		factory.CreateIntOption(
            RoleCommonOption.RoleNum,
            1, 1, spawnNum, 1);

		factory.CreateIntOption(RoleCommonOption.AssignWeight, 500, 1, 1000, 1, ignorePrefix: true);

		return factory;
    }

    public abstract void CreateAbility();

    public abstract HashSet<Roles.ExtremeRoleId> GetRoleFilter();

    public abstract void Initialize();

    protected abstract void OnMeetingEndHook();

    protected abstract void OnMeetingStartHook();

    protected abstract void CreateSpecificOption(OptionFactory parentOps);

    protected abstract void UseAbility(RPCOperator.RpcCaller caller);

	protected static bool IsCommonUse() =>
		CachedPlayerControl.LocalPlayer != null &&
		CachedPlayerControl.LocalPlayer.Data != null &&
		CachedPlayerControl.LocalPlayer.Data.IsDead &&
		CachedPlayerControl.LocalPlayer.PlayerControl.CanMove;

	protected static void EnumCheck<T>(T isEnum) where T : struct, IConvertible
    {
        if (!typeof(int).IsAssignableFrom(Enum.GetUnderlyingType(typeof(T))))
        {
            throw new ArgumentException(nameof(T));
        }
    }
}
