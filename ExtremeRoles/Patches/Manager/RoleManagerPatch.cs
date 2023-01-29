using System;
using System.Collections.Generic;
using System.Linq;

using HarmonyLib;
using AmongUs.GameOptions;

using ExtremeRoles.GameMode;
using ExtremeRoles.GhostRoles;
using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Module.RoleAssign;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Extension.State;
using ExtremeRoles.Performance;
using ExtremeRoles.Performance.Il2Cpp;

namespace ExtremeRoles.Patches.Manager
{
    internal static class NewRoleManagerFunction
    {
        private struct CombinationRoleListData
        {
            public byte CombType { get; private set; }
            public List<MultiAssignRoleBase> RoleList { get; private set; }
            public int GameControlId { get; private set; }

            public CombinationRoleListData(
                int controlId, byte combType,
                List<MultiAssignRoleBase> roleList)
            {
                CombType = combType;
                RoleList = roleList;
                GameControlId = controlId;
            }
        }

        public static void Prefix()
        {
            if (OptionHolder.AllOption[(int)OptionHolder.CommonOptionKey.UseXion].GetValue() &&
                ExtremeGameModeManager.Instance.RoleSelector.CanUseXion)
            {
                PlayerControl loaclPlayer = PlayerControl.LocalPlayer;
                
                PlayerRoleAssignData assignData = PlayerRoleAssignData.Instance;
                assignData.AddAssignData(
                    new PlayerToSingleRoleAssignData(
                        loaclPlayer.PlayerId,
                        (int)ExtremeRoleId.Xion));
                assignData.RemvePlayer(loaclPlayer);

                loaclPlayer.RpcSetRole(RoleTypes.Crewmate);
                loaclPlayer.Data.IsDead = true;
            }
        }

        public static void Postfix()
        {
            uint netId = PlayerControl.LocalPlayer.NetId;

            RPCOperator.Call(netId, RPCOperator.Command.Initialize);
            RPCOperator.Initialize();

            RoleSpawnDataManager spawnData = new RoleSpawnDataManager();
            PlayerRoleAssignData assignData = PlayerRoleAssignData.Instance;

            addCombinationExtremeRoleAssignData(
                ref spawnData, ref assignData);
        }

        private static void addCombinationExtremeRoleAssignData(
            ref RoleSpawnDataManager spawnData,
            ref PlayerRoleAssignData assignData)
        {
            if (!spawnData.CurrentCombRoleSpawnData.Any()) { return; }

            List<CombinationRoleListData> combRoleListData = createCombinationRoleListData(
                ref spawnData);
            var shuffleRoleListData = combRoleListData.OrderBy(x => RandomGenerator.Instance.Next());
            assignData.Shuffle();

            List<PlayerControl> anotherRoleAssignPlayer = new List<PlayerControl>();

            foreach (var roleListData in combRoleListData)
            {
                foreach (var role in roleListData.RoleList)
                {
                    PlayerControl removePlayer = null;

                    foreach (PlayerControl player in assignData.AllNotAssignPlayer)
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

                        assignData.AddAssignData(
                            new PlayerToCombRoleAssignData(
                                player.PlayerId, (int)role.Id,
                                roleListData.CombType,
                                (byte)roleListData.GameControlId,
                                (byte)player.Data.Role.Role));

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
                    assignData.AddPlayer(player);
                }
            }
        }

        private static List<CombinationRoleListData> createCombinationRoleListData(
            ref RoleSpawnDataManager spawnData)
        {
            List<CombinationRoleListData> roleListData = new List<CombinationRoleListData>();

            int curImpNum = 0;
            int curCrewNum = 0;
            int maxImpNum = GameOptionsManager.Instance.CurrentGameOptions.GetInt(
                Int32OptionNames.NumImpostors);
            int gameControlId = 0;

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
                            isSpawn = !ExtremeGhostRoleManager.IsGlobalSpawnLimit(role.Team);
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
                    spawnData.ReduceSpawnLimit(ExtremeRoleType.Neutral , reduceNeutralRole );

                    curImpNum  = curImpNum  + reduceImpostorRole;
                    curCrewNum = curCrewNum + (reduceCrewmateRole + reduceNeutralRole);

                    var spawnRoles = new List<MultiAssignRoleBase>();
                    foreach (var role in roleManager.Roles)
                    {
                        spawnRoles.Add((MultiAssignRoleBase)role.Clone());
                    }

                    notAssignPlayer.ReduceImpostorAssignNum(reduceImpostorRole);
                    roleListData.Add(
                        new CombinationRoleListData(
                            gameControlId, combType, spawnRoles));
                    ++gameControlId;
                }
            }

