using Hazel;
using Microsoft.Extensions.DependencyInjection;
using UnityEngine;

using ExtremeRoles.GameMode.RoleSelector;
using ExtremeRoles.Module.Ability;
using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Roles.API.Interface.Status;

#nullable enable

namespace ExtremeRoles.Roles.Solo.Liberal.Encloser;

public sealed class EncloserRole : SingleRoleBase, IRoleAutoBuildAbility, IRoleUpdate, IRoleResetMeeting
{

	public enum Option
	{
		MetsuLimit,
		MetsuKillMoney
	}

	public enum Mode : byte
	{
		Stake,
		Metsu
	}

	public enum RpcOpsType : byte
	{
		PlaceStake,
		UseMetsu
	}

	public ExtremeAbilityButton Button
	{
		get => this.abilityHandler != null ? this.abilityHandler.Button : null!;
		set
		{
			if (this.abilityHandler != null)
			{
				this.abilityHandler.Button = value;
			}
		}
	}

	public override IStatusModel? Status => this.status;

	private EncloserStatusModel? status;
	private EncloserAbilityHandler? abilityHandler;

	public EncloserRole() : base(
		RoleArgs.BuildLiberalMilitant(ExtremeRoleId.Encloser))
	{
	}

	public override string GetRoleTag()
		=> "Ec";

	public void CreateAbility()
	{
		var loader = this.Loader;
		int stakeCount = loader.GetValue<RoleAbilityCommonOption, int>(RoleAbilityCommonOption.AbilityCount);
		int metsuLimit = loader.GetValue<Option, int>(Option.MetsuLimit);
		int metsuKillMoney = loader.GetValue<Option, int>(Option.MetsuKillMoney);

		this.status = new EncloserStatusModel(stakeCount, metsuKillMoney, metsuLimit);
		this.abilityHandler = new EncloserAbilityHandler(this.status);
		
		this.AbilityClass = this.abilityHandler;

		this.abilityHandler?.CreateAbility();
	}

	public bool UseAbility()
	{
		return this.abilityHandler != null && this.abilityHandler.UseAbility();
	}

	public bool IsAbilityUse()
	{
		return this.abilityHandler != null && this.abilityHandler.IsAbilityUse();
	}

	public void ResetOnMeetingStart()
	{
	}

	public void ResetOnMeetingEnd(NetworkedPlayerInfo? exiledPlayer = null)
	{
	}

	public void Update(PlayerControl rolePlayer)
	{
		this.status?.Update();
	}

	protected override void CreateSpecificOption(
		AutoParentSetOptionCategoryFactory factory)
	{
		IRoleAbility.CreateAbilityCountOption(factory, 3, 10);
		factory.CreateIntOption(
			Option.MetsuLimit,
			1, 1, 10, 1);
		factory.CreateIntOption(
			Option.MetsuKillMoney,
			10, 0, 100, 1);
	}

	protected override void RoleSpecificInit()
	{
		var liberalOption = ExtremeRolesPlugin.Instance.Provider.GetRequiredService<LiberalDefaultOptionLoader>();
		LiberalSettingOverrider.OverrideDefault(this, liberalOption);
	}

	public static void RpcOps(MessageReader reader)
	{
		byte rolePlayerId = reader.ReadByte();
		RpcOpsType ops = (RpcOpsType)reader.ReadByte();

		if (!ExtremeRoleManager.TryGetSafeCastedRole<EncloserRole>(rolePlayerId, out var encloser) ||
			encloser.abilityHandler == null)
		{
			return;
		}

		switch (ops)
		{
			case RpcOpsType.PlaceStake:
				float x = reader.ReadSingle();
				float y = reader.ReadSingle();
				encloser.abilityHandler.HandlePlaceStake(rolePlayerId, new Vector2(x, y));
				break;
			case RpcOpsType.UseMetsu:
				encloser.abilityHandler.HandleUseMetsuRpc();
				break;
		}
	}
}
