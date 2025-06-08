﻿using System;
using System.Collections.Generic;
using System.Linq;

using InnerNet;
using Hazel;
using AmongUs.GameOptions;

using ExtremeRoles.Module.ExtremeShipStatus;
using ExtremeRoles.Module.RoleAssign;
using ExtremeRoles.Extension.Player;
using ExtremeRoles.Extension.Ship;
using ExtremeRoles.Performance;
using ExtremeRoles.Compat.ModIntegrator;
using ExtremeRoles.Compat;
using ExtremeRoles.Module.GameResult;
using ExtremeRoles.Roles.Solo.Neutral.Yandere;



namespace ExtremeRoles;

public static class RPCOperator
{
    public enum Command : byte
    {
        // メインコントール
        Initialize = 70,
        ForceEnd,
        SetRoleToAllPlayer,
        ShareOption,
        CustomVentUse,
        StartVentAnimation,
        UncheckedSnapTo,
        UncheckedShapeShift,
        UncheckedMurderPlayer,
		UncheckedExiledPlayer,
		UncheckedRevive,
        CleanDeadBody,
        FixForceRepairSpecialSabotage,
        ReplaceDeadReason,
        SetRoleWin,
        SetWinGameControlId,
        SetWinPlayer,
        ShareMapId,
        ShareVersion,
        PlaySound,
        ReplaceTask,
        IntegrateModCall,
        CloseMeetingVoteButton,
        MeetingReporterRpc,
		UpdateExtremeSystemType,

        // 役職関連
        // 役職メインコントール
        ReplaceRole,

        // コンビロール全般
        HeroHeroAcademia,
        KidsAbility,
        MoverAbility,
		AcceleratorAbility,

		// クルーメイト
		BodyGuardAbility,
        TimeMasterAbility,
        AgencyTakeTask,
        FencerAbility,
        CuresMakerCurseKillCool,
        CarpenterUseAbility,
        SurvivorDeadWin,
        CaptainAbility,
        ResurrecterRpc,
        TeleporterSetPortal,
		BaitAwakeRole,
		SummonerOps,

        // インポスター
        CarrierAbility,
        PainterPaintBody,
        OverLoaderSwitchAbility,
        CrackerCrackDeadBody,
        MeryAbility,
        LastWolfSwitchLight,
        CommanderAttackCommand,
        HypnotistAbility,
        UnderWarperUseVentWithNoAnime,
        SlimeAbility,
        ZombieRpc,
		ThiefAddDeadbodyEffect,

		// ニュートラル
		AliceShipBroken,
        JesterOutburstKill,
        YandereSetOneSidedLover,
        TotocalcioSetBetPlayer,
		MinerHandle,
		MadmateToFakeImpostor,
		ArtistRpcOps,

        // 幽霊役職
        SetGhostRole,
        UseGhostRoleAbility,

        XionAbility,
    }

    public sealed class RpcCaller : IDisposable
    {
        private MessageWriter writer;

        public RpcCaller(
            uint netId, Command cmd,
            SendOption sendOpt = SendOption.Reliable, int target = -1)
        {
            this.writer = AmongUsClient.Instance.StartRpcImmediately(
                netId, (byte)cmd, sendOpt, target);
        }

        public void WriteBoolean(bool value)
        {
            this.writer.Write(value);
        }

        public void WriteSByte(sbyte value)
        {
            this.writer.Write(value);
        }

        public void WriteByte(byte value)
        {
            this.writer.Write(value);
        }
        public void WriteUShot(ushort value)
        {
            this.writer.Write(value);
        }

        public void WriteUInt(uint value)
        {
            this.writer.Write(value);
        }

        public void WriteInt(int value)
        {
            this.writer.Write(value);
        }

        public void WriteUlong(ulong value)
        {
            this.writer.Write(value);
        }

        public void WriteFloat(float value)
        {
            this.writer.Write(value);
        }

        public void WriteStr(string value)
        {
            this.writer.Write(value);
        }

        public void WritePackedInt(int value)
        {
            this.writer.WritePacked(value);
        }
        public void WritePackedUInt(uint value)
        {
            this.writer.WritePacked(value);
        }

        public void WriteNetObject(InnerNetObject obj)
		{
			this.writer.WriteNetObject(obj);
		}

