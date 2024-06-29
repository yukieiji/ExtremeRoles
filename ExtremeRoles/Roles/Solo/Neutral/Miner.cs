using System.Collections.Generic;

using Hazel;
using UnityEngine;

using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Module.ExtremeShipStatus;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Roles.API.Extension.Neutral;
using ExtremeRoles.Performance;
using ExtremeRoles.Performance.Il2Cpp;
using ExtremeRoles.Resources;
using ExtremeRoles.Module.CustomMonoBehaviour;
using ExtremeRoles.Compat;
using ExtremeRoles.Module.Ability;

using ExtremeRoles.Module.CustomOption.Factory;



#nullable enable

namespace ExtremeRoles.Roles.Solo.Neutral;

public sealed class Miner :
	SingleRoleBase,
	IRoleAutoBuildAbility,
	IRoleUpdate,
	IRoleSpecialSetUp,
	IRoleSpecialReset
{
    public enum MinerOption
    {
		LinkingAllVent,
		MineKillRange,
		CanShowMine,
		RolePlayerShowMode,
		AnotherPlayerShowMode,
		CanShowNoneActiveAnotherPlayer,
        NoneActiveTime,
        ShowKillLog
    }
	public enum MinerRpc : byte
	{
		SetMine,
		ActiveMine,
		RemoveMine,
	}
	public enum ShowMode : byte
	{
		MineSeeNone,
		MineSeeOnlySe,
		MineSeeOnlyImg,
		MineSeeBoth
	}
	public readonly record struct MineEffectParameter(
		ShowMode RolePlayerShowMode,
		ShowMode AnotherPlayerShowMode,
		bool CanShowNoneActiveAtherPlayer);

	private bool isLinkingVent = false;
	private int mineId = 0;

    private float killRange;
    private float nonActiveTime;
    private float timer;
    private bool isShowKillLog;
	private bool isShowAnotherPlayer;
	private MinerMineEffect? noneActiveMine = null;

#pragma warning disable CS8618
	public ExtremeAbilityButton Button { get; set; }
	private Dictionary<int, MinerMineEffect> mines;
	private MineEffectParameter parameter;
    private TextPopUpper killLogger;

    public Miner() : base(
        ExtremeRoleId.Miner,
        ExtremeRoleType.Neutral,
        ExtremeRoleId.Miner.ToString(),
        ColorPalette.MinerIvyGreen,
        false, false, true, false)
    { }
#pragma warning restore CS8618
	public static void RpcHandle(ref MessageReader reader)
	{
		MinerRpc rpc = (MinerRpc)reader.ReadByte();
		byte playerId = reader.ReadByte();
		Miner? miner = ExtremeRoleManager.GetSafeCastedRole<Miner>(playerId);
		int id = reader.ReadInt32();
		switch (rpc)
		{
			case MinerRpc.SetMine:
				float x = reader.ReadSingle();
				float y = reader.ReadSingle();
				if (miner is null) { return; }
				setMine(miner, new(x, y), id);
				break;
			case MinerRpc.ActiveMine:
				if (miner is null) { return; }
				activateMine(miner, id);
				break;
			case MinerRpc.RemoveMine:
				if (miner is null) { return; }
				removeMine(miner, id);
				break;
		}
	}

	private static void setMine(Miner miner, Vector2 pos, int id, bool isRolePlayer=false)
	{
		GameObject obj = new GameObject($"Miner:{miner.GameControlId}_Mine:{id}");
		if (CompatModManager.Instance.TryGetModMap(out var modMap))
		{
			modMap.AddCustomComponent(
				obj, Compat.Interface.CustomMonoBehaviourType.MovableFloorBehaviour);
		}
		obj.transform.position = pos;
		var mine = obj.AddComponent<MinerMineEffect>();
		miner.noneActiveMine = mine;
		miner.noneActiveMine.SetParameter(isRolePlayer, miner.killRange, in miner.parameter);
		ExtremeRolesPlugin.ShipState.AddMeetingResetObject(miner.noneActiveMine);
	}

	private static void activateMine(Miner miner, int id)
	{
		if (miner.noneActiveMine == null) { return; }
		miner.noneActiveMine.SwithAcitve();
		miner.mines.Add(id, miner.noneActiveMine);
	}

	private static void removeMine(Miner miner, int id)
	{
		if (!miner.mines.TryGetValue(id, out var mine))
		{
			return;
		}
		if (mine != null)
		{
			mine.Clear();
		}
		miner.mines.Remove(id);
	}

	public void IntroBeginSetUp()
	{
		return;
	}

	public void IntroEndSetUp()
	{
		if (this.isLinkingVent)
		{
			Map.RelinkVent();
		}
	}

	public void CreateAbility()
    {
        this.CreateNormalActivatingAbilityButton(
            "setMine",
			Resources.Loader.CreateSpriteFromResources(
				Path.MinerSetMine),
            abilityOff: CleanUp,
            forceAbilityOff: () => { });
    }

    public bool UseAbility()
    {
		var pos = PlayerControl.LocalPlayer.GetTruePosition();

		if (this.isShowAnotherPlayer)
		{
			using (var caller = RPCOperator.CreateCaller(RPCOperator.Command.MinerHandle))
			{
				caller.WriteByte((byte)MinerRpc.SetMine);
				caller.WriteByte(PlayerControl.LocalPlayer.PlayerId);
				caller.WriteInt(this.mineId);
				caller.WriteFloat(pos.x);
				caller.WriteFloat(pos.y);
			}
		}

		setMine(this, pos, this.mineId, true);
		return true;
    }

    public void CleanUp()
    {
		if (this.isShowAnotherPlayer)
		{
			using (var caller = RPCOperator.CreateCaller(RPCOperator.Command.MinerHandle))
			{
				caller.WriteByte((byte)MinerRpc.ActiveMine);
				caller.WriteByte(PlayerControl.LocalPlayer.PlayerId);
				caller.WriteInt(this.mineId);
			}
		}
		activateMine(this, this.mineId);
		++this.mineId;
	}

    public bool IsAbilityUse() => IRoleAbility.IsCommonUse();

    public void AllReset(PlayerControl rolePlayer)
    {
        this.resetAllMine();
    }

    public void ResetOnMeetingStart()
    {
        if (this.killLogger != null)
        {
            this.killLogger.Clear();
        }
    }

    public void ResetOnMeetingEnd(NetworkedPlayerInfo? exiledPlayer = null)
    {
        return;
    }

    public void Update(PlayerControl rolePlayer)
    {
        if (rolePlayer.Data.IsDead ||
			rolePlayer.Data.Disconnected ||
			CachedShipStatus.Instance == null ||
			GameData.Instance == null ||
			!CachedShipStatus.Instance.enabled ||
			ExtremeRolesPlugin.ShipState.AssassinMeetingTrigger) { return; }

        if (MeetingHud.Instance || ExileController.Instance)
        {
            this.timer = this.nonActiveTime;
            return;
        }

        if (this.timer > 0.0f)
        {
            this.timer -= Time.deltaTime;
            return;
        }

        if (this.mines.Count == 0) { return; }

        HashSet<int> activateMine = new HashSet<int>();
        HashSet<byte> killedPlayer = new HashSet<byte>();

        foreach (var (id, mine) in this.mines)
        {
			if (mine == null)
			{
				activateMine.Add(id);
				continue;
			}
            Vector2 pos = mine.transform.position;

            foreach (NetworkedPlayerInfo playerInfo in
                GameData.Instance.AllPlayers.GetFastEnumerator())
            {
                if (playerInfo == null ||
					killedPlayer.Contains(playerInfo.PlayerId)) { continue; }

                var assassin = ExtremeRoleManager.GameRole[
                    playerInfo.PlayerId] as Combination.Assassin;

                if (assassin != null &&
					(!assassin.CanKilled || !assassin.CanKilledFromNeutral))
                {
					continue;
				}

                if (!playerInfo.Disconnected &&
                    !playerInfo.IsDead &&
                    playerInfo.Object != null &&
                    !playerInfo.Object.inVent)
                {
					Vector2 vector = playerInfo.Object.GetTruePosition() - pos;
					float magnitude = vector.magnitude;
					if (magnitude <= this.killRange &&
						!PhysicsHelpers.AnyNonTriggersBetween(
							pos, vector.normalized,
							magnitude, Constants.ShipAndObjectsMask))
					{
						activateMine.Add(id);
						killedPlayer.Add(playerInfo.PlayerId);
						break;
					}
				}
            }
        }

        foreach (int id in activateMine)
        {
			if (this.isShowAnotherPlayer)
			{
				using (var caller = RPCOperator.CreateCaller(RPCOperator.Command.MinerHandle))
				{
					caller.WriteByte((byte)MinerRpc.RemoveMine);
					caller.WriteByte(PlayerControl.LocalPlayer.PlayerId);
					caller.WriteInt(id);
				}
			}
			removeMine(this, id);
        }

        foreach (byte player in killedPlayer)
        {
            Helper.Player.RpcUncheckMurderPlayer(
                rolePlayer.PlayerId,
                player, 0);
            ExtremeRolesPlugin.ShipState.RpcReplaceDeadReason(
                player, ExtremeShipStatus.PlayerStatus.Explosion);

            if (this.isShowKillLog)
            {
                NetworkedPlayerInfo killPlayer = GameData.Instance.GetPlayerById(player);

                if (killPlayer != null)
                {
                    // 以下のテキスト表示処理
                    // [AUER32-ACM] {プレイヤー名} 100↑
                    // AmongUs ExtremeRoles v3.2.0.0 - AntiCrewmateMine
                    this.killLogger.AddText(
                        $"[AUER32-ACM] {Helper.Design.ColoedString(new Color32(255, 153, 51, byte.MaxValue), killPlayer.DefaultOutfit.PlayerName)} 100↑");
                }
            }
        }

    }

    public override void ExiledAction(PlayerControl rolePlayer)
    {
		this.resetAllMine();
	}
    public override void RolePlayerKilledAction(
        PlayerControl rolePlayer, PlayerControl killerPlayer)
    {
        this.resetAllMine();
    }

    public override bool IsSameTeam(SingleRoleBase targetRole) =>
        this.IsNeutralSameTeam(targetRole);

    protected override void CreateSpecificOption(
        AutoParentSetOptionCategoryFactory factory)
    {
		factory.CreateBoolOption(
			MinerOption.LinkingAllVent,
			false);
		IRoleAbility.CreateCommonAbilityOption(
            factory, 2.0f);
        factory.CreateFloatOption(
            MinerOption.MineKillRange,
            1.8f, 0.5f, 5f, 0.1f);
		var showOpt = factory.CreateBoolOption(
			MinerOption.CanShowMine,
			false);
		factory.CreateSelectionOption(
			MinerOption.RolePlayerShowMode,
			[
				ShowMode.MineSeeOnlySe.ToString(),
				ShowMode.MineSeeOnlyImg.ToString(),
				ShowMode.MineSeeBoth.ToString(),
			], showOpt);
		var anotherPlayerShowMode = factory.CreateSelectionOption<MinerOption, ShowMode>(
			MinerOption.AnotherPlayerShowMode, showOpt);
		factory.CreateBoolOption(
			MinerOption.CanShowNoneActiveAnotherPlayer,
			false, anotherPlayerShowMode);
		factory.CreateFloatOption(
            MinerOption.NoneActiveTime,
            20.0f, 1.0f, 45f, 0.5f,
			format: OptionUnit.Second);
        factory.CreateBoolOption(
            MinerOption.ShowKillLog,
            true);
	}

    protected override void RoleSpecificInit()
    {
        var cate = this.Loader;

		this.isLinkingVent = cate.GetValue<MinerOption, bool>(
			MinerOption.LinkingAllVent);

        this.killRange = cate.GetValue<MinerOption, float>(
            MinerOption.MineKillRange);
        this.nonActiveTime = cate.GetValue<MinerOption, float>(
            MinerOption.NoneActiveTime);
        this.isShowKillLog = cate.GetValue<MinerOption, bool>(
            MinerOption.ShowKillLog);

        this.mines = new Dictionary<int, MinerMineEffect>();
        this.timer = this.nonActiveTime;
		this.mineId = 0;

		bool isShowMine = cate.GetValue<MinerOption, bool>(
			MinerOption.CanShowMine);

		var rolePlayerShowMode = (ShowMode)(cate.GetValue<MinerOption, int>(
			MinerOption.RolePlayerShowMode) + 1);
		var anotherPlayerShowMode = (ShowMode)cate.GetValue<MinerOption, int>(
			MinerOption.AnotherPlayerShowMode);
		this.isShowAnotherPlayer = anotherPlayerShowMode != ShowMode.MineSeeNone && isShowMine;
		this.parameter = new MineEffectParameter(
			RolePlayerShowMode: isShowMine ? rolePlayerShowMode : ShowMode.MineSeeNone,
			AnotherPlayerShowMode: anotherPlayerShowMode,
			CanShowNoneActiveAtherPlayer:
				cate.GetValue<MinerOption, bool>(
					MinerOption.CanShowNoneActiveAnotherPlayer) &&
				this.isShowAnotherPlayer);

		this.killLogger = new TextPopUpper(
            2, 3.5f, new Vector3(0, -1.2f, 0.0f),
            TMPro.TextAlignmentOptions.Center, false);
    }
	private void resetAllMine()
	{
		foreach (int id in this.mines.Keys)
		{
			removeMine(this, id);
		}
	}
}
