using System;
using System.Linq;

using AmongUs.GameOptions;
using UnityEngine;
using Microsoft.Extensions.DependencyInjection;

using ExtremeRoles.Compat;
using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Module.Interface;
using ExtremeRoles.Module.Ability;
using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.CustomOption.Interfaces;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Resources;

namespace ExtremeRoles.Roles.Combination.InvestigatorOffice;

public sealed class InvestigatorApprentice : MultiAssignRoleBase, IRoleAutoBuildAbility, IRoleReportHook
{

	public struct InvestigatorApprenticeOptionHolder
	{
		public int OptionOffset;
		public bool HasOtherVision;
		public float Vision;
		public bool ApplyEnvironmentVisionEffect;
		public bool HasOtherButton;
		public int HasOtherButtonNum;

		public enum InvestigatorApprenticeOption
		{
			HasOtherVision,
			Vision,
			ApplyEnvironmentVisionEffect,
			HasOtherButton,
			HasOtherButtonNum,
		}

		public static void CreateOption(
			AutoParentSetOptionCategoryFactory factory)
		{
			var visionOpt = factory.CreateBoolOption(
				InvestigatorApprenticeOption.HasOtherVision,
				false);

			factory.CreateFloatOption(
				InvestigatorApprenticeOption.Vision,
				2f, 0.25f, 5f, 0.25f,
				visionOpt,
				format: OptionUnit.Multiplier);

			factory.CreateBoolOption(
				InvestigatorApprenticeOption.ApplyEnvironmentVisionEffect,
				false, visionOpt);

			IRoleAbility.CreateAbilityCountOption(
				factory, 1, 10, 3.0f);

			var buttonOpt = factory.CreateBoolOption(
				InvestigatorApprenticeOption.HasOtherButton,
				false);
			factory.CreateIntOption(
				InvestigatorApprenticeOption.HasOtherButtonNum,
				1, 1, 10, 1, buttonOpt,
				format: OptionUnit.Shot);
		}

		public static InvestigatorApprenticeOptionHolder LoadOptions(in OptionLoadWrapper loader)
		{
			return new InvestigatorApprenticeOptionHolder()
			{
				HasOtherVision = loader.GetValue<InvestigatorApprenticeOption, bool>(
					InvestigatorApprenticeOption.HasOtherVision),
				Vision = loader.GetValue<InvestigatorApprenticeOption, float>(
					InvestigatorApprenticeOption.Vision),
				ApplyEnvironmentVisionEffect = loader.GetValue<InvestigatorApprenticeOption, bool>(
					InvestigatorApprenticeOption.ApplyEnvironmentVisionEffect),
				HasOtherButton = loader.GetValue<InvestigatorApprenticeOption, bool>(
					InvestigatorApprenticeOption.HasOtherButton),
				HasOtherButtonNum = loader.GetValue<InvestigatorApprenticeOption, int>(
					InvestigatorApprenticeOption.HasOtherButtonNum),
			};
		}
	}

	public ExtremeAbilityButton Button
	{
		get => meetingButton;
		set
		{
			meetingButton = value;
		}
	}

	private bool useAbility;
	private bool hasOtherButton;
	private bool callAnotherButton;
	private int buttonNum;
	private ExtremeAbilityButton meetingButton;
	private Minigame meeting;

	public override IOptionLoader Loader { get; }

	public InvestigatorApprentice(
		IOptionLoader loader,
		int gameControlId,
		InvestigatorApprenticeOptionHolder option
		) : base(
			RoleCore.BuildCrewmate(
				ExtremeRoleId.InvestigatorApprentice,
				ColorPalette.InvestigatorApprenticeKonai),
			false, true, false, false)
	{
		Loader = loader;
		SetControlId(gameControlId);
		HasOtherVision = option.HasOtherVision;
		if (HasOtherVision)
		{
			Vision = option.Vision;
			IsApplyEnvironmentVision = option.ApplyEnvironmentVisionEffect;
		}
		else
		{
			Vision = GameOptionsManager.Instance.CurrentGameOptions.GetFloat(
				FloatOptionNames.CrewLightMod);
		}
		hasOtherButton = option.HasOtherButton;
		buttonNum = option.HasOtherButtonNum;
		callAnotherButton = false;
	}