		public void WriteWriter(MessageWriter writer, bool includeHeader)
		{
			this.writer.Write(writer, includeHeader);
		}

		public void Dispose()
        {
            AmongUsClient.Instance.FinishRpcImmediately(this.writer);
        }
    }

    public static RpcCaller CreateCaller(Command ops)
    {
        return CreateCaller(PlayerControl.LocalPlayer.NetId, ops);
    }

    public static RpcCaller CreateCaller(uint netId, Command ops)
    {
        return new RpcCaller(netId, ops);
    }

    public static void Call(Command ops)
    {
        Call(PlayerControl.LocalPlayer.NetId, ops);
    }

    public static void Call(uint netId, Command ops)
    {
        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(
            netId, (byte)ops,
            Hazel.SendOption.Reliable, -1);
        AmongUsClient.Instance.FinishRpcImmediately(writer);
    }

    public static void CleanDeadBody(byte targetId)
    {
        DeadBody[] array = UnityEngine.Object.FindObjectsOfType<DeadBody>();
        for (int i = 0; i < array.Length; ++i)
        {
            if (GameData.Instance.GetPlayerById(array[i].ParentId).PlayerId == targetId)
            {
                UnityEngine.Object.Destroy(array[i].gameObject);
                break;
            }
        }
    }

    public static void Initialize()
    {
        Helper.Player.ResetTarget();
        ExtremeRolesPlugin.ShipState.Initialize();


		ExtremeGameResultManager.TryDestroy();
		Module.CustomVent.TryDestroy();

		// チェックポイントリセット
		Helper.Logging.ResetCkpt();

        // キルアニメーションリセット
        Patches.KillAnimationCoPerformKillPatch.HideNextAnimation = false;

        // 各種表示系リセット
        Patches.Manager.HudManagerUpdatePatch.Reset();
        Module.VisionComputer.Instance.ResetModifier();

        // ミーティング能力リセット
        Patches.Meeting.Hud.MeetingHudSelectPatch.SetSelectBlock(false);

        // 各種システムコンソールリセット
        Patches.MiniGame.VitalsMinigameUpdatePatch.Initialize();
        Patches.MapOverlay.MapCountOverlayUpdatePatch.Initialize();
	}

    public static void ForceEnd()
    {
		if (Helper.GameSystem.IsLobby) { return; }

        foreach (PlayerControl player in PlayerCache.AllPlayerControl)
        {
            if (!player.Data.Role.IsImpostor)
            {
                player.RemoveProtection();
                player.MurderPlayer(player);
                player.Data.IsDead = true;
            }
        }
    }
    public static void FixForceRepairSpecialSabotage(byte systemType)
    {
		Helper.GameSystem.ForceRepairrSpecialSabotage((SystemTypes)systemType);
    }

    public static void SetRoleToAllPlayer(
        List<Module.Interface.IPlayerToExRoleAssignData> assignData)
    {
        foreach (var data in assignData)
        {
            switch (data.RoleType)
            {
                case (byte)Module.Interface.IPlayerToExRoleAssignData.ExRoleType.Single:
                    Roles.ExtremeRoleManager.SetPlyerIdToSingleRoleId(
                        data.RoleId, data.PlayerId, data.ControlId);
                    break;
                case (byte)Module.Interface.IPlayerToExRoleAssignData.ExRoleType.Comb:
                    var combData = (PlayerToCombRoleAssignData)data;
                    Roles.ExtremeRoleManager.SetPlayerIdToMultiRoleId(
                        combData.CombTypeId,
                        combData.RoleId,
                        combData.PlayerId,
                        combData.ControlId,
                        combData.AmongUsRoleId);
                    break;
            }
        }
    }

    public static void ShareOption(in MessageReader reader)
    {
		OptionManager.ShareOption(reader);
    }

    public static void ReplaceDeadReason(byte playerId, byte reason)
    {
        ExtremeRolesPlugin.ShipState.ReplaceDeadReason(
            playerId, (ExtremeShipStatus.PlayerStatus)reason);
    }

