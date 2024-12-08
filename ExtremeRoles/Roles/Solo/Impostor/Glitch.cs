﻿using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Buffers;

using UnityEngine;

using ExtremeRoles.Helper;
using ExtremeRoles.Compat.Interface;
using ExtremeRoles.Module.Ability;
using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.SystemType.Roles;
using ExtremeRoles.Module.SystemType;
using ExtremeRoles.Resources;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Performance;

using BepInEx.Unity.IL2CPP.Utils;


#nullable enable

namespace ExtremeRoles.Roles.Solo.Impostor;

public sealed class Glitch : SingleRoleBase, IRoleAutoBuildAbility
{
	public enum Ops : byte
	{
		Range,
		EffectOnImpo,
		Delay,
		ActiveTime,
	}

	public ExtremeAbilityButton? Button { get; set; }

	private GlitchDummySystem? system;

	private List<(SystemConsoleType, Vector3)> allPos = [];

	private float timer;
	private float range;

	private SystemConsoleType console = SystemConsoleType.EmergencyButton;

	public Glitch() : base(
		ExtremeRoleId.Glitch,
		ExtremeRoleType.Impostor,
		ExtremeRoleId.Glitch.ToString(),
		Palette.ImpostorRed,
		true, false, true, true)
	{ }

	public void CreateAbility()
	{
		this.CreateNormalAbilityButton(
			"dummyDeadBody",
			UnityObjectLoader.LoadFromResources<Sprite>(ObjectPath.TestButton));
		this.allPos.Clear();
	}

	public bool IsAbilityUse()
	{
		if (!IRoleAbility.IsCommonUse())
		{
			return false;
		}

		if (this.allPos.Count == 0)
		{
			addSecurity();
			addVital();
			addAdmin();
		}

		var playerPos = PlayerControl.LocalPlayer.GetTruePosition();
		var rent = ArrayPool<(float, SystemConsoleType)>.Shared.Rent(this.allPos.Count);
		int index = 0;

		foreach (var (console, pos) in this.allPos)
		{
			Vector2 vector = new Vector2(pos.x, pos.y) - playerPos;
			float magnitude = vector.magnitude;
			if (magnitude <= this.range)
			{
				rent[index] = (magnitude, console);
				index++;
			}
		}

		var sliced = rent[0..index];
		Array.Sort(sliced, (a, b) => {
			float aMag = a.Item1;
			float bMag = b.Item1;
			if (aMag > bMag)
			{
				return 1;
			}
			if (aMag < bMag)
			{
				return -1;
			}
			return 0;
		});

		this.console = sliced.First().Item2;

		ArrayPool<(float, SystemConsoleType)>.Shared.Return(rent);

		return this.console is not SystemConsoleType.EmergencyButton;
	}

	public void ResetOnMeetingEnd(NetworkedPlayerInfo? exiledPlayer = null)
	{
		return;
	}

	public void ResetOnMeetingStart()
	{
		return;
	}

	public bool UseAbility()
	{
		if (FastDestroyableSingleton<HudManager>.Instance == null ||
			this.console is SystemConsoleType.EmergencyButton)
		{
			return false;
		}
		FastDestroyableSingleton<HudManager>.Instance.StartCoroutine(
			delayActive(this.console, this.timer));

		this.console = SystemConsoleType.EmergencyButton;

		return true;
	}

	protected override void CreateSpecificOption(
		AutoParentSetOptionCategoryFactory factory)
	{
		IRoleAbility.CreateAbilityCountOption(
			factory, 2, 10);
		factory.CreateFloatOption(
			Ops.Range, 1.5f, 0.1f, 7.5f, 0.1f,
			format: OptionUnit.Second);
		factory.CreateBoolOption(
			Ops.EffectOnImpo, false);
		factory.CreateFloatOption(
			Ops.Delay, 5.0f, 0.0f, 30.0f, 0.5f,
			format: OptionUnit.Second);
		factory.CreateIntOption(
			Ops.ActiveTime, 5, 0, 120, 1,
			format: OptionUnit.Second);
		this.allPos = [];
	}

	protected override void RoleSpecificInit()
	{
		var loader = this.Loader;

		this.timer = loader.GetValue<Ops, float>(Ops.Delay);
		this.range = loader.GetValue<Ops, float>(Ops.Range);

		_ = ExtremeSystemTypeManager.Instance.CreateOrGet(
			ExtremeSystemType.GlitchDummySystem,
			() => new GlitchDummySystem(
				!loader.GetValue<Ops, bool>(Ops.EffectOnImpo),
				loader.GetValue<Ops, float>(Ops.ActiveTime)));
	}

	private IEnumerator delayActive(SystemConsoleType type, float timer)
	{
		if (this.system is null)
		{
			yield break;
		}

		var waiter = new WaitForFixedUpdate();
		while (timer > 0.0f)
		{
			timer -= Time.fixedDeltaTime;
			yield return waiter;
		}
		ExtremeSystemTypeManager.RpcUpdateSystem(
			ExtremeSystemType.GlitchDummySystem, x => x.Write((byte)type));
	}

	private void addSecurity()
	{
		var security = Map.GetSecuritySystemConsole();
		if (security != null)
		{
			this.allPos.Add(
				(SystemConsoleType.SecurityCamera,
				security.transform.position));
		}
	}

	private void addVital()
	{
		var vital = Map.GetSecuritySystemConsole();
		if (vital != null)
		{
			this.allPos.Add(
				(SystemConsoleType.VitalsLabel,
				vital.transform.position));
		}
	}

	private void addAdmin()
	{
		var allAdmin = Map.GetAdminConsole();
		foreach (var admin in allAdmin)
		{
			this.allPos.Add(
				(SystemConsoleType.AdminModule,
				admin.transform.position));
		}
	}
}