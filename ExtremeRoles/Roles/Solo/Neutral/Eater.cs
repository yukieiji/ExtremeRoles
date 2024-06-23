using System.Collections;
using System.Collections.Generic;


using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Module.AbilityBehavior;
using ExtremeRoles.Module.AbilityModeSwitcher;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Roles.API.Extension.Neutral;
using ExtremeRoles.Performance;
using ExtremeRoles.Resources;

using ExtremeRoles.Module.CustomOption;

using BepInEx.Unity.IL2CPP.Utils;

using ExtremeRoles.Module.NewOption.Factory;

#nullable enable

namespace ExtremeRoles.Roles.Solo.Neutral;

public sealed class Eater : SingleRoleBase, IRoleAutoBuildAbility, IRoleMurderPlayerHook, IRoleUpdate
{
    public enum EaterOption
    {
        CanUseVent,
        EatRange,
        DeadBodyEatActiveCoolTimePenalty,
        KillEatCoolTimePenalty,
        KillEatActiveCoolTimeReduceRate,
        IsResetCoolTimeWhenMeeting,
        IsShowArrowForDeadBody
    }

    public enum EaterAbilityMode : byte
    {
        Kill,
        DeadBody
    }

    public ExtremeAbilityButton? Button {  get; set; }
    private PlayerControl? tmpTarget;
    private PlayerControl? targetPlayer;
    private NetworkedPlayerInfo? targetDeadBody;

    private float range;
    private float deadBodyEatActiveCoolTimePenalty;
    private float killEatCoolTimePenalty;
    private float killEatActiveCoolTimeReduceRate;

    private float defaultCoolTime;
    private bool isResetCoolTimeWhenMeeting;
    private bool isShowArrow;
    private bool isActivated;


	private Dictionary<byte, Arrow> deadBodyArrow;
    private GraphicAndActiveTimeSwitcher<EaterAbilityMode> modeFactory;

#pragma warning disable CS8618 // null 非許容のフィールドには、コンストラクターの終了時に null 以外の値が入っていなければなりません。Null 許容として宣言することをご検討ください。
	public Eater() : base(
#pragma warning restore CS8618 // null 非許容のフィールドには、コンストラクターの終了時に null 以外の値が入っていなければなりません。Null 許容として宣言することをご検討ください。
	   ExtremeRoleId.Eater,
       ExtremeRoleType.Neutral,
       ExtremeRoleId.Eater.ToString(),
       ColorPalette.EaterMaroon,
       false, false, false, false)
    { }

    public void CreateAbility()
    {
        var allOpt = OptionManager.Instance;

		var deadBodyMode = new GraphicAndActiveTimeMode<EaterAbilityMode>(
			EaterAbilityMode.DeadBody,
			new ButtonGraphic(
				Translation.GetString("deadBodyEat"),
				Loader.CreateSpriteFromResources(
					Path.EaterDeadBodyEat)),
			1.0f);

        this.CreateAbilityCountButton(
            deadBodyMode.Graphic.Text, deadBodyMode.Graphic.Img,
            IsAbilityCheck, CleanUp, ForceCleanUp);

		if (this.Button is null) { return; }

        this.modeFactory = new GraphicAndActiveTimeSwitcher<EaterAbilityMode>(
            this.Button.Behavior,
			deadBodyMode,
			new(
				EaterAbilityMode.Kill,
				new ButtonGraphic(
					Translation.GetString("eatKill"),
					Loader.CreateSpriteFromResources(
						Path.EaterEatKill)),
				this.Button.Behavior.ActiveTime));
    }

    public void HookMuderPlayer(
        PlayerControl source, PlayerControl target)
    {
        if (MeetingHud.Instance ||
            source.PlayerId == CachedPlayerControl.LocalPlayer.PlayerId ||
            !this.isShowArrow) { return; }

        DeadBody[] array = UnityEngine.Object.FindObjectsOfType<DeadBody>();
        for (int i = 0; i < array.Length; ++i)
        {
            if (GameData.Instance.GetPlayerById(array[i].ParentId).PlayerId == target.PlayerId)
            {
                Arrow arr = new Arrow(this.NameColor);
                arr.UpdateTarget(array[i].transform.position);

                this.deadBodyArrow.Add(target.PlayerId, arr);
                break;
            }
        }
    }

