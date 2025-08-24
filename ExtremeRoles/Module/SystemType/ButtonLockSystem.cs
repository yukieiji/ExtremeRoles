using System;
using System.Collections.Generic;

using Hazel;

using ExtremeRoles.Module.Interface;

#nullable enable

namespace ExtremeRoles.Module.SystemType;

public sealed class ButtonLockSystem : IExtremeSystemType
{
	public enum Ops
	{
		Lock,
		Unlock,
	}

	private bool isBlocked = false;

	private readonly Dictionary<int, Func<bool>> blockCondtion = [];

	public static ButtonLockSystem CreateOrGetAbilityButtonLockSystem()
		=> ExtremeSystemTypeManager.Instance.CreateOrGet<ButtonLockSystem>(ExtremeSystemType.AbilityButtonLockSystem);
	public static bool IsAbilityButtonLock()
		=> 
		ExtremeSystemTypeManager.Instance.TryGet<ButtonLockSystem>(ExtremeSystemType.AbilityButtonLockSystem, out var system) &&
		system.isBlocked;

	public static ButtonLockSystem CreateOrGetReportButtonLock()
		=> ExtremeSystemTypeManager.Instance.CreateOrGet<ButtonLockSystem>(ExtremeSystemType.ReportButtonLockSystem);
	public static bool IsReportButtonLock()
		=>
		ExtremeSystemTypeManager.Instance.TryGet<ButtonLockSystem>(ExtremeSystemType.ReportButtonLockSystem, out var system) &&
		system.isBlocked;


	public static ButtonLockSystem CreateOrGetKillButtonLockSystem()
		=> ExtremeSystemTypeManager.Instance.CreateOrGet<ButtonLockSystem>(ExtremeSystemType.KillButtonLockSystem);
	public static bool IsKillButtonLock()
		=>
		ExtremeSystemTypeManager.Instance.TryGet<ButtonLockSystem>(ExtremeSystemType.KillButtonLockSystem, out var system) &&
		system.isBlocked;

	public void RpcLock(ExtremeSystemType lockType, Action<MessageWriter> writeFunc)
	{
		if (lockType is 
				ExtremeSystemType.AbilityButtonLockSystem or 
				ExtremeSystemType.ReportButtonLockSystem or 
				ExtremeSystemType.KillButtonLockSystem)
		{
			ExtremeRolesPlugin.Logger.LogError("Invalid Systems");
			return;
		}

		ExtremeSystemTypeManager.RpcUpdateSystem(lockType, writeFunc.Invoke);
	}

	public void AddCondtion(int id, Func<bool> condtion)
	{
		blockCondtion[id] = condtion;
	}

	public void Reset(ResetTiming timing, PlayerControl? resetPlayer = null)
	{

	}

	public void UpdateSystem(PlayerControl player, MessageReader msgReader)
	{
		var id = (Ops)msgReader.ReadByte();

		switch (id)
		{
			case Ops.Lock:
				int condtionId = msgReader.ReadPackedInt32();
				if (!blockCondtion.TryGetValue(condtionId, out var func))
				{
					return;
				}
				isBlocked = func.Invoke();
				break;
			case Ops.Unlock:
				isBlocked = false;
				break;
			default:
				break;
		}
	}
}
