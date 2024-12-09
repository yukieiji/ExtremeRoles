using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

using UnityEngine;

using ExtremeRoles.Performance;
using ExtremeRoles.Module.Ability;
using ExtremeRoles.Module.Ability.Factory;
using ExtremeRoles.Module.Ability.Behavior.Interface;
using ExtremeRoles.Module.CustomOption.Interfaces;
using ExtremeRoles.Module.CustomOption.Factory;


namespace ExtremeRoles.Roles.API.Interface;

public enum RoleAbilityCommonOption
{
    AbilityCoolTime = 70,
    AbilityCount,
    AbilityActiveTime,
}

public interface IRoleAbility : IRoleResetMeeting
{
	public ExtremeAbilityButton Button
	{
		get;
		set;
	}

	public void CreateAbility();

	public void RoleAbilityInit()
	{
		if (this.Button == null) { return; }

		var cate = ((SingleRoleBase)this).Loader;
		this.Button.Behavior.SetCoolTime(
			cate.GetValue<RoleAbilityCommonOption, float>(RoleAbilityCommonOption.AbilityCoolTime));

		if (this.Button.Behavior is IActivatingBehavior activatingBehavior &&
			cate.TryGetValueOption<RoleAbilityCommonOption, float>(
				RoleAbilityCommonOption.AbilityActiveTime,
				out var activeTimeOption))
		{
			activatingBehavior.ActiveTime = activeTimeOption.Value;
		}

		if (this.Button.Behavior is ICountBehavior countBehavior &&
			cate.TryGetValueOption<RoleAbilityCommonOption, int>(
				RoleAbilityCommonOption.AbilityCount,
				out var countOption))
		{
			countBehavior.SetAbilityCount(countOption.Value);
		}

		this.Button.OnMeetingEnd();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	protected static bool IsCommonUse()
	{
		PlayerControl localPlayer = PlayerControl.LocalPlayer;

		return
			localPlayer != null &&
			localPlayer.Data != null &&
			!localPlayer.Data.IsDead &&
			localPlayer.CanMove;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	protected static bool IsCommonUseWithMinigame()
	{
		var localPlayer = PlayerControl.LocalPlayer;
		var hud = FastDestroyableSingleton<HudManager>.Instance;
		return
			!(
				localPlayer == null ||
				localPlayer.Data == null ||
				localPlayer.Data.IsDead ||
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

	public static bool IsLocalPlayerAbilityUse(in IReadOnlySet<ExtremeRoleId> fillter)
	{
		SingleRoleBase role = ExtremeRoleManager.GetLocalPlayerRole();

		return isAbilityUse(role, fillter) ||
			(
				role is MultiAssignRoleBase multiAssignRole &&
				isAbilityUse(multiAssignRole.AnotherRole, fillter)
			);
	}
	private static bool isAbilityUse(in SingleRoleBase role, in IReadOnlySet<ExtremeRoleId> fillter)
		=>
			role is not null &&
			fillter.Contains(role.Id) &&
			role is IRoleAbility abilityRole &&
			abilityRole.Button is not null &&
			abilityRole.Button.IsAbilityActive();

	public const float DefaultCoolTime = 30.0f;
	public const float MinCoolTime = 0.5f;
	public const float MaxCoolTime = 120.0f;
	private const float minActiveTime = 0.5f;
	private const float maxActiveTime = 60.0f;
	public const float Step = 0.5f;


	public static void CreateCommonAbilityOption(
		AutoParentSetOptionCategoryFactory factory,
		float defaultActiveTime = float.MaxValue,
		IOption parentOpt = null)
	{
		factory.CreateFloatOption(
			RoleAbilityCommonOption.AbilityCoolTime,
			DefaultCoolTime, MinCoolTime, MaxCoolTime, Step,
			parentOpt,
			format: OptionUnit.Second);

		if (defaultActiveTime != float.MaxValue)
		{
			defaultActiveTime = Mathf.Clamp(
				defaultActiveTime, minActiveTime, maxActiveTime);
			factory.CreateFloatOption(
				RoleAbilityCommonOption.AbilityActiveTime,
				defaultActiveTime, minActiveTime, maxActiveTime, Step,
				parentOpt,
				format: OptionUnit.Second);
		}

	}

	public static void CreateAbilityCountOption(
		AutoParentSetOptionCategoryFactory factory,
		int defaultAbilityCount,
		int maxAbilityCount,
		float defaultActiveTime = float.MaxValue,
		int minAbilityCount = 1,
		IOption parentOpt = null)
	{
		CreateCommonAbilityOption(
			factory,
			defaultActiveTime,
			parentOpt);

		factory.CreateIntOption(
			RoleAbilityCommonOption.AbilityCount,
			defaultAbilityCount, minAbilityCount,
			maxAbilityCount, 1,
			format: OptionUnit.Shot);

	}
}



public interface IRoleAutoBuildAbility : IRoleAbility
{
    public bool UseAbility();

    public bool IsAbilityUse();
}

public static class IRoleAutoBuildAbilityMixin
{
	public static void CreateNormalAbilityButton(
		this IRoleAutoBuildAbility self,
		string textKey,
		Sprite sprite,
		Action abilityOff = null,
		Action forceAbilityOff = null,
		KeyCode hotkey = KeyCode.F)
	{

		self.Button = RoleAbilityFactory.CreateReusableAbility(
			textKey: textKey,
			img: sprite,
			canUse: self.IsAbilityUse,
			ability: self.UseAbility,
			abilityOff: abilityOff,
			forceAbilityOff: forceAbilityOff,
			hotKey: hotkey);

		self.RoleAbilityInit();
	}

	public static void CreateNormalActivatingAbilityButton(
		this IRoleAutoBuildAbility self,
		string textKey,
		Sprite sprite,
		Func<bool> checkAbility = null,
		Action abilityOff = null,
		Action forceAbilityOff = null,
		KeyCode hotkey = KeyCode.F)
	{

		self.Button = RoleAbilityFactory.CreateActivatingReusableAbility(
			textKey: textKey,
			img: sprite,
			canUse: self.IsAbilityUse,
			ability: self.UseAbility,
			canActivating: checkAbility,
			abilityOff: abilityOff,
			forceAbilityOff: forceAbilityOff,
			hotKey: hotkey);

		self.RoleAbilityInit();
	}

	public static void CreateAbilityCountButton(
		this IRoleAutoBuildAbility self,
		string textKey,
		Sprite sprite,
		Action abilityOff = null,
		Action forceAbilityOff = null,
		KeyCode hotkey = KeyCode.F)
	{
		self.Button = RoleAbilityFactory.CreateCountAbility(
			textKey: textKey,
			img: sprite,
			canUse: self.IsAbilityUse,
			ability: self.UseAbility,
			abilityOff: abilityOff,
			forceAbilityOff: forceAbilityOff,
			hotKey: hotkey);

		self.RoleAbilityInit();
	}

	public static void CreateActivatingAbilityCountButton(
		this IRoleAutoBuildAbility self,
		string textKey,
		Sprite sprite,
		Func<bool> checkAbility = null,
		Action abilityOff = null,
		Action forceAbilityOff = null,
		bool isReduceOnActive = false,
		KeyCode hotkey = KeyCode.F)
	{
		self.Button = RoleAbilityFactory.CreateActivatingCountAbility(
			textKey: textKey,
			img: sprite,
			canUse: self.IsAbilityUse,
			ability: self.UseAbility,
			canActivating: checkAbility,
			abilityOff: abilityOff,
			forceAbilityOff: forceAbilityOff,
			isReduceOnActive: isReduceOnActive,
			hotKey: hotkey);

		self.RoleAbilityInit();
	}

	public static void CreateReclickableAbilityButton(
		this IRoleAutoBuildAbility self,
		string textKey,
		Sprite sprite,
		Func<bool> checkAbility = null,
		Action abilityOff = null,
		KeyCode hotkey = KeyCode.F)
	{
		self.Button = RoleAbilityFactory.CreateReclickAbility(
			textKey: textKey,
			img: sprite,
			canUse: self.IsAbilityUse,
			ability: self.UseAbility,
			canActivating: checkAbility,
			abilityOff: abilityOff,
			hotKey: hotkey);

		self.RoleAbilityInit();
	}

	public static void CreateReclickableCountAbilityButton(
		this IRoleAutoBuildAbility self,
		string textKey,
		Sprite sprite,
		Func<bool> checkAbility = null,
		Action abilityOff = null,
		KeyCode hotkey = KeyCode.F)
	{
		self.Button = RoleAbilityFactory.CreateReclickCountAbility(
			textKey: textKey,
			img: sprite,
			canUse: self.IsAbilityUse,
			ability: self.UseAbility,
			canActivating: checkAbility,
			abilityOff: abilityOff,
			hotKey: hotkey);

		self.RoleAbilityInit();
	}

	public static void CreateBatteryAbilityButton(
		this IRoleAutoBuildAbility self,
		string textKey,
		Sprite sprite,
		Func<bool> checkAbility = null,
		Action abilityOff = null,
		Action forceAbilityOff = null,
		KeyCode hotkey = KeyCode.F)
	{

		self.Button = RoleAbilityFactory.CreateBatteryAbility(
			textKey: textKey,
			img: sprite,
			canUse: self.IsAbilityUse,
			ability: self.UseAbility,
			canActivating: checkAbility,
			abilityOff: abilityOff,
			forceAbilityOff: forceAbilityOff,
			hotKey: hotkey);

		self.RoleAbilityInit();
	}

	public static void CreatePassiveAbilityButton(
		this IRoleAutoBuildAbility self,
		string activateButtonName,
		string deactivateButtonName,
		Sprite activateSprite,
		Sprite deactivateSprite,
		Action abilityCleanUp,
		Func<bool> checkAbility = null,
		KeyCode hotkey = KeyCode.F)
	{
		self.Button = RoleAbilityFactory.CreatePassiveAbility(
			activateTextKey: activateButtonName,
			activateImg: activateSprite,
			deactivateTextKey: deactivateButtonName,
			deactivateImg: deactivateSprite,
			canUse: self.IsAbilityUse,
			ability: self.UseAbility,
			canActivating: checkAbility,
			abilityOff: abilityCleanUp,
			hotKey: hotkey);

		self.RoleAbilityInit();
	}
}