    public static void CustomVentUse(
        int ventId, byte playerId, byte isEnter)
    {
        PlayerControl player = Helper.Player.GetPlayerControlById(playerId);
        if (player == null) { return; }

        MessageReader reader = new MessageReader();

        byte[] bytes = BitConverter.GetBytes(ventId);
        if (!BitConverter.IsLittleEndian)
        {
            Array.Reverse(bytes);
        }
        reader.Buffer = bytes;
        reader.Length = bytes.Length;

		ShipStatus.Instance.StartVentAnimation(ventId);

		player.MyPhysics.HandleRpc(isEnter != 0 ? (byte)19 : (byte)20, reader);
    }

    public static void StartVentAnimation(int ventId)
    {
		ShipStatus.Instance.StartVentAnimation(ventId);
    }

    public static void UncheckedSnapTo(
        byte teleporterId, UnityEngine.Vector2 pos)
    {
        PlayerControl teleportPlayer = Helper.Player.GetPlayerControlById(teleporterId);
		if (teleportPlayer == null) { return; }

		var prevPos = teleportPlayer.GetTruePosition();
		teleportPlayer.NetTransform.SnapTo(pos);

		if (CompatModManager.Instance.TryGetModMap<SubmergedIntegrator>(out var mapMod) &&
			PlayerControl.LocalPlayer.PlayerId == teleporterId)
		{
			int targetFloor = mapMod.GetFloor(pos);
			int prevFloor   = mapMod.GetFloor(prevPos);
			if (targetFloor != prevFloor)
			{
				mapMod.ChangeFloor(teleportPlayer, targetFloor);
			}
		}

	}

    public static void UncheckedShapeShift(
        byte sourceId, byte targetId, byte useAnimation)
    {
        PlayerControl source = Helper.Player.GetPlayerControlById(sourceId);
        PlayerControl target = Helper.Player.GetPlayerControlById(targetId);

        bool animate = true;

        if (useAnimation != byte.MaxValue)
        {
            animate = false;
        }
        source.Shapeshift(target, animate);
    }

    public static void UncheckedMurderPlayer(
        byte sourceId, byte targetId, byte useAnimation)
    {
		if (Helper.GameSystem.IsLobby) { return; }

		PlayerControl source = Helper.Player.GetPlayerControlById(sourceId);
        PlayerControl target = Helper.Player.GetPlayerControlById(targetId);

        if (source != null && target != null)
        {
            if (useAnimation == 0)
            {
                Patches.KillAnimationCoPerformKillPatch.HideNextAnimation = true;
            }
            source.MurderPlayer(target);
        }
    }

	public static void UncheckedExiledPlayer(byte targetId)
	{
		if (Helper.GameSystem.IsLobby ||
			!Roles.ExtremeRoleManager.TryGetRole(targetId, out var role))
		{
			return;
		}

		PlayerControl target = Helper.Player.GetPlayerControlById(targetId);
		if (target == null)
		{
			return;
		}
		target.Exiled();
		role.ExiledAction(target);
	}

	public static void UncheckedRevive(byte targetId)
    {
        PlayerControl target = Helper.Player.GetPlayerControlById(targetId);

        if (target != null)
        {
            target.Revive();

            // なんか起きて失敗
            if (target.Data == null ||
                target.Data.IsDead ||
                target.Data.Disconnected) { return; }

            // 死体は消しておく
            CleanDeadBody(target.PlayerId);
        }
    }


    public static void SetWinGameControlId(int id)
    {
        ExtremeRolesPlugin.ShipState.SetWinControlId(id);
    }

    public static void SetWinPlayer(List<byte> playerId)
    {
        foreach (byte id in playerId)
        {
            NetworkedPlayerInfo player = GameData.Instance.GetPlayerById(id);
            if (player == null) { continue; }
            ExtremeRolesPlugin.ShipState.AddWinner(player);
        }
    }

    public static void SetRoleWin(byte winPlayerId)
    {
		if (Roles.ExtremeRoleManager.TryGetRole(winPlayerId, out var role))
		{
			role.IsWin = true;
		}
    }
    public static void ShareMapId(byte mapId)
    {
        GameOptionsManager.Instance.CurrentGameOptions.SetByte(
            ByteOptionNames.MapId, mapId);
    }

    public static void AddVersionData(MessageReader reader)
    {
        Module.CustomMonoBehaviour.VersionChecker.SerializeLocalVersion(reader);
    }

