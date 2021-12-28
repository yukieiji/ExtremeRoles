using System;
using System.Collections.Generic;
using System.Linq;

using HarmonyLib;
using Hazel;

using ExtremeRoles.Helper;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;

namespace ExtremeRoles.Patches.Manager
{
    [HarmonyPatch(typeof(RoleManager), nameof(RoleManager.SelectRoles))]
    class RoleManagerSelectRolesPatch
    {

        private static Random roleRng = null;

        public static void Postfix()
        {
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(
                PlayerControl.LocalPlayer.NetId,
                (byte)RPCOperator.Command.GameInit,
                Hazel.SendOption.Reliable, -1);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
            RPCOperator.GameInit();

            PlayerControl[] playeres = PlayerControl.AllPlayerControls.ToArray();
            bool useStrongGen = OptionsHolder.AllOptions[
                (int)OptionsHolder.CommonOptionKey.UseStrongRandomGen].GetValue();
            if (useStrongGen)
            {
                roleRng = RandomGenerator.CreateStrong();
                RandomGenerator.SetUnityStrongRandomSeed();
            }
            else
            {
                roleRng = RandomGenerator.Create();
                RandomGenerator.SetUnityRandomSeed();
            }
            
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

            PlayerControl player = PlayerControl.LocalPlayer;

            foreach (var(roles, id) in assignMultiAssignRole)
            {
                foreach (var (role, index) in roles.Select((role, index) => (role, index)))
                {
                    bool assign = false;
                    List<int> tempList = new List<int>(
                        playerIndexList.OrderBy(item => roleRng.Next()).ToList());
                    foreach(int playerIndex in tempList)
                    {
                        player = PlayerControl.AllPlayerControls[playerIndex];
                        assign = isAssignedToMultiRole(
                            role, player);
                        if (!assign) { continue; }
                        
                        if (!role.CanHasAnotherRole)
                        {
                            playerIndexList.Remove(playerIndex);
                        }

                        setCombinationRoleToPlayer(
                            player, role.BytedRoleId, (byte)index, (byte)id);
                        break;
                    }
                }
            }


        }

        private static List<(List<MultiAssignRoleBase>, int)> getMultiAssignedRoles(
            ref RoleAssignmentData extremeRolesData)
        {
            List<(List<MultiAssignRoleBase>, int)> assignRoles = new List<(List<MultiAssignRoleBase>, int)>();

            var roleDataLoop = extremeRolesData.CombinationRole.OrderBy(
                item => roleRng.Next()).ToList();
            List<(List<MultiAssignRoleBase>, (int, int))> newRoleData = new List<(List<MultiAssignRoleBase>, (int, int))> ();

            int gameControlId = 0;

            foreach (var oneRole in roleDataLoop)
            {
                var (roles, (num, spawnRate)) = oneRole;

                for (int i = 0; i < num; i++)
                {
                    bool isSpawn = isRoleSpawn(num, spawnRate);
                    int reduceCrewmateRole = 0;
                    int reduceImpostorRole = 0;
                    int reduceNeutralRole = 0;

                    foreach (var role in roles)
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
                        (extremeRolesData.CrewmateRoles - reduceCrewmateRole >= 0) &&
                        (extremeRolesData.NeutralRoles - reduceNeutralRole >= 0) &&
                        (extremeRolesData.ImpostorRoles - reduceImpostorRole >= 0));
                    //Modules.Helpers.DebugLog($"Role:{oneRole.ToString()}   isSpawn?:{isSpawn}");
                    if (!isSpawn) { continue; }
                    
                    extremeRolesData.CrewmateRoles = extremeRolesData.CrewmateRoles - reduceCrewmateRole;
                    extremeRolesData.NeutralRoles = extremeRolesData.NeutralRoles - reduceNeutralRole;
                    extremeRolesData.ImpostorRoles = extremeRolesData.ImpostorRoles - reduceImpostorRole;

                    var spawnRoles = new List<MultiAssignRoleBase>();
                    foreach (var role in roles)
                    {
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
                    if (role.IsImposter())
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
                    if (role.IsImposter() && role.CanHasAnotherRole)
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
            int roleNum, int spawnData)
        {
            if (roleNum <= 0) { return false; }
            if (spawnData < UnityEngine.Random.RandomRange(0, 100)) { return false; }

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

            do
            {
                shuffledArange = shuffledArange.OrderBy(item => roleRng.Next()).ToList();
                Logging.Debug($"NotAssignPlayerNum:{shuffledArange.Count()}");
                assignedPlayers = 1;

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
                                item => roleRng.Next()).ToList();
                            break;
                        case RoleTypes.Crewmate:
                            shuffledRoles = shuffleRolesForCrewmate.OrderBy(
                                item => roleRng.Next()).ToList();
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
                        var(roleNum, spawnRate) = extremeRolesData.RoleSpawnSettings[
                            roleData.Role][role.BytedRoleId];

                        result = isRoleSpawn(roleNum, spawnRate);
                        result = result && checkLimitRoleSpawnNum(role, ref extremeRolesData);


                        Logging.Debug($"Role:{role.Id}: AssignResult:{result}");

                        if (result)
                        {
                            setNormalRoleToPlayer(player, role.BytedRoleId);
                            shuffledArange.Remove(index);
                            extremeRolesData.RoleSpawnSettings[roleData.Role][role.BytedRoleId] = (
                                --roleNum,
                                spawnRate);
                            assigned = false;
                            break;
                        }
                        else
                        {
                            extremeRolesData.RoleSpawnSettings[roleData.Role][role.BytedRoleId] = (
                                roleNum,
                                spawnRate);
                            assigned = true;
                        }
                    }
                    if (assigned)
                    {
                        ++assignedPlayers; 
                    }
                    
                }

                Logging.Debug($"AssignedPlayerNum:{assignedPlayers}");

                Logging.Debug($"Imposter Role Num:{shuffleRolesForImpostor.Count}");
                Logging.Debug($"Crewmate Role Num:{shuffleRolesForCrewmate.Count}");

                if (shuffledArange.Count == assignedPlayers ||
                    shuffledArange.Count + shuffleRolesForImpostor.Count == assignedPlayers ||
                    shuffledArange.Count + shuffleRolesForCrewmate.Count == assignedPlayers ||
                    (shuffleRolesForImpostor.Count == 0 &&  shuffleRolesForCrewmate.Count == 0))
                {
                    tempList = new List<int>(shuffledArange);

                    foreach (int index in tempList)
                    {
                        PlayerControl player = PlayerControl.AllPlayerControls[index];
                        setNormalRoleToPlayer(
                            player, (byte)(player.Data.Role.Role));
                        shuffledArange.Remove(index);
                    }

                }

            }
            while (shuffledArange.Count != 0);
        }


