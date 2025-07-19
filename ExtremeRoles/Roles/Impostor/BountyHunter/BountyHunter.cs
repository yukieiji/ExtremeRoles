using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using TMPro;

using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.SystemType.OnemanMeetingSystem;

using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Performance;

namespace ExtremeRoles.Roles.Solo.Impostor;

public sealed class BountyHunter : SingleRoleBase, IRoleUpdate, IRoleSpecialSetUp, IRoleResetMeeting
{

    private byte targetId;

    private float targetTimer;
    private float changeTargetTime;

    private bool isShowArrow;
    private float targetArrowUpdateTimer;
    private float targetArrowUpdateTime = 0.0f;

    private float defaultKillCool;
    private float targetKillCool;
    private float noneTargetKillCool;

    private TextMeshPro targetTimerText = null;
    private Arrow targetArrow = null;

    private Dictionary<byte, PoolablePlayer> PlayerIcon;

    public enum BountyHunterOption
    {
        TargetUpdateTime,
        TargetKillCoolTime,
        NoneTargetKillCoolTime,
        IsShowArrow,
        ArrowUpdateCycle
    }

    public BountyHunter() : base(
		RoleCore.BuildImpostor(ExtremeRoleId.BountyHunter),
        true, false, true, true)
    { }

    public void ResetOnMeetingEnd(NetworkedPlayerInfo exiledPlayer = null)
    {
        this.setNewTarget();

        if (this.targetArrow != null)
        {
            this.targetArrow.SetActive(true);
            this.updateArrow();
        }
    }

    public void ResetOnMeetingStart()
    {
		this.KillCoolTime = this.defaultKillCool;

        if (this.targetArrow != null)
        {
            this.targetArrow.SetActive(false);
        }

        if (this.PlayerIcon.ContainsKey(this.targetId))
        {
            this.PlayerIcon[this.targetId].gameObject.SetActive(false);
        }
    }

    public override string GetFullDescription()
    {
        return string.Format(
            base.GetFullDescription(),
            this.targetKillCool,
            this.noneTargetKillCool,
            Player.GetPlayerControlById(this.targetId).Data.PlayerName);
    }

    public override bool TryRolePlayerKillTo(
        PlayerControl rolePlayer, PlayerControl targetPlayer)
    {
        if (targetPlayer.PlayerId == this.targetId)
        {
            this.KillCoolTime = this.targetKillCool;
            this.setNewTarget();
        }
        else
        {
            this.KillCoolTime = this.noneTargetKillCool;
        }

        return true;
    }

    protected override void CreateSpecificOption(
        AutoParentSetOptionCategoryFactory factory)
    {

        factory.CreateFloatOption(
            BountyHunterOption.TargetUpdateTime,
            60f, 30.0f, 120f, 0.5f,
            format: OptionUnit.Second);

        factory.CreateFloatOption(
            BountyHunterOption.TargetKillCoolTime,
            5f, 1.0f, 60f, 0.5f,
            format: OptionUnit.Second);

        factory.CreateFloatOption(
            BountyHunterOption.NoneTargetKillCoolTime,
            45f, 1.0f, 120f, 0.5f,
            format: OptionUnit.Second);

        var arrowOption = factory.CreateBoolOption(
            BountyHunterOption.IsShowArrow,
            false);

        factory.CreateFloatOption(
            BountyHunterOption.ArrowUpdateCycle,
            10f, 1.0f, 120f, 0.5f,
            arrowOption, format: OptionUnit.Second);

    }

    protected override void RoleSpecificInit()
    {

        if (!this.HasOtherKillCool)
        {
            this.HasOtherKillCool = true;
            this.KillCoolTime = Player.DefaultKillCoolTime;
        }

        this.defaultKillCool = this.KillCoolTime;

        var cate = this.Loader;

        this.changeTargetTime = cate.GetValue<BountyHunterOption, float>(
            BountyHunterOption.TargetUpdateTime);
        this.targetKillCool = cate.GetValue<BountyHunterOption, float>(
            BountyHunterOption.TargetKillCoolTime);
        this.noneTargetKillCool = cate.GetValue<BountyHunterOption, float>(
            BountyHunterOption.NoneTargetKillCoolTime);
        this.isShowArrow = cate.GetValue<BountyHunterOption, bool>(
            BountyHunterOption.IsShowArrow);
        if (this.isShowArrow)
        {
            this.targetArrowUpdateTime = cate.GetValue<BountyHunterOption, float>(
                BountyHunterOption.ArrowUpdateCycle);
        }
        this.targetArrowUpdateTimer = 0;
        this.targetTimer = 0;
        this.targetId = byte.MaxValue;
        this.PlayerIcon = new Dictionary<byte, PoolablePlayer>();
    }

