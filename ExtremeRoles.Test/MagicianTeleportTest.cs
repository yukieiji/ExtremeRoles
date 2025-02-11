using BepInEx.Logging;
using ExtremeRoles.Extension.Player;
using ExtremeRoles.Performance;
using ExtremeRoles.Roles.Solo.Impostor;
using ExtremeRoles.Test.Patches;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace ExtremeRoles.Test;

public static class MagicianTeleportTest
{
	public static IEnumerator Test(ManualLogSource logger)
	{
		RpcSnapHookCheck.Pos = PlayerCache.AllPlayerControl
			.Where(x => x.IsValid())
			.Select(x => new Vector2(x.transform.position.x, x.transform.position.y))
			.ToList();

		var param = new Magician.AbilityParameter(
			1.0f,
			RandomGenerator.Instance.Next(2) > 1,
			RandomGenerator.Instance.Next(2) > 1,
			RandomGenerator.Instance.Next(2) > 1);

		Magician.UseAbility(param);

		yield return new WaitForSeconds(5.0f);

		var missing = PlayerCache.AllPlayerControl.Where(
			x =>
			{
				var pos = new Vector2(x.transform.position.x, x.transform.position.y);
				return x.IsValid() && !RpcSnapHookCheck.Pos.Any(x => (x - pos).magnitude < 1.5f );
			});
		logger.LogInfo($"param:{param}");
		logger.LogWarning($"missing size:{missing.Count()}");
		foreach (var x in missing)
		{
			logger.LogWarning($"Missing Player:{x.PlayerId}");
		}

		RpcSnapHookCheck.Pos = null;
	}
}
