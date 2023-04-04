using System;
using System.Collections.Generic;
using System.Linq;

using HarmonyLib;
using AmongUs.GameOptions;

using ExtremeRoles.GameMode;
using ExtremeRoles.GhostRoles;
using ExtremeRoles.Helper;
using ExtremeRoles.Module.RoleAssign;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Extension.State;
using ExtremeRoles.Performance;
using ExtremeRoles.Roles.Solo.Host;

namespace ExtremeRoles.Patches.Manager;


[HarmonyPatch(typeof(RoleManager), nameof(RoleManager.SelectRoles))]
public static class RoleManagerSelectRolesPatch
{
    public static void Prefix()
    {
        if (!ExtremeGameModeManager.Instance.RoleSelector.IsCanUseAndEnableXion()) { return; }

        PlayerControl loaclPlayer = PlayerControl.LocalPlayer;

        PlayerRoleAssignData assignData = PlayerRoleAssignData.Instance;

            assignData.AddAssignData(
                new PlayerToSingleRoleAssignData(
                    loaclPlayer.PlayerId,
                    (int)ExtremeRoleId.Xion,
                    assignData.GetControlId()));
            assignData.RemvePlayer(loaclPlayer);

        if (Xion.IsAllPlyerDummy())
        {
            // ダミープレイヤーは役職がアサインされてないので無理やりアサインする
            List<PlayerControl> allPlayer = assignData.NeedRoleAssignPlayer;

            var gameOption = GameOptionsManager.Instance;
            var currentOption = gameOption.CurrentGameOptions;

            int adjustedNumImpostors = currentOption.GetAdjustedNumImpostors(allPlayer.Count);

            var il2CppListPlayer = new Il2CppSystem.Collections.Generic.List<GameData.PlayerInfo>();

            foreach (PlayerControl player in allPlayer)
            {
                il2CppListPlayer.Add(player.Data);
            }

            GameManager.Instance.LogicRoleSelection.AssignRolesForTeam(
                il2CppListPlayer, currentOption, RoleTeamTypes.Impostor,
                adjustedNumImpostors,
                new Il2CppSystem.Nullable<RoleTypes>()
                { 
                    value = RoleTypes.Impostor,
                    has_value = true
                });
            GameManager.Instance.LogicRoleSelection.AssignRolesForTeam(
                il2CppListPlayer, currentOption, RoleTeamTypes.Crewmate,
                int.MaxValue,
                new Il2CppSystem.Nullable<RoleTypes>()
                {
                    value = RoleTypes.Crewmate,
                    has_value = true
                });

            // アサイン済みにする
            foreach (PlayerControl player in allPlayer)
            {
                player.roleAssigned = true;
            }
        }

        loaclPlayer.RpcSetRole(RoleTypes.Crewmate);
        loaclPlayer.Data.IsDead = true;
    }
    public static void Postfix()
    {
        uint netId = PlayerControl.LocalPlayer.NetId;

        RPCOperator.Call(netId, RPCOperator.Command.Initialize);
        RPCOperator.Initialize();

        // スポーンデータ作成
        RoleSpawnDataManager spawnData = new RoleSpawnDataManager();
        GhostRoleSpawnDataManager.Instance.Create(spawnData.UseGhostCombRole);

        PlayerRoleAssignData assignData = PlayerRoleAssignData.Instance;

        addCombinationExtremeRoleAssignData(
            ref spawnData, ref assignData);
        addSingleExtremeRoleAssignData(
            ref spawnData, ref assignData);
        addNotAssignPlayerToVanillaRoleAssign(ref assignData);
    }

    private static void addNotAssignPlayerToVanillaRoleAssign(
        ref PlayerRoleAssignData assignData)
    {
        foreach (PlayerControl player in assignData.NeedRoleAssignPlayer)
        {
            var roleId = player.Data.Role.Role;
            Logging.Debug(
                        $"------------------- AssignToPlayer:{player.Data.PlayerName} -------------------");
            Logging.Debug($"---AssignRole:{roleId}---");
            assignData.AddAssignData(new PlayerToSingleRoleAssignData(
                player.PlayerId, (byte)roleId, assignData.GetControlId()));
        }
    }

