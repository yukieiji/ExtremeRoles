using System.Linq;
using System.Collections.Generic;
using Hazel;

using UnityEngine;

using ExtremeRoles.Helper;
using ExtremeRoles.Performance;
using ExtremeRoles.Performance.Il2Cpp;

namespace ExtremeRoles.Roles.Solo.Host
{
    public sealed partial class Xion
    {
        public const float MaxSpeed = 20.0f;
        public const float MinSpeed = 0.01f;

        public enum XionRpcOpsCode : byte
        {
            ForceEndGame,
            SpawnDummyDeadBody,
            UpdateSpeed,
        }
        private enum SpeedOps : byte
        {
            Reset,
            Up,
            Down
        }

        private const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

        private static List<PlayerControl> bot = new List<PlayerControl>();

        public static void UseAbility(ref MessageReader reader)
        {
            byte playerId = reader.ReadByte();
            XionRpcOpsCode ops = (XionRpcOpsCode)reader.ReadByte();
            switch (ops)
            {
                case XionRpcOpsCode.ForceEndGame:
                    RPCOperator.ForceEnd();
                    break;
                case XionRpcOpsCode.SpawnDummyDeadBody:
                    float posX = reader.ReadSingle();
                    float posY = reader.ReadSingle();
                    spawnDummyDeadBody(playerId, posX, posY);
                    break;
                case XionRpcOpsCode.UpdateSpeed:
                    Xion xion = ExtremeRoleManager.GetSafeCastedRole<Xion>(playerId);
                    SpeedOps speedOps = (SpeedOps)reader.ReadByte();
                    updateSpeed(xion, speedOps);
                    break;
                default:
                    break;
            }
        }

        public void RpcCallMeeting()
        {
            PlayerControl xionPlayer = CachedPlayerControl.LocalPlayer;
            MeetingRoomManager.Instance.AssignSelf(xionPlayer, null);
            FastDestroyableSingleton<HudManager>.Instance.OpenMeetingRoom(xionPlayer);
            xionPlayer.RpcStartMeeting(null);
        }

        public void RpcForceEndGame()
        {
            MessageWriter writer = createWriter(XionRpcOpsCode.ForceEndGame);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
            RPCOperator.ForceEnd();
        }

        public void RpcRepairSabotage()
        {
            foreach (PlayerTask task in 
                PlayerControl.LocalPlayer.myTasks.GetFastEnumerator())
            {
                if (task == null) { continue; }

                TaskTypes taskType = task.TaskType;

                if (ExtremeRolesPlugin.Compat.IsModMap)
                {
                    if (ExtremeRolesPlugin.Compat.ModMap.IsCustomSabotageTask(taskType))
                    {
                        ExtremeRolesPlugin.Compat.ModMap.RpcRepairCustomSabotage(
                            taskType);
                        continue;
                    }
                }
                switch (taskType)
                {
                    case TaskTypes.FixLights:

                        RPCOperator.Call(
                            PlayerControl.LocalPlayer.NetId,
                            RPCOperator.Command.FixLightOff);
                        RPCOperator.FixLightOff();
                        break;
                    case TaskTypes.RestoreOxy:
                        CachedShipStatus.Instance.RpcRepairSystem(
                            SystemTypes.LifeSupp, 0 | 64);
                        CachedShipStatus.Instance.RpcRepairSystem(
                            SystemTypes.LifeSupp, 1 | 64);
                        break;
                    case TaskTypes.ResetReactor:
                        CachedShipStatus.Instance.RpcRepairSystem(
                            SystemTypes.Reactor, 16);
                        break;
                    case TaskTypes.ResetSeismic:
                        CachedShipStatus.Instance.RpcRepairSystem(
                            SystemTypes.Laboratory, 16);
                        break;
                    case TaskTypes.FixComms:
                        CachedShipStatus.Instance.RpcRepairSystem(
                            SystemTypes.Comms, 16 | 0);
                        CachedShipStatus.Instance.RpcRepairSystem(
                            SystemTypes.Comms, 16 | 1);
                        break;
                    case TaskTypes.StopCharles:
                        CachedShipStatus.Instance.RpcRepairSystem(
                            SystemTypes.Reactor, 0 | 16);
                        CachedShipStatus.Instance.RpcRepairSystem(
                            SystemTypes.Reactor, 1 | 16);
                        break;
                    default:
                        break;
                }
            }

            foreach (var door in CachedShipStatus.Instance.AllDoors)
            {
                CachedShipStatus.Instance.RpcRepairSystem(
                    SystemTypes.Doors, door.Id | 64);
                door.SetDoorway(true);
            }
        }

