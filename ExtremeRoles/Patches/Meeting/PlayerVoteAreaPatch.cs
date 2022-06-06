using System;
using System.Collections;
using System.Collections.Generic;

using HarmonyLib;

using UnityEngine;

using BepInEx.IL2CPP.Utils.Collections;

namespace ExtremeRoles.Patches.Meeting
{
	public static class NamePlateHelper
	{
		public static bool NameplateChange = true;
		private static Sprite blankNameplate = null;

		private const string blankNamePlateId = "nameplate_NoPlate";

		public static void UpdateNameplate(PlayerVoteArea pva, byte playerId = byte.MaxValue)
		{
			blankNameplate = blankNameplate ?? HatManager.Instance.GetNamePlateById(
				blankNamePlateId).viewData.viewData.Image;
			var nameplate = blankNameplate;
			if (!OptionHolder.Client.HideNamePlate)
			{
				var p = Helper.Player.GetPlayerControlById(
					playerId != byte.MaxValue ? playerId : pva.TargetPlayerId);
				var nameplateId = p?.CurrentOutfit?.NamePlateId;
				nameplate = HatManager.Instance.GetNamePlateById(nameplateId).viewData.viewData.Image;
			}
			pva.Background.sprite = nameplate;
		}
	}

	
	[HarmonyPatch(typeof(PlayerVoteArea), nameof(PlayerVoteArea.SetCosmetics))]
	public static class PlayerVoteAreaCosmetics
	{
		public static void Postfix(PlayerVoteArea __instance, GameData.PlayerInfo playerInfo)
		{
			NamePlateHelper.UpdateNameplate(
				__instance, playerInfo.PlayerId);
		}
	}

	[HarmonyPatch(typeof(PlayerVoteArea), nameof(PlayerVoteArea.Select))]
	public static class PlayerVoteAreaSelectPatch
	{
		private static Dictionary<byte, UiElement> meetingKillButton;