        private static void setNormalRoleToPlayer(
            PlayerControl player, byte roleId)
        {

            Logging.Debug($"Player:{player.name}  RoleId:{roleId}");

            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(
                PlayerControl.LocalPlayer.NetId,
                (byte)RPCOperator.Command.SetNormalRole,
                Hazel.SendOption.Reliable, -1);
            
            writer.Write(roleId);
            writer.Write(player.PlayerId);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
            RPCOperator.SetNormalRole(
                roleId, player.PlayerId);
        }

        private static void setCombinationRoleToPlayer(
            PlayerControl player, byte roleId, byte combinationRoleIndex, byte gameId)
        {

            Logging.Debug($"Player:{player.name}  RoleId:{roleId}");

            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(
                PlayerControl.LocalPlayer.NetId,
                (byte)RPCOperator.Command.SetCombinationRole,
                Hazel.SendOption.Reliable, -1);

            writer.Write(roleId);
            writer.Write(player.PlayerId);
            writer.Write(combinationRoleIndex);
            writer.Write(gameId);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
            RPCOperator.SetCombinationRole(
                roleId, player.PlayerId, combinationRoleIndex, gameId);
        }


        private static RoleAssignmentData createRoleData()
        {
            List<SingleRoleBase> RolesForVanillaImposter = new List<SingleRoleBase>();
            List<SingleRoleBase> RolesForVanillaCrewmate = new List<SingleRoleBase>();
            
            // コンビネーションロールに含まれているロール、コンビネーション全体のスポーン数、スポーンレート
            List<(List<MultiAssignRoleBase>, (int, int))> combinationRole = new List<
                (List<MultiAssignRoleBase>, (int, int))>();

            Dictionary<byte, (int, int)> RoleSpawnSettingsForImposter = new Dictionary<byte, (int, int)>();
            Dictionary<byte, (int, int)> RoleSpawnSettingsForCrewmate = new Dictionary<byte, (int, int)>();

            var allOption = OptionsHolder.AllOptions;

            int crewmateRolesNum = UnityEngine.Random.RandomRange(
                allOption[(int)OptionsHolder.CommonOptionKey.MinCremateRoles].GetValue(),
                allOption[(int)OptionsHolder.CommonOptionKey.MaxCremateRoles].GetValue());
            int neutralRolesNum = UnityEngine.Random.RandomRange(
                allOption[(int)OptionsHolder.CommonOptionKey.MinNeutralRoles].GetValue(),
                allOption[(int)OptionsHolder.CommonOptionKey.MaxNeutralRoles].GetValue());
            int impostorRolesNum = UnityEngine.Random.RandomRange(
                allOption[(int)OptionsHolder.CommonOptionKey.MinImpostorRoles].GetValue(),
                allOption[(int)OptionsHolder.CommonOptionKey.MaxImpostorRoles].GetValue());


            foreach (var role in ExtremeRoleManager.CombRole)
            {
                int spawnRate = computePercentage(allOption[
                    role.GetRoleOptionId(RoleCommonOption.SpawnRate)]);
                int roleSet = allOption[
                    role.GetRoleOptionId(RoleCommonOption.RoleNum)].GetValue();

                Logging.Debug($"Role:{role}    SpawnRate:{spawnRate}   RoleSet:{roleSet}");

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
                    item => roleRng.Next()).ToList(),
                RolesForVanillaImposter = RolesForVanillaImposter.OrderBy(
                    item => roleRng.Next()).ToList(),
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
            public List<(List<MultiAssignRoleBase>, (int, int))> CombinationRole = new List<
                (List<MultiAssignRoleBase>, (int, int))>();

            public Dictionary<
                RoleTypes, Dictionary<byte, (int, int)>> RoleSpawnSettings = 
                    new Dictionary<RoleTypes, Dictionary<byte, (int, int)>>();
            public int CrewmateRoles { get; set; }
            public int NeutralRoles { get; set; }
            public int ImpostorRoles { get; set; }
        }

    }
}
