using System;
using System.Collections;
using System.Collections.Generic;

using HarmonyLib;

using UnityEngine;

namespace ExtremeRoles.Patches
{
	[HarmonyPatch(typeof(PlayerVoteArea), nameof(PlayerVoteArea.Select))]
	class PlayerVoteAreaSelectPatch
	{
		static bool Prefix(PlayerVoteArea __instance)
		{

			if (!AssassinMeeting.AssassinMeetingTrigger) { return true; }

			if (PlayerControl.LocalPlayer.PlayerId != AssassinMeeting.ExiledAssassinId)
			{
				return false;
			}

			if (!__instance.Parent)
			{
				return false;
			}
			if (!__instance.voteComplete &&
				__instance.Parent.Select((int)__instance.TargetPlayerId))
			{
				__instance.Buttons.SetActive(true);
				//  at ExtremeRoles.Patches.PlayerVoteAreaSelectPatch.Prefix (PlayerVoteArea __instance) [0x0012a] in <4cdd481f50f94d46b5c49231d84de06c>:0
				float startPos = __instance.AnimateButtonsFromLeft ? 0.2f : 1.95f;
				__instance.StartCoroutine(
					(Il2CppSystem.Collections.IEnumerator)EffectsAllWrap(
						new IEnumerator[]
							{
								EffectsLerpWrap(0.25f, delegate(float t)
								{
									__instance.CancelButton.transform.localPosition = Vector2.Lerp(
										Vector2.right * startPos,
										Vector2.right * 1.3f,
										Effects.ExpOut(t));
								}),
								EffectsLerpWrap(0.35f, delegate(float t)
								{
									__instance.ConfirmButton.transform.localPosition = Vector2.Lerp(
										Vector2.right * startPos,
										Vector2.right * 0.65f,
										Effects.ExpOut(t));
								})
							}
						)
					);
				//BepInEx.IL2CPP.Utils.Collections.ManagedIl2CppEnumerator
				Il2CppSystem.Collections.Generic.List<UiElement> selectableElements = new Il2CppSystem.Collections.Generic.List<UiElement>();
				selectableElements.Add(__instance.CancelButton);
				selectableElements.Add(__instance.ConfirmButton);
				ControllerManager.Instance.OpenOverlayMenu(
					__instance.name,
					__instance.CancelButton,
					__instance.ConfirmButton, selectableElements, false);
			}

			return false;
		}
		private static IEnumerator EffectsLerpWrap(
			float duration, Action<float> action)
		{
			for (float t = 0f; t < duration; t += Time.deltaTime)
			{
				action(t / duration);
				yield return null;
			}
			action(1f);
			yield break;
		}
		private static IEnumerator EffectsAllWrap(IEnumerator[] items)
		{
			Stack<IEnumerator>[] enums = new Stack<IEnumerator>[items.Length];
			for (int i = 0; i < items.Length; i++)
			{
				enums[i] = new Stack<IEnumerator>();
				enums[i].Push(items[i]);
			}
			int num;
			for (int cap = 0; cap < 100000; cap = num)
			{
				bool flag = false;
				for (int j = 0; j < enums.Length; j++)
				{
					if (enums[j].Count > 0)
					{
						flag = true;
						IEnumerator enumerator = enums[j].Peek();
						if (enumerator.MoveNext())
						{
							if (enumerator.Current is IEnumerator)
							{
								enums[j].Push((IEnumerator)enumerator.Current);
							}
						}
						else
						{
							enums[j].Pop();
						}
					}
				}
				if (!flag)
				{
					break;
				}
				yield return null;
				num = cap + 1;
			}
			yield break;
		}

	}

	[HarmonyPatch(typeof(PlayerVoteArea), nameof(PlayerVoteArea.SetDead))]
	class PlayerVoteAreaSetDeadPatch
	{
		static bool Prefix(
			PlayerVoteArea __instance,
			[HarmonyArgument(0)] bool didReport,
			[HarmonyArgument(1)] bool isDead,
			[HarmonyArgument(2)] bool isGuardian = false)
		{
			if (!AssassinMeeting.AssassinMeetingTrigger) { return true; }


			__instance.AmDead = false;
			__instance.DidReport = didReport;
			__instance.Megaphone.enabled = didReport;
			__instance.Overlay.gameObject.SetActive(false);
			__instance.XMark.gameObject.SetActive(false);
			__instance.GAIcon.gameObject.SetActive(isGuardian);

			return false;
		}
	}
}