    private static void addCombinationExtremeRoleAssignData(
        ref RoleSpawnDataManager spawnData,
        ref PlayerRoleAssignData assignData)
    {
        Logging.Debug(
            $"----------------------------- CombinationRoleAssign Start!! -----------------------------");

        if (!spawnData.CurrentCombRoleSpawnData.Any()) { return; }

        List<CombinationRoleAssignData> combRoleListData = createCombinationRoleListData(
            assignData, ref spawnData);
        var shuffledRoleListData = combRoleListData.OrderBy(x => RandomGenerator.Instance.Next());
        assignData.Shuffle();

        List<PlayerControl> anotherRoleAssignPlayer = new List<PlayerControl>();

        foreach (var roleListData in shuffledRoleListData)
        {
            foreach (var role in roleListData.RoleList)
            {
                PlayerControl removePlayer = null;

                foreach (PlayerControl player in assignData.NeedRoleAssignPlayer)
                {
                    Logging.Debug(
                        $"------------------- AssignToPlayer:{player.Data.PlayerName} -------------------");
                    Logging.Debug($"---AssignRole:{role.Id}---");

                    bool assign = isCanMulitAssignRoleToPlayer(role, player);

                    Logging.Debug($"AssignResult:{assign}");

                    if (!assign)
                    {
                        Logging.Debug($"Assign missing!!");
                        continue;
                    }
                    if (role.CanHasAnotherRole)
                    {
                        anotherRoleAssignPlayer.Add(player);
                    }
                    removePlayer = player;

                    assignData.AddCombRoleAssignData(
                        new PlayerToCombRoleAssignData(
                            player.PlayerId, (int)role.Id,
                            roleListData.CombType,
                            (byte)roleListData.GameControlId,
                            (byte)player.Data.Role.Role),
                        role.Team);

                    Logging.Debug($"------------------- Assign End -------------------");

                    break;
                }

                if (removePlayer != null)
                {
                    assignData.RemvePlayer(removePlayer);
                }
            }
        }

        foreach (PlayerControl player in anotherRoleAssignPlayer)
        {
            if (player != null)
            {
                Logging.Debug($"------------------- AditionalPlayer -------------------");
                assignData.AddPlayer(player);
            }
        }
        Logging.Debug(
            $"----------------------------- CombinationRoleAssign End!! -----------------------------");
    }

    private static void addSingleExtremeRoleAssignData(
        ref RoleSpawnDataManager spawnData,
        ref PlayerRoleAssignData assignData)
    {
        Logging.Debug(
            $"----------------------------- SingleRoleAssign Start!! -----------------------------");
        addImpostorSingleExtremeRoleAssignData(
            ref spawnData, ref assignData);
        addNeutralSingleExtremeRoleAssignData(
            ref spawnData, ref assignData);
        addCrewmateSingleExtremeRoleAssignData(
            ref spawnData, ref assignData);
        Logging.Debug(
            $"----------------------------- SingleRoleAssign End!! -----------------------------");
    }

    private static void addImpostorSingleExtremeRoleAssignData(
        ref RoleSpawnDataManager spawnData,
        ref PlayerRoleAssignData assignData)
    {
        addSingleExtremeRoleAssignDataFromTeamAndPlayer(
            ref spawnData, ref assignData,
            ExtremeRoleType.Impostor,
            assignData.GetCanImpostorAssignPlayer(),
            new HashSet<RoleTypes> { RoleTypes.Shapeshifter });
    }

    private static void addNeutralSingleExtremeRoleAssignData(
        ref RoleSpawnDataManager spawnData,
        ref PlayerRoleAssignData assignData)
    {
        List<PlayerControl> neutralAssignTargetPlayer = new List<PlayerControl>();

        foreach (PlayerControl player in assignData.GetCanCrewmateAssignPlayer())
        {

            RoleTypes vanillaRoleId = player.Data.Role.Role;

            if ((
                    assignData.TryGetCombRoleAssign(player.PlayerId, out ExtremeRoleType team) &&
                    team != ExtremeRoleType.Neutral
                )
                ||
                (
                    !ExtremeGameModeManager.Instance.RoleSelector.IsVanillaRoleToMultiAssign &&
                    vanillaRoleId != RoleTypes.Crewmate
                ))
            {
                continue;
            }
            neutralAssignTargetPlayer.Add(player);
        }

        int assignNum = Math.Clamp(
            spawnData.MaxRoleNum[ExtremeRoleType.Neutral],
            0, Math.Min(
                neutralAssignTargetPlayer.Count,
                spawnData.CurrentSingleRoleUseNum[ExtremeRoleType.Neutral]));

        neutralAssignTargetPlayer = neutralAssignTargetPlayer.OrderBy(
            x => RandomGenerator.Instance.Next()).Take(assignNum).ToList();

        addSingleExtremeRoleAssignDataFromTeamAndPlayer(
            ref spawnData, ref assignData,
            ExtremeRoleType.Neutral,
            neutralAssignTargetPlayer,
            new HashSet<RoleTypes> { RoleTypes.Engineer, RoleTypes.Scientist });
    }

