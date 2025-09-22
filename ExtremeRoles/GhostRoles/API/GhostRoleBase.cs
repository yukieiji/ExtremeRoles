using ExtremeRoles.GhostRoles.API.Interface;
using ExtremeRoles.Helper;
using ExtremeRoles.Module.Ability;
using ExtremeRoles.Module.Ability.Behavior.Interface;
using ExtremeRoles.Module.CustomOption.Interfaces;
using ExtremeRoles.Module.Interface;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ExtremeRoles.GhostRoles.API;

#nullable enable

public enum GhostRoleOption
{
    IsReportAbility = 90
}

public abstract class GhostRoleBase
{

	public int GameControlId { get; protected set; }
	public bool HasTask { get; protected set; }

	public ExtremeAbilityButton? Button { get; protected set; }

	public GhostRoleCore Core { get; }
	public IGhostTeam Team { get; }
	public IGhostRoleVisual Visual { get; protected set; }
    private int controlId;

	public virtual IOptionLoader Loader
	{
		get
		{
			if (!OptionManager.Instance.TryGetCategory(
					this.Core.Tab,
					ExtremeRolesPlugin.Instance.Provider.GetRequiredService<IRoleParentOptionIdGenerator>().Get(this.Core.Id),
					out var cate))
			{
				throw new ArgumentException("Can't find category");
			}
			return cate.Loader;
		}
	}

	public GhostRoleBase(
		bool hasTask,
		GhostRoleCore core,
		IGhostRoleVisual? visual=null)
	{
		this.Core = core;
		this.Team = new GhostTeam(this.Core.DefaultTeam);

		visual ??= new DefaultGhostRoleVisual(this.Core);
		this.Visual = visual;

		this.HasTask = hasTask;
	}

	public GhostRoleBase(
        bool hasTask,
        ExtremeRoleType team,
        ExtremeGhostRoleId id,
        string roleName,
        Color color,
        OptionTab tab = OptionTab.GeneralTab,
		IGhostRoleVisual? visual = null)
    {
		if (tab == OptionTab.GeneralTab)
		{
			tab = team switch
			{
				ExtremeRoleType.Crewmate => OptionTab.GhostCrewmateTab,
				ExtremeRoleType.Impostor => OptionTab.GhostImpostorTab,
				ExtremeRoleType.Neutral => OptionTab.GhostNeutralTab,
				_ => OptionTab.GeneralTab,
			};
		}

		this.Core = new GhostRoleCore(
			roleName,
			id, color, team, tab);
		this.Team = new GhostTeam(team);

		visual ??= new DefaultGhostRoleVisual(this.Core);
		this.Visual = visual;


		this.HasTask = hasTask;
    }

    public virtual GhostRoleBase Clone()
    {
        GhostRoleBase copy = (GhostRoleBase)this.MemberwiseClone();
        Color baseColor = this.Core.Color;

		if (this is ICombination combRole &&
			copy is ICombination copyComb)
		{
			copyComb.OffsetInfo = combRole.OffsetInfo;
		}

		/*
        copy.Color = new Color(
            baseColor.r,
            baseColor.g,
            baseColor.b,
            baseColor.a);
		*/

        return copy;
    }

    public string GetColoredRoleName() => this.Visual.ColoredRoleName;

    public virtual string GetFullDescription() => Tr.GetString(
       $"{this.Core.Id}FullDescription");

    public virtual Color GetTargetRoleSeeColor(
        byte targetPlayerId, SingleRoleBase targetRole, GhostRoleBase? targetGhostRole)
    {
        var overLoader = targetRole as Roles.Solo.Impostor.OverLoader;

        if (overLoader != null &&
			overLoader.IsOverLoad)
        {
			return Palette.ImpostorRed;
		}

        bool isGhostRoleImpostor = false;
        if (targetGhostRole != null)
        {
            isGhostRoleImpostor = targetGhostRole.Team.IsImpostor();
        }

        if ((targetRole.IsImpostor() || targetRole.FakeImpostor || isGhostRoleImpostor) &&
            this.Team.IsImpostor())
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

        var loader = this.Loader;
        this.Button.Behavior.SetCoolTime(
            loader.GetValue<RoleAbilityCommonOption, float>(RoleAbilityCommonOption.AbilityCoolTime));

        if (this.Button.Behavior is IActivatingBehavior activatingBehavior &&
			loader.TryGetValueOption<RoleAbilityCommonOption, float>(
                RoleAbilityCommonOption.AbilityActiveTime,
				out var activeTimeOtion))
        {
			activatingBehavior.ActiveTime = activeTimeOtion.Value;
        }

        if (this.Button.Behavior is ICountBehavior behavior &&
			loader.TryGetValueOption<RoleAbilityCommonOption, int>(
                RoleAbilityCommonOption.AbilityCount,
                out var countOption))
        {
            behavior.SetAbilityCount(countOption.Value);
        }
        this.Button.OnMeetingEnd();
    }

    protected bool IsReportAbility() => this.Loader.GetValue<GhostRoleOption, bool>(GhostRoleOption.IsReportAbility);

    public abstract void CreateAbility();

    public abstract HashSet<Roles.ExtremeRoleId> GetRoleFilter();

    protected abstract void OnMeetingEndHook();

    protected abstract void OnMeetingStartHook();

    protected abstract void UseAbility(RPCOperator.RpcCaller caller);

	protected static bool IsCommonUse()
	{
		var localPlayer = PlayerControl.LocalPlayer;

		return
			localPlayer != null &&
			localPlayer.Data != null &&
			localPlayer.Data.IsDead &&
			localPlayer.CanMove;
	}
	protected static bool IsCommonUseWithMinigame()
	{
		var localPlayer = PlayerControl.LocalPlayer;
		var hud = HudManager.Instance;
		return
			!(
				localPlayer == null ||
				localPlayer.Data == null ||
				!localPlayer.Data.IsDead ||
				localPlayer.inVent ||
				localPlayer.MyPhysics.DoingCustomAnimation ||
				localPlayer.shapeshifting ||
				localPlayer.waitingForShapeshiftResponse ||
				hud == null ||
				hud.Chat.IsOpenOrOpening ||
				hud.KillOverlay.IsOpen ||
				hud.GameMenu.IsOpen ||
				hud.IsIntroDisplayed ||
				(MapBehaviour.Instance != null && MapBehaviour.Instance.IsOpenStopped) ||
				MeetingHud.Instance != null ||
				PlayerCustomizationMenu.Instance != null ||
				ExileController.Instance != null ||
				IntroCutscene.Instance != null
			);
	}

	protected static void EnumCheck<T>(T isEnum) where T : struct, IConvertible
    {
        if (!typeof(int).IsAssignableFrom(Enum.GetUnderlyingType(typeof(T))))
        {
            throw new ArgumentException(nameof(T));
        }
    }
}
