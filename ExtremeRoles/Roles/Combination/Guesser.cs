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
using ExtremeRoles.Module.CustomOption;

namespace ExtremeRoles.Roles.Combination;

public sealed class GuesserManager : FlexibleCombinationRoleManagerBase
{
    public GuesserManager() : base(new Guesser(), 1)
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
        string.Concat(this.roleNamePrefix, this.RawRoleName);

    private bool canGuessNoneRole;

    private int bulletNum;
    private int maxGuessNum;
    private int curGuessNum;

    private GameObject uiPrefab = null;
    private GuesserUi guesserUi = null;

    private TextMeshPro meetingGuessText = null;
    private string roleNamePrefix;

    private static HashSet<ExtremeRoleId> alwaysMissRole = new HashSet<ExtremeRoleId>()
    {
        ExtremeRoleId.Assassin,
        ExtremeRoleId.Marlin,
        ExtremeRoleId.Villain
    };

    private sealed class GuesserRoleInfoCreater
    {
        public List<GuessBehaviour.RoleInfo> Result { get; } = new List<GuessBehaviour.RoleInfo>();

        Dictionary<ExtremeRoleType, List<ExtremeRoleId>> separetedRoleId;

        private sealed class NormalExRAssignState
        {
            public bool IsJackalOn { get; set; } = false;
            public bool IsJackalForceReplaceLover { get; set; } = false;
            public bool IsQueenOn { get; set; } = false;
        };

        public void Create(bool includeNoneRole)
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
                if (role == RoleTypes.Crewmate ||
                    role == RoleTypes.Impostor ||
                    role == RoleTypes.GuardianAngel ||
                    role == RoleTypes.CrewmateGhost ||
                    role == RoleTypes.ImpostorGhost)
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
                            team = ExtremeRoleType.Crewmate;
                            break;
                        case RoleTypes.Shapeshifter:
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

            var allOption = AllOptionHolder.Instance;

            foreach (var (id, role) in ExtremeRoleManager.NormalRole)
            {
                int spawnOptSel = allOption.GetValue<int>(
                    role.GetRoleOptionId(RoleCommonOption.SpawnRate));
                int roleNum = allOption.GetValue<int>(
                    role.GetRoleOptionId(RoleCommonOption.RoleNum));

                if (spawnOptSel < 1 || roleNum <= 0)
                {
                    continue;
                }

                ExtremeRoleId exId = (ExtremeRoleId)id;
                ExtremeRoleType team = role.Team;

                // クイーンとサーヴァントとジャッカルとサイドキックはニュートラルの最後に追加する(役職のパターンがいくつかあるため)
                if (exId != ExtremeRoleId.Queen &&
                    exId != ExtremeRoleId.Jackal)
                {
                    add(exId, team);
                    separetedRoleId[team].Add(exId);
                }
                switch (exId)
                {
                    case ExtremeRoleId.Jackal:
                        assignState.IsJackalOn = true;
                        assignState.IsJackalForceReplaceLover = allOption.GetValue<bool>(
                            role.GetRoleOptionId(
                                Solo.Neutral.Jackal.JackalOption.ForceReplaceLover));
                        break;
                    case ExtremeRoleId.Queen:
                        assignState.IsQueenOn = true;
                        break;
                    case ExtremeRoleId.Hypnotist:
                        // 本来はニュートラルであるがソート用にインポスターとして突っ込む
                        add(ExtremeRoleId.Doll, ExtremeRoleType.Impostor);
                        this.separetedRoleId[ExtremeRoleType.Neutral].Add(ExtremeRoleId.Doll);
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
                    int spawnOptSel = allOption.GetValue<int>(
                        roleMng.GetRoleOptionId(RoleCommonOption.SpawnRate));
                    int roleNum = allOption.GetValue<int>(
                        roleMng.GetRoleOptionId(RoleCommonOption.RoleNum));

                    if (spawnOptSel < 1 || roleNum <= 0)
                    {
                        continue;
                    }
                    if (roleMng is FlexibleCombinationRoleManagerBase flexMng)
                    {
                        add(flexMng.BaseRole.Id, queenTeam, servantId);
                    }
                    else
                    {
                        foreach (var role in roleMng.Roles)
                        {
                            add(role.Id, queenTeam, servantId);
                        }
                    }
                }

                this.separetedRoleId[queenTeam].Add(ExtremeRoleId.Queen);
                this.separetedRoleId[queenTeam].Add(servantId);
            }
        }

