using UnityEngine;

using ExtremeRoles.Module.Ability.Behavior.Interface;
using ExtremeRoles.Module.Ability.ModeSwitcher;

#nullable enable

namespace ExtremeRoles.Roles.Solo.Liberal.Encloser;

public sealed class EncloserStakeAbilityHandler(
	EncloserStatusModel statusModel,
	ICountBehavior count,
	GraphicSwitcher<EncloserRole.Mode> switcher)
{
	private readonly EncloserStatusModel encloserStatus = statusModel;
	private readonly ICountBehavior count = count;
	private readonly GraphicSwitcher<EncloserRole.Mode> switcher = switcher;

	public void CleanUp()
	{
		if (!this.encloserStatus.IsUseMetsu)
		{
			return;
		}

		this.switcher.Switch(EncloserRole.Mode.Metsu);
		this.count.SetAbilityCount(this.encloserStatus.RemainingMetsuCount);
	}

	public bool Invoke()
	{
		var localPlayer = PlayerControl.LocalPlayer;
		byte encloserPlayerId = localPlayer.PlayerId;

		Vector2 pos = localPlayer.GetTruePosition();
		using (var caller = RPCOperator.CreateCaller(RPCOperator.Command.EncloserOps))
		{
			caller.WriteByte(encloserPlayerId);
			caller.WriteByte((byte)EncloserRole.RpcOpsType.PlaceStake);
			caller.WriteFloat(pos.x);
			caller.WriteFloat(pos.y);
		}

		HandlePlaceStake(encloserPlayerId, pos);

		return true;
	}
	public void HandlePlaceStake(byte encloserPlayerId, Vector2 pos)
	{
		this.encloserStatus.PlaceStake(pos, isLocalPlayerIsEncloser(encloserPlayerId));
	}

	private static bool isLocalPlayerIsEncloser(byte encloserPlayerId)
		=> PlayerControl.LocalPlayer != null && PlayerControl.LocalPlayer.PlayerId == encloserPlayerId;
}
