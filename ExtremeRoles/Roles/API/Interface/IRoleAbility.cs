using System;
using System.Runtime.CompilerServices;

using UnityEngine;

using ExtremeRoles.Module;
using ExtremeRoles.Module.AbilityFactory;
using ExtremeRoles.Module.AbilityBehavior.Interface;


using ExtremeRoles.Module.CustomOption.Factory;

using ExtremeRoles.Performance;
using ExtremeRoles.Module.CustomOption.Interfaces;

namespace ExtremeRoles.Roles.API.Interface;

public enum RoleAbilityCommonOption
{
    AbilityCoolTime = 35,
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

		if (cate.TryGetValueOption<RoleAbilityCommonOption, float>(
				RoleAbilityCommonOption.AbilityActiveTime,
				out var activeTimeOption))
		{
			this.Button.Behavior.SetActiveTime(activeTimeOption.Value);
		}

		if (this.Button.Behavior is ICountBehavior countBehavior)
		{
			countBehavior.SetAbilityCount(
				cate.GetValue<RoleAbilityCommonOption, int>(
					RoleAbilityCommonOption.AbilityCount));
		}

		this.Button.OnMeetingEnd();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	protected static bool IsCommonUse()
	{
		PlayerControl localPlayer = CachedPlayerControl.LocalPlayer;

		return
			localPlayer != null &&
			localPlayer.Data != null &&
			!localPlayer.Data.IsDead &&
			localPlayer.CanMove &&
			MeetingHud.Instance == null &&
			ExileController.Instance == null &&
			IntroCutscene.Instance == null;
	}

	private const float defaultCoolTime = 30.0f;
	private const float minCoolTime = 0.5f;
	private const float maxCoolTime = 120.0f;
	private const float minActiveTime = 0.5f;
	private const float maxActiveTime = 60.0f;
	private const float step = 0.5f;


	public static void CreateCommonAbilityOption(
		AutoParentSetOptionCategoryFactory factory,
		float defaultActiveTime = float.MaxValue,
		IOption parentOpt = null)
	{
		factory.CreateFloatOption(
			RoleAbilityCommonOption.AbilityCoolTime,
			defaultCoolTime, minCoolTime, maxCoolTime, step,
			parentOpt,
			format: OptionUnit.Second);

		if (defaultActiveTime != float.MaxValue)
		{
			defaultActiveTime = Mathf.Clamp(
				defaultActiveTime, minActiveTime, maxActiveTime);
			factory.CreateFloatOption(
				RoleAbilityCommonOption.AbilityActiveTime,
				defaultActiveTime, minActiveTime, maxActiveTime, step,
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
		Func<bool> checkAbility = null,
		Action abilityOff = null,
		Action forceAbilityOff = null,
		KeyCode hotkey = KeyCode.F)
	{

		self.Button = RoleAbilityFactory.CreateReusableAbility(
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
		Func<bool> checkAbility = null,
		Action abilityOff = null,
		Action forceAbilityOff = null,
		bool isReduceOnActive = false,
		KeyCode hotkey = KeyCode.F)
	{
		self.Button = RoleAbilityFactory.CreateCountAbility(
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

	public static void CreateChargeAbilityButton(
		this IRoleAutoBuildAbility self,
		string textKey,
		Sprite sprite,
		Func<bool> checkAbility = null,
		Action abilityOff = null,
		Action forceAbilityOff = null,
		KeyCode hotkey = KeyCode.F)
	{

		self.Button = RoleAbilityFactory.CreateChargableAbility(
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