            return roleListData;
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
                    curImpUseNum  + reduceImpostorRoleNum  <= maxImpNum
                ) 
                // クルーのスポーン上限チェック
                &&
                (
                    spawnData.IsSpawnLimitTeam(ExtremeRoleType.Crewmate, reduceCrewmateRoleNum) &&
                    isLimitCrewAssignNum
                )
                // ニュートラルのスポーン上限チェック
                &&
                (
                    spawnData.IsSpawnLimitTeam(ExtremeRoleType.Neutral, reduceNeutralRoleNum) &&
                    isLimitCrewAssignNum
                )
                // インポスターのスポーン上限チェック
                &&
                (
                    spawnData.IsSpawnLimitTeam(ExtremeRoleType.Impostor, reduceImpostorRoleNum) &&
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

    [HarmonyPatch(typeof(RoleManager), nameof(RoleManager.SelectRoles))]
    public static class RoleManagerSelectRolesPatch
    {
        private static List<IAssignedPlayer> roleList = new List<IAssignedPlayer>();
        private static bool useXion = false;

        public static void Prefix()
        {
            roleList.Clear();
            useXion = OptionHolder.AllOption[(int)OptionHolder.CommonOptionKey.UseXion].GetValue();
            
            if (useXion && ExtremeGameModeManager.Instance.RoleSelector.CanUseXion)
            {
                PlayerControl loaclPlayer = PlayerControl.LocalPlayer;
                roleList.Add(new AssignedPlayerToSingleRoleData(
                    loaclPlayer.PlayerId, (int)ExtremeRoleId.Xion));
                loaclPlayer.RpcSetRole(RoleTypes.Crewmate);
                loaclPlayer.Data.IsDead = true;
            }
        }
        public static void Postfix()
        {

            uint netId = PlayerControl.LocalPlayer.NetId;

            RPCOperator.Call(netId, RPCOperator.Command.Initialize);
            RPCOperator.Initialize();

            PlayerControl[] playeres = PlayerControl.AllPlayerControls.ToArray();

            var playerIndexList = Enumerable.Range(0, playeres.Count()).ToList();

            if (useXion && ExtremeGameModeManager.Instance.RoleSelector.CanUseXion)
            {
                playerIndexList.RemoveAll(i => playeres[i].PlayerId == PlayerControl.LocalPlayer.PlayerId);
            }

            RoleAssignmentData extremeRolesData = createRoleData();

            List<IAssignedPlayer> assignedPlayerData = roleList;
            Dictionary<byte, ExtremeRoleType> combRoleAssignedPlayerId = new Dictionary<byte, ExtremeRoleType>();

            createCombinationExtremeRoleAssign(
                ref extremeRolesData,
                ref playerIndexList,
                ref assignedPlayerData,
                ref combRoleAssignedPlayerId);
            createNormalExtremeRoleAssign(
                ref extremeRolesData,
                ref playerIndexList,
                ref assignedPlayerData,
                combRoleAssignedPlayerId);
            roleList = assignedPlayerData;
        }

        public static void AllPlayerAssignToExRole()
        {
            using (var caller = RPCOperator.CreateCaller(
                PlayerControl.LocalPlayer.NetId,
                RPCOperator.Command.SetRoleToAllPlayer))
            {
                caller.WritePackedInt(roleList.Count); // 何個あるか

                foreach (IAssignedPlayer data in roleList)
                {
                    caller.WriteByte(data.PlayerId); // PlayerId
                    caller.WriteByte(data.RoleType); // RoleType : single or comb
                    caller.WritePackedInt(data.RoleId); // RoleId

                    if (data.RoleType == (byte)IAssignedPlayer.ExRoleType.Comb)
                    {
                        var combData = (AssignedPlayerToCombRoleData)data;
                        caller.WriteByte(combData.CombTypeId); // combTypeId
                        caller.WriteByte(combData.GameContId); // byted GameContId
                        caller.WriteByte(combData.AmongUsRoleId); // byted AmongUsVanillaRoleId
                    }
                }
            }
            RPCOperator.SetRoleToAllPlayer(roleList);
            RoleAssignState.Instance.SwitchRoleAssignToEnd();
            roleList.Clear();
        }

        private static bool checkLimitRoleSpawnNum(
            SingleRoleBase role,
            ref RoleAssignmentData extremeRolesData)
        {

            bool result;

            switch (role.Team)
            {
                case ExtremeRoleType.Crewmate:
                    result = ((extremeRolesData.CrewmateRoles - 1) >= 0);
                    break;
                case ExtremeRoleType.Neutral:
                    result = ((extremeRolesData.NeutralRoles - 1) >= 0);
                    break;
                case ExtremeRoleType.Impostor:
                    result = ((extremeRolesData.ImpostorRoles - 1) >= 0);
                    break;
                default:
                    result = false;
                    break;
            }

            return result;
        }

        private static int computePercentage(Module.IOption self)
            => (int)decimal.Multiply(
                self.GetValue(), self.ValueCount);

        private static void createCombinationExtremeRoleAssign(
            ref RoleAssignmentData extremeRolesData,
            ref List<int> playerIndexList,
            ref List<IAssignedPlayer> assinedPlayer,
            ref Dictionary<byte, ExtremeRoleType> assinedPlayerId)
        {

            Logging.Debug($"NotAssignPlayerNum:{playerIndexList.Count}");

            if (extremeRolesData.CombinationRole.Count == 0) { return; }

            List<(List<(byte, MultiAssignRoleBase)>, int)> assignMultiAssignRole = getMultiAssignedRoles(
                ref extremeRolesData);

            List<int> needAnotherRoleAssigns = new List<int>();

            PlayerControl player = PlayerControl.LocalPlayer;

            foreach (var (roles, id) in assignMultiAssignRole)
            {
                foreach (var (combType, role) in roles)
                {
                    bool assign = false;
                    List<int> tempList = new List<int>(
                        playerIndexList.OrderBy(item => RandomGenerator.Instance.Next()).ToList());
                    foreach (int playerIndex in tempList)
                    {
                        player = PlayerControl.AllPlayerControls[playerIndex];

                        Logging.Debug(
                            $"-------------------AssignToPlayer:{player.Data.PlayerName}-------------------");
                        Logging.Debug($"---AssignRole:{role.Id}---");
                        
                        assign = isAssignedToMultiRole(role, player);

                        Logging.Debug($"AssignResult:{assign}");

                        if (!assign) { continue; }

                        if (role.CanHasAnotherRole)
                        {
                            needAnotherRoleAssigns.Add(playerIndex);
                        }
                        playerIndexList.Remove(playerIndex);

                        assinedPlayerId.Add(player.PlayerId, role.Team);
                        assinedPlayer.Add(new AssignedPlayerToCombRoleData(
                            player.PlayerId, (int)role.Id,
                            combType, (byte)id,
                            (byte)player.Data.Role.Role));

                        Logging.Debug($"-------------------AssignEnd-------------------");
                        
                        break;
                    }
                }
            }

            if (needAnotherRoleAssigns.Count != 0)
            {
                playerIndexList.AddRange(needAnotherRoleAssigns);
            }
        }

        private static Tuple<int, int> getNotAssignedPlayer(bool multiAssign)
        {

            int crewNum = 0;
            int impNum = 0;

            foreach (PlayerControl player in 
                PlayerControl.AllPlayerControls.GetFastEnumerator())
            {
                if (multiAssign)
                {
                    switch (player.Data.Role.Role)
                    {
                        case RoleTypes.Crewmate:
                        case RoleTypes.Scientist:
                        case RoleTypes.Engineer:
                            ++crewNum;
                            break;
                        case RoleTypes.Impostor:
                        case RoleTypes.Shapeshifter:
                            ++impNum;
                            break;
                        default:
                            break;
                    }
                }
                else
                {
                    switch (player.Data.Role.Role)
                    {
                        case RoleTypes.Crewmate:
                            ++crewNum;
                            break;
                        case RoleTypes.Impostor:
                            ++impNum;
                            break;
                        default:
                            break;
                    }
                }
            }

            return Tuple.Create(crewNum, impNum);
        }

        private static List<(List<(byte, MultiAssignRoleBase)>, int)> getMultiAssignedRoles(
            ref RoleAssignmentData extremeRolesData)
        {
            List<(List<(byte, MultiAssignRoleBase)>, int)> assignRoles = new List<(List<(byte, MultiAssignRoleBase)>, int)>();

            var roleDataLoop = extremeRolesData.CombinationRole.OrderBy(
                item => RandomGenerator.Instance.Next()).ToList();

            int gameControlId = 0;
            int curImpNum = 0;
            int curCrewNum = 0;
            int maxImpNum = GameOptionsManager.Instance.CurrentGameOptions.GetInt(
                Int32OptionNames.NumImpostors);
            foreach (var oneRole in roleDataLoop)
            {
                var ((combType, roleManager), (num, spawnRate, isMultiAssign)) = oneRole;
                var (crewNum, impNum) = getNotAssignedPlayer(isMultiAssign);

                for (int i = 0; i < num; i++)
                {
                    roleManager.AssignSetUpInit(curImpNum);
                    bool isSpawn = isRoleSpawn(num, spawnRate);
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
                            isSpawn = !ExtremeGhostRoleManager.IsGlobalSpawnLimit(role.Team);
                        }
                    }

                    isSpawn = (
                        isSpawn &&
                        (
                            curCrewNum + (reduceCrewmateRole + reduceNeutralRole) <= crewNum &&
                            curImpNum + reduceImpostorRole <= maxImpNum
                        ) &&
                        (
                            (extremeRolesData.CrewmateRoles - reduceCrewmateRole >= 0) && 
                            crewNum >= reduceCrewmateRole + reduceNeutralRole
                        ) &&
                        (
                            (extremeRolesData.NeutralRoles - reduceNeutralRole >= 0) && 
                            crewNum >= reduceCrewmateRole + reduceNeutralRole
                        ) &&
                        (
                            (extremeRolesData.ImpostorRoles - reduceImpostorRole >= 0) && 
                            impNum >= reduceImpostorRole
                        )
                    );


                    // Logging.Debug($"Role:{oneRole}   isSpawn?:{isSpawn}");
                    if (!isSpawn) { continue; }

                    extremeRolesData.CrewmateRoles = extremeRolesData.CrewmateRoles - reduceCrewmateRole;
                    extremeRolesData.NeutralRoles = extremeRolesData.NeutralRoles - reduceNeutralRole;
                    extremeRolesData.ImpostorRoles = extremeRolesData.ImpostorRoles - reduceImpostorRole;

                    curImpNum = curImpNum + reduceImpostorRole;
                    curCrewNum = curCrewNum + (reduceCrewmateRole + reduceNeutralRole);

                    var spawnRoles = new List<(byte, MultiAssignRoleBase)>();
                    foreach (var role in roleManager.Roles)
                    {
                        if (role.IsImpostor())
                        {
                            ++impNum;
                        }
                        spawnRoles.Add(
                            (combType, (MultiAssignRoleBase)role.Clone()));
                    }
                    assignRoles.Add((spawnRoles, gameControlId));
                    ++gameControlId;
                }
            }

            return assignRoles;
        }

        private static bool isAssignedToMultiRole(
            MultiAssignRoleBase role,
            PlayerControl player)
        {
            switch (player.Data.Role.Role)
            {
                case RoleTypes.Impostor:
                    if (role.IsImpostor())
                    {
                        return true;
                    }
                    break;
                case RoleTypes.Crewmate:
                    if ((role.IsCrewmate() || role.IsNeutral()) &&
                         role.Team != ExtremeRoleType.Null)
                    {
                        return true;
                    }
                    break;
                case RoleTypes.Shapeshifter:
                    if (role.IsImpostor() && role.CanHasAnotherRole)
                    {
                        return true;
                    }
                    break;
                case RoleTypes.Engineer:
                case RoleTypes.Scientist:
                    if ((role.IsCrewmate() || role.IsNeutral()) &&
                        role.CanHasAnotherRole &&
                        role.Team != ExtremeRoleType.Null)
                    {
                        return true;
                    }
                    break;
                default:
                    return false;
            }
            return false;
        }

        private static bool isRoleSpawn(
            int roleNum, int spawnRate)
        {
            if (roleNum <= 0) { return false; }
            if (spawnRate < UnityEngine.Random.RandomRange(0, 110)) { return false; }

            return true;
        }

        private static void createNormalExtremeRoleAssign(
            ref RoleAssignmentData extremeRolesData,
            ref List<int> playerIndexList,
            ref List<IAssignedPlayer> assignedPlayer,
            Dictionary<byte, ExtremeRoleType> combRoleAssignedPlayerId)
        {

            List<SingleRoleBase> shuffleRolesForImpostor = extremeRolesData.RolesForVanillaImposter;
            List<SingleRoleBase> shuffleRolesForCrewmate = extremeRolesData.RolesForVanillaCrewmate;

            bool assigned = false;
            int assignedPlayers = 1;

            List<int> shuffledArange = playerIndexList.OrderBy(
                item => RandomGenerator.Instance.Next()).ToList();
            Logging.Debug($"NotAssignPlayerNum:{shuffledArange.Count}");

            List<int> tempList = new List<int>(shuffledArange);

            foreach (int index in tempList)
            {
                assigned = false;

                List<SingleRoleBase> shuffledRoles = new List<SingleRoleBase>();
                PlayerControl player = PlayerControl.AllPlayerControls[index];
                RoleBehaviour roleData = player.Data.Role;
                
                Logging.Debug(
                    $"-------------------AssignToPlayer:{player.Data.PlayerName}-------------------");

                // Modules.Helpers.DebugLog($"ShufflePlayerIndex:{shuffledArange.Count()}");

                RoleTypes roleType = roleData.Role;

                // Classicが優先されるので上に上げて速度を上げる
                if (!ExtremeGameModeManager.Instance.RoleSelector.IsVanillaRoleToMultiAssign)
                {
                    switch (roleData.Role)
                    {
                        case RoleTypes.Impostor:
                            shuffledRoles = shuffleRolesForImpostor.OrderBy(
                                item => RandomGenerator.Instance.Next()).ToList();
                            break;
                        case RoleTypes.Crewmate:
                            shuffledRoles = shuffleRolesForCrewmate.OrderBy(
                                item => RandomGenerator.Instance.Next()).ToList();
                            break;
                        default:
                            assignedPlayer.Add(new AssignedPlayerToSingleRoleData(
                            player.PlayerId, (int)roleData.Role));
                            shuffledArange.Remove(index);
                            assigned = true;
                            break;
                    }
                }
                else
                {
                    shuffledRoles = roleType switch
                    {
                        RoleTypes.Impostor or RoleTypes.Shapeshifter
                            => shuffleRolesForImpostor.OrderBy(
                                item => RandomGenerator.Instance.Next()).ToList(),
                        _ => shuffleRolesForCrewmate.OrderBy(
                                item => RandomGenerator.Instance.Next()).ToList()
                    };
                }

                if (assigned)
                {
                    ++assignedPlayers;
                    continue;
                };

                bool result = false;
                foreach (var role in shuffledRoles)
                {
                    // Logging.Debug($"KeyFound?:{extremeRolesData.RoleSpawnSettings[roleData.Role].ContainsKey(role.BytedRoleId)}");
                    Logging.Debug($"---AssignRole:{role.Id}---");
                    int intedRoleId = (int)role.Id;
                    var (roleNum, spawnRate) = extremeRolesData.RoleSpawnSettings[
                        roleData.Role][intedRoleId];

                    result = isRoleSpawn(roleNum, spawnRate);
                    Logging.Debug($"IsRoleSpawn:{result}");
                    result = result && checkLimitRoleSpawnNum(role, ref extremeRolesData);
                    Logging.Debug($"IsNotSpawnLimitNum:{result}");

                    if (combRoleAssignedPlayerId.ContainsKey(player.PlayerId))
                    {
                        result = result && combRoleAssignedPlayerId[player.PlayerId] == role.Team;
                        Logging.Debug($"IsSameTeam:{result}");
                    }

                    if (result)
                    {
                        reduceToSpawnDataNum(role.Team, ref extremeRolesData);
                        assignedPlayer.Add(new AssignedPlayerToSingleRoleData(
                            player.PlayerId, (int)role.Id));

                        shuffledArange.Remove(index);
                        extremeRolesData.RoleSpawnSettings[roleData.Role][intedRoleId] = (
                            --roleNum,
                            spawnRate);
                        break;
                    }
                    else
                    {
                        extremeRolesData.RoleSpawnSettings[roleData.Role][intedRoleId] = (
                            roleNum,
                            spawnRate);
                    }
                }

                Logging.Debug($"-------------------AssignEnd-------------------");

            }

            foreach (int index in shuffledArange)
            {
                PlayerControl player = PlayerControl.AllPlayerControls[index];
                assignedPlayer.Add(new AssignedPlayerToSingleRoleData(
                    player.PlayerId, (byte)player.Data.Role.Role));
            }
        }

        private static void reduceToSpawnDataNum(
            ExtremeRoleType team,
            ref RoleAssignmentData extremeRolesData)
        {
            switch (team)
            {
                case ExtremeRoleType.Crewmate:
                    extremeRolesData.CrewmateRoles = extremeRolesData.CrewmateRoles - 1;
                    break;
                case ExtremeRoleType.Impostor:
                    extremeRolesData.ImpostorRoles = extremeRolesData.ImpostorRoles - 1;
                    break;
                case ExtremeRoleType.Neutral:
                    extremeRolesData.NeutralRoles = extremeRolesData.NeutralRoles - 1;
                    break;
                default:
                    break;
            }
        }

        private static RoleAssignmentData createRoleData()
        {
            List<SingleRoleBase> RolesForVanillaImposter = new List<SingleRoleBase>();
            List<SingleRoleBase> RolesForVanillaCrewmate = new List<SingleRoleBase>();

            // コンビネーションロールに含まれているロール、コンビネーション全体のスポーン数、スポーンレート
            List<((byte, CombinationRoleManagerBase), (int, int, bool))> combinationRole = new List<
                ((byte, CombinationRoleManagerBase), (int, int, bool))>();

            Dictionary<int, (int, int)> RoleSpawnSettingsForImposter = new Dictionary<int, (int, int)>();
            Dictionary<int, (int, int)> RoleSpawnSettingsForCrewmate = new Dictionary<int, (int, int)>();

            var allOption = OptionHolder.AllOption;

            int crewmateRolesNum = UnityEngine.Random.RandomRange(
                allOption[(int)OptionHolder.CommonOptionKey.MinCrewmateRoles].GetValue(),
                allOption[(int)OptionHolder.CommonOptionKey.MaxCrewmateRoles].GetValue());
            int neutralRolesNum = UnityEngine.Random.RandomRange(
                allOption[(int)OptionHolder.CommonOptionKey.MinNeutralRoles].GetValue(),
                allOption[(int)OptionHolder.CommonOptionKey.MaxNeutralRoles].GetValue());
            int impostorRolesNum = UnityEngine.Random.RandomRange(
                allOption[(int)OptionHolder.CommonOptionKey.MinImpostorRoles].GetValue(),
                allOption[(int)OptionHolder.CommonOptionKey.MaxImpostorRoles].GetValue());


            foreach (var roleId in ExtremeGameModeManager.Instance.RoleSelector.UseCombRoleType)
            {
                byte combType = (byte)roleId;
                var role = ExtremeRoleManager.CombRole[combType];
                int spawnRate = computePercentage(allOption[
                    role.GetRoleOptionId(RoleCommonOption.SpawnRate)]);
                int roleSet = allOption[
                    role.GetRoleOptionId(RoleCommonOption.RoleNum)].GetValue();
                bool multiAssign = allOption[
                    role.GetRoleOptionId(CombinationRoleCommonOption.IsMultiAssign)].GetValue();

                Logging.Debug($"Role:{role}    SpawnRate:{spawnRate}   RoleSet:{roleSet}");

                if (roleSet <= 0 || spawnRate <= 0.0)
                {
                    continue;
                }

                combinationRole.Add(
                    ((combType, role), (roleSet, spawnRate, multiAssign)));

                var ghostComb = role as GhostAndAliveCombinationRoleManagerBase;
                if (ghostComb != null)
                {
                    ExtremeGhostRoleManager.AddCombGhostRole(
                        (CombinationRoleType)combType, ghostComb);
                }
            }

            foreach (var roleId in ExtremeGameModeManager.Instance.RoleSelector.UseNormalRoleId)
            {
                int intedRoleId = (int)roleId;
                SingleRoleBase role = ExtremeRoleManager.NormalRole[intedRoleId];

                int spawnRate = computePercentage(allOption[
                    role.GetRoleOptionId(RoleCommonOption.SpawnRate)]);
                int roleNum = allOption[
                    role.GetRoleOptionId(RoleCommonOption.RoleNum)].GetValue();

                Logging.Debug(
                    $"Role Name:{role.RoleName}  SpawnRate:{spawnRate}   RoleNum:{roleNum}");

                if (roleNum <= 0 || spawnRate <= 0.0)
                {
                    continue;
                }

                (int, int) addData = (
                    roleNum,
                    spawnRate);

                switch (role.Team)
                {
                    case ExtremeRoleType.Impostor:
                        RolesForVanillaImposter.Add(role);
                        RoleSpawnSettingsForImposter[intedRoleId] = addData;
                        break;
                    case ExtremeRoleType.Crewmate:
                    case ExtremeRoleType.Neutral:
                        RolesForVanillaCrewmate.Add(role);
                        RoleSpawnSettingsForCrewmate[intedRoleId] = addData;
                        break;
                    case ExtremeRoleType.Null:
                        break;
                    default:
                        throw new Exception("Unknown teamType detect!!");
                }
            }

            ExtremeGhostRoleManager.CreateGhostRoleAssignData();

            return new RoleAssignmentData
            {
                RolesForVanillaCrewmate = RolesForVanillaCrewmate.OrderBy(
                    item => RandomGenerator.Instance.Next()).ToList(),
                RolesForVanillaImposter = RolesForVanillaImposter.OrderBy(
                    item => RandomGenerator.Instance.Next()).ToList(),
                CombinationRole = combinationRole,

                RoleSpawnSettings = new Dictionary<RoleTypes, Dictionary<int, (int, int)>>()
                { 
                    {RoleTypes.Impostor, RoleSpawnSettingsForImposter},
                    {RoleTypes.Crewmate, RoleSpawnSettingsForCrewmate},
                },

                CrewmateRoles = crewmateRolesNum,
                NeutralRoles = neutralRolesNum,
                ImpostorRoles = impostorRolesNum,
            };
        }

        private struct RoleAssignmentData
        {
            public List<SingleRoleBase> RolesForVanillaImposter;
            public List<SingleRoleBase> RolesForVanillaCrewmate;
            public List<((byte, CombinationRoleManagerBase), (int, int, bool))> CombinationRole;

            public Dictionary<RoleTypes, Dictionary<int, (int, int)>> RoleSpawnSettings;
            public int CrewmateRoles;
            public int NeutralRoles;
            public int ImpostorRoles;
        }
    }

    [HarmonyPatch(typeof(RoleManager), nameof(RoleManager.AssignRoleOnDeath))]
    public static class RoleManagerAssignRoleOnDeathPatch
    {
        public static bool Prefix([HarmonyArgument(0)] PlayerControl player)
        {
            if (ExtremeRoleManager.GameRole.Count == 0) { return true; }
            if (!RoleAssignState.Instance.IsRoleSetUpEnd) { return true; }
            if (GameOptionsManager.Instance.CurrentGameOptions.GameMode != GameModes.Normal)
            { 
                return true;
            }

            var role = ExtremeRoleManager.GameRole[player.PlayerId];
            if (!role.IsAssignGhostRole()) { return false; }
            if (ExtremeGhostRoleManager.IsCombRole(role.Id)) { return false; }

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
            // バニラ幽霊クルー役職にニュートラルがアサインされる時やゲームモードがクラッシクではない時は常にTrueを返す
            if (ExtremeGameModeManager.Instance.ShipOption.IsAssignNeutralToVanillaCrewGhostRole ||
                GameOptionsManager.Instance.CurrentGameOptions.GameMode != GameModes.Normal)
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
}
