using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

using Hazel;
using BepInEx.Configuration;

using ExtremeRoles.Helper;
using ExtremeRoles.Module;

namespace ExtremeRoles
{
    public static class OptionsHolder
    {
        public static string[] SpawnRate = new string[] { "0%", "10%", "20%", "30%", "40%", "50%", "60%", "70%", "80%", "90%", "100%" };
        public static string[] OptionPreset = new string[] { "preset1", "preset2", "preset3", "preset4", "preset5" };
        public static string[] Range = new string[] { "short", "middle", "long"};

        public static int SelectedPreset = 0;
        public const int VanillaMaxPlayerNum = 15;
        public const int MaxImposterNum = 3; 
        public enum CommonOptionKey
        {
            PresetSelection = 0,

            UseStrongRandomGen,

            MinCremateRoles,
            MaxCremateRoles,
            MinNeutralRoles,
            MaxNeutralRoles,
            MinImpostorRoles,
            MaxImpostorRoles,

            NumMeatings,
            DesableVent,
            DisableSkipInEmergencyMeeting,
            NoVoteToSelf,
            HidePlayerName,
            ParallelMedBayScans,
            RandomMap,
            EngineerUseImpostorVent,
        }

        public static Dictionary<int, CustomOptionBase> AllOption = new Dictionary<int, CustomOptionBase>();

        public static void Create()
        {
            CreateConfigOption();

            Roles.ExtremeRoleManager.GameRole.Clear();
            AllOption.Clear();

            CustomOption.Create(
                (int)CommonOptionKey.PresetSelection, Design.ColoedString(
                    new Color(204f / 255f, 204f / 255f, 0, 1f),
                    CommonOptionKey.PresetSelection.ToString()),
                OptionPreset, null, true);

            CustomOption.Create(
                (int)CommonOptionKey.UseStrongRandomGen, Design.ColoedString(
                    new Color(204f / 255f, 204f / 255f, 0, 1f),
                    CommonOptionKey.UseStrongRandomGen.ToString()), true);

            CustomOption.Create(
                (int)CommonOptionKey.MinCremateRoles, Design.ColoedString(
                    new Color(204f / 255f, 204f / 255f, 0, 1f),
                    CommonOptionKey.MinCremateRoles.ToString()),
                0, 0, VanillaMaxPlayerNum, 1, null, true);
            CustomOption.Create(
                (int)CommonOptionKey.MaxCremateRoles, Design.ColoedString(
                    new Color(204f / 255f, 204f / 255f, 0, 1f),
                    CommonOptionKey.MaxCremateRoles.ToString()),
                0, 0, VanillaMaxPlayerNum, 1);

            CustomOption.Create(
                (int)CommonOptionKey.MinNeutralRoles, Design.ColoedString(
                    new Color(204f / 255f, 204f / 255f, 0, 1f),
                    CommonOptionKey.MinNeutralRoles.ToString()),
                0, 0, VanillaMaxPlayerNum - 1, 1);
            CustomOption.Create(
                (int)CommonOptionKey.MaxNeutralRoles, Design.ColoedString(
                    new Color(204f / 255f, 204f / 255f, 0, 1f),
                    CommonOptionKey.MaxNeutralRoles.ToString()),
                0, 0, VanillaMaxPlayerNum - 1, 1);

            CustomOption.Create(
                (int)CommonOptionKey.MinImpostorRoles, Design.ColoedString(
                    new Color(204f / 255f, 204f / 255f, 0, 1f),
                    CommonOptionKey.MinImpostorRoles.ToString()),
                0, 0, MaxImposterNum, 1);
            CustomOption.Create(
                (int)CommonOptionKey.MaxImpostorRoles, Design.ColoedString(
                    new Color(204f / 255f, 204f / 255f, 0, 1f),
                    CommonOptionKey.MaxImpostorRoles.ToString()),
                0, 0, MaxImposterNum, 1);

            CustomOption.Create(
                (int)CommonOptionKey.NumMeatings,
                CommonOptionKey.NumMeatings.ToString(),
                10, 0, 100, 1, null, true);

            var ventOption = CustomOption.Create(
                (int)CommonOptionKey.DisableSkipInEmergencyMeeting,
                CommonOptionKey.DisableSkipInEmergencyMeeting.ToString(),
                false);

            var blockMeating = CustomOption.Create(
                (int)CommonOptionKey.DisableSkipInEmergencyMeeting,
                CommonOptionKey.DisableSkipInEmergencyMeeting.ToString(),
                false);
            CustomOption.Create(
                (int)CommonOptionKey.NoVoteToSelf,
                CommonOptionKey.NoVoteToSelf.ToString(),
                false, blockMeating);
            CustomOption.Create(
                (int)CommonOptionKey.HidePlayerName,
                CommonOptionKey.HidePlayerName.ToString(),
                false);
            CustomOption.Create(
                (int)CommonOptionKey.ParallelMedBayScans,
                CommonOptionKey.ParallelMedBayScans.ToString(), false);
            CustomOption.Create(
                (int)CommonOptionKey.RandomMap,
                CommonOptionKey.RandomMap.ToString(), false, null, false);

            CustomOption.Create(
                (int)CommonOptionKey.EngineerUseImpostorVent,
                CommonOptionKey.EngineerUseImpostorVent.ToString(),
                false, ventOption);

            int offset = 50;

            Roles.ExtremeRoleManager.CreateNormalRoleOptions(
                offset);

            offset = 1000;
            Roles.ExtremeRoleManager.CreateCombinationRoleOptions(
                offset);
        }