    public static void PlaySound(
        byte soundType, float volume)
    {
        Helper.Sound.PlaySound(
            (Helper.Sound.Type)soundType, volume);
    }

    public static void ReplaceTask(
        byte callerId, int index, int taskIndex)
    {
        Helper.GameSystem.ReplaceToNewTask(
            callerId, index, taskIndex);
    }

    public static void IntegrateModCall(
        ref MessageReader readeer)
    {
		Compat.CompatModManager.Instance.IntegrateModCall(ref readeer);
    }

    public static void CloseMeetingButton()
    {
        if (MeetingHud.Instance == null) { return; }
        Patches.Meeting.Hud.MeetingHudSelectPatch.SetSelectBlock(true);
        foreach (PlayerVoteArea pva in MeetingHud.Instance.playerStates)
        {
            if (pva == null) { continue; }
            pva.Cancel();
        }
    }

    public static void ReplaceRole(
        byte callerId, byte targetId, byte operation)
    {
        Roles.ExtremeRoleManager.RoleReplace(
            callerId, targetId,
            (Roles.ExtremeRoleManager.ReplaceOperation)operation);
    }

    public static void MeetingReporterRpcOp(ref MessageReader reader)
    {
        Module.MeetingReporter.RpcOp(ref reader);
    }

	public static void UpdateExtremeSystemType(ref MessageReader reader)
	{
		Module.SystemType.ExtremeSystemTypeManager.UpdateSystem(reader);
	}

	public static void HeroHeroAcademiaCommand(
        ref MessageReader reader)
    {
        Roles.Combination.HeroAcademia.RpcCommand(
            ref reader);
    }

    public static void KidsAbilityCommand(
        ref MessageReader reader)
    {
        Roles.Combination.Delinquent.Ability(
            ref reader);
    }

    public static void MoverAbility(ref MessageReader reader)
    {
        Roles.Combination.Mover.Ability(ref reader);
    }

	public static void AcceleratorAbility(ref MessageReader reader)
	{
		Roles.Combination.Accelerator.Ability(ref reader);
	}

	public static void BodyGuardAbility(ref MessageReader reader)
    {
        Roles.Solo.Crewmate.BodyGuard.Ability(ref reader);
    }

    public static void TimeMasterAbility(ref MessageReader reader)
    {
        Roles.Solo.Crewmate.TimeMaster.Ability(ref reader);
    }

    public static void AgencyTakeTask(
        byte targetPlayerId, List<int> getTaskId)
    {
        Roles.Solo.Crewmate.Agency.TakeTargetPlayerTask(
            targetPlayerId, getTaskId);
    }
    public static void FencerAbility(ref MessageReader reader)
    {
        Roles.Solo.Crewmate.Fencer.Ability(ref reader);
    }

    public static void CuresMakerCurseKillCool(
        byte playerId, byte targetPlayerId)
    {
        Roles.Solo.Crewmate.CurseMaker.CurseKillCool(
            playerId, targetPlayerId);
    }

    public static void CarpenterUseAbility(ref MessageReader reader)
    {
        Roles.Solo.Crewmate.Carpenter.UpdateMapObject(ref reader);
    }

    public static void SurvivorDeadWin(byte playerId)
    {
        Roles.Solo.Crewmate.Survivor.DeadWin(playerId);
    }

    public static void CaptainTargetVote(ref MessageReader reader)
    {
        Roles.Solo.Crewmate.Captain.UseAbility(ref reader);
    }

    public static void ResurrecterRpc(ref MessageReader reader)
    {
        Roles.Solo.Crewmate.Resurrecter.RpcAbility(ref reader);
    }

    public static void TeleporterSetPortal(
        byte teleporterId, float x, float y)
    {
        Roles.Solo.Crewmate.Teleporter.SetPortal(
            teleporterId, new UnityEngine.Vector2(x, y));
    }

	public static void BaitAwakeRole(
		byte playerId)
	{
		Roles.Solo.Crewmate.Bait.Awake(playerId);
	}
	public static void SummonerRpcOps(
		byte rolePlayerId, byte targetPlayerId, float x, float y, bool isDead)
	{
		Roles.Solo.Crewmate.Summoner.RpcOps(
			rolePlayerId, targetPlayerId, x, y, isDead);
	}

