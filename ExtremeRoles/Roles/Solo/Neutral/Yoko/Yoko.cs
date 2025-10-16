using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using BepInEx.Unity.IL2CPP.Utils;

using ExtremeRoles.Module;
using ExtremeRoles.Module.Ability;
using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.SystemType;
using ExtremeRoles.Module.SystemType.Roles;
using ExtremeRoles.Resources;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface.Status;
using ExtremeRoles.Roles.API.Extension.Neutral;
using ExtremeRoles.Roles.API.Extension.State;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Performance.Il2Cpp;
using ExtremeRoles.Module.GameResult;

#nullable enable

namespace ExtremeRoles.Roles.Solo.Neutral.Yoko;

public sealed class YokoRole :
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
		YashiroActiveTime,
		YashiroProtectRange,
		YashiroSeelTime,
		YashiroUpdateWithMeeting,
	}

	private float searchRange;
    private float searchTime;
    private float timer;
    private int trueInfoGage;

	private Vector2 prevPos;

    private TMPro.TextMeshPro? tellText;
	private YokoStatusModel? status;
    public override IStatusModel? Status => status;

    private readonly HashSet<ExtremeRoleId> noneEnemy = new HashSet<ExtremeRoleId>()
    {
        ExtremeRoleId.Villain,
        ExtremeRoleId.Vigilante,
        ExtremeRoleId.Missionary,
        ExtremeRoleId.Lover,
    };

	public ExtremeAbilityButton? Button { get; set; }

	public YokoRole() : base(
		RoleCore.BuildNeutral(
			ExtremeRoleId.Yoko,
			ColorPalette.YokoShion),
        false, false, false, false,
        true, false, true, false, false)
    {
    }

    public void ModifiedWinPlayer(
        NetworkedPlayerInfo rolePlayerInfo,
        GameOverReason reason,
		in WinnerTempData winner)
    {
        if (rolePlayerInfo.IsDead || rolePlayerInfo.Disconnected) { return; }

        switch ((RoleGameOverReason)reason)
        {
            case (RoleGameOverReason)GameOverReason.CrewmatesByTask:
            case (RoleGameOverReason)GameOverReason.ImpostorsBySabotage:
            case RoleGameOverReason.AssassinationMarin:
			case RoleGameOverReason.TeroristoTeroWithShip:
			case RoleGameOverReason.MonikaThisGameIsMine:
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

	public override bool IsSameTeam(SingleRoleBase targetRole) =>
        this.IsNeutralSameTeam(targetRole);

    protected override void CreateSpecificOption(
        AutoParentSetOptionCategoryFactory factory)
    {
        factory.CreateBoolOption(
            YokoOption.CanRepairSabo,
            false);
        factory.CreateBoolOption(
            YokoOption.CanUseVent,
            false);
        factory.CreateFloatOption(
            YokoOption.SearchRange,
            7.5f, 5.0f, 15.0f, 0.5f);
        factory.CreateFloatOption(
            YokoOption.SearchTime,
            10f, 3.0f, 30f, 0.5f,
            format: OptionUnit.Second);
        factory.CreateIntOption(
            YokoOption.TrueInfoRate,
            50, 25, 80, 5,
            format: OptionUnit.Percentage);

		var yashiroOpt = factory.CreateBoolOption(
			YokoOption.UseYashiro,
			false);
		IRoleAbility.CreateAbilityCountOption(factory, 3, 10, 5f, parentOpt: yashiroOpt);

		factory.CreateIntOption(
			YokoOption.YashiroActiveTime,
			30, 1, 360, 1,
			yashiroOpt,
			format: OptionUnit.Second);

		factory.CreateIntOption(
			YokoOption.YashiroSeelTime,
			10, 1, 360, 1,
			yashiroOpt,
			format: OptionUnit.Second);

		factory.CreateFloatOption(
			YokoOption.YashiroProtectRange,
			5.0f, 1.0f, 10.0f, 0.1f,
			yashiroOpt);

		factory.CreateBoolOption(
			YokoOption.YashiroUpdateWithMeeting,
			true, yashiroOpt);
	}
    protected override void RoleSpecificInit()
    {
		var cate = Loader;
        CanRepairSabotage = cate.GetValue<YokoOption, bool>(YokoOption.CanRepairSabo);
        UseVent = cate.GetValue<YokoOption, bool>(YokoOption.CanUseVent);
        searchRange = cate.GetValue<YokoOption, float>(YokoOption.SearchRange);
        searchTime = cate.GetValue<YokoOption, float>(YokoOption.SearchTime);
        trueInfoGage = cate.GetValue<YokoOption, int>(YokoOption.TrueInfoRate);

		YokoYashiroSystem? system = null;

		if (cate.GetValue<YokoOption, bool>(YokoOption.UseYashiro))
		{
			float activeTime = cate.GetValue<YokoOption, int>(YokoOption.YashiroActiveTime);
			float sealTime = cate.GetValue<YokoOption, int>(YokoOption.YashiroSeelTime);
			float protectRange = cate.GetValue<YokoOption, float>(YokoOption.YashiroProtectRange);
			bool isUpdateMeeting = cate.GetValue<YokoOption, bool>(YokoOption.YashiroUpdateWithMeeting);

			system = ExtremeSystemTypeManager.Instance.CreateOrGet(
				YokoYashiroSystem.Type,
				() => new YokoYashiroSystem(activeTime, sealTime, protectRange, isUpdateMeeting));
		}

		status = new YokoStatusModel(system);
		AbilityClass = new YokoAbilityHandler(status);

		timer = searchTime;
    }
    public void ResetOnMeetingEnd(NetworkedPlayerInfo? exiledPlayer = null)
    {
        return;
    }

    public void ResetOnMeetingStart()
    {
        if (tellText != null)
        {
            tellText.gameObject.SetActive(false);
        }
    }
    public void Update(PlayerControl rolePlayer)
    {

        if (!GameProgressSystem.IsTaskPhase)
		{
			return;
		}

		if (Button != null)
		{
			Button.SetButtonShow(this.status?.yashiro is not null);
		}

		if (Minigame.Instance)
		{
			return;
		}

        if (timer > 0)
        {
            timer -= Time.deltaTime;
            return;
        }

        Vector2 truePosition = rolePlayer.GetTruePosition();

        timer = searchTime;
        bool isEnemy = false;

        foreach (NetworkedPlayerInfo player in GameData.Instance.AllPlayers.GetFastEnumerator())
        {

			if (player == null ||
				player.Disconnected ||
				player.IsDead ||
				player.PlayerId == PlayerControl.LocalPlayer.PlayerId)
			{
				continue;
			}

			PlayerControl @object = player.Object;
			SingleRoleBase targetRole = ExtremeRoleManager.GameRole[player.PlayerId];

			if (@object == null || IsSameTeam(targetRole))
			{
				continue;
			}

			Vector2 vector = @object.GetTruePosition() - truePosition;
			float magnitude = vector.magnitude;
			if (magnitude <= searchRange &&
				this.isEnemy(targetRole))
			{
				isEnemy = true;
				break;
			}
		}

        if (trueInfoGage <= RandomGenerator.Instance.Next(101))
        {
            isEnemy = !isEnemy;
        }

        string text = Tr.GetString("notFindEnemy");

        if (isEnemy)
        {
            text = Tr.GetString("findEnemy");
        }

        rolePlayer.StartCoroutine(showText(text));
    }

    private IEnumerator showText(string text)
    {
        if (tellText == null)
        {
            tellText = Object.Instantiate(
                Prefab.Text, Camera.main.transform, false);
            tellText.fontSize =
                tellText.fontSizeMax =
                tellText.fontSizeMin = 2.25f;
            tellText.transform.localPosition = new Vector3(0.0f, -0.9f, -250.0f);
            tellText.alignment = TMPro.TextAlignmentOptions.Center;
            tellText.gameObject.layer = 5;
        }
        tellText.text = text;
        tellText.gameObject.SetActive(true);

        yield return new WaitForSeconds(3.5f);

        tellText.gameObject.SetActive(false);

    }
    private bool isEnemy(SingleRoleBase role)
    {
		var id = role.Core.Id;
        if (this.noneEnemy.Contains(id))
        {
            return false;
        }
        else if (
            role.IsImpostor() ||
            role.CanKill() ||
			id == ExtremeRoleId.Fencer)
        {
            return true;

        }
        else if (isYoko(role))
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
                return isYoko(multiAssignRole.AnotherRole);
            }
        }
        return targetRole.Core.Id == ExtremeRoleId.Yoko;
    }

	public bool UseAbility()
	{
		if (this.status?.yashiro is null)
		{
			return false;
		}
		prevPos = PlayerControl.LocalPlayer.GetTruePosition();

		return true;
	}

	public void CleanUp()
	{
		if (this.status?.yashiro is null) { return; }

		Vector2 pos = PlayerControl.LocalPlayer.GetTruePosition();

		status.yashiro.RpcSetYashiro(GameControlId, pos);
	}

	public bool IsAbilityUse()
	{
		if (this.status?.yashiro is null) { return false; }

		Vector2 pos = PlayerControl.LocalPlayer.GetTruePosition();

		return status.yashiro.CanSet(pos);
	}

	public bool IsAbilityActive() =>
		prevPos == PlayerControl.LocalPlayer.GetTruePosition();

	public void CreateAbility()
	{
		this.CreateActivatingAbilityCountButton(
			"yokoYashiro",
			UnityObjectLoader.LoadFromResources(
				ExtremeRoleId.Yoko),
			IsAbilityActive,
			CleanUp,
			() => { });
	}
}
