using UnityEngine;

using ExtremeRoles.Extension.Player;
using ExtremeRoles.Module.Ability;
using ExtremeRoles.Module.Ability.Behavior;
using ExtremeRoles.Module.Ability.Behavior.Interface;
using ExtremeRoles.Module.Ability.Factory;
using ExtremeRoles.Module.Ability.ModeSwitcher;
using ExtremeRoles.Resources;
using ExtremeRoles.Roles.API.Interface.Ability;

#nullable enable

namespace ExtremeRoles.Roles.Solo.Liberal.Encloser;

public sealed class EncloserAbilityHandler(
	EncloserStatusModel statusModel) : IAbility
{
	public ExtremeAbilityButton Button { get; set; } = null!;
	private EncloserStatusModel encloserStatus = statusModel;
	private EncloserRole.Mode currentMode => this.modeSwitcher?.Current ?? EncloserRole.Mode.Stake;
	private GraphicSwitcher<EncloserRole.Mode>? modeSwitcher;
	private EncloserStakeAbilityhandler? stake;
	private EncloserMetsuAbilityhandler? mestu;

	public void CreateAbility()
	{
		Sprite bombSprite = UnityObjectLoader.LoadSpriteFromResources(ObjectPath.Bomb);

		var stakeGraphic = new ButtonGraphic(Tr.GetString("Stake"), bombSprite);
		var metsuGraphic = new ButtonGraphic(Tr.GetString("Metsu"), bombSprite);

		this.Button = RoleAbilityFactory.CreateCountAbility(
			stakeGraphic.Text,
			stakeGraphic.Img,
			this.IsAbilityUse,
			this.UseAbility);

		this.Button.SetLabelToCrewmate();

		this.modeSwitcher = new GraphicSwitcher<EncloserRole.Mode>(
			this.Button.Behavior,
			new GraphicMode<EncloserRole.Mode>(EncloserRole.Mode.Stake, stakeGraphic),
			new GraphicMode<EncloserRole.Mode>(EncloserRole.Mode.Metsu, metsuGraphic));

		this.modeSwitcher.Switch(EncloserRole.Mode.Stake);

		if (this.Button.Behavior is ICountBehavior countBehavior)
		{
			this.stake = new EncloserStakeAbilityhandler(this.encloserStatus, countBehavior, this.modeSwitcher);
			this.mestu = new EncloserMetsuAbilityhandler(this.encloserStatus, countBehavior, this.modeSwitcher);
		}
	}

	public bool UseAbility()
		=> this.currentMode switch
		{
			EncloserRole.Mode.Stake => this.stake is not null && this.stake.Invoke(),
			EncloserRole.Mode.Metsu => this.mestu is not null && this.mestu.Invoke(),
			_ => false
		};

	public bool IsAbilityUse()
	{
		if (this.encloserStatus.RemainingMetsuCount <= 0)
		{
			return false;
		}

		PlayerControl localPlayer = PlayerControl.LocalPlayer;
		bool isCommonUse = localPlayer != null && localPlayer.IsAlive() && localPlayer.CanMove;

		return this.currentMode switch
		{
			EncloserRole.Mode.Stake => isCommonUse && this.encloserStatus.CurStakeCount < this.encloserStatus.StakeCount,
			EncloserRole.Mode.Metsu => isCommonUse && this.encloserStatus.IsUseMetsu,
			_ => false
		};
	}

	public void AbilityOff()
	{
		switch (this.currentMode)
		{
			case EncloserRole.Mode.Stake:
				this.stake?.CleanUp();
				break;
			case EncloserRole.Mode.Metsu:
				this.mestu?.CleanUp();
				break;
			default:
				break;
		}
	}

	public void HandlePlaceStake(byte encloserPlayerId, Vector2 pos)
	{
		this.stake?.HandlePlaceStake(encloserPlayerId, pos);
	}

	public void HandleUseMetsuRpc()
	{
		this.mestu?.HandleUseMetsuRpc();
	}
}