    private static void addCrewmateSingleExtremeRoleAssignData(
        ref RoleSpawnDataManager spawnData,
        ref PlayerRoleAssignData assignData)
    {
        addSingleExtremeRoleAssignDataFromTeamAndPlayer(
            ref spawnData, ref assignData,
            ExtremeRoleType.Crewmate,
            assignData.GetCanCrewmateAssignPlayer(),
            new HashSet<RoleTypes> { RoleTypes.Engineer, RoleTypes.Scientist });
    }

    private static void addSingleExtremeRoleAssignDataFromTeamAndPlayer(
        ref RoleSpawnDataManager spawnData,
        ref PlayerRoleAssignData assignData,
        ExtremeRoleType team,
        List<PlayerControl> targetPlayer,
        HashSet<RoleTypes> vanilaTeams)
    {

        Dictionary<int, SingleRoleSpawnData> teamSpawnData = spawnData.CurrentSingleRoleSpawnData[team];

        if (!targetPlayer.Any() || !targetPlayer.Any()) { return; }

        List<int> spawnCheckRoleId = createSingleRoleIdData(teamSpawnData);

        if (!spawnCheckRoleId.Any()) { return; }

        var shuffledSpawnCheckRoleId = spawnCheckRoleId.OrderBy(x => RandomGenerator.Instance.Next()).ToList();
        var shuffledTargetPlayer = targetPlayer.OrderBy(x => RandomGenerator.Instance.Next());

        foreach (PlayerControl player in shuffledTargetPlayer)
        {
            Logging.Debug(
                $"-------------------AssignToPlayer:{player.Data.PlayerName}-------------------");
            PlayerControl removePlayer = null;

            RoleTypes vanillaRoleId = player.Data.Role.Role;

            if (vanilaTeams.Contains(vanillaRoleId))
            {
                // マルチアサインでコンビ役職にアサインされてないプレイヤーは追加でアサインが必要
                removePlayer =
                    ExtremeGameModeManager.Instance.RoleSelector.IsVanillaRoleToMultiAssign
                    ||
                    (
                        assignData.TryGetCombRoleAssign(player.PlayerId, out ExtremeRoleType combTeam) &&
                        combTeam == team
                    )
                    ? null : player;

                assignData.AddAssignData(
                    new PlayerToSingleRoleAssignData(
                        player.PlayerId, (int)vanillaRoleId,
                        assignData.GetControlId()));
                Logging.Debug($"---AssignRole:{vanillaRoleId}---");
            }

            if (spawnData.IsCanSpawnTeam(team) &&
                shuffledSpawnCheckRoleId.Any() &&
                removePlayer == null)
            {
                removePlayer = player;
                int intedRoleId = shuffledSpawnCheckRoleId[0];
                shuffledSpawnCheckRoleId.RemoveAt(0);

                Logging.Debug($"---AssignRole:{intedRoleId}---");

                spawnData.ReduceSpawnLimit(team);
                assignData.AddAssignData(
                    new PlayerToSingleRoleAssignData(
                        player.PlayerId, intedRoleId, assignData.GetControlId()));
            }

            Logging.Debug($"-------------------AssignEnd-------------------");
            if (removePlayer != null)
            {
                assignData.RemvePlayer(removePlayer);
            }
        }
    }

