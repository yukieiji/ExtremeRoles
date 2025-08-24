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

	public static bool IsAbilityButtonLock()
		=> 
		ExtremeSystemTypeManager.Instance.TryGet<ButtonLockSystem>(ExtremeSystemType.AbilityButtonLockSystem, out var system) &&
		system.isBlocked;

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
