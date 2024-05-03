using System.Collections.Generic;

using UnityEngine;
using AmongUs.GameOptions;

using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Resources;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Performance;
using ExtremeRoles.Performance.Il2Cpp;
using ExtremeRoles.Module.CustomMonoBehaviour.Minigames;
using ExtremeRoles.Module.SystemType.Roles;
using System.Linq;
using ExtremeRoles.Module.SystemType;
using ExtremeRoles.Module.Ability.AbilityBehavior.Interface;
using ExtremeRoles.Module.Ability;

#nullable enable

namespace ExtremeRoles.Roles.Solo.Crewmate;

public sealed class Delusioner :
    SingleRoleBase,
    IRoleAutoBuildAbility,
    IRoleAwake<RoleTypes>,
    IRoleVoteModifier
{
    public int Order => (int)IRoleVoteModifier.ModOrder.DelusionerCheckVote;

    public bool IsAwake
    {
        get
        {
            return GameSystem.IsLobby || this.isAwakeRole;
        }
    }

    public RoleTypes NoneAwakeRole => RoleTypes.Crewmate;

    public ExtremeAbilityButton? Button { get; set; }

    public enum DelusionerOption
    {
        AwakeVoteNum,
        IsOnetimeAwake,
        Range,
        VoteCoolTimeReduceRate,
        DeflectDamagePenaltyRate,
        IsIncludeLocalPlayer,
        IsIncludeSpawnPoint,
		EnableCounter
    }

    private bool isAwakeRole;
    private bool isOneTimeAwake;

    private float range;

    private byte targetPlayerId;

    private int awakeVoteCount;
    private int curVoteCount = 0;

    private bool includeLocalPlayer;
    private bool includeSpawnPoint;

    private float defaultCoolTime;
    private float curCoolTime;

    private int voteCoolTimeReduceRate;
    private float deflectDamagePenaltyMod;

	private AbilityState prevState;
	private DelusionerCounterSystem? system = null;


    public Delusioner() : base(
        ExtremeRoleId.Delusioner,
        ExtremeRoleType.Crewmate,
        ExtremeRoleId.Delusioner.ToString(),
        ColorPalette.DelusionerPink,
        false, true, false, false)
    { }

    public void CreateAbility()
    {
        this.CreateAbilityCountButton(
            "deflectDamage",
            Loader.CreateSpriteFromResources(
                Path.DelusionerDeflectDamage));
        this.Button?.SetLabelToCrewmate();
    }

    public string GetFakeOptionString() => "";

    public bool IsAbilityUse()
    {
        if (!this.IsAwake) { return false; }

        this.targetPlayerId = byte.MaxValue;

        PlayerControl target = Player.GetClosestPlayerInRange(
            CachedPlayerControl.LocalPlayer, this,
            this.range);
        if (target == null) { return false; }

        this.targetPlayerId = target.PlayerId;

        return IRoleAbility.IsCommonUse();
    }

    public void ModifiedVote(
        byte rolePlayerId,
        ref Dictionary<byte, byte> voteTarget,
        ref Dictionary<byte, int> voteResult)
    {
        return;
    }

    public void ModifiedVoteAnime(
        MeetingHud instance,
        GameData.PlayerInfo rolePlayer,
        ref Dictionary<byte, int> voteIndex)
    {
        if (voteIndex.TryGetValue(
            rolePlayer.PlayerId,
            out int forRolePlayerVote))
        {
            this.curVoteCount = this.curVoteCount + forRolePlayerVote;
            this.isAwakeRole = this.curVoteCount >= this.awakeVoteCount;
            if (this.Button != null &&
                this.voteCoolTimeReduceRate > 0)
            {
                int curVoteCooltimeReduceRate = this.voteCoolTimeReduceRate * forRolePlayerVote;

                this.Button.SetButtonShow(true);
                this.Button.Behavior.SetCoolTime(
                    this.defaultCoolTime * ((100.0f - (float)curVoteCooltimeReduceRate) / 100.0f));
            }
        }

        if (this.isAwakeRole &&
            this.isOneTimeAwake)
        {
            this.curVoteCount = 0;
        }
    }

    public void ResetModifier()
    {
        return;
    }

    public void ResetOnMeetingEnd(GameData.PlayerInfo? exiledPlayer = null)
    {
        return;
    }

    public void ResetOnMeetingStart()
    {
        this.curCoolTime = this.defaultCoolTime;
    }

    public void Update(PlayerControl rolePlayer)
    {
		if (this.Button == null ||
			rolePlayer == null ||
			rolePlayer.Data == null ||
			rolePlayer.Data.IsDead ||
			rolePlayer.Data.Disconnected)
		{
			return;
		}

        if (!this.isAwakeRole)
        {
            this.Button.SetButtonShow(false);
        }
		else if (
			this.system is not null &&
			this.prevState == AbilityState.CoolDown &&
			this.Button.State == AbilityState.Ready &&
			this.Button.Behavior is ICountBehavior countBehavior &&
			countBehavior.AbilityCount > 0)
		{
			this.system.ReadyCounter(countBehavior.AbilityCount);
		}
		this.prevState = this.Button.State;
    }

    public bool UseAbility()
    {
		PlayerControl rolePlayer = CachedPlayerControl.LocalPlayer;

		bool result = useAbilityTo(
			rolePlayer,
			this.targetPlayerId,
			this.includeLocalPlayer, new HashSet<byte>());
		if (result && this.system is not null)
		{
			this.system.Remove();
		}
		return true;
    }

    public override string GetColoredRoleName(bool isTruthColor = false)
    {
        if (isTruthColor || IsAwake)
        {
            return base.GetColoredRoleName();
        }
        else
        {
            return Design.ColoedString(
                Palette.White, Translation.GetString(RoleTypes.Crewmate.ToString()));
        }
    }
    public override string GetFullDescription()
    {
        if (IsAwake)
        {
            return Translation.GetString(
                $"{this.Id}FullDescription");
        }
        else
        {
            return Translation.GetString(
                $"{RoleTypes.Crewmate}FullDescription");
        }
    }

    public override string GetImportantText(bool isContainFakeTask = true)
    {
        if (IsAwake)
        {
            return base.GetImportantText(isContainFakeTask);

        }
        else
        {
            return Design.ColoedString(
                Palette.White,
                $"{this.GetColoredRoleName()}: {Translation.GetString("crewImportantText")}");
        }
    }

    public override string GetIntroDescription()
    {
        if (IsAwake)
        {
            return base.GetIntroDescription();
        }
        else
        {
            return Design.ColoedString(
                Palette.CrewmateBlue,
                CachedPlayerControl.LocalPlayer.Data.Role.Blurb);
        }
    }

    public override Color GetNameColor(bool isTruthColor = false)
    {
        if (isTruthColor || IsAwake)
        {
            return base.GetNameColor(isTruthColor);
        }
        else
        {
            return Palette.White;
        }
    }

	public override bool TryRolePlayerKilledFrom(
		PlayerControl rolePlayer, PlayerControl fromPlayer)
	{
		byte rolePlayerId = rolePlayer.PlayerId;
		if (this.system is null ||
			!this.system.TryGetCounter(rolePlayerId, out int countNum))
		{
			return true;
		}

		List<PlayerControl> allPlayer = Player.GetAllPlayerInRange(
			rolePlayer, this, this.range);

		int num = allPlayer.Count;
		if (allPlayer.Count == 0)
		{
			return true;
		}

		int reduceNum = Mathf.Clamp(allPlayer.Count, 0, countNum);
		var targets = allPlayer.OrderBy(
			x => RandomGenerator.Instance.Next())
			.Take(reduceNum)
			.Select(x => x.PlayerId)
			.ToHashSet();
		foreach (byte target in targets)
		{
			this.useAbilityTo(rolePlayer, target, false, targets);
		}

		this.system.ReduceCounter(rolePlayerId, reduceNum);

		var newTaget = Player.GetClosestPlayerInKillRange(rolePlayer);

		return newTaget != null && newTaget.PlayerId == rolePlayerId;
	}

	protected override void CreateSpecificOption(
        IOptionInfo parentOps)
    {
        CreateIntOption(
            DelusionerOption.AwakeVoteNum,
            3, 0, 8, 1, parentOps,
            format: OptionUnit.VoteNum);
        CreateBoolOption(
            DelusionerOption.IsOnetimeAwake,
            false, parentOps);

        CreateFloatOption(
            DelusionerOption.Range,
            2.5f, 0.0f, 7.5f, 0.1f,
            parentOps);

        this.CreateAbilityCountOption(
            parentOps, 3, 25);

        CreateIntOption(
            DelusionerOption.VoteCoolTimeReduceRate,
            5, 0, 100, 5, parentOps,
            format: OptionUnit.Percentage);
        CreateIntOption(
            DelusionerOption.DeflectDamagePenaltyRate,
            10, 0, 100, 5, parentOps,
            format: OptionUnit.Percentage);

        CreateBoolOption(
            DelusionerOption.IsIncludeLocalPlayer,
            true, parentOps);
        CreateBoolOption(
            DelusionerOption.IsIncludeSpawnPoint,
            false, parentOps);
		CreateBoolOption(
			DelusionerOption.EnableCounter,
			false, parentOps);

	}

    protected override void RoleSpecificInit()
    {
        var allOpt = OptionManager.Instance;
        this.awakeVoteCount = allOpt.GetValue<int>(
            GetRoleOptionId(DelusionerOption.AwakeVoteNum));
        this.isOneTimeAwake = allOpt.GetValue<bool>(
            GetRoleOptionId(DelusionerOption.IsOnetimeAwake));
        this.voteCoolTimeReduceRate = allOpt.GetValue<int>(
            GetRoleOptionId(DelusionerOption.VoteCoolTimeReduceRate));
        this.deflectDamagePenaltyMod = 100f - (allOpt.GetValue<int>(
            GetRoleOptionId(DelusionerOption.DeflectDamagePenaltyRate)) / 100f);
        this.range = allOpt.GetValue<float>(
            GetRoleOptionId(DelusionerOption.Range));

        this.includeLocalPlayer = allOpt.GetValue<bool>(
            GetRoleOptionId(DelusionerOption.IsIncludeLocalPlayer));
        this.includeSpawnPoint = allOpt.GetValue<bool>(
            GetRoleOptionId(DelusionerOption.IsIncludeSpawnPoint));

        this.isOneTimeAwake = this.isOneTimeAwake && this.awakeVoteCount > 0;
        this.defaultCoolTime = allOpt.GetValue<float>(
            GetRoleOptionId(RoleAbilityCommonOption.AbilityCoolTime));

		if (allOpt.GetValue<bool>(
				GetRoleOptionId(DelusionerOption.EnableCounter)))
		{
			this.system = ExtremeSystemTypeManager.Instance.CreateOrGet<DelusionerCounterSystem>(
				DelusionerCounterSystem.Type);
		}
		this.prevState = AbilityState.CoolDown;

        this.curCoolTime = this.defaultCoolTime;
        this.isAwakeRole = this.awakeVoteCount == 0;

        this.curVoteCount = 0;

        if (this.isAwakeRole)
        {
            this.isOneTimeAwake = false;
        }
    }

	private bool useAbilityTo(
		in PlayerControl rolePlayer,
		in byte teloportTarget,
		in bool includeRolePlayer,
		in IReadOnlySet<byte> ignores)
	{
		List<Vector2> randomPos = new List<Vector2>(
			CachedPlayerControl.AllPlayerControls.Count);
		var allPlayer = GameData.Instance.AllPlayers;

		if (includeRolePlayer)
		{
			randomPos.Add(rolePlayer.transform.position);
		}

		if (this.includeSpawnPoint)
		{
			Map.AddSpawnPoint(randomPos, teloportTarget);
		}

		foreach (GameData.PlayerInfo player in allPlayer.GetFastEnumerator())
		{
			if (player == null ||
				player.Disconnected ||
				player.PlayerId == rolePlayer.PlayerId ||
				player.PlayerId == teloportTarget ||
				player.IsDead ||
				player.Object == null ||
				player.Object.onLadder || // はしご中？
				player.Object.inVent || // ベント入ってる？
				player.Object.inMovingPlat || // なんか乗ってる状態
				ignores.Contains(player.PlayerId))
			{
				continue;
			}

			Vector3 targetPos = player.Object.transform.position;

			if (ExtremeSpawnSelectorMinigame.IsCloseWaitPos(targetPos))
			{
				continue;
			}

			randomPos.Add(targetPos);
		}

		if (randomPos.Count == 0)
		{
			return false;
		}

		Player.RpcUncheckSnap(teloportTarget, randomPos[
			RandomGenerator.Instance.Next(randomPos.Count)]);

		if (this.Button != null &&
			this.deflectDamagePenaltyMod < 1.0f)
		{
			this.curCoolTime = this.curCoolTime * this.deflectDamagePenaltyMod;
			this.Button.Behavior.SetCoolTime(this.curCoolTime);
		}
		return true;
	}
}
#if DEBUG
[HarmonyLib.HarmonyPatch(typeof(SpawnInMinigame), nameof(SpawnInMinigame.Begin))]
public static class AirShipSpawnCheck
{
    public static void Postfix(SpawnInMinigame __instance)
    {
        if (AmongUsClient.Instance.NetworkMode == NetworkModes.FreePlay)
        {
            foreach (SpawnInMinigame.SpawnLocation pos in __instance.Locations)
            {
                Logging.Debug($"Name:{pos.Name}  Pos:{pos.Location}");
            }
        }
    }
}
#endif
