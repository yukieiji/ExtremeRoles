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
		Exorcist,
		Boxer,
	}

	public enum Ops
	{
		RpcLock,
		Unlock,
	}

	private readonly ExtremeSystemType thisSystemTime = type;

	private bool isBlocked => this.blockedConditionId.Count > 0;

	private readonly HashSet<int> blockedConditionId = [];
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

	public void Lock(int conditionId)
	{
		if (!this.blockCondition.TryGetValue(conditionId, out var func))
		{
			func = () => true;
			this.blockCondition[conditionId] = func;
		}
		if (func.Invoke())
		{
			this.blockedConditionId.Add(conditionId);
		}
	}

	public void UnLock(int conditionId)
	{
		this.blockedConditionId.Remove(conditionId);
	}

	public void AddCondition(int id, Func<bool> condition)
	{
		this.blockCondition[id] = condition;
	}

	public void Reset(ResetTiming timing, PlayerControl? resetPlayer = null)
	{

	}

	public void UpdateSystem(PlayerControl player, MessageReader msgReader)
	{
		var ops = (Ops)msgReader.ReadByte();
		int conditionId = msgReader.ReadPackedInt32();

		lock (this.blockedConditionId)
		{
			switch (ops)
			{
				case Ops.RpcLock:
					Lock(conditionId);
					break;
				case Ops.Unlock:
					UnLock(conditionId);
					break;
				default:
					break;
			}
		}
	}
}
