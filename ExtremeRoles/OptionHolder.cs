using System;
using System.Collections.Concurrent;
using System.Linq;

using UnityEngine;

using Hazel;
using BepInEx.Configuration;

using ExtremeRoles.Helper;
using ExtremeRoles.Module;

namespace ExtremeRoles
{
    public static class OptionHolder
    {

        public const int VanillaMaxPlayerNum = 15;
        public const int MaxImposterNum = 3;
        private const int chunkSize = 50;

        public static string[] SpawnRate = new string[] {
            "0%", "10%", "20%", "30%", "40%",
            "50%", "60%", "70%", "80%", "90%", "100%" };
        public static string[] OptionPreset = new string[] { 
            "preset1", "preset2", "preset3", "preset4", "preset5",
            "preset6", "preset7", "preset8", "preset9", "preset10" };
        public static string[] Range = new string[] { "short", "middle", "long"};

        public static int OptionsPage = 1;
        public static int SelectedPreset = 0;

        private static IRegionInfo[] defaultRegion;

        public static string ConfigPreset
        {
            get => $"Preset:{SelectedPreset}";
        }

        public enum CommonOptionKey
        {
            PresetSelection = 0,

            UseStrongRandomGen,

            MinCrewmateRoles,
            MaxCrewmateRoles,
            MinNeutralRoles,
            MaxNeutralRoles,
            MinImpostorRoles,
            MaxImpostorRoles,

            NumMeating,
            DisableSkipInEmergencyMeeting,
            DisableSelfVote,
            DesableVent,
            EngineerUseImpostorVent,
            CanKillVentInPlayer,
            ParallelMedBayScans,
            RandomMap,
            IsSameNeutralSameWin,
            DisableNeutralSpecialForceEnd,
            EnableHorseMode,

            IsBlockNeutralAssignToVanillaCrewGhostRole,
            IsRemoveAngleIcon,
        }

        public static ConcurrentDictionary<int, CustomOptionBase> AllOption = new ConcurrentDictionary<int, CustomOptionBase>();

        public static void Create()
        {

            defaultRegion = ServerManager.DefaultRegions;

            CreateConfigOption();

            Roles.ExtremeRoleManager.GameRole.Clear();
            AllOption.Clear();

            new SelectionCustomOption(
                (int)CommonOptionKey.PresetSelection, Design.ColoedString(
                    new Color(204f / 255f, 204f / 255f, 0, 1f),
                    CommonOptionKey.PresetSelection.ToString()),
                OptionPreset, null, true);


            new BoolCustomOption(
               (int)CommonOptionKey.UseStrongRandomGen, Design.ColoedString(
                    new Color(204f / 255f, 204f / 255f, 0, 1f),
                    CommonOptionKey.UseStrongRandomGen.ToString()), true);

            new IntCustomOption(
                (int)CommonOptionKey.MinCrewmateRoles, Design.ColoedString(
                    new Color(204f / 255f, 204f / 255f, 0, 1f),
                    CommonOptionKey.MinCrewmateRoles.ToString()),
                0, 0, (VanillaMaxPlayerNum - 1) * 2, 1, null, true);
            new IntCustomOption(
                (int)CommonOptionKey.MaxCrewmateRoles, Design.ColoedString(
                    new Color(204f / 255f, 204f / 255f, 0, 1f),
                    CommonOptionKey.MaxCrewmateRoles.ToString()),
                0, 0, (VanillaMaxPlayerNum - 1) * 2, 1);

            new IntCustomOption(
                (int)CommonOptionKey.MinNeutralRoles, Design.ColoedString(
                    new Color(204f / 255f, 204f / 255f, 0, 1f),
                    CommonOptionKey.MinNeutralRoles.ToString()),
                0, 0, (VanillaMaxPlayerNum - 2) * 2, 1);
            new IntCustomOption(
                (int)CommonOptionKey.MaxNeutralRoles, Design.ColoedString(
                    new Color(204f / 255f, 204f / 255f, 0, 1f),
                    CommonOptionKey.MaxNeutralRoles.ToString()),
                0, 0, (VanillaMaxPlayerNum - 2) * 2, 1);

            new IntCustomOption(
                (int)CommonOptionKey.MinImpostorRoles, Design.ColoedString(
                    new Color(204f / 255f, 204f / 255f, 0, 1f),
                    CommonOptionKey.MinImpostorRoles.ToString()),
                0, 0, MaxImposterNum * 2, 1);
            new IntCustomOption(
                (int)CommonOptionKey.MaxImpostorRoles, Design.ColoedString(
                    new Color(204f / 255f, 204f / 255f, 0, 1f),
                    CommonOptionKey.MaxImpostorRoles.ToString()),
                0, 0, MaxImposterNum * 2, 1);

            new IntCustomOption(
                (int)CommonOptionKey.NumMeating,
                CommonOptionKey.NumMeating.ToString(),
                10, 0, 100, 1, null, true);

            new BoolCustomOption(
                (int)CommonOptionKey.DisableSkipInEmergencyMeeting,
                CommonOptionKey.DisableSkipInEmergencyMeeting.ToString(),
                false);
            new BoolCustomOption(
                (int)CommonOptionKey.DisableSelfVote,
                CommonOptionKey.DisableSelfVote.ToString(),
                false);

            var ventOption = new BoolCustomOption(
               (int)CommonOptionKey.DesableVent,
               CommonOptionKey.DesableVent.ToString(),
               false);
            new BoolCustomOption(
                (int)CommonOptionKey.CanKillVentInPlayer,
                CommonOptionKey.CanKillVentInPlayer.ToString(),
                false, ventOption, invert: true);
            new BoolCustomOption(
                (int)CommonOptionKey.EngineerUseImpostorVent,
                CommonOptionKey.EngineerUseImpostorVent.ToString(),
                false, ventOption, invert: true);

            new BoolCustomOption(
                (int)CommonOptionKey.ParallelMedBayScans,
                CommonOptionKey.ParallelMedBayScans.ToString(), false);
            new BoolCustomOption(
                (int)CommonOptionKey.RandomMap,
                CommonOptionKey.RandomMap.ToString(), false);

            new BoolCustomOption(
                (int)CommonOptionKey.IsSameNeutralSameWin,
                CommonOptionKey.IsSameNeutralSameWin.ToString(),
                true);
            new BoolCustomOption(
                (int)CommonOptionKey.DisableNeutralSpecialForceEnd,
                CommonOptionKey.DisableNeutralSpecialForceEnd.ToString(),
                false);

            new BoolCustomOption(
                (int)CommonOptionKey.EnableHorseMode,
                CommonOptionKey.EnableHorseMode.ToString(),
                false);

            new BoolCustomOption(
                (int)CommonOptionKey.IsBlockNeutralAssignToVanillaCrewGhostRole,
                CommonOptionKey.IsBlockNeutralAssignToVanillaCrewGhostRole.ToString(),
                true);
            new BoolCustomOption(
                (int)CommonOptionKey.IsRemoveAngleIcon,
                CommonOptionKey.IsRemoveAngleIcon.ToString(),
                false);

            int offset = 50;

            Roles.ExtremeRoleManager.CreateNormalRoleOptions(
                offset);

            offset = 5000;
            Roles.ExtremeRoleManager.CreateCombinationRoleOptions(
                offset);
        }

