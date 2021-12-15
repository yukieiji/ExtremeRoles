using System.Collections.Generic;
using UnityEngine;

namespace ExtremeRoles
{
    static class MapOption
    {
        // Set values
        public static int MaxNumberOfMeetings = 10;
        public static bool BlockSkippingInEmergencyMeetings = false;
        public static bool NoVoteIsSelfVote = false;
        public static bool HidePlayerNames = false;

        public static bool RandomizeColors = false;
        public static bool AllowDupeNames = false;

        public static int RestrictDevices = 0;
        public static float RestrictAdminTime = 600f;
        public static float RestrictAdminTimeMax = 600f;
        public static float RestrictCamerasTime = 600f;
        public static float RestrictCamerasTimeMax = 600f;
        public static float RestrictVitalsTime = 600f;
        public static float RestrictVitalsTimeMax = 600f;
        public static bool DisableVents = false;

        public static bool GhostsSeeRoles = true;
        public static bool GhostsSeeTasks = true;
        public static bool GhostsSeeVotes = true;
        public static bool ShowRoleSummary = true;
        public static bool AllowParallelMedBayScans = false;

        // Updating values
        public static int MeetingsCount = 0;
        public static List<SurvCamera> CamerasToAdd = new List<SurvCamera>();
        public static List<Vent> VentsToSeal = new List<Vent>();
        public static Dictionary<byte, PoolablePlayer> PlayerIcons = new Dictionary<byte, PoolablePlayer>();

        public static void Init()
        {
            MeetingsCount = 0;
            CamerasToAdd = new List<SurvCamera>();
            VentsToSeal = new List<Vent>();
            PlayerIcons = new Dictionary<byte, PoolablePlayer>();

            var allOption = OptionsHolder.AllOptions;

            MaxNumberOfMeetings = Mathf.RoundToInt(
                allOption[(int)OptionsHolder.CommonOptionKey.NumMeatings].GetSelection());
            BlockSkippingInEmergencyMeetings = allOption[(int)OptionsHolder.CommonOptionKey.DisableSkipInEmergencyMeeting].GetBool();
            NoVoteIsSelfVote = allOption[(int)OptionsHolder.CommonOptionKey.NoVoteToSelf].GetBool();
            HidePlayerNames = allOption[(int)OptionsHolder.CommonOptionKey.HidePlayerName].GetBool();

            GhostsSeeRoles = ExtremeRolesPlugin.GhostsSeeRoles.Value;
            GhostsSeeTasks = ExtremeRolesPlugin.GhostsSeeTasks.Value;
            GhostsSeeVotes = ExtremeRolesPlugin.GhostsSeeVotes.Value;
            ShowRoleSummary = ExtremeRolesPlugin.ShowRoleSummary.Value;
        }

    }
}
