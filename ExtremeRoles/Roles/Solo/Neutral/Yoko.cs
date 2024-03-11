using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using ExtremeRoles.Module;
using ExtremeRoles.Module.SystemType.Roles;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Extension.State;
using ExtremeRoles.Roles.API.Extension.Neutral;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Performance;
using ExtremeRoles.Performance.Il2Cpp;

using BepInEx.Unity.IL2CPP.Utils;
using ExtremeRoles.Module.SystemType;

#nullable enable

namespace ExtremeRoles.Roles.Solo.Neutral;

public sealed class Yoko :
    SingleRoleBase,
	IRoleAutoBuildAbility,
    IRoleUpdate,
    IRoleWinPlayerModifier
{
    public enum YokoOption
    {
        CanRepairSabo,
        CanUseVent,
        SearchRange,
        SearchTime,
        TrueInfoRate,
		UseYashiro,
		YashiroActiveIsInfinity,
		YashiroActiveTime,
		YashiroProtectRange,
		YashiroSeelIsInfinity,
		YashiroSeelTime,
		YashiroUpdateWithMeeting,
	}

	private float searchRange;
    private float searchTime;
    private float timer;
    private int trueInfoGage;

	private Vector2 prevPos;

    private TMPro.TextMeshPro? tellText;
	private YokoYashiroSystem? yashiro;

    private readonly HashSet<ExtremeRoleId> noneEnemy = new HashSet<ExtremeRoleId>()
    {
        ExtremeRoleId.Villain,
        ExtremeRoleId.Vigilante,
        ExtremeRoleId.Missionary,
        ExtremeRoleId.Lover,
    };

	public ExtremeAbilityButton? Button { get; set; }

	public Yoko() : base(
        ExtremeRoleId.Yoko,
        ExtremeRoleType.Neutral,
        ExtremeRoleId.Yoko.ToString(),
        ColorPalette.YokoShion,
        false, false, false, false,
        true, false, true, false, false)
    { }

    public void ModifiedWinPlayer(
        GameData.PlayerInfo rolePlayerInfo,
        GameOverReason reason,
		ref ExtremeGameResult.WinnerTempData winner)
    {
        if (rolePlayerInfo.IsDead || rolePlayerInfo.Disconnected) { return; }

        switch ((RoleGameOverReason)reason)
        {
            case (RoleGameOverReason)GameOverReason.HumansByTask:
            case (RoleGameOverReason)GameOverReason.ImpostorBySabotage:
            case RoleGameOverReason.AssassinationMarin:
			case RoleGameOverReason.TeroristoTeroWithShip:
				break;
            case RoleGameOverReason.YokoAllDeceive:
				winner.Add(rolePlayerInfo);
                break;
            default:
				winner.AllClear();
                winner.Add(rolePlayerInfo);
                ExtremeRolesPlugin.ShipState.SetGameOverReason(
                    (GameOverReason)RoleGameOverReason.YokoAllDeceive);
                break;
        }
    }

	public override bool TryRolePlayerKilledFrom(
		PlayerControl rolePlayer, PlayerControl fromPlayer)
		=> this.yashiro is null || !this.yashiro.IsNearActiveYashiro(
			rolePlayer.GetTruePosition());

	public override bool IsSameTeam(SingleRoleBase targetRole) =>
        this.IsNeutralSameTeam(targetRole);

    protected override void CreateSpecificOption(
        IOptionInfo parentOps)
    {
        CreateBoolOption(
            YokoOption.CanRepairSabo,
            false, parentOps);
        CreateBoolOption(
            YokoOption.CanUseVent,
            false, parentOps);
        CreateFloatOption(
            YokoOption.SearchRange,
            7.5f, 5.0f, 15.0f, 0.5f,
            parentOps);
        CreateFloatOption(
            YokoOption.SearchTime,
            10f, 3.0f, 30f, 0.5f,
            parentOps,
            format: OptionUnit.Second);
        CreateIntOption(
            YokoOption.TrueInfoRate,
            50, 25, 80, 5, parentOps,
            format: OptionUnit.Percentage);

		var yashiroOpt = CreateBoolOption(
			YokoOption.UseYashiro,
			false, parentOps);
		this.CreateAbilityCountOption(yashiroOpt, 3, 10, 5f);

		var yashiroActiveOnOpt = CreateBoolOption(
			YokoOption.YashiroActiveIsInfinity,
			false, yashiroOpt);
		CreateIntOption(
			YokoOption.YashiroActiveTime,
			30, 1, 120, 1,
			yashiroActiveOnOpt,
			format: OptionUnit.Second,
			invert: true,
			enableCheckOption: yashiroOpt);
		CreateFloatOption(
			YokoOption.YashiroProtectRange,
			5.0f, 1.0f, 10.0f, 0.1f,
			yashiroOpt);

		var yashiroSeelOnOpt = CreateBoolOption(
			YokoOption.YashiroSeelIsInfinity,
			false, yashiroOpt);
		CreateIntOption(
			YokoOption.YashiroSeelTime,
			10, 1, 120, 1,
			yashiroSeelOnOpt,
			format: OptionUnit.Second,
			invert: true,
			enableCheckOption: yashiroOpt);
		CreateBoolOption(
			YokoOption.YashiroUpdateWithMeeting,
			true, yashiroOpt);
	}
    protected override void RoleSpecificInit()
    {
		var opt = OptionManager.Instance;
        this.CanRepairSabotage = opt.GetValue<bool>(
            GetRoleOptionId(YokoOption.CanRepairSabo));
        this.UseVent = opt.GetValue<bool>(
            GetRoleOptionId(YokoOption.CanUseVent));
        this.searchRange = opt.GetValue<float>(
            GetRoleOptionId(YokoOption.SearchRange));
        this.searchTime = opt.GetValue<float>(
            GetRoleOptionId(YokoOption.SearchTime));
        this.trueInfoGage = opt.GetValue<int>(
            GetRoleOptionId(YokoOption.TrueInfoRate));

		this.yashiro = null;

		if (opt.GetValue<bool>(GetRoleOptionId(YokoOption.UseYashiro)))
		{
			float activeTime = opt.GetValue<bool>(
				GetRoleOptionId(YokoOption.YashiroActiveIsInfinity)) ?
			float.MaxValue : opt.GetValue<int>(GetRoleOptionId(YokoOption.YashiroActiveTime));
			float sealTime = opt.GetValue<bool>(
				GetRoleOptionId(YokoOption.YashiroSeelIsInfinity)) ?
				float.MaxValue : opt.GetValue<int>(GetRoleOptionId(YokoOption.YashiroSeelTime));
			float protectRange = opt.GetValue<float>(GetRoleOptionId(YokoOption.YashiroProtectRange));
			bool isUpdateMeeting = opt.GetValue<bool>(
				GetRoleOptionId(YokoOption.YashiroUpdateWithMeeting));

			this.yashiro = ExtremeSystemTypeManager.Instance.CreateOrGet(
				YokoYashiroSystem.Type,
				() => new YokoYashiroSystem(activeTime, sealTime, protectRange, isUpdateMeeting));
		}

		this.timer = this.searchTime;
    }
    public void ResetOnMeetingEnd(GameData.PlayerInfo? exiledPlayer = null)
    {
        return;
    }

    public void ResetOnMeetingStart()
    {
        if (this.tellText != null)
        {
            this.tellText.gameObject.SetActive(false);
        }
    }
    public void Update(PlayerControl rolePlayer)
    {

        if (CachedShipStatus.Instance == null ||
            GameData.Instance == null) { return; }

        if (!CachedShipStatus.Instance.enabled ||
            MeetingHud.Instance != null ||
            ExtremeRolesPlugin.ShipState.AssassinMeetingTrigger) { return; }

        if (Minigame.Instance) { return; }

        if (this.timer > 0)
        {
            this.timer -= Time.deltaTime;
            return;
        }

        Vector2 truePosition = rolePlayer.GetTruePosition();

        this.timer = this.searchTime;
        bool isEnemy = false;

        foreach (GameData.PlayerInfo player in GameData.Instance.AllPlayers.GetFastEnumerator())
        {

			if (player == null ||
				player.Disconnected ||
				player.IsDead ||
				player.PlayerId == CachedPlayerControl.LocalPlayer.PlayerId)
			{
				continue;
			}

			PlayerControl @object = player.Object;
			SingleRoleBase targetRole = ExtremeRoleManager.GameRole[player.PlayerId];

			if (@object == null || this.IsSameTeam(targetRole))
			{
				continue;
			}

			Vector2 vector = @object.GetTruePosition() - truePosition;
			float magnitude = vector.magnitude;
			if (magnitude <= this.searchRange &&
				this.isEnemy(targetRole))
			{
				isEnemy = true;
				break;
			}
		}

        if (this.trueInfoGage <= RandomGenerator.Instance.Next(101))
        {
            isEnemy = !isEnemy;
        }

        string text = Helper.Translation.GetString("notFindEnemy");

        if (isEnemy)
        {
            text = Helper.Translation.GetString("findEnemy");
        }

        rolePlayer.StartCoroutine(showText(text));
    }

    private IEnumerator showText(string text)
    {
        if (this.tellText == null)
        {
            this.tellText = Object.Instantiate(
                Prefab.Text, Camera.main.transform, false);
            this.tellText.fontSize =
                this.tellText.fontSizeMax =
                this.tellText.fontSizeMin = 2.25f;
            this.tellText.transform.localPosition = new Vector3(0.0f, -0.9f, -250.0f);
            this.tellText.alignment = TMPro.TextAlignmentOptions.Center;
            this.tellText.gameObject.layer = 5;
        }
        this.tellText.text = text;
        this.tellText.gameObject.SetActive(true);

        yield return new WaitForSeconds(3.5f);

        this.tellText.gameObject.SetActive(false);

    }
    private bool isEnemy(SingleRoleBase role)
    {

        if (this.noneEnemy.Contains(role.Id))
        {
            return false;
        }
        else if (
            role.IsImpostor() ||
            role.CanKill() ||
            role.Id == ExtremeRoleId.Fencer)
        {
            return true;

        }
        else if (this.isYoko(role))
        {
            return true;
        }
        return false;
    }
    private bool isYoko(SingleRoleBase targetRole)
    {
        var multiAssignRole = targetRole as MultiAssignRoleBase;

        if (multiAssignRole != null)
        {
            if (multiAssignRole.AnotherRole != null)
            {
                return this.isYoko(multiAssignRole.AnotherRole);
            }
        }
        return targetRole.Id == ExtremeRoleId.Yoko;
    }

	public bool UseAbility()
	{
		if (this.yashiro is null)
		{
			return false;
		}
		this.prevPos = CachedPlayerControl.LocalPlayer.PlayerControl.GetTruePosition();

		return true;
	}

	public void CleanUp()
	{
		if (this.yashiro is null) { return; }

		Vector2 pos = CachedPlayerControl.LocalPlayer.PlayerControl.GetTruePosition();

		this.yashiro.RpcSetYashiro(this.GameControlId, pos);
	}

	public bool IsAbilityUse()
	{
		if (this.yashiro is null) { return false; }

		Vector2 pos = CachedPlayerControl.LocalPlayer.PlayerControl.GetTruePosition();

		return this.yashiro.CanSet(pos);
	}

	public bool IsAbilityActive() =>
		this.prevPos == CachedPlayerControl.LocalPlayer.PlayerControl.GetTruePosition();

	public void CreateAbility()
	{
		this.CreateAbilityCountButton(
			"yokoYashiro",
			Resources.Loader.CreateSpriteFromResources(
				Resources.Path.TestButton),
			this.IsAbilityActive,
			this.CleanUp,
			() => { });

		if (this.Button != null)
		{
			this.Button.SetButtonShow(this.yashiro is not null);
		}
	}
}
