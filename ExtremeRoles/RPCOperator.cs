﻿using System;
using Hazel;

namespace ExtremeRoles
{
    public static class RPCOperator
    {

        public enum Command
        {

            Initialize = 60,
            ForceEnd,
            SetNormalRole,
            SetCombinationRole,
            ShareOption,
            UncheckedMurderPlayer,
            CleanDeadBody,

            ReplaceRole,

            AliceAbility
        }

        public static void CleanDeadBody(byte targetId)
        {
            DeadBody[] array = UnityEngine.Object.FindObjectsOfType<DeadBody>();
            for (int i = 0; i < array.Length; ++i)
            {
                if (GameData.Instance.GetPlayerById(array[i].ParentId).PlayerId == targetId)
                {
                    UnityEngine.Object.Destroy(array[i].gameObject);
                    break;
                }
            }
        }

        public static void Initialize()
        {
            OptionsHolder.Load();
            RandomGenerator.Initialize();
            Roles.ExtremeRoleManager.Initialize();
            ExtremeRolesPlugin.GameDataStore.Initialize();
        }

        public static void ForceEnd()
        {
            foreach (PlayerControl player in PlayerControl.AllPlayerControls)
            {
                if (!player.Data.Role.IsImpostor)
                {
                    player.RemoveInfected();
                    player.MurderPlayer(player);
                    player.Data.IsDead = true;
                }
            }
        }

        public static void SetCombinationRole(
            byte roleId, byte playerId, byte roleIndex, byte id)
        {
            Roles.ExtremeRoleManager.SetPlayerIdToMultiRoleId(
                roleId, playerId, roleIndex, id);
        }

        public static void SetNormalRole(byte roleId, byte playerId)
        {
            Roles.ExtremeRoleManager.SetPlyerIdToSingleRoleId(
                roleId, playerId);
        }

        public static void ShareOption(int numOptions, MessageReader reader)
        {
            OptionsHolder.ShareOption(numOptions, reader);
        }

        public static void ReplaceRole(
            byte callerId, byte targetId, byte operation)
        {
            Roles.ExtremeRoleManager.RoleReplace(
                callerId, targetId,
                (Roles.ExtremeRoleManager.ReplaceOperation)operation);
        }

        public static void UncheckedMurderPlayer(
            byte sourceId, byte targetId, byte useAnimation)
        {

            PlayerControl source = Helper.Player.GetPlayerControlById(sourceId);
            PlayerControl target = Helper.Player.GetPlayerControlById(targetId);

            if (source != null && target != null)
            {
                if (useAnimation == 0)
                {
                    Patches.KillAnimationCoPerformKillPatch.hideNextAnimation = true;
                }
                source.MurderPlayer(target);

                var targetRole = Roles.ExtremeRoleManager.GameRole[targetId];
                var assassin = targetRole as Roles.Combination.Assassin;
                if (assassin != null)
                {
                    ExtremeRolesPlugin.GameDataStore.AssassinMeetingTrigger = true;
                }

                targetRole.RolePlayerKilledAction(
                    target, source);
                
                if (!targetRole.HasTask)
                {
                    target.ClearTasks();
                }

            }
        }
        public static void AliceAbility(byte callerId)
        {
            Roles.Solo.Neutral.Alice.ShipBroken(callerId);
        }
    }

}