    public static void CarrierAbility(
        byte callerId, float x, float y,
        byte targetId, bool deadBodyPickUp)
    {
        Roles.Solo.Impostor.Carrier.Ability(
            callerId, x, y, targetId, deadBodyPickUp);
    }

    public static void PainterPaintBody(
        byte targetId, byte isRandomModeMessage)
    {
        Roles.Solo.Impostor.Painter.PaintDeadBody(
            targetId, isRandomModeMessage);
    }
    public static void OverLoaderSwitchAbility(
        byte callerId, byte activate)
    {

        Roles.Solo.Impostor.OverLoader.SwitchAbility(
            callerId, activate == byte.MaxValue);
    }
    public static void CrackerCrackDeadBody(
        byte callerId, byte targetId)
    {
        Roles.Solo.Impostor.Cracker.CrackDeadBody(
            callerId, targetId);
    }
    public static void MaryAbility(ref MessageReader reader)
    {
        Roles.Solo.Impostor.Mery.Ability(ref reader);
    }
    public static void LastWolfSwitchLight(byte swichStatus)
    {
        Roles.Solo.Impostor.LastWolf.SwitchLight(
            swichStatus == byte.MinValue);
    }
    public static void CommanderAttackCommand(byte rolePlayerId)
    {
        Roles.Solo.Impostor.Commander.AttackCommad(
            rolePlayerId);
    }

    public static void HypnotistAbility(ref MessageReader reader)
    {
        Roles.Solo.Impostor.Hypnotist.Ability(ref reader);
    }

    public static void UnderWarperUseVentWithNoAnime(
        byte playerId, int ventId, bool isEnter)
    {
        Roles.Solo.Impostor.UnderWarper.UseVentWithNoAnimation(
            playerId, ventId, isEnter);
    }

    public static void SlimeAbility(ref MessageReader reader)
    {
        Roles.Solo.Impostor.Slime.Ability(ref reader);
    }

    public static void ZombieRpc(ref MessageReader reader)
    {
        Roles.Solo.Impostor.Zombie.RpcAbility(ref reader);
    }

	public static void ThiefAddEffect(byte addEffectTargetDeadBody)
	{
		Roles.Solo.Impostor.Thief.AddEffect(addEffectTargetDeadBody);
	}

	public static void AliceShipBroken(
        byte callerId, byte targetPlayerId, List<int> taskId)
    {
        Roles.Solo.Neutral.Alice.ShipBroken(
            callerId, targetPlayerId, taskId);
    }
    public static void JesterOutburstKill(
        byte killerId, byte targetId)
    {
        Roles.Solo.Neutral.Jester.OutburstKill(
            killerId, targetId);
    }
	public static void MinerHandle(ref MessageReader reader)
	{
		Roles.Solo.Neutral.Miner.RpcHandle(ref reader);
	}
	public static void YandereSetOneSidedLover(
        byte playerId, byte loverId)
    {
        YandereRole.SetOneSidedLover(
            playerId, loverId);
    }
    public static void TotocalcioSetBetPlayer(
        byte playerId, byte betPlayerId)
    {
        Roles.Solo.Neutral.Totocalcio.SetBetTarget(
            playerId, betPlayerId);
    }
    public static void MadmateToFakeImpostor(byte playerId)
    {
        Roles.Solo.Neutral.Madmate.ToFakeImpostor(playerId);
    }
	public static void ArtistDrawOps(in MessageReader reader)
	{
		Roles.Solo.Neutral.Artist.DrawOps(reader);
	}

	public static void SetGhostRole(
        ref MessageReader reader)
    {
        GhostRoles.ExtremeGhostRoleManager.SetGhostRoleToPlayerId(
            ref reader);
    }

    public static void UseGhostRoleAbility(
        byte abilityType, bool isReport, ref MessageReader reader)
    {
        GhostRoles.ExtremeGhostRoleManager.UseAbility(
            abilityType, isReport, ref reader);
    }

    public static void XionAbility(ref MessageReader reader)
    {
        Roles.Solo.Host.Xion.UseAbility(ref reader);
    }

}