    public bool IsAbilityUse()
    {
        if (this.Button == null ||
            this.modeFactory == null) { return false; }

        this.tmpTarget = Player.GetClosestPlayerInRange(
            CachedPlayerControl.LocalPlayer, this, this.range);

        this.targetDeadBody = Player.GetDeadBodyInfo(
            this.range);

        bool hasPlayerTarget = this.tmpTarget != null;
        bool hasDedBodyTarget = this.targetDeadBody != null;

        this.modeFactory.Switch(
            !hasDedBodyTarget && hasPlayerTarget ?
            EaterAbilityMode.Kill : EaterAbilityMode.DeadBody);

        return IRoleAbility.IsCommonUse() &&
            (hasPlayerTarget || hasDedBodyTarget);
    }

    public void ResetOnMeetingEnd(NetworkedPlayerInfo? exiledPlayer = null)
    {
        if (this.Button != null)
        {
            if (this.isResetCoolTimeWhenMeeting)
            {
                this.Button.Behavior.SetCoolTime(this.defaultCoolTime);
                this.Button.OnMeetingEnd();
            }
            if (!this.isActivated)
            {
                var mode = this.modeFactory.Get(EaterAbilityMode.Kill);
                mode.Time *= this.killEatActiveCoolTimeReduceRate;
                this.modeFactory.Add(mode);
            }
        }
        this.isActivated = false;
    }

    public void ResetOnMeetingStart()
    {
        foreach (Arrow arrow in this.deadBodyArrow.Values)
        {
            arrow.Clear();
        }
        this.deadBodyArrow.Clear();
    }

    public bool UseAbility()
    {
		// ターゲットが存在するときのチェック
		if (this.tmpTarget != null)
		{
			byte playerId = this.tmpTarget.PlayerId;

			if (ExtremeRoleManager.GameRole[playerId] is Combination.Assassin assassin &&
				(!assassin.CanKilled || !assassin.CanKilledFromNeutral))
			{
				return false;
			}
		}
        this.targetPlayer = this.tmpTarget;
        return true;
    }

    public void ForceCleanUp()
    {
        this.tmpTarget = null;
    }

    public void Update(PlayerControl rolePlayer)
    {

        if (CachedShipStatus.Instance == null ||
            GameData.Instance == null ||
            this.IsWin) { return; }
        if (!CachedShipStatus.Instance.enabled ||
            ExtremeRolesPlugin.ShipState.AssassinMeetingTrigger) { return; }

        DeadBody[] array = UnityEngine.Object.FindObjectsOfType<DeadBody>();
        HashSet<byte> existDeadBodyPlayerId = new HashSet<byte>();
        for (int i = 0; i < array.Length; ++i)
        {
            byte playerId = GameData.Instance.GetPlayerById(array[i].ParentId).PlayerId;

            if (this.deadBodyArrow.TryGetValue(playerId, out Arrow? arrow) &&
				arrow is not null)
            {
                arrow.Update();
                existDeadBodyPlayerId.Add(playerId);
            }
        }

        HashSet<byte> removePlayerId = new HashSet<byte>();
        foreach (byte playerId in this.deadBodyArrow.Keys)
        {
            if (!existDeadBodyPlayerId.Contains(playerId))
            {
                removePlayerId.Add(playerId);
            }
        }

        foreach (byte playerId in removePlayerId)
        {
            this.deadBodyArrow[playerId].Clear();
            this.deadBodyArrow.Remove(playerId);
        }

        if (this.Button?.Behavior is AbilityCountBehavior behavior &&
            behavior.AbilityCount != 0) { return; }

        ExtremeRolesPlugin.ShipState.RpcRoleIsWin(rolePlayer.PlayerId);
        this.IsWin = true;
    }

    public void CleanUp()
    {
        if (this.targetDeadBody != null)
        {
            Player.RpcCleanDeadBody(this.targetDeadBody.PlayerId);

            if (this.deadBodyArrow.ContainsKey(this.targetDeadBody.PlayerId))
            {
                this.deadBodyArrow[this.targetDeadBody.PlayerId].Clear();
                this.deadBodyArrow.Remove(this.targetDeadBody.PlayerId);
            }

            this.targetDeadBody = null;

            if (this.Button == null) { return; }

            var mode = this.modeFactory.Get(EaterAbilityMode.Kill);
            mode.Time *= this.deadBodyEatActiveCoolTimePenalty;
            this.modeFactory.Add(mode);
        }
        else if (this.targetPlayer != null)
        {
            Player.RpcUncheckMurderPlayer(
                CachedPlayerControl.LocalPlayer.PlayerId,
                this.targetPlayer.PlayerId, 0);

            ExtremeRolesPlugin.ShipState.RpcReplaceDeadReason(
                this.targetPlayer.PlayerId,
                Module.ExtremeShipStatus.ExtremeShipStatus.PlayerStatus.Eatting);

            if (!this.targetPlayer.Data.IsDead) { return; }

            FastDestroyableSingleton<HudManager>.Instance.StartCoroutine(
                this.cleanDeadBodyOps(this.targetPlayer.PlayerId));

            this.isActivated = true;
        }

    }

