using Hazel;

using UnityEngine;

using ExtremeRoles.Module.Ability;
using ExtremeRoles.Module.Ability.AutoActivator;
using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.Interface;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Roles.API.Interface.Status;
using ExtremeRoles.Module.Interface;

#nullable enable

namespace ExtremeRoles.Roles.Solo.Impostor.Flasher;

/// <summary>
/// フラッシュで周囲の視界を奪うインポスター。
/// </summary>
public sealed class FlasherRole : SingleRoleBase, IRoleAbility
{
	public enum Option
	{
		Range,
		IsAffectImpostor,
		FlashColor,
	}

	public enum FlasherRpcOps : byte
	{
		FlashStart,
		FlashAfterglow,
		FlashEnd,
	}

	/// <summary>画面の塗りつぶしが継続する時間の既定値</summary>
	public const float DefaultActiveTime = 1.5f;

	public ExtremeAbilityButton? Button { get; set; }

	public override IStatusModel? Status => this.status;

	private FlasherStatusModel? status;
	private FlasherAbilityHandler? handler;

	public FlasherRole() : base(
		RoleArgs.BuildImpostor(ExtremeRoleId.Flasher))
	{ }

	public static void RpcOps(in MessageReader reader)
	{
		var ops = (FlasherRpcOps)reader.ReadByte();
		byte rolePlayerId = reader.ReadByte();

		if (!ExtremeRoleManager.TryGetSafeCastedRole<FlasherRole>(
			rolePlayerId, out var flasher))
		{
			return;
		}

		flasher.handler?.ApplyRpcOps(ops, reader);
	}

	/// <summary>
	/// 能力を使える状態かどうかを返す。
	/// </summary>
	public bool IsAbilityUse() => IRoleAbility.IsCommonUse();

	public void CreateAbility()
	{
		if (this.status == null || this.handler == null)
		{
			RoleSpecificInit();
		}

		this.Button = new ExtremeAbilityButton(
			this.handler!.CreateBehavior(),
			new RoleButtonActivator(),
			KeyCode.F);

		((IRoleAbility)this).RoleAbilityInit();
	}

	public void ResetOnMeetingStart()
	{
		this.handler?.ForceEnd();
	}

	public void ResetOnMeetingEnd(NetworkedPlayerInfo? exiledPlayer)
	{ }

	protected override void CreateSpecificOption(
		AutoParentSetOptionCategoryFactory factory)
	{
		IRoleAbility.CreateAbilityCountOption(
			factory, 2, 10, DefaultActiveTime);

		factory.CreateFloatOption(
			Option.Range,
			5.0f, 0.5f, 25.0f, 0.1f);

		factory.CreateBoolOption(Option.IsAffectImpostor, false);

		factory.CreateSelectionOption<Option, FlasherColor>(
			Option.FlashColor);
	}

	protected override void RoleSpecificInit()
	{
		if (this.status != null && this.handler != null)
		{
			return;
		}

		var loader = this.Loader;

		this.status = new FlasherStatusModel(
			loader.GetValue<Option, bool>(Option.IsAffectImpostor),
			loader.GetValue<Option, float>(Option.Range),
			(FlasherColor)loader.GetValue<Option, int>(Option.FlashColor));

		float activeTime = loader.TryGetValue(
			RoleAbilityCommonOption.AbilityActiveTime, out float loadedActiveTime)
			? loadedActiveTime
			: DefaultActiveTime;

		this.handler = new FlasherAbilityHandler(
			this.status, this, new FlasherScreenEffect(), activeTime, new DefaultUnityObjectFactory());
	}
}
