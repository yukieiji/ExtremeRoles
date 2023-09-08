using System;
using System.Collections.Generic;

using UnityEngine;

using ExtremeRoles.Helper;
using ExtremeRoles.Module.AbilityBehavior;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;

using OptionFactory = ExtremeRoles.Module.CustomOption.Factorys.AutoParentSetFactory;

namespace ExtremeRoles.GhostRoles.API;

#nullable enable

public enum GhostRoleOption
{
    IsReportAbility = 40
}

public abstract class GhostRoleBase
{
    private const float defaultCoolTime = 60.0f;
    private const float minCoolTime = 5.0f;
    private const float maxCoolTime = 120.0f;
    private const float minActiveTime = 0.5f;
    private const float maxActiveTime = 30.0f;
    private const float step = 0.5f;

    public ExtremeRoleType Team => this.TeamType;

    public ExtremeGhostRoleId Id => this.RoleId;

    public int OptionOffset => this.OptionIdOffset;

    public string Name => this.RoleName;

    public Module.ExtremeAbilityButton? Button { get; protected set; }

    public Color RoleColor => this.NameColor;
    public bool HasTask => this.Task;

    public int GameControlId => this.controlId;

    protected ExtremeRoleType TeamType;
    protected ExtremeGhostRoleId RoleId;
    protected string RoleName;
    protected Color NameColor;
    protected int OptionIdOffset;

    protected bool Task;

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
        this.Task = hasTask;
        this.TeamType = team;
        this.RoleId = id;
        this.RoleName = roleName;
        this.NameColor = color;

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
        Color baseColor = this.NameColor;

        copy.NameColor = new Color(
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

    public bool IsCrewmate() => this.TeamType == ExtremeRoleType.Crewmate;

    public bool IsImpostor() => this.TeamType == ExtremeRoleType.Impostor;

    public bool IsNeutral() => this.TeamType == ExtremeRoleType.Neutral;

    public bool IsVanillaRole() => this.RoleId == ExtremeGhostRoleId.VanillaRole;

    public virtual string GetColoredRoleName() => Design.ColoedString(
        this.NameColor, Translation.GetString(this.RoleName));

    public virtual string GetFullDescription() => Translation.GetString(
       $"{this.Id}FullDescription");

    public virtual string GetImportantText() =>
        Design.ColoedString(
            this.NameColor,
            string.Format("{0}: {1}",
                Design.ColoedString(
                    this.NameColor,
                    Translation.GetString(this.RoleName)),
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
			color: this.NameColor);

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
		PlayerControl.LocalPlayer &&
		PlayerControl.LocalPlayer.Data.IsDead &&
		PlayerControl.LocalPlayer.CanMove;

	protected static void CreateButtonOption(
		OptionFactory factory,
		float defaultActiveTime = float.MaxValue)
	{

		factory.CreateFloatOption(
			RoleAbilityCommonOption.AbilityCoolTime,
			defaultCoolTime, minCoolTime,
			maxCoolTime, step,
			format: OptionUnit.Second);

		if (defaultActiveTime != float.MaxValue)
		{
			defaultActiveTime = Mathf.Clamp(
				defaultActiveTime, minActiveTime, maxActiveTime);

			factory.CreateFloatOption(
				RoleAbilityCommonOption.AbilityActiveTime,
				defaultActiveTime, minActiveTime, maxActiveTime, step,
				format: OptionUnit.Second);
		}

		factory.CreateBoolOption(
		   GhostRoleOption.IsReportAbility,
		   true);
	}

	protected static void CreateCountButtonOption(
		OptionFactory factory,
		int defaultAbilityCount,
		int maxAbilityCount,
		float defaultActiveTime = float.MaxValue)
	{
		CreateButtonOption(factory, defaultActiveTime);

		factory.CreateIntOption(
			RoleAbilityCommonOption.AbilityCount,
			defaultAbilityCount, 1,
			maxAbilityCount, 1,
			format: OptionUnit.Shot);
	}


	protected static void EnumCheck<T>(T isEnum) where T : struct, IConvertible
    {
        if (!typeof(int).IsAssignableFrom(Enum.GetUnderlyingType(typeof(T))))
        {
            throw new ArgumentException(nameof(T));
        }
    }
}
