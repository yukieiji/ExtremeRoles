using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

using Hazel;

using ExtremeRoles.Helper;
using ExtremeRoles.Module;


namespace ExtremeRoles
{
    public class OptionsHolder
    {
        public static string[] Rates = new string[] { "0%", "10%", "20%", "30%", "40%", "50%", "60%", "70%", "80%", "90%", "100%" };
        public static string[] Presets = new string[] { "preset1", "preset2", "preset3", "preset4", "preset5" };

        public static int SelectedPreset = 0;
        public static int VanillaMaxPlayerNum = 15;
        public enum CommonOptionKey
        {
            PresetSelection = 0,
            MinCremateRoles,
            MaxCremateRoles,
            MinNeutralRoles,
            MaxNeutralRoles,
            MinImpostorRoles,
            MaxImpostorRoles,

            NumMeatings = 50,
            DisableSkipInEmergencyMeeting,
            NoVoteToSelf,
            HidePlayerName,
            ParallelMedBayScans,
            RandomMap,
            EngineerUseImpostorVent
        }

        public static Dictionary<int, CustomOption> AllOptions = new Dictionary<int, CustomOption>();

        public static void Load()
        {
            Roles.ExtremeRoleManager.GameRole.Clear();
            AllOptions.Clear();

            CustomOption.Create(
                (int)CommonOptionKey.PresetSelection, Design.Cs(
                    new Color(204f / 255f, 204f / 255f, 0, 1f),
                    CommonOptionKey.PresetSelection.ToString()),
                Presets, null, true);

            CustomOption.Create(
                (int)CommonOptionKey.MinCremateRoles, Design.Cs(
                    new Color(204f / 255f, 204f / 255f, 0, 1f),
                    CommonOptionKey.MinCremateRoles.ToString()),
                0f, 0f, VanillaMaxPlayerNum, 1f, null, true);
            CustomOption.Create(
                (int)CommonOptionKey.MaxCremateRoles, Design.Cs(
                    new Color(204f / 255f, 204f / 255f, 0, 1f),
                    CommonOptionKey.MaxCremateRoles.ToString()),
                0f, 0f, VanillaMaxPlayerNum, 1f);

            CustomOption.Create(
                (int)CommonOptionKey.MinNeutralRoles, Design.Cs(
                    new Color(204f / 255f, 204f / 255f, 0, 1f),
                    CommonOptionKey.MinNeutralRoles.ToString()),
                0f, 0f, VanillaMaxPlayerNum, 1f);
            CustomOption.Create(
                (int)CommonOptionKey.MaxNeutralRoles, Design.Cs(
                    new Color(204f / 255f, 204f / 255f, 0, 1f),
                    CommonOptionKey.MaxNeutralRoles.ToString()),
                0f, 0f, VanillaMaxPlayerNum, 1f);

            CustomOption.Create(
                (int)CommonOptionKey.MinImpostorRoles, Design.Cs(
                    new Color(204f / 255f, 204f / 255f, 0, 1f),
                    CommonOptionKey.MinImpostorRoles.ToString()),
                0f, 0f, 3f, 1f);
            CustomOption.Create(
                (int)CommonOptionKey.MaxImpostorRoles, Design.Cs(
                    new Color(204f / 255f, 204f / 255f, 0, 1f),
                    CommonOptionKey.MaxImpostorRoles.ToString()),
                0f, 0f, 3f, 1f);

            CustomOption.Create(
                (int)CommonOptionKey.NumMeatings,
                CommonOptionKey.NumMeatings.ToString(),
                10, 0, VanillaMaxPlayerNum, 1, null, true);
            var blockMeating = CustomOption.Create(
                (int)CommonOptionKey.DisableSkipInEmergencyMeeting,
                CommonOptionKey.DisableSkipInEmergencyMeeting.ToString(), false);
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
                false);

            int offset = 100;

            Roles.ExtremeRoleManager.CreateNormalRoleOptions(
                offset);
            offset = 300;
            Roles.ExtremeRoleManager.CreateCombinationRoleOptions(
                offset);

        }
        public static void SwitchPreset(int newPreset)
        {
            SelectedPreset = newPreset;
            foreach (CustomOption option in AllOptions.Values)
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
                (byte)CustomRPC.ShareOption, Hazel.SendOption.Reliable);
            messageWriter.WritePacked((uint)AllOptions.Count);
            
            foreach (CustomOption option in AllOptions.Values)
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
                    CustomOption option = AllOptions.Values.FirstOrDefault(opt => opt.Id == (int)optionId);

                    //FirstOrDefault(option => option.id == (int)optionId);
                    option.UpdateSelection((int)selection);
                }
            }
            catch (Exception e)
            {
                Logging.Error($"Error while deserializing options:{e.Message}");
            }
        }
    }
}
