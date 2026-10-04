using UnityEngine;

using ExtremeRoles.Helper;
using ExtremeRoles.Extension.Player;
using ExtremeRoles.Module.Ability.Behavior.Interface;
using ExtremeRoles.Module.Ability.ModeSwitcher;
using ExtremeRoles.Module.GameResult;
using ExtremeRoles.Module.SystemType.Roles;
using ExtremeRoles.Performance;
using ExtremeRoles.Roles.API.Interface.Ability;

#nullable enable

namespace ExtremeRoles.Roles.Solo.Liberal.Encloser;

public sealed class EncloserMetsuAbilityHandler(
	EncloserStatusModel statusModel,
	ICountBehavior count,
	GraphicSwitcher<EncloserRole.Mode> switcher)
{
	private readonly EncloserStatusModel encloserStatus = statusModel;
	private readonly ICountBehavior count = count;
	private readonly GraphicSwitcher<EncloserRole.Mode> switcher = switcher;

	public void CleanUp()
	{
		this.encloserStatus.RemainingMetsuCount = count.AbilityCount;
		if (this.encloserStatus.RemainingMetsuCount <= 0)
		{
			return;
		}
		this.switcher.Switch(EncloserRole.Mode.Stake);
		count.SetAbilityCount(this.encloserStatus.StakeCount);
	}

	public bool Invoke()
	{
		var localPlayer = PlayerControl.LocalPlayer;
		byte encloserPlayerId = localPlayer.PlayerId;

		int nonLiberalKills = 0;

		foreach (var pc in PlayerCache.AllPlayerControl)
		{
			if (pc.IsInValid())
			{
				continue;
			}

			Vector2 pPos = pc.GetTruePosition();

			if (!this.encloserStatus.IsKillPosition(pPos) ||
				!ExtremeRoleManager.TryGetRole(pc.PlayerId, out var targetRole) ||
				targetRole.Core.Id is ExtremeRoleId.Leader ||
				(
					targetRole.AbilityClass is IInvincible invincible &&
					invincible.IsBlockKillFrom(encloserPlayerId)
				))
			{
				continue;
			}

			if (!targetRole.IsLiberal())
			{
				nonLiberalKills++;
			}

			Player.RpcUncheckMurderPlayer(encloserPlayerId, pc.PlayerId, byte.MinValue);
		}

		if (nonLiberalKills > 0)
		{
			float totalMoney = nonLiberalKills * this.encloserStatus.MetsuKillMoney;
			LiberalMoneyBankSystem.RpcUpdateSystem(
				encloserPlayerId,
				LiberalMoneyHistory.Reason.AddOnKill,
				totalMoney,
				0.0f);
		}

		using (var caller = RPCOperator.CreateCaller(RPCOperator.Command.EncloserOps))
		{
			caller.WriteByte(encloserPlayerId);
			caller.WriteByte((byte)EncloserRole.RpcOpsType.UseMetsu);
		}
		this.encloserStatus.ClearStake();
		return true;
	}
}
