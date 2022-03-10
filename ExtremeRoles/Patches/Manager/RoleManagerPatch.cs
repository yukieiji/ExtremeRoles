using System;
using System.Collections.Generic;
using System.Linq;

using HarmonyLib;
using UnityEngine;

using ExtremeRoles.Helper;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;

namespace ExtremeRoles.Patches.Manager
{
    [HarmonyPatch(typeof(RoleManager), nameof(RoleManager.SelectRoles))]
    class RoleManagerSelectRolesPatch
    {

        public static void Postfix()
        {

            uint netId = PlayerControl.LocalPlayer.NetId;

            RPCOperator.Call(netId, RPCOperator.Command.Initialize);
            RPCOperator.Initialize();

            PlayerControl[] playeres = PlayerControl.AllPlayerControls.ToArray();
            
            RoleAssignmentData extremeRolesData = createRoleData();
            var playerIndexList = Enumerable.Range(0, playeres.Count()).ToList();

            combinationExtremeRoleAssign(
                ref extremeRolesData, ref playerIndexList);
            normalExtremeRoleAssign(
                extremeRolesData, playerIndexList);
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
                    if (result) { extremeRolesData.CrewmateRoles = extremeRolesData.CrewmateRoles - 1; }
                    break;
                case ExtremeRoleType.Neutral:
                    result = ((extremeRolesData.NeutralRoles - 1) >= 0);
                    if (result) { extremeRolesData.NeutralRoles = extremeRolesData.NeutralRoles - 1; }
                    break;
                case ExtremeRoleType.Impostor:
                    result = ((extremeRolesData.ImpostorRoles - 1) >= 0);
                    if (result) { extremeRolesData.ImpostorRoles = extremeRolesData.ImpostorRoles - 1; }
                    break;
                default:
                    result = false;
                    break;
            }

            return result;
        }

        private static int computePercentage(Module.CustomOptionBase self)
            => (int)Decimal.Multiply(
                self.GetValue(), self.Selections.ToList().Count);

