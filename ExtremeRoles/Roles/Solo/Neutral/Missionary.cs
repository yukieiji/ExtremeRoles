using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Module.ExtremeShipStatus;
using ExtremeRoles.Resources;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Roles.API.Extension.Neutral;
using ExtremeRoles.Performance.Il2Cpp;
using ExtremeRoles.Performance;

using BepInEx.Unity.IL2CPP.Utils;
using System.Linq;
using ExtremeRoles.Module.Ability;

using ExtremeRoles.Module.CustomOption.Factory;



#nullable enable

namespace ExtremeRoles.Roles.Solo.Neutral;

public sealed class Missionary :
	SingleRoleBase,
	IRoleAutoBuildAbility,
	IRoleUpdate,
	IRoleVoteCheck
{
    public enum MissionaryOption
    {
        TellDeparture,
        DepartureMinTime,
        DepartureMaxTime,
        PropagateRange,
		IsUseSolemnJudgment,
		MaxJudgementNum,
	}

	public PlayerControl? targetPlayer;

	private float timer;

	private float propagateRange;
	private float minTimerTime;
	private float maxTimerTime;
	private bool tellDeparture;
	private bool isUseSolemnJudgment;
	private int maxJudgementTarget;

#pragma warning disable CS8618
	public ExtremeAbilityButton Button { get; set; }
	private List<PlayerControl> lamb;
	private HashSet<byte> judgementTarget;
	private TMPro.TextMeshPro tellText;

	public Missionary() : base(
		ExtremeRoleId.Missionary,
		ExtremeRoleType.Neutral,
		ExtremeRoleId.Missionary.ToString(),
		ColorPalette.MissionaryBlue,
		false, false, false, false)
	{ }
#pragma warning restore CS8618

	public override string GetRolePlayerNameTag(SingleRoleBase targetRole, byte targetPlayerId)
	{
		if (this.lamb.Any(x => x.PlayerId == targetPlayerId))
		{
			return Design.ColoedString(this.NameColor, " ×");
		}
		else if (this.judgementTarget.Contains(targetPlayerId))
		{
			return Design.ColoedString(this.NameColor, " ★");
		}
		else
		{
			return base.GetRolePlayerNameTag(targetRole, targetPlayerId);
		}
	}

	public override bool IsSameTeam(SingleRoleBase targetRole) =>
        this.IsNeutralSameTeam(targetRole);

    protected override void CreateSpecificOption(
        AutoParentSetOptionCategoryFactory factory)
    {
        factory.CreateBoolOption(
            MissionaryOption.TellDeparture,
            true);
        factory.CreateFloatOption(
            MissionaryOption.DepartureMinTime,
            10f, 1.0f, 15f, 0.5f,
			format: OptionUnit.Second);
        factory.CreateFloatOption(
            MissionaryOption.DepartureMaxTime,
            30f, 15f, 120f, 0.5f
            , format: OptionUnit.Second);
        factory.CreateFloatOption(
            MissionaryOption.PropagateRange,
            1.2f, 0.0f, 2.0f, 0.1f);

        IRoleAbility.CreateCommonAbilityOption(factory);

		var useOpt = factory.CreateBoolOption(
			MissionaryOption.IsUseSolemnJudgment,
			false);
		factory.CreateIntOption(
			MissionaryOption.MaxJudgementNum,
			3, 1, GameSystem.VanillaMaxPlayerNum, 1,
			useOpt);
	}

    protected override void RoleSpecificInit()
    {
        this.lamb = new List<PlayerControl>(PlayerCache.AllPlayerControl.Count);
        this.timer = 0;

		var cate = this.Loader;

        this.tellDeparture = cate.GetValue<MissionaryOption, bool>(
            MissionaryOption.TellDeparture);
        this.maxTimerTime = cate.GetValue<MissionaryOption, float>(
            MissionaryOption.DepartureMaxTime);
        this.minTimerTime = cate.GetValue<MissionaryOption, float>(
            MissionaryOption.DepartureMinTime);
        this.propagateRange = cate.GetValue<MissionaryOption, float>(
            MissionaryOption.PropagateRange);
		this.isUseSolemnJudgment = cate.GetValue<MissionaryOption, bool>(
		   MissionaryOption.IsUseSolemnJudgment);
		this.maxJudgementTarget = cate.GetValue<MissionaryOption, int>(
		   MissionaryOption.MaxJudgementNum);

		this.judgementTarget = new HashSet<byte>();

		resetTimer();
    }

    public void CreateAbility()
    {
        this.CreateNormalAbilityButton(
            "propagate", Resources.UnityObjectLoader.LoadSpriteFromResources(
				ObjectPath.MissionaryPropagate));
    }

    public bool IsAbilityUse()
    {
        this.targetPlayer = Player.GetClosestPlayerInRange(
            PlayerControl.LocalPlayer, this,
            this.propagateRange);

		if (this.targetPlayer == null)
		{
			return false;
		}

		return
			IRoleAbility.IsCommonUse() &&
			!this.lamb.Contains(this.targetPlayer);
    }

    public void ResetOnMeetingStart()
    {
		updateJudgementTarget();
		if (this.tellText != null)
        {
            this.tellText.gameObject.SetActive(false);
        }
    }

    public void ResetOnMeetingEnd(NetworkedPlayerInfo? exiledPlayer = null)
    {
		if (exiledPlayer != null)
		{
			this.judgementTarget.Remove(exiledPlayer.PlayerId);
		}
		updateJudgementTarget();

		if (this.tellText != null)
        {
            this.tellText.gameObject.SetActive(false);
        }
    }

    public void Update(PlayerControl rolePlayer)
    {
        if (this.lamb.Count == 0 ||
			CachedShipStatus.Instance == null ||
            GameData.Instance == null ||
			!CachedShipStatus.Instance.enabled ||
			ExtremeRolesPlugin.ShipState.AssassinMeetingTrigger) { return; }

		this.lamb.RemoveAll(
			x =>
				x == null ||
				x.Data == null ||
				x.Data.IsDead ||
				x.Data.Disconnected);
		// 削除しきって誰もいなかったらタイマー自体をリセットする、じゃないとタイマーが短い状態で次の人が追加される
		if (this.lamb.Count == 0)
		{
			resetTimer();
			return;
		}

		this.timer -= Time.deltaTime;
        if (this.timer > 0) { return; }

        resetTimer();

		PlayerControl targetPlayer = this.lamb[0];

        if (targetPlayer == null ||
			targetPlayer.Data.IsDead ||
			targetPlayer.Data.Disconnected) { return; }

        Player.RpcUncheckMurderPlayer(
            targetPlayer.PlayerId,
            targetPlayer.PlayerId,
            byte.MaxValue);

        ExtremeRolesPlugin.ShipState.RpcReplaceDeadReason(
            targetPlayer.PlayerId, ExtremeShipStatus.PlayerStatus.Departure);

        if (this.tellDeparture)
        {
            rolePlayer.StartCoroutine(showText());
        }
    }

    public bool UseAbility()
    {
		if (this.targetPlayer == null) { return false; }

		byte playerId = this.targetPlayer.PlayerId;
        var assassin = ExtremeRoleManager.GameRole[playerId] as Combination.Assassin;

		if (assassin != null)
		{
			if (!assassin.CanKilled)
			{
				return false;
			}
			if (!assassin.CanKilledFromNeutral)
			{
				return false;
			}
		}

		if (this.judgementTarget.Contains(playerId))
		{
			Player.RpcUncheckMurderPlayer(
				playerId, playerId, byte.MaxValue);
			this.judgementTarget.Remove(playerId);
		}
		else
		{
			this.lamb.Add(this.targetPlayer);
		}
		this.targetPlayer = null;
        return true;
    }

    private void resetTimer()
    {
        this.timer = Random.RandomRange(
            this.minTimerTime, this.maxTimerTime);
    }

    private IEnumerator showText()
    {
        if (this.tellText == null)
        {
            this.tellText = Object.Instantiate(
                Prefab.Text, Camera.main.transform, false);
            this.tellText.transform.localPosition = new Vector3(-3.75f, -2.5f, -250.0f);
            this.tellText.enableWordWrapping = true;
            this.tellText.GetComponent<RectTransform>().sizeDelta = new Vector2(3.0f, 0.75f);
            this.tellText.alignment = TMPro.TextAlignmentOptions.BottomLeft;
            this.tellText.gameObject.layer = 5;
            this.tellText.text = Tr.GetString("departureText");
        }
        this.tellText.gameObject.SetActive(true);

        yield return new WaitForSeconds(3.5f);

        this.tellText.gameObject.SetActive(false);

    }

	public void VoteTo(byte target)
	{
		if (!this.isUseSolemnJudgment ||
			target == 252 ||
			target == 253 ||
			target == 254 ||
			target == byte.MaxValue ||
			this.judgementTarget.Count > this.maxJudgementTarget) { return; }

		this.judgementTarget.Add(target);
	}

	private void updateJudgementTarget()
	{
		foreach (var player in GameData.Instance.AllPlayers.GetFastEnumerator())
		{
			if (player == null) { continue; }

			if (player.Disconnected ||
				player.IsDead)
			{
				this.judgementTarget.Remove(player.PlayerId);
			}
		}
	}
}