        private void addExRCombRole(NormalExRAssignState assignState)
        {
            var allOption = AllOptionHolder.Instance;

            foreach (var (id, roleMng) in ExtremeRoleManager.CombRole)
            {
                int spawnOptSel = allOption.GetValue<int>(
                    roleMng.GetRoleOptionId(RoleCommonOption.SpawnRate));
                int roleNum = allOption.GetValue<int>(
                    roleMng.GetRoleOptionId(RoleCommonOption.RoleNum));

                bool multiAssign = allOption.GetValue<bool>(
                    roleMng.GetRoleOptionId(
                        CombinationRoleCommonOption.IsMultiAssign));

                if (spawnOptSel < 1 || roleNum <= 0)
                {
                    continue;
                }

                bool isNotTraitor = id != (byte)CombinationRoleType.Traitor;

                if (roleMng is FlexibleCombinationRoleManagerBase flexMng &&
                    isNotTraitor)
                {
                    ExtremeRoleType team = flexMng.BaseRole.Team;
                    ExtremeRoleId baseRoleId = flexMng.BaseRole.Id;

                    if (multiAssign)
                    {
                        if (allOption.TryGet<bool>(
                                flexMng.GetRoleOptionId(
                                    CombinationRoleCommonOption.IsAssignImposter),
                                out var option) &&
                            option.GetValue())
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
                        ExtremeRoleType team = role.Team;
                        listAdd(role.Id, team, this.separetedRoleId[team]);
                    }
                }
                else
                {
                    foreach (var role in roleMng.Roles)
                    {
                        add(role.Id, role.Team);
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


    public Guesser(
        ) : base(
            ExtremeRoleId.Guesser,
            ExtremeRoleType.Crewmate,
            ExtremeRoleId.Guesser.ToString(),
            ColorPalette.GuesserRedYellow,
            false, true, false, false,
            tab: OptionTab.Combination)
    { }

    private static void missGuess()
    {
        Player.RpcUncheckMurderPlayer(
            CachedPlayerControl.LocalPlayer.PlayerId,
            CachedPlayerControl.LocalPlayer.PlayerId,
            byte.MinValue);
        Sound.RpcPlaySound(Sound.SoundType.Kill);
    }

    public void GuessAction(GuessBehaviour.RoleInfo roleInfo, byte playerId)
    {
        ExtremeRolesPlugin.Logger.LogDebug($"TargetPlayerId:{playerId}  GuessTo:{roleInfo}");
        
        // まず弾をへらす
        this.bulletNum = this.bulletNum - 1;
        this.curGuessNum = this.curGuessNum + 1;

        var targetRole = ExtremeRoleManager.GameRole[playerId];
        
        ExtremeRoleId roleId = targetRole.Id;
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
                anotherRoleId = multiRole.AnotherRole.Id;
            }
        }
        
        if ((
                BodyGuard.IsBlockMeetingKill && 
                BodyGuard.TryGetShiledPlayerId(playerId, out byte _)
            ) || alwaysMissRole.Contains(targetRole.Id))
        {
            missGuess();
        }
        else if (
            roleInfo.Id == roleId && 
            roleInfo.AnothorId == anotherRoleId)
        {
            Player.RpcUncheckMurderPlayer(
                CachedPlayerControl.LocalPlayer.PlayerId,
                playerId, byte.MinValue);
            Sound.RpcPlaySound(Sound.SoundType.Kill);
        }
        else
        {
            missGuess();
        }
    }

    public void IntroEndSetUp()
    {
        return;
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
            if (this.uiPrefab == null)
            {
                this.uiPrefab = UnityEngine.Object.Instantiate(
                    Loader.GetUnityObjectFromResources<GameObject>(
                        Path.GusserUiResources,
                        Path.GusserUiPrefab),
                    CachedShipStatus.Instance.transform);

                this.uiPrefab.SetActive(false);
            }
            if (this.guesserUi == null)
            {
                GameObject obj = UnityEngine.Object.Instantiate(
                    this.uiPrefab, MeetingHud.Instance.transform);
                this.guesserUi = obj.GetComponent<GuesserUi>();

                GuesserRoleInfoCreater creator = new GuesserRoleInfoCreater();
                creator.Create(this.canGuessNoneRole);

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

            byte targetPlayerId = instance.TargetPlayerId;
            this.guesserUi.SetTitle(
                string.Format(
                    Translation.GetString("guesserUiTitle"),
                    GameData.Instance.GetPlayerById(
                        targetPlayerId)?.DefaultOutfit.PlayerName));
            this.guesserUi.SetInfo(
                string.Format(
                    Translation.GetString("guesserUiInfo"),
                    this.bulletNum, this.maxGuessNum));
            this.guesserUi.SetTarget(targetPlayerId);
            this.guesserUi.gameObject.SetActive(true);
        }
        return openGusserUi;
    }

    public void SetSprite(SpriteRenderer render)
    {
        render.sprite = Loader.CreateSpriteFromResources(
            Path.GuesserGuess);
        render.transform.localScale *= new Vector2(0.625f, 0.625f);
    }

    public void ResetOnMeetingEnd(GameData.PlayerInfo exiledPlayer = null)
    {
        this.guesserUi = null;
    }

    public void ResetOnMeetingStart()
    {
        this.curGuessNum = 0;
    }

    public void Update(PlayerControl rolePlayer)
    {
        if (MeetingHud.Instance)
        {
            if (this.meetingGuessText == null)
            {
                this.meetingGuessText = UnityEngine.Object.Instantiate(
                    FastDestroyableSingleton<HudManager>.Instance.TaskPanel.taskText,
                    MeetingHud.Instance.transform);
                this.meetingGuessText.alignment = TMPro.TextAlignmentOptions.BottomLeft;
                this.meetingGuessText.transform.position = Vector3.zero;
                this.meetingGuessText.transform.localPosition = new Vector3(-2.85f, 3.15f, -20f);
                this.meetingGuessText.transform.localScale *= 0.9f;
                this.meetingGuessText.color = Palette.White;
                this.meetingGuessText.gameObject.SetActive(false);
            }

            this.meetingGuessText.text = string.Format(
                Translation.GetString("guesserUiInfo"),
                this.bulletNum, this.maxGuessNum);
            meetingInfoSetActive(true);
        }
        else
        {
            meetingInfoSetActive(false);
        }
    }

    protected override void CreateSpecificOption(
        IOptionInfo parentOps)
    {
        var imposterSetting = AllOptionHolder.Instance.Get<bool>(
            GetManagerOptionId(CombinationRoleCommonOption.IsAssignImposter),
            AllOptionHolder.ValueType.Bool);
        CreateKillerOption(imposterSetting);

        CreateBoolOption(
            GuesserOption.CanCallMeeting,
            false, parentOps);
        CreateIntOption(
            GuesserOption.GuessNum,
            1, 1, GameSystem.MaxImposterNum, 1,
            parentOps,
            format: OptionUnit.Shot);
        CreateIntOption(
            GuesserOption.MaxGuessNumWhenMeeting,
            1, 1, GameSystem.MaxImposterNum, 1,
            parentOps,
            format: OptionUnit.Shot);
        var noneGuessRoleOpt = CreateBoolOption(
            GuesserOption.CanGuessNoneRole,
            false, parentOps);
        CreateSelectionOption(
            GuesserOption.GuessNoneRoleMode,
            new string[]
            {
                GuessMode.BothGuesser.ToString(),
                GuessMode.NiceGuesserOnly.ToString(),
                GuessMode.EvilGuesserOnly.ToString(),
            }, noneGuessRoleOpt);
    }

    protected override void RoleSpecificInit()
    {
        this.uiPrefab = null;
        this.guesserUi = null;
        var allOption = AllOptionHolder.Instance;

        this.CanCallMeeting = allOption.GetValue<bool>(
            GetRoleOptionId(GuesserOption.CanCallMeeting));

        bool canGuessNoneRole = allOption.GetValue<bool>(
            GetRoleOptionId(GuesserOption.CanGuessNoneRole));
        GuessMode guessMode = (GuessMode)allOption.GetValue<int>(
            GetRoleOptionId(GuesserOption.GuessNoneRoleMode));

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

        this.bulletNum = allOption.GetValue<int>(
            GetRoleOptionId(GuesserOption.GuessNum));
        this.maxGuessNum = allOption.GetValue<int>(
            GetRoleOptionId(GuesserOption.MaxGuessNumWhenMeeting));

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
