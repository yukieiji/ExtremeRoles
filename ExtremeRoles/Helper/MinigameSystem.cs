using AmongUs.GameOptions;
using ExtremeRoles.Extension.Il2Cpp;
using ExtremeRoles.Performance;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

using UnityObject = UnityEngine.Object;

namespace ExtremeRoles.Helper;

#nullable enable

public static class MinigameSystem
{
	public static VitalsMinigame? Vital
	{
		get
		{
			var role = FastDestroyableSingleton<RoleManager>.Instance.GetRole(RoleTypes.Scientist);

			if (!role.IsTryCast<ScientistRole>(out var scientist))
			{
				return null;
			}

			return scientist.VitalsPrefab;
		}
	}

	public static Minigame Create(Minigame prefab, Console? console = null)
	{
		Minigame minigame = UnityObject.Instantiate(
			prefab, Camera.main.transform, false);
		minigame.transform.SetParent(Camera.main.transform, false);
		minigame.transform.localPosition = new Vector3(0.0f, 0.0f, -50f);
		if (console != null)
		{
			minigame.Console = console;
		}
		return minigame;
	}

	public static Minigame Open(
		Minigame prefab,
		PlayerTask? task = null,
		Console? console = null)
	{
		Minigame minigame = Create(prefab, console);
		minigame.Begin(task);

		return minigame;
	}
}
