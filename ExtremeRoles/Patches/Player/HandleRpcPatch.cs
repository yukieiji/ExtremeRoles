using System.Collections.Generic;

using HarmonyLib;

using Hazel;
using UnityEngine;

using ExtremeRoles.Module.RoleAssign;
using ExtremeRoles.Module.SystemType;

namespace ExtremeRoles.Patches.Player;

#nullable enable

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.HandleRpc))]
public static class PlayerControlHandleRpcPatch
{
	public static void Postfix(
		PlayerControl __instance,
		[HarmonyArgument(0)] byte callId,
		[HarmonyArgument(1)] MessageReader reader)
	{

		if (reader == null)
		{
			return;
		}

		switch ((RPCOperator.Command)callId)
		{
			case RPCOperator.Command.Initialize:
				RPCOperator.Initialize();
				break;
			case RPCOperator.Command.ForceEnd:
				RPCOperator.ForceEnd();
				break;
			case RPCOperator.Command.SetRoleToAllPlayer:
				List<Module.Interface.IPlayerToExRoleAssignData> assignData =
					new List<Module.Interface.IPlayerToExRoleAssignData>();
				int assignDataNum = reader.ReadPackedInt32();
				for (int i = 0; i < assignDataNum; ++i)
				{
					byte assignedPlayerId = reader.ReadByte();
					byte assignRoleType = reader.ReadByte();
					int exRoleId = reader.ReadPackedInt32();
					int controlId = reader.ReadPackedInt32();
					switch (assignRoleType)
					{
						case (byte)Module.Interface.IPlayerToExRoleAssignData.ExRoleType.Single:
							assignData.Add(new
								PlayerToSingleRoleAssignData(
									assignedPlayerId, exRoleId, controlId));
							break;
						case (byte)Module.Interface.IPlayerToExRoleAssignData.ExRoleType.Comb:
							byte assignCombType = reader.ReadByte(); // combTypeId
							byte bytedAmongUsVanillaRoleId = reader.ReadByte(); // byted AmongUsVanillaRoleId
							assignData.Add(new
								PlayerToCombRoleAssignData(
									assignedPlayerId, exRoleId, assignCombType,
									controlId, bytedAmongUsVanillaRoleId));
							break;
					}
				}
				RPCOperator.SetRoleToAllPlayer(assignData);
				GameProgressSystem.Current = GameProgressSystem.Progress.RoleSetUpEnd;
				break;
			case RPCOperator.Command.ShareOption:
				RPCOperator.ShareOption(reader);
				break;
			case RPCOperator.Command.CustomVentUse:
				int ventId = reader.ReadPackedInt32();
				byte ventingPlayer = reader.ReadByte();
				byte isEnter = reader.ReadByte();
				RPCOperator.CustomVentUse(ventId, ventingPlayer, isEnter);
				break;
			case RPCOperator.Command.StartVentAnimation:
				int animationVentId = reader.ReadPackedInt32();
				RPCOperator.StartVentAnimation(animationVentId);
				break;
			case RPCOperator.Command.UncheckedSnapTo:
				byte snapPlayerId = reader.ReadByte();
				float snapX = reader.ReadSingle();
				float snapY = reader.ReadSingle();
				RPCOperator.UncheckedSnapTo(
					snapPlayerId, new Vector2(snapX, snapY));
				break;
			case RPCOperator.Command.UncheckedShapeShift:
				byte shapeShiftPlayerId = reader.ReadByte();
				byte shapeShiftTargetPlayerId = reader.ReadByte();
				byte shapeShiftAnimationTrigger = reader.ReadByte();
				RPCOperator.UncheckedShapeShift(
					shapeShiftPlayerId,
					shapeShiftTargetPlayerId,
					shapeShiftAnimationTrigger);
				break;
			case RPCOperator.Command.UncheckedMurderPlayer:
				byte sourceId = reader.ReadByte();
				byte targetId = reader.ReadByte();
				byte killAnimationTrigger = reader.ReadByte();
				RPCOperator.UncheckedMurderPlayer(
					sourceId, targetId, killAnimationTrigger);
				break;
			case RPCOperator.Command.UncheckedExiledPlayer:
				byte exiledTargetId = reader.ReadByte();
				RPCOperator.UncheckedExiledPlayer(exiledTargetId);
				break;
			case RPCOperator.Command.UncheckedRevive:
				byte reviveTargetId = reader.ReadByte();
				RPCOperator.UncheckedRevive(reviveTargetId);
				break;
			case RPCOperator.Command.UncheckedReportDeadbody:
				byte reporter = reader.ReadByte();
				byte reportTargetId = reader.ReadByte();
				RPCOperator.UncheckedReportDeadBody(reporter, reportTargetId);
				break;
			case RPCOperator.Command.CleanDeadBody:
				byte deadBodyPlayerId = reader.ReadByte();
				RPCOperator.CleanDeadBody(deadBodyPlayerId);
				break;
			case RPCOperator.Command.FixForceRepairSpecialSabotage:
				RPCOperator.FixForceRepairSpecialSabotage(
					reader.ReadByte());
				break;
			case RPCOperator.Command.ReplaceDeadReason:
				byte changePlayerId = reader.ReadByte();
				byte reason = reader.ReadByte();
				RPCOperator.ReplaceDeadReason(
					changePlayerId, reason);
				break;
			case RPCOperator.Command.SetRoleWin:
				byte rolePlayerId = reader.ReadByte();
				RPCOperator.SetRoleWin(rolePlayerId);
				break;
			case RPCOperator.Command.SetWinGameControlId:
				int id = reader.ReadInt32();
				RPCOperator.SetWinGameControlId(id);
				break;
			case RPCOperator.Command.SetWinPlayer:
				int playerNum = reader.ReadInt32();
				List<byte> winPlayerId = new List<byte>(playerNum);
				for (int i = 0; i < playerNum; ++i)
				{
					winPlayerId.Add(reader.ReadByte());
				}
				RPCOperator.SetWinPlayer(winPlayerId);
				break;
			case RPCOperator.Command.ShareMapId:
				byte mapId = reader.ReadByte();
				RPCOperator.ShareMapId(mapId);
				break;
			case RPCOperator.Command.ShareVersion:
				RPCOperator.AddVersionData(reader);
				break;
			case RPCOperator.Command.PlaySound:
				byte soundType = reader.ReadByte();
				float volume = reader.ReadSingle();
				RPCOperator.PlaySound(soundType, volume);
				break;
			case RPCOperator.Command.ReplaceTask:
				byte replaceTargetPlayerId = reader.ReadByte();
				int taskIndex = reader.ReadInt32();
				int taskId = reader.ReadInt32();
				RPCOperator.ReplaceTask(
					replaceTargetPlayerId, taskIndex, taskId);
				break;
			case RPCOperator.Command.IntegrateModCall:
				RPCOperator.IntegrateModCall(ref reader);
				break;
			case RPCOperator.Command.CloseMeetingVoteButton:
				RPCOperator.CloseMeetingButton();
				break;
			case RPCOperator.Command.MeetingReporterRpc:
				RPCOperator.MeetingReporterRpcOp(ref reader);
				break;
			case RPCOperator.Command.UpdateExtremeSystemType:
				RPCOperator.UpdateExtremeSystemType(ref reader);
				break;
			case RPCOperator.Command.ReplaceRole:
				byte targetPlayerId = reader.ReadByte();
				byte replaceTarget = reader.ReadByte();
				byte ops = reader.ReadByte();
				RPCOperator.ReplaceRole(
					targetPlayerId, replaceTarget, ops);
				break;
			case RPCOperator.Command.HeroHeroAcademia:
				RPCOperator.HeroHeroAcademiaCommand(ref reader);
				break;
			case RPCOperator.Command.KidsAbility:
				RPCOperator.KidsAbilityCommand(ref reader);
				break;
			case RPCOperator.Command.MoverAbility:
				RPCOperator.MoverAbility(ref reader);
				break;
			case RPCOperator.Command.AcceleratorAbility:
				RPCOperator.AcceleratorAbility(ref reader);
				break;
			case RPCOperator.Command.BodyGuardAbility:
				RPCOperator.BodyGuardAbility(ref reader);
				break;
			case RPCOperator.Command.TimeMasterAbility:
				RPCOperator.TimeMasterAbility(ref reader);
				break;
			case RPCOperator.Command.AgencyTakeTask:
				byte agencyTargetPlayerId = reader.ReadByte();
				int getTaskNum = reader.ReadInt32();

				List<int> getTaskId = new List<int>(getTaskNum);

				for (int i = 0; i < getTaskNum; ++i)
				{
					getTaskId.Add(reader.ReadInt32());
				}

				RPCOperator.AgencyTakeTask(
					agencyTargetPlayerId, getTaskId);
				break;
			case RPCOperator.Command.FencerAbility:
				RPCOperator.FencerAbility(ref reader);
				break;
			case RPCOperator.Command.CuresMakerCurseKillCool:
				byte curesMakerPlayerId = reader.ReadByte();
				byte curesPlayerId = reader.ReadByte();
				RPCOperator.CuresMakerCurseKillCool(
					curesMakerPlayerId, curesPlayerId);
				break;
			case RPCOperator.Command.CarpenterUseAbility:
				RPCOperator.CarpenterUseAbility(ref reader);
				break;
			case RPCOperator.Command.SurvivorDeadWin:
				byte survivorPlayerId = reader.ReadByte();
				RPCOperator.SurvivorDeadWin(survivorPlayerId);
				break;
			case RPCOperator.Command.CaptainAbility:
				RPCOperator.CaptainTargetVote(ref reader);
				break;
			case RPCOperator.Command.ResurrecterRpc:
				RPCOperator.ResurrecterRpc(ref reader);
				break;
			case RPCOperator.Command.TeleporterSetPortal:
				byte teleporterPlayerId = reader.ReadByte();
				float portalX = reader.ReadSingle();
				float portalY = reader.ReadSingle();
				RPCOperator.TeleporterSetPortal(
					teleporterPlayerId, portalX, portalY);
				break;
			case RPCOperator.Command.BaitAwakeRole:
				byte baitRolePlayerId = reader.ReadByte();
				RPCOperator.BaitAwakeRole(baitRolePlayerId);
				break;
			case RPCOperator.Command.SummonerOps:
				byte summonerPlayerId  = reader.ReadByte();
				float x = reader.ReadSingle();
				float y = reader.ReadSingle();
				byte summonTargetPlayerId = reader.ReadByte();
				bool isDead = reader.ReadBoolean();
				RPCOperator.SummonerRpcOps(
					summonerPlayerId, summonTargetPlayerId, x, y, isDead);
				break;
			case RPCOperator.Command.ExorcistOps:
				RPCOperator.ExorcistRpcOps(reader);
				break;
			case RPCOperator.Command.CarrierAbility:
				byte carrierCarryOpCallPlayerId = reader.ReadByte();
				float carrierPlayerPosX = reader.ReadSingle();
				float carrierPlayerPosY = reader.ReadSingle();
				byte carryDeadBodyPlayerId = reader.ReadByte();
				bool isCarryDeadBody = reader.ReadBoolean();
				RPCOperator.CarrierAbility(
					carrierCarryOpCallPlayerId,
					carrierPlayerPosX,
					carrierPlayerPosY,
					carryDeadBodyPlayerId,
					isCarryDeadBody);
				break;
			case RPCOperator.Command.PainterPaintBody:
				byte painterPlayerId = reader.ReadByte();
				byte isRandomModeMessage = reader.ReadByte();
				RPCOperator.PainterPaintBody(
					painterPlayerId,
					isRandomModeMessage);
				break;
			case RPCOperator.Command.OverLoaderSwitchAbility:
				byte overLoaderPlayerId = reader.ReadByte();
				byte activate = reader.ReadByte();
				RPCOperator.OverLoaderSwitchAbility(
					overLoaderPlayerId, activate);
				break;
			case RPCOperator.Command.CrackerCrackDeadBody:
				byte crackerId = reader.ReadByte();
				byte crackTarget = reader.ReadByte();
				RPCOperator.CrackerCrackDeadBody(crackerId, crackTarget);
				break;
			case RPCOperator.Command.MeryAbility:
				RPCOperator.MaryAbility(ref reader);
				break;
			case RPCOperator.Command.LastWolfSwitchLight:
				byte swichStatus = reader.ReadByte();
				RPCOperator.LastWolfSwitchLight(swichStatus);
				break;
			case RPCOperator.Command.CommanderAttackCommand:
				byte commanderPlayerId = reader.ReadByte();
				RPCOperator.CommanderAttackCommand(commanderPlayerId);
				break;
			case RPCOperator.Command.HypnotistAbility:
				RPCOperator.HypnotistAbility(ref reader);
				break;
			case RPCOperator.Command.UnderWarperUseVentWithNoAnime:
				byte underWarperPlayerId = reader.ReadByte();
				int targetVentId = reader.ReadPackedInt32();
				bool isVentEnter = reader.ReadBoolean();
				RPCOperator.UnderWarperUseVentWithNoAnime(
					underWarperPlayerId, targetVentId, isVentEnter);
				break;
			case RPCOperator.Command.SlimeAbility:
				RPCOperator.SlimeAbility(ref reader);
				break;
			case RPCOperator.Command.ZombieRpc:
				RPCOperator.ZombieRpc(ref reader);
				break;
			case RPCOperator.Command.ThiefAddDeadbodyEffect:
				byte addEffectTargetDeadBody = reader.ReadByte();
				RPCOperator.ThiefAddEffect(addEffectTargetDeadBody);
				break;
			case RPCOperator.Command.BoxerRpcOps:
				RPCOperator.BoxerRpcOps(reader);
				break;
			case RPCOperator.Command.AliceShipBroken:
				byte alicePlayerId = reader.ReadByte();
				byte newTaskSetPlayerId = reader.ReadByte();
				int newTaskNum = reader.ReadInt32();

				List<int> task = new List<int>(newTaskNum);

				for (int i = 0; i < newTaskNum; ++i)
				{
					task.Add(reader.ReadInt32());
				}
				RPCOperator.AliceShipBroken(
					alicePlayerId, newTaskSetPlayerId, task);
				break;
			case RPCOperator.Command.JesterOutburstKill:
				byte outburstKillerId = reader.ReadByte();
				byte killTargetId = reader.ReadByte();
				RPCOperator.JesterOutburstKill(
					outburstKillerId, killTargetId);
				break;
			case RPCOperator.Command.YandereSetOneSidedLover:
				byte yanderePlayerId = reader.ReadByte();
				byte loverPlayerId = reader.ReadByte();
				RPCOperator.YandereSetOneSidedLover(
					yanderePlayerId, loverPlayerId);
				break;
			case RPCOperator.Command.TotocalcioSetBetPlayer:
				byte totocalcioPlayerId = reader.ReadByte();
				byte betPlayerId = reader.ReadByte();
				RPCOperator.TotocalcioSetBetPlayer(
					totocalcioPlayerId, betPlayerId);
				break;
			case RPCOperator.Command.MinerHandle:
				RPCOperator.MinerHandle(ref reader);
				break;
			case RPCOperator.Command.MadmateToFakeImpostor:
				byte madmatePlayerId = reader.ReadByte();
				RPCOperator.MadmateToFakeImpostor(
					madmatePlayerId);
				break;
			case RPCOperator.Command.ArtistRpcOps:
				RPCOperator.ArtistDrawOps(reader);
				break;
			case RPCOperator.Command.SetGhostRole:
				RPCOperator.SetGhostRole(
					ref reader);
				break;
			case RPCOperator.Command.UseGhostRoleAbility:
				byte useGhostRoleType = reader.ReadByte();
				bool isReport = reader.ReadBoolean();
				RPCOperator.UseGhostRoleAbility(
					useGhostRoleType, isReport, ref reader);
				break;
			case RPCOperator.Command.XionAbility:
				RPCOperator.XionAbility(ref reader);
				break;
			default:
				break;
		}
	}
}

// ロビーで大量のNullエラーが出るバニラの不具合の修正(Nullチェックちゃんとしろ！！！！！！)
[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.HandleRoleRpc))]
public static class PlayerControlHandleRoleRpcPatch
{
	public static bool Prefix(PlayerControl __instance)
		=> __instance.Data != null && __instance.Data.Role != null;
}
