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

public sealed class Blackmailer : SingleRoleBase, IRoleAutoBuildAbility, IRoleSpecialSetUp, IRoleUpdate
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

	private Dictionary<byte, PoolablePlayer> playerIcons = new();
	private GridArrange? grid;

	public Blackmailer() : base(
		RoleArgs.BuildImpostor(
			ExtremeRoleId.Blackmailer,
			RolePropPresets.OptionalDefault))
	{ }

	public void CreateAbility()
	{
		this.CreateNormalActivatingAbilityButton(
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

		if (this.system != null &&
			!this.multipleBlackmail &&
			this.system.IsBlackmailed(this.tmpTarget.PlayerId))
		{
			return false;
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

	public void IntroBeginSetUp()
	{ }

	public void IntroEndSetUp()
	{
		GameObject bottomLeft = new GameObject("BlackmailIcons");
		bottomLeft.transform.SetParent(
			HudManager.Instance.UseButton.transform.parent.parent);
		AspectPosition aspectPosition = bottomLeft.AddComponent<AspectPosition>();
		aspectPosition.Alignment = AspectPosition.EdgeAlignments.LeftBottom;
		aspectPosition.anchorPoint = new Vector2(0.5f, 0.5f);
		aspectPosition.DistanceFromEdge = new Vector3(0.375f, 0.35f);
		aspectPosition.AdjustPosition();

		this.grid = bottomLeft.AddComponent<GridArrange>();
		this.grid.CellSize = new Vector2(0.5f, 0.75f);
		this.grid.MaxColumns = 14;
		this.grid.Alignment = GridArrange.StartAlign.Right;
		this.grid.cells = new();

		this.playerIcons = Helper.Player.CreatePlayerIcon(
			bottomLeft.transform, Vector3.one * 0.275f);
		updateShowIcon(true);
	}

	public void ResetOnMeetingStart()
	{
		foreach (var (_, poolPlayer) in this.playerIcons)
		{
			poolPlayer.gameObject.SetActive(false);
		}
	}

	public void ResetOnMeetingEnd(NetworkedPlayerInfo? exiledPlayer = null)
	{ }

	public void Update(PlayerControl rolePlayer)
	{
		if (!GameProgressSystem.IsTaskPhase)
		{
			return;
		}
		updateShowIcon();
	}

	protected override void CreateSpecificOption(AutoParentSetOptionCategoryFactory factory)
	{
		factory.CreateFloatOption(
			BlackmailerOption.Range,
			1.0f, 0.1f, 4.0f, 0.1f);

		IRoleAbility.CreateCommonAbilityOption(factory, 3.0f);

		factory.CreateBoolOption(
			BlackmailerOption.MultipleBlackmail,
			false);
	}

	protected override void RoleSpecificInit()
	{		var cate = this.Loader;
		this.range = cate.GetValue<BlackmailerOption, float>(BlackmailerOption.Range);
		this.multipleBlackmail = cate.GetValue<BlackmailerOption, bool>(BlackmailerOption.MultipleBlackmail);

		this.system = ExtremeSystemTypeManager.Instance.CreateOrGet<BlackmailerSystem>(
			ExtremeSystemType.BlackmailerSystem);
	}

	private void updateShowIcon(bool forceUpdate = false)
	{
		bool updateNeeded = forceUpdate;
		foreach (var (playerId, poolPlayer) in this.playerIcons)
		{
			bool isBlackmailed = this.system != null && this.system.IsBlackmailed(playerId);
			NetworkedPlayerInfo? player = GameData.Instance.GetPlayerById(playerId);

			bool shouldShow = isBlackmailed && player != null && !player.IsDead && !player.Disconnected;

			if (poolPlayer.gameObject.activeSelf != shouldShow)
			{
				poolPlayer.gameObject.SetActive(shouldShow);
				updateNeeded = true;
			}
		}

		if (updateNeeded && this.grid != null)
		{
			this.grid.ArrangeChilds();
		}
	}
}
