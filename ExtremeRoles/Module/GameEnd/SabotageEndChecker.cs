using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

using ExtremeRoles.Extension.Il2Cpp;
using ExtremeRoles.Module.Interface;
using ExtremeRoles.Module.SystemType;
using ExtremeRoles.Module.SystemType.Roles;
using ExtremeRoles.Roles;

#nullable enable

namespace ExtremeRoles.Module.GameEnd;

public sealed class LifeSuppSystemEndChecker(LifeSuppSystemType system) : IGameEndChecker
{
	private readonly LifeSuppSystemType system = system;

	public bool TryCheckGameEnd(out GameOverReason reason)
	{
		reason = GameOverReason.ImpostorsBySabotage;
		return this.system.Countdown < 0.0f;
	}

	public void CleanUp()
	{
		if (this.system != null)
		{
			this.system.Countdown = 10000f;
		}
	}
}

public sealed class CriticalSystemEndChecker(ICriticalSabotage system) : IGameEndChecker
{
	private readonly ICriticalSabotage system = system;

	public bool TryCheckGameEnd(out GameOverReason reason)
	{
		reason = GameOverReason.ImpostorsBySabotage;
		return this.system.Countdown < 0.0f;
	}

	public void CleanUp()
	{
		if (this.system != null)
		{
			this.system.ClearSabotage();
		}
	}
}

public sealed class TeroristTeroSabotageSystemEndChecker(TeroristTeroSabotageSystem system) : IGameEndChecker
{
	private readonly TeroristTeroSabotageSystem system = system;

	public bool TryCheckGameEnd(out GameOverReason reason)
	{
		reason = (GameOverReason)RoleGameOverReason.TeroristoTeroWithShip;
		return this.system.ExplosionTimer < 0.0f;
	}

	public void CleanUp()
	{
		if (this.system != null)
		{
			this.system.Clear();
		}
	}
}

public sealed class SabotageEndChecker : IGameEndChecker	
{
	private IReadOnlyList<IGameEndChecker> innerChecker;

	public SabotageEndChecker()
	{
		List<IGameEndChecker> checkers = new List<IGameEndChecker>(5);

		var systems = ShipStatus.Instance.Systems;

		if (systems.TryGetValue(SystemTypes.LifeSupp, out var system) &&
			system.IsTryCast<LifeSuppSystemType>(out var lifeSuppSystem))
		{
			checkers.Add(new LifeSuppSystemEndChecker(lifeSuppSystem));
		}

		if (tryGetCriticalSystemEndCheck(SystemTypes.Reactor, out var reactor))
		{
			checkers.Add(reactor);
		}
		if (tryGetCriticalSystemEndCheck(SystemTypes.Laboratory, out var lab))
		{
			checkers.Add(lab);
		}
		if (tryGetCriticalSystemEndCheck(SystemTypes.HeliSabotage, out var heli))
		{
			checkers.Add(heli);
		}
		if (ExtremeSystemTypeManager.Instance.TryGet<TeroristTeroSabotageSystem>(
			TeroristTeroSabotageSystem.SystemType, out var teroSabo))
		{
			checkers.Add(new TeroristTeroSabotageSystemEndChecker(teroSabo));
		}
		this.innerChecker = checkers;
	}

	private static bool tryGetCriticalSystemEndCheck(
		SystemTypes tyep, [NotNullWhen(true)] out IGameEndChecker? checker)
	{
		if (!(
				ShipStatus.Instance.Systems.TryGetValue(tyep, out var system) &&
				system.IsTryCast<ICriticalSabotage>(out var critical))
			)
		{
			checker = null;
			return false;
		}
		checker = new CriticalSystemEndChecker(critical);
		return true;
	}

	public bool TryCheckGameEnd(out GameOverReason reason)
	{
		reason = GameOverReason.ImpostorsBySabotage;
		foreach (var check in this.innerChecker)
		{
			if (check.TryCheckGameEnd(out reason))
			{
				return true; 
			}
		}
		return false;
	}

	public void CleanUp()
	{
	}
}
