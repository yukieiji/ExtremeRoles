using System;

using UnityEngine;

using ExtremeRoles.GhostRoles;
using ExtremeRoles.GhostRoles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Module.Ability.Behavior;
using ExtremeRoles.Module.Ability.AutoActivator;
using ExtremeRoles.Performance;



using OptionFactory = ExtremeRoles.Module.CustomOption.Factory.AutoParentSetOptionCategoryFactory;

namespace ExtremeRoles.Module.Ability.Factory;

public static class GhostRoleAbilityFactory
{
	private const float defaultCoolTime = 60.0f;
	private const float minCoolTime = 5.0f;
	private const float maxCoolTime = 120.0f;
	private const float minActiveTime = 0.5f;
	private const float maxActiveTime = 30.0f;
	private const float step = 0.5f;

	public static void CreateButtonOption(
		OptionFactory factory,
		float defaultActiveTime = float.MaxValue)
	{

		factory.CreateNewFloatOption(
			RoleAbilityCommonOption.AbilityCoolTime,
			defaultCoolTime, minCoolTime,
			maxCoolTime, step,
			format: OptionUnit.Second);

		if (defaultActiveTime != float.MaxValue)
		{
			defaultActiveTime = Mathf.Clamp(
				defaultActiveTime, minActiveTime, maxActiveTime);

			factory.CreateNewFloatOption(
				RoleAbilityCommonOption.AbilityActiveTime,
				defaultActiveTime, minActiveTime, maxActiveTime, step,
				format: OptionUnit.Second);
		}

		factory.CreateNewBoolOption(
		   GhostRoleOption.IsReportAbility,
		   true);
	}

	public static void CreateCountButtonOption(
		OptionFactory factory,
		int defaultAbilityCount,
		int maxAbilityCount,
		float defaultActiveTime = float.MaxValue)
	{
		CreateButtonOption(factory, defaultActiveTime);

		factory.CreateNewIntOption(
			RoleAbilityCommonOption.AbilityCount,
			defaultAbilityCount, 1,
			maxAbilityCount, 1,
			format: OptionUnit.Shot);
	}

	public static ExtremeAbilityButton CreateReusableAbility(
		AbilityType type,
		Sprite img,
		bool isReport,
		Func<bool> abilityPreCheck,
		Func<bool> canUse,
		Action<RPCOperator.RpcCaller> ability,
		Action rpcHostCallAbility,
		Action abilityOff = null,
		Action forceAbilityOff = null,
		KeyCode hotKey = KeyCode.F)
	{

		return new ExtremeAbilityButton(
			new ReusableActivatingBehavior(
				text: Tr.GetString($"{type}Button"),
				img: img,
				canUse: CreateGhostRoleUseFunc(canUse),
				ability: CreateGhostRoleAbility(
					type, isReport, abilityPreCheck,
					ability, rpcHostCallAbility),
				abilityOff: abilityOff,
				forceAbilityOff: forceAbilityOff),
			new GhostRoleButtonActivator(),
			hotKey
		);
	}

	public static ExtremeAbilityButton CreateReusableActivatingAbility(
		AbilityType type,
		Sprite img,
		bool isReport,
		Func<bool> abilityPreCheck,
		Func<bool> canUse,
		Action<RPCOperator.RpcCaller> ability,
		Action rpcHostCallAbility,
		Func<bool> canActivating = null,
		Action abilityOff = null,
		Action forceAbilityOff = null,
		KeyCode hotKey = KeyCode.F)
	{

		return new ExtremeAbilityButton(
			new ReusableActivatingBehavior(
				text: Tr.GetString($"{type}Button"),
				img: img,
				canUse: CreateGhostRoleUseFunc(canUse),
				ability: CreateGhostRoleAbility(
					type, isReport, abilityPreCheck,
					ability, rpcHostCallAbility),
				canActivating: canActivating,
				abilityOff: abilityOff,
				forceAbilityOff: forceAbilityOff),
			new GhostRoleButtonActivator(),
			hotKey
		);
	}

	public static ExtremeAbilityButton CreateCountAbility(
		AbilityType type,
		Sprite img,
		bool isReport,
		Func<bool> abilityPreCheck,
		Func<bool> canUse,
		Action<RPCOperator.RpcCaller> ability,
		Action rpcHostCallAbility,
		bool isReduceOnActive = false,
		Action abilityOff = null,
		Action forceAbilityOff = null,
		KeyCode hotKey = KeyCode.F)
	{

		return new ExtremeAbilityButton(
			new ActivatingCountBehavior(
				text: Tr.GetString(
					string.Concat(type.ToString(), "Button")),
				img: img,
				canUse: CreateGhostRoleUseFunc(canUse),
				ability: CreateGhostRoleAbility(
					type, isReport, abilityPreCheck,
					ability, rpcHostCallAbility),
				abilityOff: abilityOff,
				forceAbilityOff: forceAbilityOff,
				isReduceOnActive: isReduceOnActive),
			new GhostRoleButtonActivator(),
			hotKey
		);
	}

	public static ExtremeAbilityButton CreateActivatingCountAbility(
		AbilityType type,
		Sprite img,
		bool isReport,
		Func<bool> abilityPreCheck,
		Func<bool> canUse,
		Action<RPCOperator.RpcCaller> ability,
		Action rpcHostCallAbility,
		bool isReduceOnActive = false,
		Func<bool> canActivating = null,
		Action abilityOff = null,
		Action forceAbilityOff = null,
		KeyCode hotKey = KeyCode.F)
	{

		return new ExtremeAbilityButton(
			new ActivatingCountBehavior(
				text: Tr.GetString($"{type}Button"),
				img: img,
				canUse: CreateGhostRoleUseFunc(canUse),
				ability: CreateGhostRoleAbility(
					type, isReport, abilityPreCheck,
					ability, rpcHostCallAbility),
				canActivating: canActivating,
				abilityOff: abilityOff,
				forceAbilityOff: forceAbilityOff,
				isReduceOnActive: isReduceOnActive),
			new GhostRoleButtonActivator(),
			hotKey
		);
	}

	public static Func<bool> CreateGhostRoleUseFunc(Func<bool> isUse)
	{
		return () =>
			isUse.Invoke() &&
			!PlayerTask.PlayerHasTaskOfType<IHudOverrideTask>(
				PlayerControl.LocalPlayer);
	}

	public static Func<bool> CreateGhostRoleAbility(
		AbilityType type, bool isReportAbility,
		Func<bool> abilityPreCheck,
		Action<RPCOperator.RpcCaller> ability,
		Action rpcHostCallAbility)
	{
		return () =>
		{
			if (!abilityPreCheck.Invoke())
			{
				return false;
			}

			using (var caller = RPCOperator.CreateCaller(
				RPCOperator.Command.UseGhostRoleAbility))
			{
				caller.WriteByte((byte)type);
				caller.WriteBoolean(isReportAbility);
				ability.Invoke(caller);
			}

			rpcHostCallAbility?.Invoke();

			if (isReportAbility)
			{
				MeetingReporter.Instance.AddMeetingStartReport(
					Tr.GetString(type.ToString()));
			}

			return true;
		};
	}
}
