using AmongUs.GameOptions;
using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Module.Ability;
using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.CustomOption.Factory.OptionBuilder;
using ExtremeRoles.Module.SystemType;
using ExtremeRoles.Module.SystemType.Roles;
using ExtremeRoles.Resources;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Roles.API.Interface.Status;
using System.Collections.Generic;
using UnityEngine;


#nullable enable

namespace ExtremeRoles.Roles.Solo.Crewmate.Delusioner;

public sealed class DelusionerRole :
    SingleRoleBase,
    IRoleAutoBuildAbility,
    IRoleAwake<RoleTypes>,
	IRoleHookVoteEnd
{
    public int Order => (int)IRoleVoteModifier.ModOrder.DelusionerCheckVote;

    public bool IsAwake
    {
        get
        {
            return GameSystem.IsLobby || isAwakeRole;
        }
    }

    public RoleTypes NoneAwakeRole => RoleTypes.Crewmate;

    public ExtremeAbilityButton? Button
	{ 
		get => this.ability?.Button;
		set
		{
			if (this.ability is not null)
			{
				this.ability.Button = value;
			}
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
        IsIncludeSpawnPoint,
		EnableCounter
    }

    private bool isAwakeRole;
    private bool isOneTimeAwake;

    private DelusionerStatusModel? status;
	private DelusionerAbilityHandler? ability;
    public override IStatusModel? Status => status;

    private byte targetPlayerId;

    private int awakeVoteCount;
    private int curVoteCount = 0;

    private bool includeLocalPlayer;

    private float defaultCoolTime;

    private int voteCoolTimeReduceRate;


    public DelusionerRole() : base(
		RoleCore.BuildCrewmate(
			ExtremeRoleId.Delusioner,
			ColorPalette.DelusionerPink),
        false, true, false, false)
    {
    }

    public void CreateAbility()
    {
		var loader = Loader;
		this.status = new DelusionerStatusModel(
			loader.GetValue<DelusionerOption, float>(DelusionerOption.Range),
			loader.GetValue<DelusionerOption, bool>(DelusionerOption.IsIncludeSpawnPoint),
			100f - loader.GetValue<DelusionerOption, int>(DelusionerOption.DeflectDamagePenaltyRate) / 100f
		);

		this.defaultCoolTime = loader.GetValue<RoleAbilityCommonOption, float>(
			RoleAbilityCommonOption.AbilityCoolTime);

		var system = loader.GetValue<DelusionerOption, bool>(DelusionerOption.EnableCounter) ?
			ExtremeSystemTypeManager.Instance.CreateOrGet<DelusionerCounterSystem>(
			DelusionerCounterSystem.Type) : null;

		this.ability = new DelusionerAbilityHandler(system, this.status, this);
		this.AbilityClass = this.ability;

		this.status.CurCoolTime = defaultCoolTime;

		this.CreateAbilityCountButton(
            "deflectDamage",
			UnityObjectLoader.LoadSpriteFromResources(
                ObjectPath.DelusionerDeflectDamage));
        Button?.SetLabelToCrewmate();
    }

    public string GetFakeOptionString() => "";

    public bool IsAbilityUse()
    {
        if (!IsAwake || this.status is null)
		{
			return false;
		}

        targetPlayerId = byte.MaxValue;

        PlayerControl target = Player.GetClosestPlayerInRange(
            PlayerControl.LocalPlayer, this,
            this.status.Range);
        if (target == null) { return false; }

        targetPlayerId = target.PlayerId;

        return IRoleAbility.IsCommonUse();
    }

    public void HookVoteEnd(
        MeetingHud instance,
        NetworkedPlayerInfo rolePlayer,
        IReadOnlyDictionary<byte, int> voteIndex)
    {
        if (voteIndex.TryGetValue(
            rolePlayer.PlayerId,
            out int forRolePlayerVote))
        {
            curVoteCount = curVoteCount + forRolePlayerVote;
            isAwakeRole = curVoteCount >= awakeVoteCount;
            if (Button != null &&
                voteCoolTimeReduceRate > 0)
            {
                int curVoteCooltimeReduceRate = voteCoolTimeReduceRate * forRolePlayerVote;

                Button.SetButtonShow(true);
                Button.Behavior.SetCoolTime(
                    defaultCoolTime * ((100.0f - curVoteCooltimeReduceRate) / 100.0f));
            }
        }

        if (isAwakeRole &&
            isOneTimeAwake)
        {
            curVoteCount = 0;
        }
    }

    public void ResetModifier()
    {
        return;
    }

    public void ResetOnMeetingEnd(NetworkedPlayerInfo? exiledPlayer = null)
    {
        return;
    }

    public void ResetOnMeetingStart()
    {
		if (this.status is not null)
		{
			this.status.CurCoolTime = defaultCoolTime;
		}
    }

    public void Update(PlayerControl rolePlayer)
    {
		if (Button == null ||
			this.ability is null ||
			rolePlayer == null ||
			rolePlayer.Data == null ||
			rolePlayer.Data.IsDead ||
			rolePlayer.Data.Disconnected)
		{
			return;
		}

        if (!isAwakeRole)
        {
            Button.SetButtonShow(false);
        }
		else
		{
			this.ability.ReduceCounterNum();
		}
		this.ability.UpdateButtonStatus();
    }

    public bool UseAbility()
    {
		if (this.ability is null)
		{
			return false;
		}

		PlayerControl rolePlayer = PlayerControl.LocalPlayer;

		return this.ability.UseAbilityTo(
			rolePlayer,
			targetPlayerId,
			includeLocalPlayer, new HashSet<byte>());
    }

    public override string GetColoredRoleName(bool isTruthColor = false)
    {
        if (isTruthColor || IsAwake)
        {
            return base.GetColoredRoleName();
        }
        else
        {
            return Design.ColoredString(
                Palette.White, Tr.GetString(RoleTypes.Crewmate.ToString()));
        }
    }
    public override string GetFullDescription()
    {
        if (IsAwake)
        {
            return Tr.GetString(
                $"{this.Core.Id}FullDescription");
        }
        else
        {
            return Tr.GetString(
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
            return Design.ColoredString(
                Palette.White,
                $"{GetColoredRoleName()}: {Tr.GetString("crewImportantText")}");
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
            return Design.ColoredString(
                Palette.CrewmateBlue,
                PlayerControl.LocalPlayer.Data.Role.Blurb);
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

    protected override void CreateSpecificOption(OptionCategoryScope<AutoParentSetBuilder> categoryScope)
	{
		var factory = categoryScope.Builder;
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

		factory.CreateBoolOption(
			DelusionerOption.EnableCounter,
			false);

	}

    protected override void RoleSpecificInit()
    {
        var loader = Loader;

        awakeVoteCount = loader.GetValue<DelusionerOption, int>(
            DelusionerOption.AwakeVoteNum);
        isOneTimeAwake = loader.GetValue<DelusionerOption, bool>(
            DelusionerOption.IsOnetimeAwake);
        voteCoolTimeReduceRate = loader.GetValue<DelusionerOption, int>(
            DelusionerOption.VoteCoolTimeReduceRate);

        includeLocalPlayer = loader.GetValue<DelusionerOption, bool>(
            DelusionerOption.IsIncludeLocalPlayer);

        isOneTimeAwake = isOneTimeAwake && awakeVoteCount > 0;
        isAwakeRole = awakeVoteCount == 0;

        curVoteCount = 0;

        if (isAwakeRole)
        {
            isOneTimeAwake = false;
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
