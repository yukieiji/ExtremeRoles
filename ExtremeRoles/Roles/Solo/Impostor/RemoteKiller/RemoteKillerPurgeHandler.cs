using System.Linq;
using UnityEngine;

using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Module.Ability;
using ExtremeRoles.Module.Ability.Behavior;
using ExtremeRoles.Resources;
using ExtremeRoles.Roles.API.Interface.Ability;

#nullable enable

namespace ExtremeRoles.Roles.Solo.Impostor.RemoteKiller;

public sealed class RemoteKillerPurgeHandler(RemoteKillerStatusModel status, RemoteKillerRole role) : IAbility
{
	private readonly RemoteKillerStatusModel status = status;
	private readonly RemoteKillerRole role = role;
	private ShapeShiftMinigameWrapper? minigame;
	private PlayerControl? selectedPurgeTarget;

	public ActivatingCountBehavior CreateBehavior(float coolTime, int purgeCount)
	{
		var behavior = new ActivatingCountBehavior(
			text: Tr.GetString("remoteKillerPurge"),
			img: UnityObjectLoader.LoadSpriteFromResources(ObjectPath.SucideSprite),
			canUse: IsUsePurge,
			ability: PurgeOpenMenu,
			canActivating: IsPurgeCheck,
			abilityOff: PurgeCleanUp,
			forceAbilityOff: PurgeForceCleanUp,
			isReduceOnActive: false);

		behavior.SetCoolTime(coolTime);
		behavior.ActiveTime = this.status.PurgeTime;
		behavior.SetAbilityCount(purgeCount);

		return behavior;
	}

	public bool IsUsePurge()
	{
		if (!this.role.IsAbilityUse() || !this.role.IsAbilityUseWithMinigame())
		{
			return false;
		}

		return this.status.ExecutionTargets.Any(id =>
		{
			var p = Player.GetPlayerControlById(id);
			return p != null && p.Data != null && !p.Data.IsDead && !p.Data.Disconnected;
		});
	}

	public bool PurgeOpenMenu()
	{
		if (this.selectedPurgeTarget != null)
		{
			return true;
		}

		this.minigame ??= new ShapeShiftMinigameWrapper();
		return this.minigame.IsOpen || this.minigame.OpenUi(
			OnPurgeTargetSelected,
			p => p != null && p.Data != null && !p.Data.IsDead && !p.Data.Disconnected && this.status.ExecutionTargets.Contains(p.PlayerId));
	}

	public void OnPurgeTargetSelected(PlayerControl target)
	{
		if (target == null || target.Data == null || target.Data.IsDead || target.Data.Disconnected)
		{
			return;
		}

		if (!this.status.ExecutionTargets.Contains(target.PlayerId))
		{
			return;
		}

		this.selectedPurgeTarget = target;
		byte localId = PlayerControl.LocalPlayer.PlayerId;

		using (var caller = RPCOperator.CreateCaller(RPCOperator.Command.RemoteKillerOps))
		{
			caller.WriteByte((byte)RemoteKillerRole.RemoteKillerRpc.PurgeStart);
			caller.WriteByte(localId);
		}

		if (this.role.Button != null && this.role.Button.Transform.TryGetComponent<PassiveButton>(out var button))
		{
			button.OnClick.Invoke();
		}
	}

	public bool IsPurgeCheck()
	{
		if (this.selectedPurgeTarget == null ||
			this.selectedPurgeTarget.Data == null ||
			this.selectedPurgeTarget.Data.IsDead ||
			this.selectedPurgeTarget.Data.Disconnected ||
			PlayerControl.LocalPlayer.Data.IsDead ||
			MeetingHud.Instance != null)
		{
			return false;
		}

		return true;
	}

	public void PurgeCleanUp()
	{
		if (this.selectedPurgeTarget != null)
		{
			byte localId = PlayerControl.LocalPlayer.PlayerId;
			byte targetId = this.selectedPurgeTarget.PlayerId;

			// 粛清状態の解除RPCを送信
			using (var caller = RPCOperator.CreateCaller(RPCOperator.Command.RemoteKillerOps))
			{
				caller.WriteByte((byte)RemoteKillerRole.RemoteKillerRpc.PurgeCancel);
				caller.WriteByte(localId);
			}

			// 遠隔キル（自殺アニメーション）を実行
			Player.RpcUncheckMurderPlayer(targetId, targetId, 0);

			// 執行対象リストから除外
			this.status.ExecutionTargets.Remove(targetId);
		}

		this.selectedPurgeTarget = null;
	}

	public void PurgeForceCleanUp()
	{
		if (this.status.IsPurging)
		{
			byte localId = PlayerControl.LocalPlayer.PlayerId;

			using (var caller = RPCOperator.CreateCaller(RPCOperator.Command.RemoteKillerOps))
			{
				caller.WriteByte((byte)RemoteKillerRole.RemoteKillerRpc.PurgeCancel);
				caller.WriteByte(localId);
			}
		}

		this.selectedPurgeTarget = null;
	}

	public void ResetMinigame()
	{
		this.minigame?.Reset();
	}
}