        private static void combinationExtremeRoleAssign(
            ref RoleAssignmentData extremeRolesData,
            ref List<int> playerIndexList)
        {

            Logging.Debug($"NotAssignPlayerNum:{playerIndexList.Count}");

            if (extremeRolesData.CombinationRole.Count == 0) { return; }

            List<(List<MultiAssignRoleBase>, int)> assignMultiAssignRole = getMultiAssignedRoles(
                ref extremeRolesData);

            List<int> needAnotherRoleAssigns = new List<int>();

            PlayerControl player = PlayerControl.LocalPlayer;

            foreach (var(roles, id) in assignMultiAssignRole)
            {
                foreach (var role in roles)
                {
                    bool assign = false;
                    List<int> tempList = new List<int>(
                        playerIndexList.OrderBy(item => RandomGenerator.Instance.Next()).ToList());
                    foreach(int playerIndex in tempList)
                    {
                        player = PlayerControl.AllPlayerControls[playerIndex];
                        assign = isAssignedToMultiRole(
                            role, player);
                        if (!assign) { continue; }
                        
                        if (role.CanHasAnotherRole)
                        {
                            needAnotherRoleAssigns.Add(playerIndex);
                        }
                        playerIndexList.Remove(playerIndex);

                        setCombinationRoleToPlayer(
                            player, role.BytedRoleId, (byte)id);
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

            foreach(var player in PlayerControl.AllPlayerControls)
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

        private static List<(List<MultiAssignRoleBase>, int)> getMultiAssignedRoles(
            ref RoleAssignmentData extremeRolesData)
        {
            List<(List<MultiAssignRoleBase>, int)> assignRoles = new List<(List<MultiAssignRoleBase>, int)>();

            var roleDataLoop = extremeRolesData.CombinationRole.OrderBy(
                item => RandomGenerator.Instance.Next()).ToList();

            int gameControlId = 0;
            int curImpNum = 0;

            foreach (var oneRole in roleDataLoop)
            {
                var (roleManager, (num, spawnRate, isMultiAssign)) = oneRole;
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
                    }

                    isSpawn = (
                        isSpawn &&
                        ((extremeRolesData.CrewmateRoles - reduceCrewmateRole >= 0) && crewNum >= reduceCrewmateRole) &&
                        ((extremeRolesData.NeutralRoles - reduceNeutralRole >= 0) && crewNum >= reduceNeutralRole) &&
                        ((extremeRolesData.ImpostorRoles - reduceImpostorRole >= 0) && impNum >= reduceImpostorRole));
                    //Modules.Helpers.DebugLog($"Role:{oneRole.ToString()}   isSpawn?:{isSpawn}");
                    if (!isSpawn) { continue; }
                    
                    extremeRolesData.CrewmateRoles = extremeRolesData.CrewmateRoles - reduceCrewmateRole;
                    extremeRolesData.NeutralRoles = extremeRolesData.NeutralRoles - reduceNeutralRole;
                    extremeRolesData.ImpostorRoles = extremeRolesData.ImpostorRoles - reduceImpostorRole;
                    
                    impNum = impNum - reduceImpostorRole;
                    crewNum = crewNum - (reduceCrewmateRole + reduceNeutralRole);

                    var spawnRoles = new List<MultiAssignRoleBase>();
                    foreach (var role in roleManager.Roles)
                    {
                        if (role.IsImpostor())
                        {
                            ++impNum;
                        }
                        spawnRoles.Add(
                            (MultiAssignRoleBase)role.Clone());
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

            if (ExtremeRoleManager.GameRole.ContainsKey(player.PlayerId))
            {
                if (ExtremeRoleManager.GameRole[player.PlayerId].Id == role.Id)
                {
                    return false;
                }
            }

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

        private static void normalExtremeRoleAssign(
            RoleAssignmentData extremeRolesData,
            List<int> shuffledArange)
        {

            List<SingleRoleBase> shuffleRolesForImpostor = extremeRolesData.RolesForVanillaImposter;
            List<SingleRoleBase> shuffleRolesForCrewmate = extremeRolesData.RolesForVanillaCrewmate;

            bool assigned = false;
            int assignedPlayers = 1;

            shuffledArange = shuffledArange.OrderBy(
                item => RandomGenerator.Instance.Next()).ToList();
            Logging.Debug($"NotAssignPlayerNum:{shuffledArange.Count}");

            List<int> tempList = new List<int>(shuffledArange);

            foreach (int index in tempList)
            {
                assigned = false;

                List<SingleRoleBase> shuffledRoles = new List<SingleRoleBase>();
                PlayerControl player = PlayerControl.AllPlayerControls[index];
                RoleBehaviour roleData = player.Data.Role;

                // Modules.Helpers.DebugLog($"ShufflePlayerIndex:{shuffledArange.Count()}");

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
                        setNormalRoleToPlayer(
                            player,
                            (byte)roleData.Role);
                        shuffledArange.Remove(index);
                        assigned = true;
                        break;
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
                    var (roleNum, spawnRate) = extremeRolesData.RoleSpawnSettings[
                        roleData.Role][role.BytedRoleId];

                    result = isRoleSpawn(roleNum, spawnRate);
                    result = result && checkLimitRoleSpawnNum(role, ref extremeRolesData);
                    
                    if (ExtremeRoleManager.GameRole.ContainsKey(player.PlayerId))
                    {
                        result = result && ExtremeRoleManager.GameRole[
                            player.PlayerId].Team == role.Team;
                    }

                    // Logging.Debug($"Role:{role.Id}: AssignResult:{result}");

                    if (result)
                    {
                        setNormalRoleToPlayer(player, role.BytedRoleId);
                        shuffledArange.Remove(index);
                        extremeRolesData.RoleSpawnSettings[roleData.Role][role.BytedRoleId] = (
                            --roleNum,
                            spawnRate);
                        break;
                    }
                    else
                    {
                        extremeRolesData.RoleSpawnSettings[roleData.Role][role.BytedRoleId] = (
                            roleNum,
                            spawnRate);
                    }
                }
            }

            foreach (int index in shuffledArange)
            {
                PlayerControl player = PlayerControl.AllPlayerControls[index];
                setNormalRoleToPlayer(
                    player, (byte)(player.Data.Role.Role));
            }
        }


        private static void setNormalRoleToPlayer(
            PlayerControl player, byte roleId)
        {

            Logging.Debug($"Player:{player.name}  RoleId:{roleId}");

            RPCOperator.Call(
                PlayerControl.LocalPlayer.NetId,
                RPCOperator.Command.SetNormalRole,
                new List<byte> { roleId, player.PlayerId});
            RPCOperator.SetNormalRole(
                roleId, player.PlayerId);
        }

        private static void setCombinationRoleToPlayer(
            PlayerControl player, byte roleId, byte gameId)
        {
            byte bytedRoleType = (byte)player.Data.Role.Role;
            Logging.Debug($"Player:{player.name}  RoleId:{roleId}");

            RPCOperator.Call(
                PlayerControl.LocalPlayer.NetId,
                RPCOperator.Command.SetCombinationRole,
                new List<byte> { roleId, player.PlayerId, gameId, bytedRoleType });
            RPCOperator.SetCombinationRole(
                roleId, player.PlayerId, gameId, bytedRoleType);
        }


        private static RoleAssignmentData createRoleData()
        {
            List<SingleRoleBase> RolesForVanillaImposter = new List<SingleRoleBase>();
            List<SingleRoleBase> RolesForVanillaCrewmate = new List<SingleRoleBase>();
            
            // コンビネーションロールに含まれているロール、コンビネーション全体のスポーン数、スポーンレート
            List<(CombinationRoleManagerBase, (int, int, bool))> combinationRole = new List<
                (CombinationRoleManagerBase, (int, int, bool))>();

            Dictionary<byte, (int, int)> RoleSpawnSettingsForImposter = new Dictionary<byte, (int, int)>();
            Dictionary<byte, (int, int)> RoleSpawnSettingsForCrewmate = new Dictionary<byte, (int, int)>();

            var allOption = OptionHolder.AllOption;

            int crewmateRolesNum = UnityEngine.Random.RandomRange(
                allOption[(int)OptionHolder.CommonOptionKey.MinCremateRoles].GetValue(),
                allOption[(int)OptionHolder.CommonOptionKey.MaxCremateRoles].GetValue());
            int neutralRolesNum = UnityEngine.Random.RandomRange(
                allOption[(int)OptionHolder.CommonOptionKey.MinNeutralRoles].GetValue(),
                allOption[(int)OptionHolder.CommonOptionKey.MaxNeutralRoles].GetValue());
            int impostorRolesNum = UnityEngine.Random.RandomRange(
                allOption[(int)OptionHolder.CommonOptionKey.MinImpostorRoles].GetValue(),
                allOption[(int)OptionHolder.CommonOptionKey.MaxImpostorRoles].GetValue());


            foreach (var role in ExtremeRoleManager.CombRole)
            {
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
                    (role, (roleSet, spawnRate, multiAssign)));
            }

            foreach (var role in ExtremeRoleManager.NormalRole)
            {

                byte roleId = role.BytedRoleId;
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
                        RoleSpawnSettingsForImposter[roleId] = addData;
                        break;
                    case ExtremeRoleType.Crewmate:
                    case ExtremeRoleType.Neutral:
                        RolesForVanillaCrewmate.Add(role);
                        RoleSpawnSettingsForCrewmate[roleId] = addData;
                        break;
                    case ExtremeRoleType.Null:
                        break;
                }
            }


            return new RoleAssignmentData
            {
                RolesForVanillaCrewmate = RolesForVanillaCrewmate.OrderBy(
                    item => RandomGenerator.Instance.Next()).ToList(),
                RolesForVanillaImposter = RolesForVanillaImposter.OrderBy(
                    item => RandomGenerator.Instance.Next()).ToList(),
                CombinationRole = combinationRole,

                RoleSpawnSettings = new Dictionary<RoleTypes, Dictionary<byte, (int, int)>>()
                { {RoleTypes.Impostor, RoleSpawnSettingsForImposter},
                  {RoleTypes.Crewmate, RoleSpawnSettingsForCrewmate},
                },

                CrewmateRoles = crewmateRolesNum,
                NeutralRoles = neutralRolesNum,
                ImpostorRoles = impostorRolesNum,
            };
        }

        private class RoleAssignmentData
        {
            public List<SingleRoleBase> RolesForVanillaImposter = new List<SingleRoleBase>();
            public List<SingleRoleBase> RolesForVanillaCrewmate = new List<SingleRoleBase>();
            public List<(CombinationRoleManagerBase, (int, int, bool))> CombinationRole = new List<
                (CombinationRoleManagerBase, (int, int, bool))>();

            public Dictionary<
                RoleTypes, Dictionary<byte, (int, int)>> RoleSpawnSettings = 
                    new Dictionary<RoleTypes, Dictionary<byte, (int, int)>>();
            public int CrewmateRoles { get; set; }
            public int NeutralRoles { get; set; }
            public int ImpostorRoles { get; set; }
        }

    }
}