	public static void ChangeToDetectiveApprentice(
		byte playerId)
	{
		if (!(
				ExtremeRoleManager.TryGetRole(playerId, out var role) &&
				role is MultiAssignRoleBase prevRole
			))
		{
			return;
		}

		if (prevRole is IRoleResetMeeting detectiveReset)
		{
			detectiveReset.ResetOnMeetingStart();
		}

		bool hasAnotherRole = prevRole.AnotherRole != null;

		if (hasAnotherRole &&
			playerId == PlayerControl.LocalPlayer.PlayerId)
		{
			if (prevRole.AnotherRole is IRoleAbility abilityRole)
			{
				abilityRole.Button.OnMeetingStart();
				abilityRole.Button.OnMeetingEnd();
			}
			if (prevRole.AnotherRole is IRoleResetMeeting resetRole)
			{
				resetRole.ResetOnMeetingStart();
				resetRole.ResetOnMeetingEnd();
			}
		}

		if (!OptionManager.Instance.TryGetCategory(
				OptionTab.CombinationTab,
				ExtremeRolesPlugin.Instance.Provider.GetRequiredService<IRoleParentOptionIdGenerator>().Get(CombinationRoleType.InvestigatorOffice),
				out var cate))
		{
			return;
		}

		int offset = 2 * ExtremeRoleManager.OptionOffsetPerRole;
		var loader = new OptionLoadWrapper(cate, offset);
		InvestigatorApprentice newRole = new InvestigatorApprentice(
			loader,
			prevRole.GameControlId,
			InvestigatorApprenticeOptionHolder.LoadOptions(loader));
		if (playerId == PlayerControl.LocalPlayer.PlayerId)
		{
			newRole.CreateAbility();
		}
		if (hasAnotherRole)
		{
			newRole.AnotherRole = null;
			newRole.CanHasAnotherRole = true;
			newRole.SetAnotherRole(prevRole.AnotherRole);
			newRole.Core.Team = prevRole.AnotherRole.Core.Team;
		}

		ExtremeRoleManager.SetNewRole(playerId, newRole);
	}

	public void CleanUp()
	{
		if (meeting != null)
		{
			meeting.Close();
			useAbility = false;
			meeting = null;
		}
	}

	public void CreateAbility()
	{

		this.CreateActivatingAbilityCountButton(
			"emergencyMeeting",
			UnityObjectLoader.LoadFromResources<Sprite>(ObjectPath.Meeting),
			abilityOff: CleanUp,
			checkAbility: IsOpen,
			isReduceOnActive: true);
		Button.SetLabelToCrewmate();
	}

	public bool IsAbilityUse() =>
		IRoleAbility.IsCommonUse() && Minigame.Instance == null;

	public bool IsOpen() => Minigame.Instance != null;

	public void ResetOnMeetingEnd(NetworkedPlayerInfo exiledPlayer = null)
	{
		meeting = null;
		callAnotherButton = false;
	}

	public void ResetOnMeetingStart()
	{
		if (useAbility)
		{
			callAnotherButton = true;
		}
		CleanUp();
	}

	public bool UseAbility()
	{
		useAbility = false;
		SystemConsole emergencyConsole;
		if (CompatModManager.Instance.TryGetModMap(out var modMap))
		{
			emergencyConsole = modMap.GetSystemConsole(
				Compat.Interface.SystemConsoleType.EmergencyButton);
		}
		else
		{
			// 0 = Skeld
			// 1 = Mira HQ
			// 2 = Polus
			// 3 = Dleks - deactivated
			// 4 = Airship
			string key = Map.Id switch
			{
				0 or 1 or 3 => "EmergencyConsole",
				2 => "EmergencyButton",
				4 => "task_emergency",
				5 => "ConchEmergencyButton",
				_ => string.Empty,
			};
			var systemConsoleArray = UnityEngine.Object.FindObjectsOfType<SystemConsole>();
			emergencyConsole = systemConsoleArray.FirstOrDefault(x => x.gameObject.name.Contains(key));

		}

		if (emergencyConsole == null || Camera.main == null)
		{
			return false;
		}

		meeting = MinigameSystem.Open(
			emergencyConsole.MinigamePrefab);
		useAbility = true;

		return true;

	}

	public void HookReportButton(
		PlayerControl rolePlayer,
		NetworkedPlayerInfo reporter)
	{
		if (callAnotherButton &&
			PlayerControl.LocalPlayer.PlayerId == reporter.PlayerId &&
			hasOtherButton &&
			buttonNum > 0)
		{
			--buttonNum;
			++rolePlayer.RemainingEmergencies;
			callAnotherButton = false;
		}
	}

	public void HookBodyReport(
		PlayerControl rolePlayer,
		NetworkedPlayerInfo reporter,
		NetworkedPlayerInfo reportBody)
	{
		return;
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
