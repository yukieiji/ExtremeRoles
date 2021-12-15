using System;
using System.Collections;

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

			if (PlayerControl.LocalPlayer.PlayerId == AssassinMeeting.ExiledAssassinId)
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

				float startPos = __instance.AnimateButtonsFromLeft ? 0.2f : 1.95f;
				__instance.StartCoroutine(
					Effects.All(
                        (Il2CppSystem.Collections.IEnumerator[])(
							new IEnumerator[]
								{
									EffectsLerpWrap(0.25f, delegate(float t)
									{
										__instance.CancelButton.transform.localPosition = Vector2.Lerp(
											Vector2.right * startPos, Vector2.right * 1.3f,
											Effects.ExpOut(t));
									}),
									EffectsLerpWrap(0.35f, delegate(float t)
									{
										__instance.ConfirmButton.transform.localPosition = Vector2.Lerp(
											Vector2.right * startPos, Vector2.right * 0.65f, Effects.ExpOut(t));
									})
								}
							)
						));

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
	}

}