		public static bool Prefix(PlayerVoteArea __instance)
		{
			if (!ExtremeRolesPlugin.GameDataStore.IsRoleSetUpEnd()) { return true; }
			if (Roles.ExtremeRoleManager.GameRole.Count == 0) { return true; }

			var gameData = ExtremeRolesPlugin.GameDataStore;
			var shooter = Roles.ExtremeRoleManager.GetSafeCastedLocalPlayerRole<Roles.Solo.Impostor.Shooter>();

			if (!gameData.AssassinMeetingTrigger)
			{
				if (shooter == null)
                {
					return true;
                }
				else
				{
					return shooterKillButton(
						__instance, shooter);
				}
			}

			if (PlayerControl.LocalPlayer.PlayerId != ExtremeRolesPlugin.GameDataStore.ExiledAssassinId)
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
					effectsAllWrap(
						new IEnumerator[]
							{
								effectsLerpWrap(0.25f, delegate(float t)
								{
									__instance.CancelButton.transform.localPosition = Vector2.Lerp(
										Vector2.right * startPos,
										Vector2.right * 1.3f,
										Effects.ExpOut(t));
								}),
								effectsLerpWrap(0.35f, delegate(float t)
								{
									__instance.ConfirmButton.transform.localPosition = Vector2.Lerp(
										Vector2.right * startPos,
										Vector2.right * 0.65f,
										Effects.ExpOut(t));
								})
							}
						).WrapToIl2Cpp()
					);
			
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
		private static IEnumerator effectsLerpWrap(
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
		private static IEnumerator effectsAllWrap(IEnumerator[] items)
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

		private static bool shooterKillButton(
			PlayerVoteArea instance,
			Roles.Solo.Impostor.Shooter shooter)
        {
			byte target = instance.TargetPlayerId;

			if (instance.AmDead ||
				shooter.CurShootNum <= 0 || 
				!shooter.CanShoot || target == 253 ||
				Roles.ExtremeRoleManager.GameRole[target].Id == Roles.ExtremeRoleId.Assassin)
			{ 
				return true; 
			}

			if (!instance.Parent)
			{
				return false;
			}

			if (!instance.voteComplete &&
				instance.Parent.Select((int)target))
			{
				if (meetingKillButton == null)
                {
					meetingKillButton = new Dictionary<byte, UiElement> ();
                }

				if (!meetingKillButton.ContainsKey(target))
                {

					UiElement newKillButton = GameObject.Instantiate(
						instance.ConfirmButton, instance.ConfirmButton.transform.parent);
					newKillButton.name = $"shooterKill_{target}";
					var passiveButton = newKillButton.GetComponent<PassiveButton>();
					passiveButton.OnClick = new UnityEngine.UI.Button.ButtonClickedEvent();
					passiveButton.OnClick.AddListener(
						(UnityEngine.Events.UnityAction)shooterKill);

					var render = newKillButton.GetComponent<SpriteRenderer>();
					render.sprite = HudManager.Instance.KillButton.graphic.sprite;
					render.transform.localScale *= new Vector2(0.75f, 0.75f);

					meetingKillButton.Add(target, newKillButton);

					void shooterKill()
					{
						if (instance.AmDead) { return; }
						shooter.Shoot();
						RPCOperator.Call(
							PlayerControl.LocalPlayer.NetId,
							RPCOperator.Command.UncheckedMurderPlayer,
							new List<byte> { PlayerControl.LocalPlayer.PlayerId, target, 0 });
						RPCOperator.UncheckedMurderPlayer(
							PlayerControl.LocalPlayer.PlayerId,
							target, 0);
					}

				}

				instance.Buttons.SetActive(true);
				var killbutton = meetingKillButton[target];

				float startPos = instance.AnimateButtonsFromLeft ? 0.2f : 1.95f;
				
				instance.StartCoroutine(
					effectsAllWrap(
						new IEnumerator[]
							{
								effectsLerpWrap(0.25f, delegate(float t)
								{
									instance.CancelButton.transform.localPosition = Vector2.Lerp(
										Vector2.right * startPos,
										Vector2.right * 1.3f,
										Effects.ExpOut(t));
								}),
								effectsLerpWrap(0.35f, delegate(float t)
								{
									instance.ConfirmButton.transform.localPosition = Vector2.Lerp(
										Vector2.right * startPos,
										Vector2.right * 0.65f,
										Effects.ExpOut(t));
								}),
								effectsLerpWrap(0.45f, delegate(float t)
								{
									killbutton.transform.localPosition = Vector2.Lerp(
										Vector2.right * startPos,
										Vector2.right * -0.01f,
										Effects.ExpOut(t));
								})
							}
						).WrapToIl2Cpp()
					);

				Il2CppSystem.Collections.Generic.List<UiElement> selectableElements = new Il2CppSystem.Collections.Generic.List<UiElement>();
				selectableElements.Add(instance.CancelButton);
				selectableElements.Add(instance.ConfirmButton);
				selectableElements.Add(killbutton);

				ControllerManager.Instance.OpenOverlayMenu(
					instance.name,
					instance.CancelButton,
					instance.ConfirmButton, selectableElements, false);
			}

			return false;

		}
	}

	[HarmonyPatch(typeof(PlayerVoteArea), nameof(PlayerVoteArea.SetDead))]
	public static class PlayerVoteAreaSetDeadPatch
	{
		public static bool Prefix(
			PlayerVoteArea __instance,
			[HarmonyArgument(0)] bool didReport,
			[HarmonyArgument(1)] bool isDead,
			[HarmonyArgument(2)] bool isGuardian = false)
		{
			if (!ExtremeRolesPlugin.GameDataStore.AssassinMeetingTrigger) { return true; }


			__instance.AmDead = false;
			__instance.DidReport = didReport;
			__instance.Megaphone.enabled = didReport;
			__instance.Overlay.gameObject.SetActive(false);
			__instance.XMark.gameObject.SetActive(false);

			return false;
		}

		public static void Postfix(
			PlayerVoteArea __instance,
			[HarmonyArgument(0)] bool didReport,
			[HarmonyArgument(1)] bool isDead,
			[HarmonyArgument(2)] bool isGuardian = false)
        {
			if (OptionHolder.Ship.IsRemoveAngleIcon)
			{
				__instance.GAIcon.gameObject.SetActive(false);
			}
			else
			{
				bool isGhostRole = isGuardian ||
					GhostRoles.ExtremeGhostRoleManager.GameRole.ContainsKey(__instance.TargetPlayerId);

				__instance.GAIcon.gameObject.SetActive(isGhostRole);
			}
		}
	}
}
