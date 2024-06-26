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




using ExtremeRoles.Module.CustomOption.Factory;

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

    public ExtremeAbilityButton Button
    {
        get => this.deflectDamageButton;
        set
        {
            this.deflectDamageButton = value;
        }
    }

    public enum DelusionerOption
    {
        AwakeVoteNum,
        IsOnetimeAwake,
        Range,
        VoteCoolTimeReduceRate,
        DeflectDamagePenaltyRate,
        IsIncludeLocalPlayer,
        IsIncludeSpawnPoint
    }

    private ExtremeAbilityButton deflectDamageButton;

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
			Resources.Loader.CreateSpriteFromResources(
				Path.DelusionerDeflectDamage));
        this.Button.SetLabelToCrewmate();
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
        NetworkedPlayerInfo rolePlayer,
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

    public void ResetOnMeetingEnd(NetworkedPlayerInfo exiledPlayer = null)
    {
        return;
    }

    public void ResetOnMeetingStart()
    {
        this.curCoolTime = this.defaultCoolTime;
    }

    public void Update(PlayerControl rolePlayer)
    {
        if (!this.isAwakeRole)
        {
            this.Button?.SetButtonShow(false);
        }
    }

    public bool UseAbility()
    {
        List<Vector2> randomPos = new List<Vector2>();
        byte teloportTarget = this.targetPlayerId;

        PlayerControl localPlayer = CachedPlayerControl.LocalPlayer;
        var allPlayer = GameData.Instance.AllPlayers;

        if (this.includeLocalPlayer)
        {
            randomPos.Add(localPlayer.transform.position);
        }

        if (this.includeSpawnPoint)
        {
			Map.AddSpawnPoint(randomPos, teloportTarget);
        }

		foreach (NetworkedPlayerInfo player in allPlayer.GetFastEnumerator())
        {
            if (player == null) { continue; }
            if (!player.Disconnected &&
                player.PlayerId != localPlayer.PlayerId &&
                player.PlayerId != teloportTarget &&
                !player.IsDead &&
                player.Object != null &&
				player.Object.moveable && // 動ける？
				!player.Object.inVent && // ベント入ってない
				!player.Object.inMovingPlat) // なんか乗ってる状態
            {
                Vector3 targetPos = player.Object.transform.position;

                if (ExtremeSpawnSelectorMinigame.IsCloseWaitPos(targetPos))
                {
                    continue;
                }

                randomPos.Add(player.Object.transform.position);
            }
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

    protected override void CreateSpecificOption(
        AutoParentSetOptionCategoryFactory factory)
    {
        factory.CreateIntOption(
            DelusionerOption.AwakeVoteNum,
            3, 0, 8, 1,
            format: OptionUnit.VoteNum);
        factory.CreateBoolOption(
            DelusionerOption.IsOnetimeAwake,
            false);

        factory.CreateFloatOption(
            DelusionerOption.Range,
            2.5f, 0.0f, 7.5f, 0.1f);

        IRoleAbility.CreateAbilityCountOption(
            factory, 3, 25);

        factory.CreateIntOption(
            DelusionerOption.VoteCoolTimeReduceRate,
            5, 0, 100, 5,
            format: OptionUnit.Percentage);
        factory.CreateIntOption(
            DelusionerOption.DeflectDamagePenaltyRate,
            10, 0, 100, 5,
            format: OptionUnit.Percentage);

        factory.CreateBoolOption(
            DelusionerOption.IsIncludeLocalPlayer,
            true);
        factory.CreateBoolOption(
            DelusionerOption.IsIncludeSpawnPoint,
            false);

    }

    protected override void RoleSpecificInit()
    {
        var loader = this.Loader;
        this.awakeVoteCount = loader.GetValue<DelusionerOption, int>(
            DelusionerOption.AwakeVoteNum);
        this.isOneTimeAwake = loader.GetValue<DelusionerOption, bool>(
            DelusionerOption.IsOnetimeAwake);
        this.voteCoolTimeReduceRate = loader.GetValue<DelusionerOption, int>(
            DelusionerOption.VoteCoolTimeReduceRate);
        this.deflectDamagePenaltyMod = 100f - (loader.GetValue<DelusionerOption, int>(
            DelusionerOption.DeflectDamagePenaltyRate) / 100f);
        this.range = loader.GetValue<DelusionerOption, float>(
            DelusionerOption.Range);

        this.includeLocalPlayer = loader.GetValue<DelusionerOption, bool>(
            DelusionerOption.IsIncludeLocalPlayer);
        this.includeSpawnPoint = loader.GetValue<DelusionerOption, bool>(
            DelusionerOption.IsIncludeSpawnPoint);

        this.isOneTimeAwake = this.isOneTimeAwake && this.awakeVoteCount > 0;
        this.defaultCoolTime = loader.GetValue<RoleAbilityCommonOption, float>(
            RoleAbilityCommonOption.AbilityCoolTime);
        this.curCoolTime = this.defaultCoolTime;
        this.isAwakeRole = this.awakeVoteCount == 0;

        this.curVoteCount = 0;

        if (this.isAwakeRole)
        {
            this.isOneTimeAwake = false;
        }
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