    public bool IsAbilityCheck()
    {
        if (this.targetDeadBody != null) { return true; }

        return Player.IsPlayerInRangeAndDrawOutLine(
            CachedPlayerControl.LocalPlayer,
            this.targetPlayer, this, this.range);
    }

    public override bool IsSameTeam(SingleRoleBase targetRole) =>
        this.IsNeutralSameTeam(targetRole);

    protected override void CreateSpecificOption(
        AutoParentSetOptionCategoryFactory factory)
    {

        factory.CreateBoolOption(
            EaterOption.CanUseVent,
            true);

		IRoleAbility.CreateAbilityCountOption(
			factory, 5, 7, 7.5f);

		factory.CreateFloatOption(
            EaterOption.EatRange,
            1.0f, 0.0f, 2.0f, 0.1f);
		factory.CreateIntOption(
            EaterOption.DeadBodyEatActiveCoolTimePenalty,
            10, 0, 25, 1,
            format: OptionUnit.Percentage);
		factory.CreateIntOption(
            EaterOption.KillEatCoolTimePenalty,
            10, 0, 25, 1,
            format: OptionUnit.Percentage);
		factory.CreateIntOption(
            EaterOption.KillEatActiveCoolTimeReduceRate,
            10, 0, 50, 1,
            format: OptionUnit.Percentage);
		factory.CreateBoolOption(
            EaterOption.IsResetCoolTimeWhenMeeting,
            false);
		factory.CreateBoolOption(
            EaterOption.IsShowArrowForDeadBody,
            true);
    }

    protected override void RoleSpecificInit()
    {
        this.targetDeadBody = null;
        this.targetPlayer = null;

        var cate = this.Category;

        this.UseVent = cate.GetValue<EaterOption, bool>(
            EaterOption.CanUseVent);
        this.range = cate.GetValue<EaterOption, float>(
            EaterOption.EatRange);
        this.deadBodyEatActiveCoolTimePenalty = (cate.GetValue<EaterOption, int>(
           EaterOption.DeadBodyEatActiveCoolTimePenalty) / 100.0f) + 1.0f;
        this.killEatCoolTimePenalty = (cate.GetValue<EaterOption, int>(
           EaterOption.KillEatCoolTimePenalty) / 100.0f) + 1.0f;
        this.killEatActiveCoolTimeReduceRate = 1.0f - cate.GetValue<EaterOption, int>(
           EaterOption.KillEatCoolTimePenalty) / 100.0f;
        this.isResetCoolTimeWhenMeeting = cate.GetValue<EaterOption, bool>(
           EaterOption.IsResetCoolTimeWhenMeeting);
        this.isShowArrow = cate.GetValue<EaterOption, bool>(
           EaterOption.IsShowArrowForDeadBody);

        this.defaultCoolTime = cate.GetValue<RoleAbilityCommonOption, float>(
            RoleAbilityCommonOption.AbilityCoolTime);

        this.deadBodyArrow = new Dictionary<byte, Arrow>();
        this.isActivated = false;

        if (this.Button?.Behavior is AbilityCountBehavior behaviour)
        {
            int abilityNum = cate.GetValue<RoleAbilityCommonOption, int>(
                RoleAbilityCommonOption.AbilityCount);
            int halfPlayerNum = GameData.Instance.PlayerCount / 2;

            behaviour.SetCountText("eaterWinNum");
            behaviour.SetAbilityCount(
                 abilityNum > halfPlayerNum ? halfPlayerNum : abilityNum);
        }
    }

    private IEnumerator cleanDeadBodyOps(byte targetPlayerId)
    {
        DeadBody? checkDeadBody = null;

        DeadBody[] array = UnityEngine.Object.FindObjectsOfType<DeadBody>();
        for (int i = 0; i < array.Length; ++i)
        {
            if (GameData.Instance.GetPlayerById(
                array[i].ParentId).PlayerId == targetPlayerId)
            {
                checkDeadBody = array[i];
                break;
            }
        }

        if (checkDeadBody == null) { yield break; }

        while(!checkDeadBody.enabled)
        {
            yield return null;
        }

        yield return null;

        Player.RpcCleanDeadBody(targetPlayerId);

        this.targetPlayer = null;

        if (this.Button == null) { yield break; }

        this.Button.Behavior.SetCoolTime(
            this.Button.Behavior.CoolTime * this.killEatCoolTimePenalty);
    }

}