    public void IntroBeginSetUp()
    {
        return;
    }

    public void IntroEndSetUp()
    {
        this.PlayerIcon = Player.CreatePlayerIcon(
            scale: new Vector3(0.35f, 0.35f, 1.0f));
    }

    public void Update(PlayerControl rolePlayer)
    {

        if (ShipStatus.Instance == null ||
            GameData.Instance == null ||
            MeetingHud.Instance != null)
        {
            return;
        }
        if (!ShipStatus.Instance.enabled ||
			OnemanMeetingSystemManager.IsActive)
        {
            return;
        }


        this.targetTimer -= Time.deltaTime;

        if (this.targetTimerText == null)
        {
            this.targetTimerText = UnityEngine.Object.Instantiate(
                HudManager.Instance.KillButton.cooldownTimerText);
            this.targetTimerText.alignment = TMPro.TextAlignmentOptions.Center;
        }

        if (this.PlayerIcon.Count == 0) { return; }

        this.targetTimerText.text = Mathf.CeilToInt(
            Mathf.Clamp(this.targetTimer, 0, this.changeTargetTime)).ToString();

        if (this.targetTimer <= 0)
        {
            this.setNewTarget();
        }
        if (this.isShowArrow)
        {
            this.targetArrowUpdateTimer -= Time.deltaTime;
            if (this.targetArrowUpdateTimer <= 0)
            {
                this.updateArrow();
            }
            this.targetArrow.Update();
        }
    }

    private void setNewTarget()
    {
        this.targetTimer = this.changeTargetTime;
        if (this.PlayerIcon.TryGetValue(this.targetId, out PoolablePlayer prevTarget))
        {
            prevTarget.gameObject.SetActive(false);
        }

        var allPlayer = PlayerCache.AllPlayerControl.ToArray();

        var sortedAllPlayer = allPlayer.OrderBy(
            item => RandomGenerator.Instance.Next()).ToList();

        foreach (var player in sortedAllPlayer)
        {
            if (player.Data.IsDead || player.Data.Disconnected) { continue; }

            SingleRoleBase role = ExtremeRoleManager.GameRole[player.PlayerId];

            if (role.IsImpostor() ||
                role.FakeImposter ||
                this.targetId == player.PlayerId) { continue; }

            this.targetId = player.PlayerId;
            var pool = this.PlayerIcon[this.targetId];
            pool.gameObject.SetActive(true);

            if (!pool.TryGetComponent<AspectPosition>(out var aspectPosition))
            {
                aspectPosition = pool.gameObject.AddComponent<AspectPosition>();
                aspectPosition.Alignment = AspectPosition.EdgeAlignments.LeftBottom;
                aspectPosition.anchorPoint = new Vector2(0.5f, 0.5f);
                aspectPosition.DistanceFromEdge = new Vector3(0.45f, 0.35f);
            }
            aspectPosition.AdjustPosition();

            this.targetTimerText.transform.SetParent(pool.transform);
            this.targetTimerText.transform.localPosition = new Vector3(0.0f, 0.0f, -100.0f);
            this.targetTimerText.transform.localScale = new Vector3(2.25f, 2.25f, 1.0f);
            this.targetTimerText.gameObject.SetActive(true);

            break;
        }

    }
    private void updateArrow()
    {

        if (this.targetArrow == null)
        {
            this.targetArrow = new Arrow(
                Palette.ImpostorRed);
        }

        this.targetArrowUpdateTimer = this.targetArrowUpdateTime;
        this.targetArrow.UpdateTarget(
            Player.GetPlayerControlById(this.targetId).transform.position);
    }
}
