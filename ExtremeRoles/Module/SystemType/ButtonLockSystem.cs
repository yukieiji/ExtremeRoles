using System;
using System.Collections.Generic;

using Hazel;

using ExtremeRoles.Module.Interface;

#nullable enable

namespace ExtremeRoles.Module.SystemType;

public sealed class ButtonLockSystem(ExtremeSystemType type) : IExtremeSystemType
{
	public enum ConditionId
	{
		Exorcist
	}

	public enum Ops
	{
		Lock,
		Unlock,
	}

	private readonly ExtremeSystemType thisSystemTime = type;

	private bool isBlocked => this.blockedConditionId.Count > 0;

	private HashSet<int> blockedConditionId = [];
	private readonly Dictionary<int, Func<bool>> blockCondition = [];

	public static ButtonLockSystem CreateOrGetAbilityButtonLockSystem()
		=> ExtremeSystemTypeManager.Instance.CreateOrGet(
			ExtremeSystemType.AbilityButtonLockSystem,
			() => new ButtonLockSystem(ExtremeSystemType.AbilityButtonLockSystem));
	public static bool IsAbilityButtonLock()
		=> 
		ExtremeSystemTypeManager.Instance.TryGet<ButtonLockSystem>(ExtremeSystemType.AbilityButtonLockSystem, out var system) &&
		system.isBlocked;

	public static ButtonLockSystem CreateOrGetReportButtonLock()
		=> ExtremeSystemTypeManager.Instance.CreateOrGet(
			ExtremeSystemType.ReportButtonLockSystem,
			() => new ButtonLockSystem(ExtremeSystemType.ReportButtonLockSystem));
	public static bool IsReportButtonLock()
		=>
		ExtremeSystemTypeManager.Instance.TryGet<ButtonLockSystem>(ExtremeSystemType.ReportButtonLockSystem, out var system) &&
		system.isBlocked;


	public static ButtonLockSystem CreateOrGetKillButtonLockSystem()
		=> ExtremeSystemTypeManager.Instance.CreateOrGet(
			ExtremeSystemType.KillButtonLockSystem,
			() => new ButtonLockSystem(ExtremeSystemType.KillButtonLockSystem));
	public static bool IsKillButtonLock()
		=>
		ExtremeSystemTypeManager.Instance.TryGet<ButtonLockSystem>(ExtremeSystemType.KillButtonLockSystem, out var system) &&
		system.isBlocked;

	public void RpcLock(Ops ops, int condition)
	{
		ExtremeSystemTypeManager.RpcUpdateSystem(this.thisSystemTime, x =>
		{
			x.Write((byte)ops);
			x.WritePacked(condition);
		});
	}

	public void AddCondtion(int id, Func<bool> condtion)
	{
		this.blockCondition[id] = condtion;
	}

	public void Reset(ResetTiming timing, PlayerControl? resetPlayer = null)
	{

	}

	public void UpdateSystem(PlayerControl player, MessageReader msgReader)
	{
		var ops = (Ops)msgReader.ReadByte();
		int condtionId = msgReader.ReadPackedInt32();
		if (!blockCondition.TryGetValue(condtionId, out var func))
		{
			return;
		}

		lock (this.blockedConditionId)
		{
			switch (ops)
			{
				case Ops.Lock:
					if (func.Invoke())
					{
						this.blockedConditionId.Add(condtionId);
					}
					break;
				case Ops.Unlock:
					this.blockedConditionId.Remove(condtionId);
					break;
				default:
					break;
			}
		}
	}
}
