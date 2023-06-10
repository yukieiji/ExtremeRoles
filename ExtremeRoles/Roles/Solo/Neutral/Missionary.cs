using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using ExtremeRoles.Module;
using ExtremeRoles.Module.ExtremeShipStatus;
using ExtremeRoles.Module.CustomOption;
using ExtremeRoles.Resources;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Roles.API.Extension.Neutral;
using ExtremeRoles.Performance;

using BepInEx.Unity.IL2CPP.Utils.Collections;
using ExtremeRoles.Helper;

namespace ExtremeRoles.Roles.Solo.Neutral;

public sealed class Missionary :
	SingleRoleBase,
	IRoleAbility,
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

    public ExtremeAbilityButton Button
    {
        get => this.propagate;
        set
        {
            this.propagate = value;
        }
    }

	public byte TargetPlayer = byte.MaxValue;

    private Queue<byte> lamb;
	private HashSet<byte> judgementTarget;
	private float timer;

    private float propagateRange;
    private float minTimerTime;
    private float maxTimerTime;
    private bool tellDeparture;
	private bool isUseSolemnJudgment;
	private int maxJudgementTarget;

	private TMPro.TextMeshPro tellText;

    private ExtremeAbilityButton propagate;

    public Missionary() : base(
        ExtremeRoleId.Missionary,
        ExtremeRoleType.Neutral,
        ExtremeRoleId.Missionary.ToString(),
        ColorPalette.MissionaryBlue,
        false, false, false, false)
    { }

	public override string GetRolePlayerNameTag(SingleRoleBase targetRole, byte targetPlayerId)
	{
		if (this.lamb.Contains(targetPlayerId))
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
        IOptionInfo parentOps)
    {
        CreateBoolOption(
            MissionaryOption.TellDeparture,
            true, parentOps);
        CreateFloatOption(
            MissionaryOption.DepartureMinTime,
            10f, 1.0f, 15f, 0.5f,
            parentOps, format: OptionUnit.Second);
        CreateFloatOption(
            MissionaryOption.DepartureMaxTime,
            30f, 15f, 120f, 0.5f,
            parentOps, format: OptionUnit.Second);
        CreateFloatOption(
            MissionaryOption.PropagateRange,
            1.2f, 0.0f, 2.0f, 0.1f,
            parentOps);

        this.CreateCommonAbilityOption(parentOps);

		var useOpt = CreateBoolOption(
			MissionaryOption.IsUseSolemnJudgment,
			false, parentOps);
		CreateIntOption(
			MissionaryOption.MaxJudgementNum,
			3, 1, GameSystem.VanillaMaxPlayerNum, 1,
			useOpt);
	}

    protected override void RoleSpecificInit()
    {
        this.lamb = new Queue<byte>();
		this.judgementTarget = new HashSet<byte>();
        this.timer = 0;

        this.tellDeparture = OptionManager.Instance.GetValue<bool>(
            GetRoleOptionId(MissionaryOption.TellDeparture));
        this.maxTimerTime = OptionManager.Instance.GetValue<float>(
            GetRoleOptionId(MissionaryOption.DepartureMaxTime));
        this.minTimerTime = OptionManager.Instance.GetValue<float>(
            GetRoleOptionId(MissionaryOption.DepartureMinTime));
        this.propagateRange = OptionManager.Instance.GetValue<float>(
            GetRoleOptionId(MissionaryOption.PropagateRange));
		this.isUseSolemnJudgment = OptionManager.Instance.GetValue<bool>(
		   GetRoleOptionId(MissionaryOption.IsUseSolemnJudgment));
		this.maxJudgementTarget = OptionManager.Instance.GetValue<int>(
		   GetRoleOptionId(MissionaryOption.MaxJudgementNum));

		this.judgementTarget = new HashSet<byte>();

		resetTimer();
        this.RoleAbilityInit();

    }

    public void CreateAbility()
    {
        this.CreateNormalAbilityButton(
            "propagate", Loader.CreateSpriteFromResources(
                Path.MissionaryPropagate));
    }

    public bool IsAbilityUse()
    {
        this.TargetPlayer = byte.MaxValue;
        PlayerControl target = Player.GetClosestPlayerInRange(
            CachedPlayerControl.LocalPlayer, this,
            this.propagateRange);

        if (target != null)
        {
			byte playerId = target.PlayerId;

            if (!this.lamb.Contains(playerId))
            {
                this.TargetPlayer = playerId;
            }
        }

        return this.IsCommonUse() && this.TargetPlayer != byte.MaxValue;
    }

    public void ResetOnMeetingStart()
    {
        if (this.tellText != null)
        {
            this.tellText.gameObject.SetActive(false);
        }
    }

    public void ResetOnMeetingEnd(GameData.PlayerInfo exiledPlayer = null)
    {
        if (this.tellText != null)
        {
            this.tellText.gameObject.SetActive(false);
        }
    }

    public void Update(PlayerControl rolePlayer)
    {
        if (this.lamb.Count == 0) { return; }

        if (CachedShipStatus.Instance == null ||
            GameData.Instance == null) { return; }
        if (!CachedShipStatus.Instance.enabled ||
            ExtremeRolesPlugin.ShipState.AssassinMeetingTrigger) { return; }

        this.timer -= Time.deltaTime;
        if (this.timer > 0) { return; }

        resetTimer();

        byte targetPlayerId = this.lamb.Dequeue();
        PlayerControl targetPlayer = Player.GetPlayerControlById(targetPlayerId);

        if (targetPlayer == null) { return; }
        if (targetPlayer.Data.IsDead || targetPlayer.Data.Disconnected) { return; }

        Player.RpcUncheckMurderPlayer(
            targetPlayer.PlayerId,
            targetPlayer.PlayerId,
            byte.MaxValue);

        ExtremeRolesPlugin.ShipState.RpcReplaceDeadReason(
            targetPlayer.PlayerId, ExtremeShipStatus.PlayerStatus.Departure);

        if (this.tellDeparture)
        {
            rolePlayer.StartCoroutine(showText().WrapToIl2Cpp());
        }
    }

    public bool UseAbility()
    {
        var assassin = ExtremeRoleManager.GameRole[this.TargetPlayer] as Combination.Assassin;

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

		if (this.judgementTarget.Contains(this.TargetPlayer))
		{
			Player.RpcUncheckMurderPlayer(
				this.TargetPlayer,
				this.TargetPlayer,
				byte.MaxValue);
			this.judgementTarget.Remove(this.TargetPlayer);
		}
		else
		{
			this.lamb.Enqueue(this.TargetPlayer);
		}
        this.TargetPlayer = byte.MaxValue;
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
            this.tellText.text = Helper.Translation.GetString("departureText");
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
}
