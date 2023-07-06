using HarmonyLib;
using UnityEngine;

using ExtremeRoles.GameMode;
using ExtremeRoles.Module;
using ExtremeRoles.Roles;
using ExtremeRoles.Performance;


namespace ExtremeRoles.Patches.Meeting.Hud;

#nullable enable

[HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Update))]
public static class MeetingHudUpdatePatch
{
				public static void Postfix(MeetingHud __instance)
				{

								if (InfoOverlay.Instance.IsBlock &&
												__instance.state != MeetingHud.VoteStates.Animating)
								{
												InfoOverlay.Instance.BlockShow(false);
								}
								if (NamePlateHelper.NameplateChange)
								{
												foreach (var pva in __instance.playerStates)
												{
																NamePlateHelper.UpdateNameplate(pva);
												}
												NamePlateHelper.NameplateChange = false;
								}

								if (__instance.state == MeetingHud.VoteStates.Animating) { return; }

								// From TOR
								// This fixes a bug with the original game where pressing the button and a kill happens simultaneously
								// results in bodies sometimes being created *after* the meeting starts, marking them as dead and
								// removing the corpses so there's no random corpses leftover afterwards

								foreach (DeadBody b in Object.FindObjectsOfType<DeadBody>())
								{
												if (b == null) { continue; }

												foreach (PlayerVoteArea pva in __instance.playerStates)
												{
																if (pva == null ||
																				pva.TargetPlayerId != b.ParentId ||
																				pva.AmDead)
																{
																				continue;
																}
																pva.SetDead(pva.DidReport, true);
																pva.Overlay.gameObject.SetActive(true);
												}
												Object.Destroy(b.gameObject);
								}

								// Deactivate skip Button if skipping on emergency meetings is disabled
								if (ExtremeGameModeManager.Instance.ShipOption.IsBlockSkipInMeeting)
								{
												__instance.SkipVoteButton.gameObject.SetActive(false);
								}

								if (MeetingReporter.IsExist &&
												MeetingReporter.Instance.HasChatReport)
								{
												MeetingReporter.Instance.ReportMeetingChat();
								}

								if (ExtremeRolesPlugin.ShipState.AssassinMeetingTrigger)
								{
												__instance.TitleText.text = Helper.Translation.GetString(
																"whoIsMarine");
												__instance.SkipVoteButton.gameObject.SetActive(false);

												if (CachedPlayerControl.LocalPlayer.PlayerId == ExtremeRolesPlugin.ShipState.ExiledAssassinId ||
																ExtremeRoleManager.GetLocalPlayerRole().IsImpostor())
												{
																FastDestroyableSingleton<HudManager>.Instance.Chat.gameObject.SetActive(true);
												}
												else
												{
																FastDestroyableSingleton<HudManager>.Instance.Chat.gameObject.SetActive(false);
												}
								}
				}
}