    private static List<CombinationRoleAssignData> createCombinationRoleListData(
        PlayerRoleAssignData assignData,
        ref RoleSpawnDataManager spawnData)
    {
        List<CombinationRoleAssignData> roleListData = new List<CombinationRoleAssignData>();

        int curImpNum = 0;
        int curCrewNum = 0;
        int maxImpNum = GameOptionsManager.Instance.CurrentGameOptions.GetInt(
            Int32OptionNames.NumImpostors);
        
        NotAssignPlayerData notAssignPlayer = new NotAssignPlayerData();

        foreach (var (combType, combSpawnData) in spawnData.CurrentCombRoleSpawnData)
        {
            var roleManager = combSpawnData.Role;

            for (int i = 0; i < combSpawnData.SpawnSetNum; i++)
            {
                roleManager.AssignSetUpInit(curImpNum);
                bool isSpawn = combSpawnData.IsSpawn();
                int reduceCrewmateRole = 0;
                int reduceImpostorRole = 0;
                int reduceNeutralRole = 0;

                foreach (var role in roleManager.Roles)
                {
                    switch (role.Team)
                    {
                        case ExtremeRoleType.Crewmate:
                            ++reduceCrewmateRole;
                            break;
                        case ExtremeRoleType.Impostor:
                            ++reduceImpostorRole;
                            break;
                        case ExtremeRoleType.Neutral:
                            ++reduceNeutralRole;
                            break;
                        default:
                            break;
                    }
                    if (roleManager is GhostAndAliveCombinationRoleManagerBase)
                    {
                        isSpawn = !GhostRoleSpawnDataManager.Instance.IsGlobalSpawnLimit(role.Team);
                    }
                }

                isSpawn = isSpawn && isCombinationLimit(
                    notAssignPlayer, spawnData, maxImpNum,
                    curCrewNum, curImpNum,
                    reduceCrewmateRole,
                    reduceImpostorRole,
                    reduceNeutralRole,
                    combSpawnData.IsMultiAssign);

                if (!isSpawn) { continue; }

                spawnData.ReduceSpawnLimit(ExtremeRoleType.Crewmate, reduceCrewmateRole);
                spawnData.ReduceSpawnLimit(ExtremeRoleType.Impostor, reduceImpostorRole);
                spawnData.ReduceSpawnLimit(ExtremeRoleType.Neutral, reduceNeutralRole);

                curImpNum = curImpNum + reduceImpostorRole;
                curCrewNum = curCrewNum + (reduceCrewmateRole + reduceNeutralRole);

                var spawnRoles = new List<MultiAssignRoleBase>();
                foreach (var role in roleManager.Roles)
                {
                    spawnRoles.Add((MultiAssignRoleBase)role.Clone());
                }

                notAssignPlayer.ReduceImpostorAssignNum(reduceImpostorRole);
                roleListData.Add(
                    new CombinationRoleAssignData(
                        assignData.GetControlId(),
                        combType, spawnRoles));
            }
        }

        return roleListData;
    }

    private static List<int> createSingleRoleIdData(
        Dictionary<int, SingleRoleSpawnData> spawnData)
    {
        List<int> result = new List<int>();

        foreach (var (intedRoleId, data) in spawnData)
        {
            for (int i = 0; i < data.SpawnSetNum; ++i)
            {
                if (!data.IsSpawn()) { continue; }

                result.Add(intedRoleId);
            }
        }

        return result;
    }

    private static bool isCombinationLimit(
        NotAssignPlayerData notAssignPlayer,
        RoleSpawnDataManager spawnData,
        int maxImpNum,
        int curCrewUseNum,
        int curImpUseNum,
        int reduceCrewmateRoleNum,
        int reduceImpostorRoleNum,
        int reduceNeutralRoleNum,
        bool isMultiAssign)
    {
        int crewNotAssignPlayerNum = isMultiAssign ?
            notAssignPlayer.CrewmateMultiAssignPlayerNum :
            notAssignPlayer.CrewmateSingleAssignPlayerNum;
        int impNotAssignPlayerNum = isMultiAssign ?
            notAssignPlayer.ImpostorMultiAssignPlayerNum :
            notAssignPlayer.ImpostorSingleAssignPlayerNum;

        int totalReduceCrewmateNum = reduceCrewmateRoleNum + reduceNeutralRoleNum;

        bool isLimitCrewAssignNum = crewNotAssignPlayerNum >= totalReduceCrewmateNum;
        bool isLimitImpAssignNum = impNotAssignPlayerNum >= reduceImpostorRoleNum;

        return
            // まずはアサインの上限チェック
            (
                curCrewUseNum + totalReduceCrewmateNum <= crewNotAssignPlayerNum &&
                curImpUseNum + reduceImpostorRoleNum <= maxImpNum
            )
            // クルーのスポーン上限チェック
            &&
            (
                spawnData.IsCanSpawnTeam(ExtremeRoleType.Crewmate, reduceCrewmateRoleNum) &&
                isLimitCrewAssignNum
            )
            // ニュートラルのスポーン上限チェック
            &&
            (
                spawnData.IsCanSpawnTeam(ExtremeRoleType.Neutral, reduceNeutralRoleNum) &&
                isLimitCrewAssignNum
            )
            // インポスターのスポーン上限チェック
            &&
            (
                spawnData.IsCanSpawnTeam(ExtremeRoleType.Impostor, reduceImpostorRoleNum) &&
                isLimitImpAssignNum
            );
    }

