using System.Linq;
using System.Collections.Generic;
using Hazel;

using UnityEngine;

using TMPro;

using ExtremeRoles.Roles.API;

using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Helper;
using ExtremeRoles.Performance;
using ExtremeRoles.Performance.Il2Cpp;
using AmongUs.GameOptions;
using ExtremeRoles.Module.SystemType;
using ExtremeRoles.Module.SystemType.Roles;

namespace ExtremeRoles.Roles.Solo.Host;

public sealed partial class Xion
{
    public const float MaxSpeed = 20.0f;
    public const float MinSpeed = 0.01f;

    public enum XionRpcOpsCode : byte
    {
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

    private List<GameObject> dummyDeadBody = new List<GameObject>();

    private const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

    public static void UseAbility(ref MessageReader reader)
    {
        byte playerId = reader.ReadByte();
        XionRpcOpsCode ops = (XionRpcOpsCode)reader.ReadByte();
        Xion xion = ExtremeRoleManager.GetSafeCastedRole<Xion>(playerId);
        NetworkedPlayerInfo xionPlayer = GameData.Instance.GetPlayerById(playerId);

        switch (ops)
        {
            case XionRpcOpsCode.UpdateSpeed:
                SpeedOps speedOps = (SpeedOps)reader.ReadByte();
                if (xion == null) { return; }
                updateSpeed(xion, speedOps);
                break;
            case XionRpcOpsCode.Teleport:
                float x = reader.ReadSingle();
                float y = reader.ReadSingle();
                if (xionPlayer == null ||
					xionPlayer.Object == null) { return; }
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
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(
            Input.mousePosition);
        GameObject body = GameSystem.CreateNoneReportableDeadbody(
            PlayerControl.LocalPlayer, mouseWorldPos);
        this.dummyDeadBody.Add(body);
    }

    // RPC周り
    public void RpcCallMeeting()
    {
        PlayerControl xionPlayer = PlayerControl.LocalPlayer;
        MeetingRoomManager.Instance.AssignSelf(xionPlayer, null);
        HudManager.Instance.OpenMeetingRoom(xionPlayer);
        xionPlayer.RpcStartMeeting(null);
    }

    public void RpcForceEndGame()
    {
        GameSystem.ForceEndGame();
    }

    public void RpcRepairSabotage()
    {
        GameSystem.RpcRepairAllSabotage();

        foreach (var door in ShipStatus.Instance.AllDoors)
        {
            DeconControl decon = door.GetComponentInChildren<DeconControl>();
            if (decon != null) { continue; }

            ShipStatus.Instance.RpcUpdateSystem(
                SystemTypes.Doors, (byte)(door.Id | 64));
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


	public static void RpcNoXionVote()
    {
        AmongUsClient.Instance.FinishRpcImmediately(
            createWriter(XionRpcOpsCode.NoXionVote));
    }

    public static void RpcRoleReplaceOps(byte targetPlayerId, string roleName)
    {
        if (!System.Enum.TryParse(roleName, out ExtremeRoleId roleId))
        {
            addChat(Tr.GetString("invalidRoleName"));
            return;
        }

        if (!System.Enum.IsDefined(typeof(ExtremeRoleId), roleId))
        {
            addChat(Tr.GetString("invalidRoleName"));
            return;
        }
        int intedRoleId = (int)roleId;

        if (!ExtremeRoleManager.NormalRole.ContainsKey(intedRoleId))
        {
            addChat(Tr.GetString("invalidRoleName"));
            return;
        }

        MessageWriter writer = createWriter(XionRpcOpsCode.RepcalePlayerRole);
        writer.Write(targetPlayerId);
        writer.WritePacked(intedRoleId);
        finishWrite(writer);
        replaceToRole(targetPlayerId, intedRoleId);

        addChat(
            string.Format(
                Tr.GetString("setRole"),
                Tr.GetString(
                    Player.GetPlayerControlById(
                        targetPlayerId).Data.DefaultOutfit.PlayerName),
                Tr.GetString(roleId.ToString())));
    }

    public static void RpcHostToXion()
    {
        if (xionBuffer == null)
        {
            addChat(Tr.GetString("XionNow"));
            return;
        }

        byte xionPlayerId = PlayerControl.LocalPlayer.PlayerId;

        finishWrite(createWriter(XionRpcOpsCode.BackXion));
        hostToXion(xionPlayerId);

        addChat(Tr.GetString("RevartXion"));
    }

	public static void RpcTeleportTo(Vector2 pos)
	{
		MessageWriter writer = createWriter(XionRpcOpsCode.Teleport);
		writer.Write(pos.x);
		writer.Write(pos.y);
		finishWrite(writer);
		teleport(PlayerControl.LocalPlayer, pos);
	}

    private static void rpcTeleport(PlayerControl targetPlayer)
    {
        if (targetPlayer == null) { return; }
        Vector2 targetPos = targetPlayer.transform.position;
		RpcTeleportTo(targetPos);
    }

    // RPC終了

    private static MessageWriter createWriter(XionRpcOpsCode opsCode)
    {
        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(
            PlayerControl.LocalPlayer.NetId,
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
        resetRole(
            Player.GetPlayerControlById(hostPlayerId), hostPlayerId);
        setNewRole(hostPlayerId, xionBuffer);

        if (Patches.Manager.HudManagerUpdatePatch.PlayerInfoText.TryGetValue(
            hostPlayerId, out TextMeshPro info))
        {
            info.text = xionBuffer.GetColoredRoleName(true);
        }

        RemoveXionPlayerToAllPlayerControl();

        xionBuffer = null;
    }

    private static void replaceToRole(byte targetPlayerId, int roleId)
    {
        SingleRoleBase baseRole = ExtremeRoleManager.GameRole[targetPlayerId];
        bool isXion = baseRole.Core.Id == ExtremeRoleId.Xion;

        ExtremeRolesPlugin.Logger.LogInfo(
            $"targetPlayerId:{targetPlayerId}   roleId:{roleId}");

        PlayerControl targetPlayer = Player.GetPlayerControlById(targetPlayerId);

        // 見つからなかったので探して追加
        if (targetPlayer == null)
        {
            PlayerControl[] array = Object.FindObjectsOfType<PlayerControl>();
            for (int i = 0; i < array.Length; ++i)
            {
                if (array[i].PlayerId == targetPlayerId)
                {
                    targetPlayer = array[i];

                    PlayerControl.AllPlayerControls.Add(targetPlayer);
					PlayerCache.AddPlayerControl(targetPlayer);
                    break;
                }
            }
            if (targetPlayer == null)
            {
                ExtremeRolesPlugin.Logger.LogInfo("SetRole Missing!!");
                return;
            }
        }

        if (isXion)
        {
            RPCOperator.UncheckedRevive(targetPlayerId);
        }

        resetRole(targetPlayer, targetPlayerId);

        SingleRoleBase role = ExtremeRoleManager.NormalRole[roleId];
        SingleRoleBase addRole = role.Clone();

        var roleManager = RoleManager.Instance;

        if (addRole.IsImpostor())
        {
            roleManager.SetRole(targetPlayer, RoleTypes.Impostor);
        }
        else
        {
            roleManager.SetRole(targetPlayer, RoleTypes.Crewmate);
        }

        if (addRole is IRoleAbility abilityRole &&
            PlayerControl.LocalPlayer.PlayerId == targetPlayerId)
        {
            Logging.Debug("Try Create Ability NOW!!!");
            abilityRole.CreateAbility();
        }

        addRole.Initialize();
        addRole.SetControlId(baseRole.GameControlId);

        setNewRole(targetPlayerId, addRole);

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
                xion.MoveSpeed = GameOptionsManager.Instance.CurrentGameOptions.GetFloat(
                    FloatOptionNames.PlayerSpeedMod);
                break;
            default:
                break;
        }
    }

    private static void teleport(PlayerControl xionPlayer, Vector2 targetPos)
    {
        xionPlayer.NetTransform.SnapTo(targetPos);
    }
    private static void NoXionVote(
        Xion xion)
    {
        xion.AddNoXionCount();
    }
}
