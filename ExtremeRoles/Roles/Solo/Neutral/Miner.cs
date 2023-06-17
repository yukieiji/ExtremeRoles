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

namespace ExtremeRoles.Roles.Solo.Neutral;

public sealed class Miner :
	SingleRoleBase,
	IRoleAbility,
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
	public record MineEffectParameter(
		ShowMode RolePlayerShowMode,
		ShowMode AnotherPlayerShowMode,
		bool CanShowNoneActiveAtherPlayer);

    public ExtremeAbilityButton Button { get; set; }

	private bool isLinkingVent = false;
	private int mineId = 0;
    private Dictionary<int, MinerMineEffect> mines;
    private float killRange;
    private float nonActiveTime;
    private float timer;
    private bool isShowKillLog;
	private bool isShowAnotherPlayer;
	private MinerMineEffect noneActiveMine = null;
	private MineEffectParameter parameter = null;
    private TextPopUpper killLogger = null;

    public Miner() : base(
        ExtremeRoleId.Miner,
        ExtremeRoleType.Neutral,
        ExtremeRoleId.Miner.ToString(),
        ColorPalette.MinerIvyGreen,
        false, false, true, false)
    { }

	public static void RpcHandle(ref MessageReader reader)
	{
		MinerRpc rpc = (MinerRpc)reader.ReadByte();
		byte playerId = reader.ReadByte();
		Miner miner = ExtremeRoleManager.GetSafeCastedRole<Miner>(playerId);
		int id = reader.ReadInt32();
		switch (rpc)
		{
			case MinerRpc.SetMine:
				float x = reader.ReadSingle();
				float y = reader.ReadSingle();
				if (miner == null) { return; }
				setMine(miner, new(x, y), id);
				break;
			case MinerRpc.ActiveMine:
				if (miner == null) { return; }
				activateMine(miner, id);
				break;
			case MinerRpc.RemoveMine:
				if (miner == null) { return; }
				removeMine(miner, id);
				break;
		}
	}

	private static void setMine(Miner miner, Vector2 pos, int id, bool isRolePlayer=false)
	{
		GameObject obj = new GameObject($"Miner:{miner.GameControlId}_Mine:{id}");
		obj.transform.position = pos;
		var mine = obj.AddComponent<MinerMineEffect>();
		miner.noneActiveMine = mine;
		miner.noneActiveMine.SetParameter(isRolePlayer, miner.killRange, miner.parameter);
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
			GameSystem.RelinkVent();
		}
	}

	public void CreateAbility()
    {
        this.CreateNormalAbilityButton(
            "setMine",
            Loader.CreateSpriteFromResources(
                Path.MinerSetMine),
            abilityOff: CleanUp,
            forceAbilityOff: () => { });
    }

    public bool UseAbility()
    {
		var pos = CachedPlayerControl.LocalPlayer.PlayerControl.GetTruePosition();

		if (this.isShowAnotherPlayer)
		{
			using (var caller = RPCOperator.CreateCaller(RPCOperator.Command.MinerHandle))
			{
				caller.WriteByte((byte)MinerRpc.SetMine);
				caller.WriteByte(CachedPlayerControl.LocalPlayer.PlayerId);
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
				caller.WriteByte(CachedPlayerControl.LocalPlayer.PlayerId);
				caller.WriteInt(this.mineId);
			}
		}
		activateMine(this, this.mineId);
		++this.mineId;
	}

    public bool IsAbilityUse() => this.IsCommonUse();

    public void AllReset(PlayerControl rolePlayer)
    {
        this.mines.Clear();
    }

    public void ResetOnMeetingStart()
    {
        if (this.killLogger != null)
        {
            this.killLogger.Clear();
        }
    }

    public void ResetOnMeetingEnd(GameData.PlayerInfo exiledPlayer = null)
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

            foreach (GameData.PlayerInfo playerInfo in
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
					caller.WriteByte(CachedPlayerControl.LocalPlayer.PlayerId);
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
                GameData.PlayerInfo killPlayer = GameData.Instance.GetPlayerById(player);

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
        this.mines.Clear();
    }
    public override void RolePlayerKilledAction(
        PlayerControl rolePlayer, PlayerControl killerPlayer)
    {
        this.mines.Clear();
    }

    public override bool IsSameTeam(SingleRoleBase targetRole) =>
        this.IsNeutralSameTeam(targetRole);

    protected override void CreateSpecificOption(
        IOptionInfo parentOps)
    {
		CreateBoolOption(
			MinerOption.LinkingAllVent,
			false, parentOps);
		this.CreateCommonAbilityOption(
            parentOps, 2.0f);
        CreateFloatOption(
            MinerOption.MineKillRange,
            1.8f, 0.5f, 5f, 0.1f, parentOps);
		var showOpt = CreateBoolOption(
			MinerOption.CanShowMine,
			false, parentOps);
		CreateSelectionOption(
			MinerOption.RolePlayerShowMode,
			new string[]
			{
				ShowMode.MineSeeOnlySe.ToString(),
				ShowMode.MineSeeOnlyImg.ToString(),
				ShowMode.MineSeeBoth.ToString(),
			}, showOpt);
		var anotherPlayerShowMode = CreateSelectionOption(
			MinerOption.AnotherPlayerShowMode,
			new string[]
			{
				ShowMode.MineSeeNone.ToString(),
				ShowMode.MineSeeOnlySe.ToString(),
				ShowMode.MineSeeOnlyImg.ToString(),
				ShowMode.MineSeeBoth.ToString(),
			}, showOpt);
		CreateBoolOption(
			MinerOption.CanShowNoneActiveAnotherPlayer,
			false, anotherPlayerShowMode);
		CreateFloatOption(
            MinerOption.NoneActiveTime,
            20.0f, 1.0f, 45f, 0.5f,
            parentOps, format: OptionUnit.Second);
        CreateBoolOption(
            MinerOption.ShowKillLog,
            true, parentOps);
	}

    protected override void RoleSpecificInit()
    {
        var allOpt = OptionManager.Instance;

		this.isLinkingVent = allOpt.GetValue<bool>(
			GetRoleOptionId(MinerOption.LinkingAllVent));

        this.killRange = allOpt.GetValue<float>(
            GetRoleOptionId(MinerOption.MineKillRange));
        this.nonActiveTime = allOpt.GetValue<float>(
            GetRoleOptionId(MinerOption.NoneActiveTime));
        this.isShowKillLog = allOpt.GetValue<bool>(
            GetRoleOptionId(MinerOption.ShowKillLog));

        this.mines = new Dictionary<int, MinerMineEffect>();
        this.timer = this.nonActiveTime;
		this.mineId = 0;

		bool isShowMine = allOpt.GetValue<bool>(
			GetRoleOptionId(MinerOption.CanShowMine));

		var rolePlayerShowMode = (ShowMode)(allOpt.GetValue<int>(
			GetRoleOptionId(MinerOption.RolePlayerShowMode)) + 1);
		var anotherPlayerShowMode = (ShowMode)allOpt.GetValue<int>(
			GetRoleOptionId(MinerOption.AnotherPlayerShowMode));
		this.isShowAnotherPlayer = anotherPlayerShowMode != ShowMode.MineSeeNone && isShowMine;
		this.parameter = new MineEffectParameter(
			RolePlayerShowMode: isShowMine ? rolePlayerShowMode : ShowMode.MineSeeNone,
			AnotherPlayerShowMode: anotherPlayerShowMode,
			CanShowNoneActiveAtherPlayer:
				allOpt.GetValue<bool>(
					GetRoleOptionId(MinerOption.CanShowNoneActiveAnotherPlayer)) &&
				this.isShowAnotherPlayer);

		this.killLogger = new TextPopUpper(
            2, 3.5f, new Vector3(0, -1.2f, 0.0f),
            TMPro.TextAlignmentOptions.Center, false);
    }
}
