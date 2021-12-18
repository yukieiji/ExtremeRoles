using System;
using System.Collections.Generic;
using System.Linq;

using HarmonyLib;
using Hazel;

using ExtremeRoles.Helper;
using ExtremeRoles.Roles;

namespace ExtremeRoles.Patches.Manager
{
    [HarmonyPatch(typeof(RoleManager), nameof(RoleManager.SelectRoles))]
    class RoleManagerSelectRolesPatch
    {

        private static Random RoleRng = new Random(
            UnityEngine.SystemInfo.processorFrequency);

        public static void Postfix()
        {
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(
                PlayerControl.LocalPlayer.NetId,
                (byte)CustomRPC.GameInit, Hazel.SendOption.Reliable, -1);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
            ExtremeRoleRPC.GameInit();

            PlayerControl[] playeres = PlayerControl.AllPlayerControls.ToArray();
            RoleAssignmentData extremeRolesData = CreateRoleData();
            var playerIndexList = Enumerable.Range(0, playeres.Count()).ToList();

            CombinationExtremeRoleAssign(
                ref extremeRolesData, ref playerIndexList);
            NormalExtremeRoleAssign(
                extremeRolesData, playerIndexList);
        }

        private static void CombinationExtremeRoleAssign(
            ref RoleAssignmentData extremeRolesData,
            ref List<int> playerIndexList)
        {

            Logging.Debug($"NotAssignPlayerNum:{playerIndexList.Count}");

            if (extremeRolesData.CombinationRole.Count == 0) { return; }

            List<List<MultiAssignRoleAbs>> assignMultiAssignRole = GetMultiAssignedRoles(
                ref extremeRolesData);

            PlayerControl player = PlayerControl.LocalPlayer;

            foreach (var roles in assignMultiAssignRole)
            {
                foreach (var role in roles)
                {
                    bool assign = false;
                    List<int> tempList = new List<int>(
                        playerIndexList.OrderBy(item => RoleRng.Next()).ToList());
                    foreach(int playerIndex in tempList)
                    {
                        player = PlayerControl.AllPlayerControls[playerIndex];
                        assign = IsAssignedToMultiRole(
                            role, player);
                        if (!assign) { continue; }
                        
                        if (!role.CanHasAnotherRole)
                        {
                            playerIndexList.Remove(playerIndex);
                        }

                        SetRoleToPlayer(
                            player, role.BytedRoleId, true);
                        break;
                    }
                }
            }


        }

        private static List<List<MultiAssignRoleAbs>> GetMultiAssignedRoles(
            ref RoleAssignmentData extremeRolesData)
        {
            List<List<MultiAssignRoleAbs>> assignRoles = new List<List<MultiAssignRoleAbs>>();

            var roleDataLoop = extremeRolesData.CombinationRole.OrderBy(
                item => RoleRng.Next()).ToList();
            List<(List<MultiAssignRoleAbs>, (int, int))> newRoleData = new List<(List<MultiAssignRoleAbs>, (int, int))> ();

            foreach (var oneRole in roleDataLoop)
            {
                var (roles, (num, spawnRate)) = oneRole;

                for (int i = 0; i < num; i++)
                {
                    bool isSpawn = IsRoleSpawn(num, spawnRate);
                    int reduceCrewmateRole = 0;
                    int reduceImpostorRole = 0;
                    int reduceNeutralRole = 0;

                    foreach (var role in roles)
                    {
                        switch (role.Teams)
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
                        (extremeRolesData.CrewmateRoles - reduceCrewmateRole >= 0) &&
                        (extremeRolesData.NeutralRoles - reduceNeutralRole >= 0) &&
                        (extremeRolesData.ImpostorRoles - reduceImpostorRole >= 0));
                    //Modules.Helpers.DebugLog($"Role:{oneRole.ToString()}   isSpawn?:{isSpawn}");
                    if (!isSpawn) { continue; }
                    
                    extremeRolesData.CrewmateRoles = extremeRolesData.CrewmateRoles - reduceCrewmateRole;
                    extremeRolesData.NeutralRoles = extremeRolesData.NeutralRoles - reduceNeutralRole;
                    extremeRolesData.ImpostorRoles = extremeRolesData.ImpostorRoles - reduceImpostorRole;

                    var spawnRoles = new List<MultiAssignRoleAbs>();
                    foreach (var role in roles)
                    {
                        spawnRoles.Add(
                            (MultiAssignRoleAbs)role.Clone());
                    }
                    assignRoles.Add(spawnRoles);
                }
            }

            return assignRoles;
        }

