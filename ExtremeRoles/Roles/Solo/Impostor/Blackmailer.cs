using System;
using System.Collections.Generic;

using UnityEngine;

using ExtremeRoles.Extension.Player;
using ExtremeRoles.GameMode;
using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Module.Ability;
using ExtremeRoles.Module.Ability.Factory;
using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.SystemType;
using ExtremeRoles.Module.SystemType.Roles;
using ExtremeRoles.Performance.Il2Cpp;
using ExtremeRoles.Resources;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;

#nullable enable

namespace ExtremeRoles.Roles.Solo.Impostor;

public sealed class Blackmailer : SingleRoleBase, IRoleAutoBuildAbility, IRoleUpdate
{
	public enum BlackmailerOption
	{
		Range,
		MultipleBlackmail,
	}

	public ExtremeAbilityButton Button
	{
		get => this.blackmailAbilityButton!;
		set => this.blackmailAbilityButton = value;
	}

	private ExtremeAbilityButton? blackmailAbilityButton;
	private PlayerControl? tmpTarget;
	private PlayerControl? target;
	private float range;
	private bool multipleBlackmail;
	private BlackmailerSystem? system;

	public Blackmailer() : base(
		RoleArgs.BuildImpostor(
			ExtremeRoleId.Blackmailer,
			RolePropPresets.OptionalDefault))
	{ }

	public void CreateAbility()
	{
		this.CreateActivatingAbilityCountButton(
			"blackmail",
			UnityObjectLoader.LoadSpriteFromResources(ObjectPath.TestButton),
			IsAbilityCheck,
			CleanUp,
			ForceCleanUp);
	}

	public bool IsAbilityCheck()
	{
		if (this.target == null)
		{
			return false;
		}
		return Helper.Player.IsPlayerInRangeAndDrawOutLine(
			PlayerControl.LocalPlayer,
			this.target,
			this,
			this.range);
	}

	public bool UseAbility()
	{
		this.target = this.tmpTarget;
		return true;
	}

	public bool IsAbilityUse()
	{
		this.tmpTarget = Helper.Player.GetClosestPlayerInRange(
			PlayerControl.LocalPlayer, this, this.range);

		if (this.tmpTarget == null)
		{
			return false;
		}

		if (this.system != null)
		{
			if (!this.multipleBlackmail && this.system.HasBlackmailedAny)
			{
				return false;
			}

			if (this.system.IsBlackmailed(this.tmpTarget.PlayerId))
			{
				return false;
			}
		}

		return IRoleAbility.IsCommonUse();
	}

	public void CleanUp()
	{
		if (this.target != null && this.system != null)
		{
			if (!this.multipleBlackmail)
			{
				this.system.RpcClearBlackmail();
			}
			this.system.RpcAddBlackmail(this.target.PlayerId);
		}
		this.target = null;
	}

	public void ForceCleanUp()
	{
		this.target = null;
	}

	public void ResetOnMeetingStart()
	{ }

	public void ResetOnMeetingEnd(NetworkedPlayerInfo? exiledPlayer = null)
	{ }

	public void Update(PlayerControl rolePlayer)
	{ }

	public override string GetRolePlayerNameTag(SingleRoleBase targetRole, byte targetPlayerId)
	{
		if (this.system != null && this.system.IsBlackmailed(targetPlayerId))
		{
			return $"{base.GetRolePlayerNameTag(targetRole, targetPlayerId)}<color=#FF0000>({Tr.GetString("blackmailMark")})</color>";
		}
		return base.GetRolePlayerNameTag(targetRole, targetPlayerId);
	}

	protected override void CreateSpecificOption(AutoParentSetOptionCategoryFactory factory)
	{
		factory.CreateFloatOption(
			BlackmailerOption.Range,
			1.0f, 0.1f, 4.0f, 0.1f);

		IRoleAbility.CreateAbilityCountOption(
			factory,
			defaultAbilityCount: 3,
			maxAbilityCount: 15,
			defaultActiveTime: 3.0f);

		factory.CreateBoolOption(
			BlackmailerOption.MultipleBlackmail,
			false);
	}

	protected override void RoleSpecificInit()
	{
		var cate = this.Loader;
		this.range = cate.GetValue<BlackmailerOption, float>(BlackmailerOption.Range);
		this.multipleBlackmail = cate.GetValue<BlackmailerOption, bool>(BlackmailerOption.MultipleBlackmail);

		this.system = ExtremeSystemTypeManager.Instance.CreateOrGet<BlackmailerSystem>(
			ExtremeSystemType.BlackmailerSystem);
	}
}