    private static bool isCanMulitAssignRoleToPlayer(
        MultiAssignRoleBase role,
        PlayerControl player)
    {

        RoleTypes roleType = player.Data.Role.Role;

        bool hasAnotherRole = role.CanHasAnotherRole;
        bool isImpostor = role.IsImpostor();
        bool isAssignToCrewmate = role.IsCrewmate() || role.IsNeutral();

        return
            (
                roleType == RoleTypes.Crewmate && isAssignToCrewmate
            )
            ||
            (
                roleType == RoleTypes.Impostor && isImpostor
            )
            ||
            (
                (
                    roleType == RoleTypes.Engineer ||
                    roleType == RoleTypes.Scientist
                )
                && hasAnotherRole && isAssignToCrewmate
            )
            ||
            (
                roleType == RoleTypes.Shapeshifter &&
                hasAnotherRole && isAssignToCrewmate
            );
    }
}

[HarmonyPatch(typeof(RoleManager), nameof(RoleManager.AssignRoleOnDeath))]
public static class RoleManagerAssignRoleOnDeathPatch
{
    public static bool Prefix([HarmonyArgument(0)] PlayerControl player)
    {
        if (ExtremeRoleManager.GameRole.Count == 0) { return true; }
        if (!RoleAssignState.Instance.IsRoleSetUpEnd) { return true; }

        var role = ExtremeRoleManager.GameRole[player.PlayerId];
        if (!role.IsAssignGhostRole())
        {
            var roleBehavior = player.Data.Role;

            if (!RoleManager.IsGhostRole(roleBehavior.Role))
            {
                player.RpcSetRole(roleBehavior.DefaultGhostRole);
            }
            return false;
        }
        if (GhostRoleSpawnDataManager.Instance.IsCombRole(role.Id)) { return false; }

        return true;
    }

    public static void Postfix([HarmonyArgument(0)] PlayerControl player)
    {
        if (ExtremeRoleManager.GameRole.Count == 0) { return; }
        if (!RoleAssignState.Instance.IsRoleSetUpEnd ||
            !ExtremeRoleManager.GameRole[player.PlayerId].IsAssignGhostRole()) { return; }
        
        ExtremeGhostRoleManager.AssignGhostRoleToPlayer(player);
    }
}

[HarmonyPatch(typeof(RoleManager), nameof(RoleManager.TryAssignSpecialGhostRoles))]
public static class RoleManagerTryAssignRoleOnDeathPatch
{
    // クルーの幽霊役職の処理（インポスターの時はここに来ない）
    public static bool Prefix([HarmonyArgument(0)] PlayerControl player)
    {
        if (ExtremeRoleManager.GameRole.Count == 0) { return true; }
        if (!RoleAssignState.Instance.IsRoleSetUpEnd) { return true; }
        // バニラ幽霊クルー役職にニュートラルがアサインされる時はTrueを返す
        if (ExtremeGameModeManager.Instance.ShipOption.IsAssignNeutralToVanillaCrewGhostRole)
        {
            return true;
        }

        var role = ExtremeRoleManager.GameRole[player.PlayerId];

        if (role.IsNeutral()) { return false; }

        // デフォルトのメソッドではニュートラルもクルー陣営の死亡者数にカウントされてアサインされなくなるため
        RoleTypes roleTypes = RoleTypes.GuardianAngel;

        int num = CachedPlayerControl.AllPlayerControls.Count(
            (CachedPlayerControl pc) => 
                pc.Data.IsDead && 
                !pc.Data.Role.IsImpostor &&
                ExtremeRoleManager.GameRole[pc.PlayerId].IsCrewmate());

        IRoleOptionsCollection roleOptions = GameOptionsManager.Instance.CurrentGameOptions.RoleOptions;
        if (AmongUsClient.Instance.NetworkMode == NetworkModes.FreePlay)
        {
            player.RpcSetRole(roleTypes);
            return false;
        }
        if (num > roleOptions.GetNumPerGame(roleTypes))
        {
            return false;
        }
        
        int chancePerGame = roleOptions.GetChancePerGame(roleTypes);
        
        if (HashRandom.Next(101) < chancePerGame)
        {
            player.RpcSetRole(roleTypes);
        }

        return false;
    }
}