        private static bool IsAssignedToMultiRole(
            MultiAssignRoleAbs role,
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
                    if (role.IsImposter())
                    {
                        return true;
                    }
                    break;
                case RoleTypes.Crewmate:
                    if ((role.IsCrewmate() || role.IsNeutral()) &&
                         role.Teams != ExtremeRoleType.Null)
                    {
                        return true;
                    }
                    break;
                case RoleTypes.Shapeshifter:
                    if (role.IsImposter() && role.CanHasAnotherRole)
                    {
                        return true;
                    }
                    break;
                case RoleTypes.Engineer:
                case RoleTypes.Scientist:
                    if ((role.IsCrewmate() || role.IsNeutral()) &&
                        role.CanHasAnotherRole &&
                        role.Teams != ExtremeRoleType.Null)
                    {
                        return true;
                    }
                    break;
                default:
                    return false;
            }
            return false;
        }

        private static bool IsRoleSpawn(
            int roleNum, int spawnData)
        {
            if (roleNum <= 0) { return false; }
            if (spawnData < UnityEngine.Random.RandomRange(0, 100)) { return false; }

            return true;
        }

        private static void NormalExtremeRoleAssign(
            RoleAssignmentData extremeRolesData,
            List<int> shuffledArange)
        {

            List<SingleRoleAbs> shuffleRolesForImpostor = extremeRolesData.RolesForVanillaImposter;
            List<SingleRoleAbs> shuffleRolesForCrewmate = extremeRolesData.RolesForVanillaCrewmate;

            bool assigned = false;
            int assignedPlayers = 1;

            do
            {
                shuffledArange = shuffledArange.OrderBy(item => RoleRng.Next()).ToList();
                Logging.Debug($"NotAssignPlayerNum:{shuffledArange.Count()}");
                assignedPlayers = 1;

                List<int> tempList = new List<int>(shuffledArange);

                foreach (int index in tempList)
                {
                    assigned = false;

                    List<SingleRoleAbs> shuffledRoles = new List<SingleRoleAbs>();
                    PlayerControl player = PlayerControl.AllPlayerControls[index];
                    RoleBehaviour roleData = player.Data.Role;

                    // Modules.Helpers.DebugLog($"ShufflePlayerIndex:{shuffledArange.Count()}");

                    switch (roleData.Role)
                    {

                        case RoleTypes.Impostor:
                            shuffledRoles = shuffleRolesForCrewmate.OrderBy(
                                item => RoleRng.Next()).ToList();
                            break;
                        case RoleTypes.Crewmate:
                            shuffledRoles = shuffleRolesForCrewmate.OrderBy(
                                item => RoleRng.Next()).ToList();
                            break;
                        default:
                            SetRoleToPlayer(
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
                        var(roleNum, spawnRate) = extremeRolesData.RoleSpawnSettings[
                            roleData.Role][role.BytedRoleId];
                        result = IsRoleSpawn(roleNum, spawnRate);

                        Logging.Debug($"RoleResult:{result}");

                        if (result)
                        {
                            SetRoleToPlayer(player, role.BytedRoleId);
                            shuffledArange.Remove(index);
                            extremeRolesData.RoleSpawnSettings[roleData.Role][role.BytedRoleId] = (
                                --roleNum,
                                spawnRate);
                            assigned = false;
                            break;
                        }
                        else
                        {
                            assigned = true;
                        }
                    }
                    if (assigned) { ++assignedPlayers; }
                    
                }

                Logging.Debug($"AssignedPlayerNum:{assignedPlayers}");

                if (shuffledArange.Count == assignedPlayers ||
                    (shuffleRolesForImpostor.Count == 0 && 
                     shuffleRolesForCrewmate.Count == 0))
                {
                    tempList = new List<int>(shuffledArange);

                    foreach (int index in tempList)
                    {
                        PlayerControl player = PlayerControl.AllPlayerControls[index];
                        SetRoleToPlayer(
                            player, (byte)(player.Data.Role.Role));
                        shuffledArange.Remove(index);
                    }

                }

            }
            while (shuffledArange.Count != 0);
        }


        private static void SetRoleToPlayer(
            PlayerControl player, byte roleId, bool combinationRole=false)
        {

            Logging.Debug($"Player:{player.name}  RoleId:{roleId}");

            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(
                PlayerControl.LocalPlayer.NetId,
                (byte)CustomRPC.SetRole, Hazel.SendOption.Reliable, -1);
            
            writer.Write(roleId);
            writer.Write(player.PlayerId);
            writer.Write(combinationRole);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
            ExtremeRoleRPC.SetRole(
                roleId, player.PlayerId, combinationRole);
        }


        private static RoleAssignmentData CreateRoleData()
        {
            List<SingleRoleAbs> RolesForVanillaImposter = new List<SingleRoleAbs>();
            List<SingleRoleAbs> RolesForVanillaCrewmate = new List<SingleRoleAbs>();
            
            // コンビネーションロールに含まれているロール、コンビネーション全体のスポーン数、スポーンレート
            List<(List<MultiAssignRoleAbs>, (int, int))> combinationRole = new List<
                (List<MultiAssignRoleAbs>, (int, int))>();

            Dictionary<byte, (int, int)> RoleSpawnSettingsForImposter = new Dictionary<byte, (int, int)>();
            Dictionary<byte, (int, int)> RoleSpawnSettingsForCrewmate = new Dictionary<byte, (int, int)>();

            var allSetting = OptionsHolder.AllOptions;

            int crewmateRolesNum = UnityEngine.Random.RandomRange(
                allSetting[(int)OptionsHolder.CommonOptionKey.MinCremateRoles].GetSelection(),
                allSetting[(int)OptionsHolder.CommonOptionKey.MaxCremateRoles].GetSelection());
            int neutralRolesNum = UnityEngine.Random.RandomRange(
                allSetting[(int)OptionsHolder.CommonOptionKey.MinNeutralRoles].GetSelection(),
                allSetting[(int)OptionsHolder.CommonOptionKey.MaxNeutralRoles].GetSelection());
            int impostorRolesNum = UnityEngine.Random.RandomRange(
                allSetting[(int)OptionsHolder.CommonOptionKey.MinImpostorRoles].GetSelection(),
                allSetting[(int)OptionsHolder.CommonOptionKey.MaxImpostorRoles].GetSelection());


            foreach (var role in ExtremeRoleManager.CombRole)
            {
                int spawnRate = allSetting[
                    role.GetRoleSettingId(RoleCommonSetting.SpawnRate)].GetPercentage();
                int roleSet = allSetting[
                    role.GetRoleSettingId(RoleCommonSetting.RoleNum)].GetSelection() + 1;

                Logging.Debug($"SpawnRate:{spawnRate}   RoleSet:{roleSet}");

                if (roleSet <= 0 || spawnRate <= 0.0)
                {
                    continue;
                }

                combinationRole.Add(
                    (role.Roles, (roleSet, spawnRate)));
            }

            foreach (var role in ExtremeRoleManager.NormalRole)
            {

                byte roleId = role.BytedRoleId;
                int spawnRate = allSetting[
                    role.GetRoleSettingId(RoleCommonSetting.SpawnRate)].GetPercentage();
                int roleNum = allSetting[
                    role.GetRoleSettingId(RoleCommonSetting.RoleNum)].GetInt();

                Logging.Debug(
                    $"SelectopmValue:{allSetting[role.GetRoleSettingId(RoleCommonSetting.RoleNum)].Selections[0]}");
                Logging.Debug(
                    $"Role Name:{role.RoleName}  SpawnRate:{spawnRate}   RoleNum:{roleNum}");

                if (roleNum <= 0 || spawnRate <= 0.0)
                {
                    continue;
                }

                (int, int) addData = (
                    roleNum,
                    spawnRate);

                switch (role.Teams)
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
                RolesForVanillaCrewmate = RolesForVanillaCrewmate,
                RolesForVanillaImposter = RolesForVanillaImposter,
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
            public List<SingleRoleAbs> RolesForVanillaImposter = new List<SingleRoleAbs>();
            public List<SingleRoleAbs> RolesForVanillaCrewmate = new List<SingleRoleAbs>();
            public List<(List<MultiAssignRoleAbs>, (int, int))> CombinationRole = new List<
                (List<MultiAssignRoleAbs>, (int, int))>();

            public Dictionary<
                RoleTypes, Dictionary<byte, (int, int)>> RoleSpawnSettings = 
                    new Dictionary<RoleTypes, Dictionary<byte, (int, int)>>();
            public int CrewmateRoles { get; set; }
            public int NeutralRoles { get; set; }
            public int ImpostorRoles { get; set; }
        }

    }
}
