using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using AmongUs.GameOptions;
using TMPro;

using ExtremeRoles.Helper;
using ExtremeRoles.Module;

using ExtremeRoles.Module.CustomMonoBehaviour;
using ExtremeRoles.Resources;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Roles.Solo;
using ExtremeRoles.Roles.Solo.Crewmate;
using ExtremeRoles.Performance;


using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Roles.Solo.Neutral.Jackal;

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
        CanGuessNoneRole,
        GuessNoneRoleMode,
    }

    public enum GuessMode
    {
        BothGuesser,
        NiceGuesserOnly,
        EvilGuesserOnly,
    }

    public override string RoleName =>
        string.Concat(this.roleNamePrefix, this.Core.Name);

    private bool canGuessNoneRole;

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

    private sealed class GuesserRoleInfoCreater
    {
        public List<GuessBehaviour.RoleInfo> Result { get; } = new List<GuessBehaviour.RoleInfo>();

        private readonly Dictionary<ExtremeRoleType, List<ExtremeRoleId>> separetedRoleId;

        private sealed class NormalExRAssignState
        {
            public bool IsJackalOn { get; set; } = false;
            public bool IsJackalForceReplaceLover { get; set; } = false;
            public bool IsQueenOn { get; set; } = false;
        };

		public GuesserRoleInfoCreater(bool includeNoneRole)
		{
			this.separetedRoleId = new Dictionary<ExtremeRoleType, List<ExtremeRoleId>>()
			{
				{ExtremeRoleType.Crewmate, new List<ExtremeRoleId>() },
				{ExtremeRoleType.Impostor, new List<ExtremeRoleId>() },
				{ExtremeRoleType.Neutral , new List<ExtremeRoleId>() },
			};

			addVanillaRole(includeNoneRole);

			this.separetedRoleId[ExtremeRoleType.Crewmate].Add((ExtremeRoleId)RoleTypes.Crewmate);
			this.separetedRoleId[ExtremeRoleType.Impostor].Add((ExtremeRoleId)RoleTypes.Impostor);

			addAmongUsRole();
			addExRNormalRole(out NormalExRAssignState assignState);
			addExRCombRole(assignState);
		}

        private void add(
            ExtremeRoleId id,
            ExtremeRoleType team,
            ExtremeRoleId another = ExtremeRoleId.Null)
        {
            this.Result.Add(
                new GuessBehaviour.RoleInfo()
                {
                    Id = id,
                    AnothorId = another,
                    Team = team,
                });
        }

        private void addAmongUsRole()
        {
            var roleOptions = GameOptionsManager.Instance.CurrentGameOptions.RoleOptions;

            foreach (RoleTypes role in Enum.GetValues(typeof(RoleTypes)))
            {
                if (role is
						RoleTypes.Crewmate or
						RoleTypes.Impostor or
						RoleTypes.GuardianAngel or
						RoleTypes.CrewmateGhost or
						RoleTypes.ImpostorGhost)
                {
                    continue;
                }
                if (roleOptions.GetChancePerGame(role) > 0)
                {
                    ExtremeRoleType team = ExtremeRoleType.Null;
                    switch (role)
                    {
                        case RoleTypes.Engineer:
                        case RoleTypes.Scientist:
						case RoleTypes.Noisemaker:
						case RoleTypes.Tracker:
                            team = ExtremeRoleType.Crewmate;
                            break;
                        case RoleTypes.Shapeshifter:
						case RoleTypes.Phantom:
                            team = ExtremeRoleType.Impostor;
                            break;
                        default:
                            continue;
                    }
                    add((ExtremeRoleId)role, team);
                    this.separetedRoleId[team].Add((ExtremeRoleId)role);
                }
            }
        }

        private void addExRNormalRole(out NormalExRAssignState assignState)
        {
            assignState = new NormalExRAssignState();

            foreach (var (id, role) in ExtremeRoleManager.NormalRole)
            {
				var loader = role.Loader;

                int spawnOptSel = loader.GetValue<RoleCommonOption, int>(
					RoleCommonOption.SpawnRate);
                int roleNum = loader.GetValue<RoleCommonOption, int>(
                    RoleCommonOption.RoleNum);

                if (spawnOptSel < 1 || roleNum <= 0)
                {
                    continue;
                }

                ExtremeRoleId exId = (ExtremeRoleId)id;
                ExtremeRoleType team = role.Core.Team;

                // クイーンとサーヴァントとジャッカルとサイドキックはニュートラルの最後に追加する(役職のパターンがいくつかあるため)
                if (exId != ExtremeRoleId.Queen &&
                    exId != ExtremeRoleId.Jackal)
                {
                    add(exId, team);
                    this.separetedRoleId[team].Add(exId);
                }
                switch (exId)
                {
                    case ExtremeRoleId.Jackal:
                        assignState.IsJackalOn = true;
                        assignState.IsJackalForceReplaceLover = OptionManager.Instance.TryGetCategory(
							OptionTab.NeutralTab,
							ExtremeRoleManager.GetRoleGroupId(ExtremeRoleId.Jackal),
							out var cate) && cate.GetValue<JackalRole.JackalOption, bool>(JackalRole.JackalOption.ForceReplaceLover);
                        break;
                    case ExtremeRoleId.Queen:
                        assignState.IsQueenOn = true;
                        break;
                    case ExtremeRoleId.Hypnotist:
                        // 本来はニュートラルであるがソート用にインポスターとして突っ込む
                        add(ExtremeRoleId.Doll, ExtremeRoleType.Impostor);
                        this.separetedRoleId[ExtremeRoleType.Neutral].Add(ExtremeRoleId.Doll);
                        break;
					case ExtremeRoleId.Jailer:
						add(ExtremeRoleId.Yardbird, ExtremeRoleType.Crewmate);
						if (OptionManager.Instance.TryGetCategory(
								OptionTab.CrewmateTab,
								ExtremeRoleManager.GetRoleGroupId(ExtremeRoleId.Jailer),
								out var jailer) && 
							!jailer.GetValue<Jailer.Option, bool>(Jailer.Option.IsMissingToDead))
						{
							add(ExtremeRoleId.Lawbreaker, ExtremeRoleType.Neutral);
						}
						break;
					case ExtremeRoleId.Tucker:
						add(ExtremeRoleId.Chimera, ExtremeRoleType.Neutral);
						this.separetedRoleId[ExtremeRoleType.Neutral].Add(ExtremeRoleId.Chimera);
						break;
					default:
                        break;
                }
            }

            // ジャッカルとサイドキック、サイドキック + ラバーズの追加
            if (assignState.IsJackalOn)
            {
                add(ExtremeRoleId.Jackal, ExtremeRoleType.Neutral);
                add(ExtremeRoleId.Sidekick, ExtremeRoleType.Neutral);

                this.separetedRoleId[ExtremeRoleType.Neutral].Add(ExtremeRoleId.Jackal);
                this.separetedRoleId[ExtremeRoleType.Neutral].Add(ExtremeRoleId.Sidekick);
            }

            // クイーンとサーヴァント、サーヴァント + 〇〇、〇〇 + サーヴァントの追加
            if (assignState.IsQueenOn)
            {
                ExtremeRoleType queenTeam = ExtremeRoleType.Neutral;
                add(ExtremeRoleId.Queen, queenTeam);

                ExtremeRoleId servantId = ExtremeRoleId.Servant;
                if (this.separetedRoleId[queenTeam].Count > 1)
                {
                    add(servantId, queenTeam);
                }

                listAddTargetTeam(servantId, queenTeam, ExtremeRoleType.Crewmate);
                listAddTargetTeam(servantId, queenTeam, ExtremeRoleType.Impostor);

                foreach (var (id, roleMng) in ExtremeRoleManager.CombRole)
                {
					var loader = roleMng.Loader;

					int spawnOptSel = loader.GetValue<RoleCommonOption, int>(
						RoleCommonOption.SpawnRate);
					int roleNum = loader.GetValue<RoleCommonOption, int>(
						RoleCommonOption.RoleNum);

					if (spawnOptSel < 1 || roleNum <= 0)
                    {
                        continue;
                    }
                    if (roleMng is FlexibleCombinationRoleManagerBase flexMng)
                    {
                        add(flexMng.BaseRole.Core.Id, queenTeam, servantId);
                    }
                    else
                    {
                        foreach (var role in roleMng.Roles)
                        {
							if (role.IsNeutral())
							{
								continue;
							}

							add(role.Core.Id, queenTeam, servantId);
                        }
                    }
                }

                this.separetedRoleId[queenTeam].Add(ExtremeRoleId.Queen);
                this.separetedRoleId[queenTeam].Add(servantId);
            }
        }

        private void addExRCombRole(NormalExRAssignState assignState)
        {
            foreach (var (id, roleMng) in ExtremeRoleManager.CombRole)
            {
				var loader = roleMng.Loader;

				int spawnOptSel = loader.GetValue<RoleCommonOption, int>(
					RoleCommonOption.SpawnRate);
				int roleNum = loader.GetValue<RoleCommonOption, int>(
					RoleCommonOption.RoleNum);

				bool multiAssign = loader.GetValue<CombinationRoleCommonOption, bool>(
					CombinationRoleCommonOption.IsMultiAssign);

                if (spawnOptSel < 1 || roleNum <= 0)
                {
                    continue;
                }

                bool isNotTraitor = id != (byte)CombinationRoleType.Traitor;

                if (roleMng is FlexibleCombinationRoleManagerBase flexMng &&
                    isNotTraitor)
                {
                    ExtremeRoleType team = flexMng.BaseRole.Core.Team;
                    ExtremeRoleId baseRoleId = flexMng.BaseRole.Core.Id;

                    if (multiAssign)
                    {
                        if (loader.TryGetValueOption<CombinationRoleCommonOption, bool>(
								CombinationRoleCommonOption.IsAssignImposter,
                                out var option) &&
                            option.Value)
                        {
                            listAddTargetTeam(
                                baseRoleId,
                                ExtremeRoleType.Crewmate,
                                ExtremeRoleType.Crewmate);
                            listAddTargetTeam(
                                baseRoleId,
                                ExtremeRoleType.Impostor,
                                ExtremeRoleType.Impostor);
                        }
                        else
                        {
                            listAddTargetTeam(baseRoleId, team, team);
                        }
                    }
                    else
                    {
                        add(baseRoleId, team);
                    }
                    if (assignState.IsJackalOn &&
                        !assignState.IsJackalForceReplaceLover &&
                        baseRoleId == ExtremeRoleId.Lover)
                    {
                        add(baseRoleId, ExtremeRoleType.Neutral, ExtremeRoleId.Sidekick);
                    }
                }
                else if (multiAssign && isNotTraitor)
                {
                    foreach (var role in roleMng.Roles)
                    {
                        ExtremeRoleType team = role.Core.Team;
                        listAdd(role.Core.Id, team, this.separetedRoleId[team]);
                    }
                }
                else
                {
                    foreach (var role in roleMng.Roles)
                    {
                        add(role.Core.Id, role.Core.Team);
                    }
                }
            }
        }

        private void addVanillaRole(bool includeNoneRole)
        {
            if (includeNoneRole)
            {
                add((ExtremeRoleId)RoleTypes.Crewmate, ExtremeRoleType.Crewmate);
                add((ExtremeRoleId)RoleTypes.Impostor, ExtremeRoleType.Impostor);
            }
        }

        private void listAdd(ExtremeRoleId baseId, ExtremeRoleType team, List<ExtremeRoleId> list)
        {
            foreach (var roleId in list)
            {
                add(baseId, team, roleId);
            }
        }
        private void listAddTargetTeam(
            ExtremeRoleId baseId, ExtremeRoleType team, ExtremeRoleType targetType)
        {
            listAdd(baseId, team, this.separetedRoleId[targetType]);
        }
    }

	private const float defaultXPos = -2.85f;
	private const float subRoleXPos = -1.5f;

	public Guesser(
        ) : base(
			RoleCore.BuildCrewmate(
				ExtremeRoleId.Guesser,
				ColorPalette.GuesserRedYellow),
            false, true, false, false,
            tab: OptionTab.CombinationTab)
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
        byte target = instance.TargetPlayerId;

        return
            this.bulletNum <= 0 ||
            this.curGuessNum >= this.maxGuessNum ||
            target == 253;
    }

    public void ButtonMod(PlayerVoteArea instance, UiElement abilityButton)
    {

    }

    public Action CreateAbilityAction(PlayerVoteArea instance)
    {
        void openGusserUi()
        {
			byte targetPlayerId = instance.TargetPlayerId;
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

                GuesserRoleInfoCreater creator = new GuesserRoleInfoCreater(this.canGuessNoneRole);

                this.guesserUi.gameObject.SetActive(true);
                this.guesserUi.InitButton(
                    GuessAction,
                    creator.Result.OrderBy(
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

				float xPos = this.AnotherRole != null ? subRoleXPos : defaultXPos;

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
		CreateKillerOption(factory, imposterSetting);

		factory.CreateBoolOption(
            GuesserOption.CanCallMeeting,
            false);
        factory.CreateIntOption(
            GuesserOption.GuessNum,
            1, 1, GameSystem.MaxImposterNum, 1,
            format: OptionUnit.Shot);
        factory.CreateIntOption(
            GuesserOption.MaxGuessNumWhenMeeting,
            1, 1, GameSystem.MaxImposterNum, 1,
            format: OptionUnit.Shot);
        var noneGuessRoleOpt = factory.CreateBoolOption(
            GuesserOption.CanGuessNoneRole,
            false);
        factory.CreateSelectionOption<GuesserOption, GuessMode>(
            GuesserOption.GuessNoneRoleMode, noneGuessRoleOpt);
    }

    protected override void RoleSpecificInit()
    {
        this.uiPrefab = null;
        this.guesserUi = null;


        var loader = this.Loader;

        this.CanCallMeeting = loader.GetValue<GuesserOption, bool>(
            GuesserOption.CanCallMeeting);

        bool canGuessNoneRole = loader.GetValue<GuesserOption, bool>(
            GuesserOption.CanGuessNoneRole);
        GuessMode guessMode = (GuessMode)loader.GetValue<GuesserOption, int>(
            GuesserOption.GuessNoneRoleMode);

        this.canGuessNoneRole = canGuessNoneRole &&
            ((
                guessMode == GuessMode.BothGuesser
            )
            ||
            (
                guessMode == GuessMode.NiceGuesserOnly && this.IsCrewmate()
            )
            ||
            (
                guessMode == GuessMode.EvilGuesserOnly && this.IsImpostor()
            ));

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