        public static void CreateConfigOption()
        {
            var config = ExtremeRolesPlugin.Instance.Config;

            ConfigParser.StreamerMode = config.Bind(
                "ClientOption", "StreamerMode", false);
            ConfigParser.GhostsSeeTasks = config.Bind(
                "ClientOption", "GhostCanSeeRemainingTasks", true);
            ConfigParser.GhostsSeeRoles = config.Bind(
                "ClientOption", "GhostCanSeeRoles", true);
            ConfigParser.GhostsSeeVotes = config.Bind(
                "ClientOption", "GhostCanSeeVotes", true);
            ConfigParser.ShowRoleSummary = config.Bind(
                "ClientOption", "IsShowRoleSummary", true);
            ConfigParser.HideNamePlate = config.Bind(
                "ClientOption", "IsHideNamePlate", false);

            ConfigParser.StreamerModeReplacementText = config.Bind(
                "ClientOption",
                "ReplacementRoomCodeText",
                "\n\nPlaying with Extreme Roles");

            ConfigParser.Ip = config.Bind(
                "ClientOption", "CustomServerIP", "127.0.0.1");
            ConfigParser.Port = config.Bind(
                "ClientOption", "CustomServerPort", (ushort)22023);
        }

        public static void Load()
        {

            Ship.MaxNumberOfMeeting = Mathf.RoundToInt(
                AllOption[(int)CommonOptionKey.NumMeating].GetValue());
            Ship.AllowParallelMedBayScan = AllOption[
                (int)CommonOptionKey.ParallelMedBayScans].GetValue();
            Ship.BlockSkippingInEmergencyMeeting = AllOption[
                (int)CommonOptionKey.DisableSkipInEmergencyMeeting].GetValue();
            Ship.DisableVent = AllOption[
                (int)CommonOptionKey.DesableVent].GetValue();
            Ship.CanKillVentInPlayer = AllOption[
                (int)CommonOptionKey.CanKillVentInPlayer].GetValue();
            Ship.EngineerUseImpostorVent = AllOption[
                (int)CommonOptionKey.EngineerUseImpostorVent].GetValue();
            Ship.DisableSelfVote = AllOption[
                (int)CommonOptionKey.DisableSelfVote].GetValue();
            Ship.IsSameNeutralSameWin = AllOption[
                (int)CommonOptionKey.IsSameNeutralSameWin].GetValue();
            Ship.DisableNeutralSpecialForceEnd = AllOption[
                (int)CommonOptionKey.DisableNeutralSpecialForceEnd].GetValue();

            Ship.IsBlockNeutralAssignToVanillaCrewGhostRole = AllOption[
                (int)CommonOptionKey.IsBlockNeutralAssignToVanillaCrewGhostRole].GetValue();
            Ship.IsRemoveAngleIcon = AllOption[
                (int)CommonOptionKey.IsRemoveAngleIcon].GetValue();

            Client.StreamerMode = ConfigParser.StreamerMode.Value;
            Client.GhostsSeeRole = ConfigParser.GhostsSeeRoles.Value;
            Client.GhostsSeeTask = ConfigParser.GhostsSeeTasks.Value;
            Client.GhostsSeeVote = ConfigParser.GhostsSeeVotes.Value;
            Client.ShowRoleSummary = ConfigParser.ShowRoleSummary.Value;
            Client.HideNamePlate = ConfigParser.HideNamePlate.Value;
        }


