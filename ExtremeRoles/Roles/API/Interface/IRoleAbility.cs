using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

using UnityEngine;

using ExtremeRoles.Performance;
using ExtremeRoles.Module.Ability;
using ExtremeRoles.Module.Ability.Factory;
using ExtremeRoles.Module.Ability.Behavior.Interface;

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

		var allOpt = OptionManager.Instance;
		this.Button.Behavior.SetCoolTime(
			allOpt.GetValue<float>(
				this.GetRoleOptionId(RoleAbilityCommonOption.AbilityCoolTime)));

		if (this.Button.Behavior is IActivatingBehavior activatingBehavior &&
			allOpt.TryGet<float>(
				this.GetRoleOptionId(RoleAbilityCommonOption.AbilityActiveTime),
				out var activeTimeOption))
		{
			activatingBehavior.ActiveTime = activeTimeOption.GetValue();
		}

		if (this.Button.Behavior is ICountBehavior countBehavior)
		{
			countBehavior.SetAbilityCount(
				allOpt.GetValue<int>(this.GetRoleOptionId(
					RoleAbilityCommonOption.AbilityCount)));
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
}

public static class IRoleAbilityMixin
{
	private const float defaultCoolTime = 30.0f;
	private const float minCoolTime = 0.5f;
	private const float maxCoolTime = 120.0f;
	private const float minActiveTime = 0.5f;
	private const float maxActiveTime = 60.0f;
	private const float step = 0.5f;


	public static void CreateCommonAbilityOption(
		this IRoleAbility self,
		IOptionInfo parentOps,
		float defaultActiveTime = float.MaxValue)
	{

		SingleRoleBase role = (SingleRoleBase)self;

		new FloatCustomOption(
			self.GetRoleOptionId(RoleAbilityCommonOption.AbilityCoolTime),
			string.Concat(
				role.RoleName,
				RoleAbilityCommonOption.AbilityCoolTime.ToString()),
			defaultCoolTime, minCoolTime, maxCoolTime, step,
			parentOps, format: OptionUnit.Second,
			tab: role.Tab);

		if (defaultActiveTime != float.MaxValue)
		{
			defaultActiveTime = Mathf.Clamp(
				defaultActiveTime, minActiveTime, maxActiveTime);

			new FloatCustomOption(
				self.GetRoleOptionId(RoleAbilityCommonOption.AbilityActiveTime),
				string.Concat(
					role.RoleName,
					RoleAbilityCommonOption.AbilityActiveTime.ToString()),
				defaultActiveTime, minActiveTime, maxActiveTime, step,
				parentOps, format: OptionUnit.Second,
				tab: role.Tab);
		}

	}

	public static void CreateAbilityCountOption(
		this IRoleAbility self,
		IOptionInfo parentOps,
		int defaultAbilityCount,
		int maxAbilityCount,
		float defaultActiveTime = float.MaxValue,
		int minAbilityCount = 1)
	{

		SingleRoleBase role = (SingleRoleBase)self;

		self.CreateCommonAbilityOption(
			parentOps,
			defaultActiveTime);

		new IntCustomOption(
			self.GetRoleOptionId(RoleAbilityCommonOption.AbilityCount),
			string.Concat(
				role.RoleName,
				RoleAbilityCommonOption.AbilityCount.ToString()),
			defaultAbilityCount, minAbilityCount,
			maxAbilityCount, 1,
			parentOps, format: OptionUnit.Shot,
			tab: role.Tab);

	}

	public static int GetRoleOptionId(
		this IRoleAbility self,
		RoleAbilityCommonOption option) => ((RoleOptionBase)self).GetRoleOptionId((int)option);
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