        public static void CreateConfigOption()
        {
            var config = ExtremeRolesPlugin.Instance.Config;

            JsonConfig.StreamerMode = config.Bind(
                "Custom", "Enable Streamer Mode", false);
            JsonConfig.GhostsSeeTasks = config.Bind(
                "Custom", "Ghosts See Remaining Tasks", true);
            JsonConfig.GhostsSeeRoles = config.Bind(
                "Custom", "Ghosts See Roles", true);
            JsonConfig.GhostsSeeVotes = config.Bind(
                "Custom", "Ghosts See Votes", true);
            JsonConfig.ShowRoleSummary = config.Bind(
                "Custom", "Show Role Summary", true);

            JsonConfig.Ip = config.Bind(
                "Custom", "Custom Server IP", "127.0.0.1");
            JsonConfig.Port = config.Bind(
                "Custom", "Custom Server Port", (ushort)22023);
        }

        public static void Load()
        {

            Map.MaxNumberOfMeetings = Mathf.RoundToInt(
                AllOption[(int)CommonOptionKey.NumMeatings].GetValue());
            Map.BlockSkippingInEmergencyMeetings = AllOption[
                (int)CommonOptionKey.DisableSkipInEmergencyMeeting].GetValue();
            Map.NoVoteIsSelfVote = AllOption[(int)CommonOptionKey.NoVoteToSelf].GetValue();
            Map.HidePlayerNames = AllOption[(int)CommonOptionKey.HidePlayerName].GetValue();
            Map.DisableVent = AllOption[(int)CommonOptionKey.DesableVent].GetValue();

            Client.GhostsSeeRoles = JsonConfig.GhostsSeeRoles.Value;
            Client.GhostsSeeTasks = JsonConfig.GhostsSeeTasks.Value;
            Client.GhostsSeeVotes = JsonConfig.GhostsSeeVotes.Value;
            Client.ShowRoleSummary = JsonConfig.ShowRoleSummary.Value;
            Client.StreamerMode = JsonConfig.StreamerMode.Value;
        }


        public static void SwitchPreset(int newPreset)
        {
            SelectedPreset = newPreset;
            foreach (CustomOptionBase option in AllOption.Values)
            {
                if (option.Id == 0) continue;

                option.Entry = ExtremeRolesPlugin.Instance.Config.Bind(
                    $"Preset{SelectedPreset}",
                    option.Id.ToString(),
                    option.DefaultSelection);
                option.CurSelection = Mathf.Clamp(
                    option.Entry.Value, 0,
                    option.Selections.Length - 1);

                if (option.Behaviour != null && option.Behaviour is StringOption stringOption)
                {
                    stringOption.oldValue = stringOption.Value = option.CurSelection;
                    stringOption.ValueText.text = option.Selections[option.CurSelection].ToString();
                }
            }
        }

        public static void ShareOptionSelections()
        {
            if (PlayerControl.AllPlayerControls.Count <= 1 ||
                AmongUsClient.Instance?.AmHost == false &&
                PlayerControl.LocalPlayer == null) { return; }

            MessageWriter messageWriter = AmongUsClient.Instance.StartRpc(
                PlayerControl.LocalPlayer.NetId,
                (byte)RPCOperator.Command.ShareOption, Hazel.SendOption.Reliable);
            messageWriter.WritePacked((uint)AllOption.Count);
            
            foreach (CustomOptionBase option in AllOption.Values)
            {
                messageWriter.WritePacked((uint)option.Id);
                messageWriter.WritePacked(Convert.ToUInt32(option.CurSelection));
            }
            
            messageWriter.EndMessage();
        }
        public static void ShareOption(int numberOfOptions, MessageReader reader)
        {
            try
            {
                for (int i = 0; i < numberOfOptions; i++)
                {
                    uint optionId = reader.ReadPackedUInt32();
                    uint selection = reader.ReadPackedUInt32();
                    CustomOptionBase option = AllOption.Values.FirstOrDefault(opt => opt.Id == (int)optionId);

                    //FirstOrDefault(option => option.id == (int)optionId);
                    option.UpdateSelection((int)selection);
                }
            }
            catch (Exception e)
            {
                Logging.Error($"Error while deserializing options:{e.Message}");
            }
        }

        public static class JsonConfig
        {
            public static ConfigEntry<bool> GhostsSeeTasks { get; set; }
            public static ConfigEntry<bool> GhostsSeeRoles { get; set; }
            public static ConfigEntry<bool> GhostsSeeVotes { get; set; }
            public static ConfigEntry<bool> ShowRoleSummary { get; set; }
            public static ConfigEntry<bool> StreamerMode { get; set; }
            public static ConfigEntry<string> StreamerModeReplacementText { get; set; }
            public static ConfigEntry<string> StreamerModeReplacementColor { get; set; }
            public static ConfigEntry<string> Ip { get; set; }
            public static ConfigEntry<ushort> Port { get; set; }
        }

        public static class Client
        {
            public static bool GhostsSeeRoles = true;
            public static bool GhostsSeeTasks = true;
            public static bool GhostsSeeVotes = true;
            public static bool ShowRoleSummary = true;
            public static bool StreamerMode = false;
            public static bool AllowParallelMedBayScans = false;
        }

        public static class Map
        {
            public static int MaxNumberOfMeetings = 100;
            public static bool BlockSkippingInEmergencyMeetings = false;
            public static bool NoVoteIsSelfVote = false;
            public static bool HidePlayerNames = false;
            public static bool DisableVent = false;
        }
    }
}
