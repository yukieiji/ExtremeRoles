using System.Linq;
using System.Collections.Generic;
using Hazel;

using UnityEngine;

using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;

using ExtremeRoles.Roles.API.Interface;
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
            UpdateSpeed,
            Teleport,
            NoXionVote,
            BackXion,
            RepcalePlayerRole,
            TestRpc,
        }
        private enum SpeedOps : byte
        {
            Reset,
            Up,
            Down
        }

        private List<SpriteRenderer> dummyDeadBody = new List<SpriteRenderer>();

        private const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

        public static void UseAbility(ref MessageReader reader)
        {
            byte playerId = reader.ReadByte();
            XionRpcOpsCode ops = (XionRpcOpsCode)reader.ReadByte();
            Xion xion = ExtremeRoleManager.GetSafeCastedRole<Xion>(playerId);
            GameData.PlayerInfo xionPlayer = GameData.Instance.GetPlayerById(playerId);

            switch (ops)
            {
                case XionRpcOpsCode.ForceEndGame:
                    RPCOperator.ForceEnd();
                    break;
                case XionRpcOpsCode.UpdateSpeed:
                    SpeedOps speedOps = (SpeedOps)reader.ReadByte();
                    if (xion == null) { return; }
                    updateSpeed(xion, speedOps);
                    break;
                case XionRpcOpsCode.Teleport:
                    float x = reader.ReadSingle();
                    float y = reader.ReadSingle();
                    if (xionPlayer?.Object == null) { return; }
                    teleport(xionPlayer.Object, new Vector2(x, y));
                    break;
                case XionRpcOpsCode.NoXionVote:
                    if (!isXion() || xion == null) { return; }
                    NoXionVote(xion);
                    break;
                case XionRpcOpsCode.BackXion:
                    hostToXion(playerId);
                    break;
                case XionRpcOpsCode.RepcalePlayerRole:
                    byte targetPlayerId = reader.ReadByte();
                    int roleId = reader.ReadPackedInt32();
                    replaceToRole(targetPlayerId, roleId);
                    break;
                case XionRpcOpsCode.TestRpc:
                    // 色々と
                    if (xion == null) { return; }
                    // 呼び出す関数
                    break;
                default:
                    break;
            }
        }

        public void SpawnDummyDeadBody()
        {
            PlayerControl player = CachedPlayerControl.LocalPlayer;
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(
                Input.mousePosition);
            mouseWorldPos.z = mouseWorldPos.y / 1000f;

            var killAnimation = player.KillAnimations[0];
            SpriteRenderer body = UnityEngine.Object.Instantiate(
                killAnimation.bodyPrefab.bodyRenderer);

            player.SetPlayerMaterialColors(body);

            Vector3 vector = mouseWorldPos + killAnimation.BodyOffset;
            vector.z = vector.y / 1000f;
            body.transform.position = vector;
            body.transform.localScale = new Vector3(0.35f, 0.35f, 0.35f);
            this.dummyDeadBody.Add(body);
        }

        // RPC周り
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
            finishWrite(writer);
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
            finishWrite(writer);
            updateSpeed(this, SpeedOps.Up);
        }

        public void RpcSpeedDown()
        {
            MessageWriter writer = createWriter(XionRpcOpsCode.UpdateSpeed);
            writer.Write((byte)SpeedOps.Down);
            finishWrite(writer);
            updateSpeed(this, SpeedOps.Down);
        }

        public void RpcResetSpeed()
        {
            MessageWriter writer = createWriter(XionRpcOpsCode.UpdateSpeed);
            writer.Write((byte)SpeedOps.Reset);
            finishWrite(writer);
            updateSpeed(this, SpeedOps.Reset);
        }

        public void RpcTestAbilityCall()
        {
            MessageWriter writer = createWriter(XionRpcOpsCode.TestRpc);
            // 色々と
            AmongUsClient.Instance.FinishRpcImmediately(writer);
            // 必要な関数書く
        }

        public void RpcKill(byte targetPlayerId)
        {
            RPCOperator.Call(
                CachedPlayerControl.LocalPlayer.PlayerControl.NetId,
                RPCOperator.Command.UncheckedMurderPlayer,
                new List<byte> { targetPlayerId, targetPlayerId, byte.MinValue });
            RPCOperator.UncheckedMurderPlayer(
                targetPlayerId,
                targetPlayerId,
                byte.MinValue);
        }

        public void RpcRevive(byte targetPlayerId)
        {
            RPCOperator.Call(
                CachedPlayerControl.LocalPlayer.PlayerControl.NetId,
                RPCOperator.Command.UncheckedRevive,
                new List<byte> { targetPlayerId });
            RPCOperator.UncheckedRevive(
                targetPlayerId);
        }

        public void RpcTeleport(PlayerControl targetPlayer)
        {
            if (targetPlayer == null) { return; }
            Vector2 targetPos = targetPlayer.transform.position;
            MessageWriter writer = createWriter(XionRpcOpsCode.UpdateSpeed);
            writer.Write(targetPos.x);
            writer.Write(targetPos.y);
            finishWrite(writer);
            teleport(CachedPlayerControl.LocalPlayer, targetPos);
        }

        public static void RpcNoXionVote()
        {
            AmongUsClient.Instance.FinishRpcImmediately(
                createWriter(XionRpcOpsCode.NoXionVote));
        }

        public static void RpcRoleReplaceOps(byte targetPlayerId, string roleName)
        {
            if (!System.Enum.TryParse(roleName, out ExtremeRoleId roleId))
            {
                addChat(Translation.GetString("invalidRoleName"));
                return;
            }

            if (!System.Enum.IsDefined(typeof(ExtremeRoleId), roleId))
            {
                addChat(Translation.GetString("invalidRoleName"));
                return;
            }
            int intedRoleId = (int)roleId;

            MessageWriter writer = createWriter(XionRpcOpsCode.RepcalePlayerRole);
            writer.Write(targetPlayerId);
            writer.WritePacked(intedRoleId);
            finishWrite(writer);
            replaceToRole(targetPlayerId, intedRoleId);
        }

        public static void RpcHostToXion()
        {
            if (xionBuffer == null)
            {
                addChat(Translation.GetString("XionNow"));
                return;
            }

            addChat(Translation.GetString("RevartXionStart"));

            byte xionPlayerId = CachedPlayerControl.LocalPlayer.PlayerId;

            finishWrite(createWriter(XionRpcOpsCode.BackXion));
            hostToXion(xionPlayerId);

            addChat(Translation.GetString("RevartXionEnd"));
        }
        // RPC終了

        private static MessageWriter createWriter(XionRpcOpsCode opsCode)
        {
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(
                CachedPlayerControl.LocalPlayer.PlayerControl.NetId,
                (byte)RPCOperator.Command.XionAbility,
                Hazel.SendOption.Reliable, -1);
            writer.Write(PlayerId);
            writer.Write((byte)opsCode);

            return writer;
        }

        private static void finishWrite(MessageWriter writer)
        {
            AmongUsClient.Instance.FinishRpcImmediately(writer);
        }

        private static void hostToXion(byte hostPlayerId)
        {
            xionPlayerToDead(hostPlayerId);
            resetRole(hostPlayerId);
            setNewRole(hostPlayerId, xionBuffer);
            xionBuffer = null;
        }

        private static void replaceToRole(byte targetPlayerId, int roleId)
        {
            SingleRoleBase baseRole = ExtremeRoleManager.GameRole[targetPlayerId];
            bool isXion = baseRole.Id == ExtremeRoleId.Xion;

            if (isXion)
            {
                RPCOperator.UncheckedRevive(targetPlayerId);
            }

            resetRole(targetPlayerId);
            
            SingleRoleBase role = ExtremeRoleManager.NormalRole[roleId];
            SingleRoleBase addRole = role.Clone();

            IRoleAbility abilityRole = addRole as IRoleAbility;

            if (abilityRole != null && 
                CachedPlayerControl.LocalPlayer.PlayerId == targetPlayerId)
            {
                Logging.Debug("Try Create Ability NOW!!!");
                abilityRole.CreateAbility();
            }

            addRole.Initialize();
            addRole.GameControlId = baseRole.GameControlId;

            lock (ExtremeRoleManager.GameRole)
            {
                ExtremeRoleManager.GameRole[targetPlayerId] = addRole;
            }
            Logging.Debug($"PlayerId:{targetPlayerId}   AssignTo:{addRole.RoleName}");
            
            if (isXion)
            {
                xionBuffer = (Xion)baseRole;
            }
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
                    xion.IsBoost = true;
                    float newDownSpeed = xion.MoveSpeed * 0.8f;
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

        private static void teleport(PlayerControl xionPlayer, Vector2 targetPos)
        {
            xionPlayer.NetTransform.SnapTo(targetPos);
        }


        private static void spawnDummy()
        {
            var playerControl = Object.Instantiate(AmongUsClient.Instance.PlayerPrefab);
            playerControl.PlayerId = (byte)GameData.Instance.GetAvailableId();

            GameData.Instance.AddPlayer(playerControl);
            AmongUsClient.Instance.Spawn(playerControl, -2, InnerNet.SpawnFlags.None);

            var hatManager = FastDestroyableSingleton<HatManager>.Instance;

            int hat = RandomGenerator.Instance.Next(hatManager.allHats.Count);
            int pet = RandomGenerator.Instance.Next(hatManager.allPets.Count);
            int skin = RandomGenerator.Instance.Next(hatManager.allSkins.Count);
            int visor = RandomGenerator.Instance.Next(hatManager.allVisors.Count);
            int color = RandomGenerator.Instance.Next(Palette.PlayerColors.Length);

            playerControl.transform.position = CachedPlayerControl.LocalPlayer.transform.position;
            playerControl.GetComponent<DummyBehaviour>().enabled = true;
            playerControl.NetTransform.enabled = false;
            playerControl.SetName($"XionDummy_{randomString(10)}");
            playerControl.SetColor(color);
            playerControl.SetHat(hatManager.allHats[hat].ProdId, color);
            playerControl.SetPet(hatManager.allPets[pet].ProdId, color);
            playerControl.SetVisor(hatManager.allVisors[visor].ProdId, color);
            playerControl.SetSkin(hatManager.allSkins[skin].ProdId, color);
            GameData.Instance.RpcSetTasks(
                playerControl.PlayerId,
                new byte[0]);
        }
        private static void NoXionVote(
            Xion xion)
        {
            xion.AddNoXionCount();
        }

        private static string randomString(int length)
        {
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[RandomGenerator.Instance.Next(s.Length)]).ToArray());
        }

    }
}
