using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

using AmongUs.GameOptions;

using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Module.CustomMonoBehaviour;
using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.CustomOption.Implemented;
using ExtremeRoles.Module.RoleAssign;
using ExtremeRoles.Resources;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Roles.Solo;
using ExtremeRoles.Roles.Solo.Crewmate;
using ExtremeRoles.Roles.Solo.Neutral.Jackal;
using Microsoft.Extensions.DependencyInjection;
using ExtremeRoles.GameMode.RoleSelector;

namespace ExtremeRoles.Roles.Combination;

public sealed class GuesserManager : FlexibleCombinationRoleManagerBase
{
    public GuesserManager() : base(
		CombinationRoleType.Guesser,
		new Guesser(), 1)
    { }

}

public sealed class Guesser :
    MultiAssignRoleBase,
    IRoleResetMeeting,
    IRoleMeetingButtonAbility,
    IRoleUpdate
{
    public enum GuesserOption
    {
        CanCallMeeting,
        GuessNum,
        MaxGuessNumWhenMeeting,
        GuessDefaultRoleMode,

		CanCrewmate,
		CanImpostor,
		// ニュートラルが追加された時に拡張する予定
		CanDove,
		CanMilitant,
	}

	[Flags]
	public enum DefaultGuessRole
	{
		None = 0,
		Crewmate = 1 << 0,
		Impostor = 1 << 1,
		// ニュートラルが追加された時に拡張する予定
		Dove = 1 << 3,
		Militant = 1 << 4,
	}

	public enum GuessMode
    {
		DisableKey,
        BothGuesser,
        NiceGuesserOnly,
        EvilGuesserOnly,
    }

    public override string RoleName =>
        string.Concat(this.roleNamePrefix, this.Core.Name);

    private bool canGuessNoneRole;
	private DefaultGuessRole defaultGuessRole;

	private int bulletNum;
    private int maxGuessNum;
    private int curGuessNum;

    private GameObject uiPrefab = null;
    private GuesserUi guesserUi = null;

    private TextMeshPro meetingGuessText = null;
    private string roleNamePrefix;

	public Sprite AbilityImage => UnityObjectLoader.LoadFromResources(ExtremeRoleId.Guesser);

	private static IReadOnlySet<ExtremeRoleId> alwaysMissRole = new HashSet<ExtremeRoleId>()
    {
        ExtremeRoleId.Assassin,
        ExtremeRoleId.Marlin,
        ExtremeRoleId.Villain
    };

	private const float defaultXPos = -2.85f;
	private const float subRoleXPos = -1.5f;

	public Guesser(
        ) : base(
			RoleArgs.BuildCrewmate(
				ExtremeRoleId.Guesser,
				ColorPalette.GuesserRedYellow),
			OptionTab.CombinationTab)
    { }

    private static void missGuess()
    {
        Player.RpcUncheckMurderPlayer(
            PlayerControl.LocalPlayer.PlayerId,
            PlayerControl.LocalPlayer.PlayerId,
            byte.MinValue);
        Sound.RpcPlaySound(Sound.Type.Kill);
    }

    public void GuessAction(GuessBehaviour.RoleInfo roleInfo, byte playerId)
    {
        ExtremeRolesPlugin.Logger.LogDebug($"TargetPlayerId:{playerId}  GuessTo:{roleInfo}");

        // まず弾をへらす
        this.bulletNum = this.bulletNum - 1;
        this.curGuessNum = this.curGuessNum + 1;

        if (!ExtremeRoleManager.TryGetRole(playerId, out var targetRole))
        {
            return;
        }

        ExtremeRoleId roleId = targetRole.Core.Id;
        ExtremeRoleId anotherRoleId = ExtremeRoleId.Null;

        if (targetRole is VanillaRoleWrapper vanillaRole)
        {
            roleId = (ExtremeRoleId)vanillaRole.VanilaRoleId;
        }
        else if (
            targetRole is MultiAssignRoleBase multiRole &&
            multiRole.AnotherRole != null)
        {
            if (multiRole.AnotherRole is VanillaRoleWrapper anothorVanillRole)
            {
                anotherRoleId = (ExtremeRoleId)anothorVanillRole.VanilaRoleId;
            }
            else
            {
                anotherRoleId = multiRole.AnotherRole.Core.Id;
            }
        }

        if ((
                BodyGuard.IsBlockMeetingKill &&
                BodyGuard.TryGetShiledPlayerId(playerId, out byte _)
            ) || alwaysMissRole.Contains(targetRole.Core.Id))
        {
            missGuess();
        }
        else if (
            roleInfo.Id == roleId &&
            roleInfo.AnothorId == anotherRoleId)
        {
            Player.RpcUncheckMurderPlayer(
                PlayerControl.LocalPlayer.PlayerId,
                playerId, byte.MinValue);
            Sound.RpcPlaySound(Sound.Type.Kill);
        }
        else
        {
            missGuess();
        }
    }

    public bool IsBlockMeetingButtonAbility(
        PlayerVoteArea instance)
    {
        byte target = instance.PlayerId;

        return
            this.bulletNum <= 0 ||
            this.curGuessNum >= this.maxGuessNum;
    }

    public void ButtonMod(PlayerVoteArea instance, UiElement abilityButton)
    {

    }

    public Action CreateAbilityAction(PlayerVoteArea instance)
    {
        void openGusserUi()
        {
			byte targetPlayerId = instance.PlayerId;
			var info = GameData.Instance.GetPlayerById(targetPlayerId);
			if (info == null)
			{
				return;
			}

			if (this.uiPrefab == null)
            {
                this.uiPrefab = UnityEngine.Object.Instantiate(
				   UnityObjectLoader.LoadFromResources<GameObject, ExtremeRoleId>(
                        ExtremeRoleId.Guesser,
                        ObjectPath.GetRolePrefabPath(ExtremeRoleId.Guesser, "UI")),
                    ShipStatus.Instance.transform);

                this.uiPrefab.SetActive(false);
            }
            if (this.guesserUi == null)
            {
                GameObject obj = UnityEngine.Object.Instantiate(
                    this.uiPrefab, MeetingHud.Instance.transform);
                this.guesserUi = obj.GetComponent<GuesserUi>();

                var creator = ExtremeRolesPlugin.Instance.Provider.GetRequiredService<IGuesserRoleInfoCreator>();
                var roleInfos = creator.Create(this.canGuessNoneRole, this.defaultGuessRole);

                this.guesserUi.gameObject.SetActive(true);
                this.guesserUi.InitButton(
                    GuessAction,
                    roleInfos.OrderBy(
                        (GuessBehaviour.RoleInfo x) =>
                        {
                            ExtremeRoleType team = x.Team;
                            if (team == ExtremeRoleType.Neutral)
                            {
                                return 5000;
                            }
                            else
                            {
                                return (int)team;
                            }
                        })
                );
            }

			this.guesserUi.SetTitle(
                Tr.GetString("guesserUiTitle", info.DefaultOutfit.PlayerName));
            this.guesserUi.SetInfo(
                Tr.GetString("guesserUiInfo", this.bulletNum, this.maxGuessNum));
            this.guesserUi.SetTarget(targetPlayerId);
            this.guesserUi.gameObject.SetActive(true);
        }
        return openGusserUi;
    }

    public void ResetOnMeetingEnd(NetworkedPlayerInfo exiledPlayer = null)
    {
        this.guesserUi = null;
    }

    public void ResetOnMeetingStart()
    {
        this.curGuessNum = 0;
    }

    public void Update(PlayerControl rolePlayer)
    {
		var meeting = MeetingHud.Instance;
        if (meeting != null)
        {
            if (this.meetingGuessText == null)
            {
                this.meetingGuessText = UnityEngine.Object.Instantiate(
                    HudManager.Instance.TaskPanel.taskText,
					meeting.transform);
                this.meetingGuessText.alignment = TMPro.TextAlignmentOptions.BottomLeft;
                this.meetingGuessText.transform.position = Vector3.zero;

				float xPos = this.AnotherRole is IRoleMeetingButtonAbility ? subRoleXPos : defaultXPos;

                this.meetingGuessText.transform.localPosition = new Vector3(xPos, 3.15f, -20f);
                this.meetingGuessText.transform.localScale *= 0.9f;
                this.meetingGuessText.color = Palette.White;
                this.meetingGuessText.gameObject.SetActive(false);
            }

            this.meetingGuessText.text = Tr.GetString(
				"guesserUiInfo",
                this.bulletNum, this.maxGuessNum);
            meetingInfoSetActive(true);
        }
        else
        {
            meetingInfoSetActive(false);
        }
    }

    protected override void CreateSpecificOption(
        AutoParentSetOptionCategoryFactory factory)
    {
		var imposterSetting = factory.Get((int)CombinationRoleCommonOption.IsAssignImposter);
		CreateKillerOption(factory, new ParentActive(imposterSetting));

		factory.CreateBoolOption(
            GuesserOption.CanCallMeeting,
            false);
        var maxGuessNum = factory.CreateIntOption(
            GuesserOption.GuessNum,
            1, 1, GameSystem.MaxImposterNum, 1,
            format: OptionUnit.Shot);
        factory.CreateIntDynamicMaxOption(
            GuesserOption.MaxGuessNumWhenMeeting,
            1, 1, 1, maxGuessNum,
			format: OptionUnit.Shot);

        var defaultRoleMode = factory.CreateSelectionOption<GuesserOption, GuessMode>(
            GuesserOption.GuessDefaultRoleMode);

		var parentActive = new ParentActive(defaultRoleMode);

		factory.CreateBoolOption(
			GuesserOption.CanCrewmate,
			true, parentActive);
		factory.CreateBoolOption(
			GuesserOption.CanImpostor,
			true, parentActive);
		factory.CreateBoolOption(
			GuesserOption.CanDove,
			true, parentActive);
		factory.CreateBoolOption(
			GuesserOption.CanMilitant,
			true, parentActive);
	}

    protected override void RoleSpecificInit()
    {
        this.uiPrefab = null;
        this.guesserUi = null;


        var loader = this.Loader;

        this.CanCallMeeting = loader.GetValue<GuesserOption, bool>(
            GuesserOption.CanCallMeeting);

        var guessMode = (GuessMode)loader.GetValue<GuesserOption, int>(
            GuesserOption.GuessDefaultRoleMode);

        this.canGuessNoneRole = 
            (
                guessMode == GuessMode.BothGuesser
            )
            ||
            (
                guessMode == GuessMode.NiceGuesserOnly && this.IsCrewmate()
            )
            ||
            (
                guessMode == GuessMode.EvilGuesserOnly && this.IsImpostor()
            );
		
		this.defaultGuessRole = DefaultGuessRole.None;
		if (loader.GetValue<GuesserOption, bool>(
			GuesserOption.CanCrewmate))
		{
			this.defaultGuessRole |= DefaultGuessRole.Crewmate;
		}
		if (loader.GetValue<GuesserOption, bool>(
			GuesserOption.CanImpostor))
		{
			this.defaultGuessRole |= DefaultGuessRole.Impostor;
		}
		if (loader.GetValue<GuesserOption, bool>(
			GuesserOption.CanDove))
		{
			this.defaultGuessRole |= DefaultGuessRole.Dove;
		}
		if (loader.GetValue<GuesserOption, bool>(
			GuesserOption.CanMilitant))
		{
			this.defaultGuessRole |= DefaultGuessRole.Militant;
		}

		this.bulletNum = loader.GetValue<GuesserOption, int>(
            GuesserOption.GuessNum);
        this.maxGuessNum = loader.GetValue<GuesserOption, int>(
            GuesserOption.MaxGuessNumWhenMeeting);

        this.curGuessNum = 0;
        this.roleNamePrefix = this.CreateImpCrewPrefix();
    }

    private void meetingInfoSetActive(bool active)
    {
        if (this.meetingGuessText != null)
        {
            this.meetingGuessText.gameObject.SetActive(active);
        }
    }
}