        public void RpcSpeedUp()
        {
            MessageWriter writer = createWriter(XionRpcOpsCode.UpdateSpeed);
            writer.Write((byte)SpeedOps.Up);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
        }

        public void RpcSpeedDown()
        {
            MessageWriter writer = createWriter(XionRpcOpsCode.UpdateSpeed);
            writer.Write((byte)SpeedOps.Down);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
        }

        public void RpcResetSpeed()
        {
            MessageWriter writer = createWriter(XionRpcOpsCode.UpdateSpeed);
            writer.Write((byte)SpeedOps.Reset);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
        }

        private MessageWriter createWriter(XionRpcOpsCode opsCode)
        {
            PlayerControl xionPlayer = CachedPlayerControl.LocalPlayer;

            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(
                xionPlayer.NetId,
                (byte)RPCOperator.Command.XionAbility,
                Hazel.SendOption.Reliable, -1);
            writer.Write(xionPlayer.PlayerId);
            writer.Write((byte)opsCode);

            return writer;
        }

        private static void spawnDummyDeadBody(
            byte playerId, float posX, float posY)
        {
            PlayerControl player = Player.GetPlayerControlById(playerId);

            if (player == null) { return; }

            var killAnimation = player.KillAnimations[0];
            SpriteRenderer body = UnityEngine.Object.Instantiate(
                killAnimation.bodyPrefab.bodyRenderer);

            player.SetPlayerMaterialColors(body);

            Vector3 vector = new Vector3(posX, posY, posY / 1000f) + killAnimation.BodyOffset;
            body.transform.position = vector;
            body.transform.localScale = new Vector3(0.35f, 0.35f, 0.35f);
        }

        private static void updateSpeed(
            Xion xion, SpeedOps ops)
        {
            switch (ops)
            {
                case SpeedOps.Up:
                    xion.IsBoost = true;
                    float newBoostSpeed = xion.MoveSpeed * 1.25f;
                    xion.MoveSpeed = Mathf.Clamp(newBoostSpeed, MinSpeed, MaxSpeed);
                    break;
                case SpeedOps.Down:
                    xion.IsBoost = false;
                    float newDownSpeed = xion.MoveSpeed * 0.85f;
                    xion.MoveSpeed = Mathf.Clamp(newDownSpeed, MinSpeed, MaxSpeed);
                    break;
                case SpeedOps.Reset:
                    xion.IsBoost = false;
                    xion.MoveSpeed = PlayerControl.GameOptions.PlayerSpeedMod;
                    break;
                default:
                    break;
            }
        }

        private static void spawnDummy()
        {
            var playerControl = Object.Instantiate(AmongUsClient.Instance.PlayerPrefab);
            playerControl.PlayerId = (byte)GameData.Instance.GetAvailableId();

            GameData.Instance.AddPlayer(playerControl);
            AmongUsClient.Instance.Spawn(playerControl, -2, InnerNet.SpawnFlags.None);

            int hat = RandomGenerator.Instance.Next(HatManager.Instance.allHats.Count);
            int pet = RandomGenerator.Instance.Next(HatManager.Instance.allPets.Count);
            int skin = RandomGenerator.Instance.Next(HatManager.Instance.allSkins.Count);
            int visor = RandomGenerator.Instance.Next(HatManager.Instance.allVisors.Count);
            int color = RandomGenerator.Instance.Next(Palette.PlayerColors.Length);

            playerControl.transform.position = PlayerControl.LocalPlayer.transform.position;
            playerControl.GetComponent<DummyBehaviour>().enabled = true;
            playerControl.NetTransform.enabled = false;
            playerControl.SetName($"XionDummy_{randomString(10)}");
            playerControl.SetColor(color);
            playerControl.SetHat(HatManager.Instance.allHats[hat].ProdId, color);
            playerControl.SetPet(HatManager.Instance.allPets[pet].ProdId, color);
            playerControl.SetVisor(HatManager.Instance.allVisors[visor].ProdId);
            playerControl.SetSkin(HatManager.Instance.allSkins[skin].ProdId, color);
            bot.Add(playerControl);
            GameData.Instance.RpcSetTasks(
                playerControl.PlayerId,
                new byte[0]);
        }

        private static string randomString(int length)
        {
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[RandomGenerator.Instance.Next(s.Length)]).ToArray());
        }

    }
}