        public static void SwitchPreset(int newPreset)
        {
            SelectedPreset = newPreset;
            foreach (CustomOptionBase option in AllOption.Values)
            {
                if (option.Id == 0) { continue; }

                option.Entry = ExtremeRolesPlugin.Instance.Config.Bind(
                    ConfigPreset,
                    option.CleanedName,
                    option.DefaultSelection);
                option.CurSelection = Mathf.Clamp(
                    option.Entry.Value, 0,
                    option.Selections.Length - 1);

                if (option.Behaviour != null && option.Behaviour is StringOption stringOption)
                {
                    stringOption.oldValue = stringOption.Value = option.CurSelection;
                    stringOption.ValueText.text = option.GetString();
                }
            }
        }

        public static void ShareOptionSelections()
        {
            if (PlayerControl.AllPlayerControls.Count <= 1 ||
                AmongUsClient.Instance?.AmHost == false &&
                PlayerControl.LocalPlayer == null) { return; }

            var splitOption = AllOption.Select((x, i) =>
                new { data = x, indexgroup = i / chunkSize })
                .GroupBy(x => x.indexgroup, x => x.data)
                .Select(y => y.Select(x => x));

            foreach (var chunkedOption in splitOption)
            {
                MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(
                    PlayerControl.LocalPlayer.NetId,
                    (byte)RPCOperator.Command.ShareOption,
                    Hazel.SendOption.Reliable);

                writer.Write((byte)chunkedOption.Count());
                foreach (var (id, option) in chunkedOption)
                {
                    writer.WritePacked(id);
                    writer.WritePacked(option.CurSelection);
                }
                AmongUsClient.Instance.FinishRpcImmediately(writer);
            }
        }
        public static void ShareOption(int numberOfOptions, MessageReader reader)
        {
            try
            {
                for (int i = 0; i < numberOfOptions; i++)
                {
                    int optionId = reader.ReadPackedInt32();
                    int selection = reader.ReadPackedInt32();
                    AllOption[optionId].UpdateSelection(selection);
                }
            }
            catch (Exception e)
            {
                Logging.Error($"Error while deserializing options:{e.Message}");
            }
        }

        public static void UpdateRegion()
        {
            ServerManager serverManager = DestroyableSingleton<ServerManager>.Instance;
            IRegionInfo[] regions = defaultRegion;

            var CustomRegion = new DnsRegionInfo(
                ConfigParser.Ip.Value,
                "custom",
                StringNames.NoTranslation,
                ConfigParser.Ip.Value,
                ConfigParser.Port.Value,
                false);
            regions = regions.Concat(new IRegionInfo[] { CustomRegion.Cast<IRegionInfo>() }).ToArray();
            ServerManager.DefaultRegions = regions;
            serverManager.AvailableRegions = regions;
        }

        public static class ConfigParser
        {
            public static ConfigEntry<bool> GhostsSeeTasks { get; set; }
            public static ConfigEntry<bool> GhostsSeeRoles { get; set; }
            public static ConfigEntry<bool> GhostsSeeVotes { get; set; }
            public static ConfigEntry<bool> ShowRoleSummary { get; set; }
            public static ConfigEntry<bool> StreamerMode { get; set; }
            public static ConfigEntry<bool> HideNamePlate { get; set; }
            public static ConfigEntry<string> StreamerModeReplacementText { get; set; }
            public static ConfigEntry<string> Ip { get; set; }
            public static ConfigEntry<ushort> Port { get; set; }
        }

        public static class Client
        {
            public static bool GhostsSeeRole = true;
            public static bool GhostsSeeTask = true;
            public static bool GhostsSeeVote = true;
            public static bool ShowRoleSummary = true;
            public static bool StreamerMode = false;
            public static bool HideNamePlate = false;
        }

        public static class Ship
        {
            public static int MaxNumberOfMeeting = 100;
            public static bool AllowParallelMedBayScan = false;
            public static bool BlockSkippingInEmergencyMeeting = false;
            public static bool DisableVent = false;
            public static bool EngineerUseImpostorVent = false;
            public static bool CanKillVentInPlayer = false;
            public static bool DisableSelfVote = false;
            public static bool IsSameNeutralSameWin = true;
            public static bool DisableNeutralSpecialForceEnd = false;

            public static bool IsBlockNeutralAssignToVanillaCrewGhostRole = true;
            public static bool IsRemoveAngleIcon = false;
        }
    }
}
