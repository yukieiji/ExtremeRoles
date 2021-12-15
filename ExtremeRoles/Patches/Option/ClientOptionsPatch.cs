using UnityEngine;

using HarmonyLib;

namespace ExtremeRoles.Patches.Option
{
    [HarmonyPatch(typeof(OptionsMenuBehaviour), nameof(OptionsMenuBehaviour.Start))]
    public class OptionsMenuBehaviourStartPatch
    {
        private static Vector3? origin;
        private static ToggleButtonBehaviour ghostsSeeTasksButton;
        private static ToggleButtonBehaviour ghostsSeeRolesButton;
        private static ToggleButtonBehaviour ghostsSeeVotesButton;
        private static ToggleButtonBehaviour showRoleSummaryButton;

        public static float xOffset = 1.75f;
        public static float yOffset = -0.25f;

        public static void UpdateButtons()
        {
            UpdateToggle(
                ghostsSeeTasksButton,
                $"{Modules.Translation.GetString("ghostsSeeTasksButton")}: ",
                ExtremeRolesPlugin.GhostsSeeTasks.Value);
            UpdateToggle(ghostsSeeRolesButton,
                $"{Modules.Translation.GetString("ghostsSeeRolesButton")}: ",
                ExtremeRolesPlugin.GhostsSeeRoles.Value);
            UpdateToggle(
                ghostsSeeVotesButton,
                $"{Modules.Translation.GetString("ghostsSeeVotesButton")}: ",
                ExtremeRolesPlugin.GhostsSeeVotes.Value);
            UpdateToggle(showRoleSummaryButton,
                $"{Modules.Translation.GetString("showRoleSummaryButton")}: ",
                ExtremeRolesPlugin.ShowRoleSummary.Value);
        }

        private static void UpdateToggle(ToggleButtonBehaviour button, string text, bool on)
        {
            if (button == null || button.gameObject == null) { return; }

            Color color = on ? new Color(0f, 1f, 0.16470589f, 1f) : Color.white;
            button.Background.color = color;
            button.Text.text = $"{text}{(on ? Modules.Translation.GetString("optionOn") : Modules.Translation.GetString("optionOff"))}";
            if (button.Rollover) button.Rollover.ChangeOutColor(color);
        }

        private static ToggleButtonBehaviour CreateCustomToggle(string text, bool on, Vector3 offset, UnityEngine.Events.UnityAction onClick, OptionsMenuBehaviour __instance)
        {
            if (__instance.CensorChatButton != null)
            {
                var button = UnityEngine.Object.Instantiate(__instance.CensorChatButton, __instance.CensorChatButton.transform.parent);
                button.transform.localPosition = (origin ?? Vector3.zero) + offset;
                PassiveButton passiveButton = button.GetComponent<PassiveButton>();
                passiveButton.OnClick = new UnityEngine.UI.Button.ButtonClickedEvent();
                passiveButton.OnClick.AddListener(onClick);
                UpdateToggle(button, text, on);

                return button;
            }
            return null;
        }

        public static void Postfix(OptionsMenuBehaviour __instance)
        {
            if (__instance.CensorChatButton != null)
            {
                if (origin == null) origin = __instance.CensorChatButton.transform.localPosition + Vector3.up * 0.075f;
                __instance.CensorChatButton.transform.localPosition = origin.Value + Vector3.left * xOffset;
                __instance.CensorChatButton.transform.localScale = Vector3.one * 0.5f;
            }

            if ((ghostsSeeTasksButton == null || ghostsSeeTasksButton.gameObject == null))
            {
                ghostsSeeTasksButton = CreateCustomToggle(
                    $"", ExtremeRolesPlugin.GhostsSeeTasks.Value, Vector3.right * xOffset,
                    (UnityEngine.Events.UnityAction)ghostsSeeTaskToggle, __instance);

                void ghostsSeeTaskToggle()
                {
                    ExtremeRolesPlugin.GhostsSeeTasks.Value = !ExtremeRolesPlugin.GhostsSeeTasks.Value;
                    MapOption.GhostsSeeTasks = ExtremeRolesPlugin.GhostsSeeTasks.Value;
                    UpdateButtons();
                }
            }

            if ((ghostsSeeRolesButton == null || ghostsSeeRolesButton.gameObject == null))
            {
                ghostsSeeRolesButton = CreateCustomToggle(
                    $"", ExtremeRolesPlugin.GhostsSeeRoles.Value, new Vector2(-xOffset, yOffset),
                    (UnityEngine.Events.UnityAction)ghostsSeeRolesToggle, __instance);

                void ghostsSeeRolesToggle()
                {
                    ExtremeRolesPlugin.GhostsSeeRoles.Value = !ExtremeRolesPlugin.GhostsSeeRoles.Value;
                    MapOption.GhostsSeeRoles = ExtremeRolesPlugin.GhostsSeeRoles.Value;
                    UpdateButtons();
                }
            }

            if ((ghostsSeeVotesButton == null || ghostsSeeVotesButton.gameObject == null))
            {
                ghostsSeeVotesButton = CreateCustomToggle(
                    $"", ExtremeRolesPlugin.GhostsSeeVotes.Value, new Vector2(0, yOffset),
                    (UnityEngine.Events.UnityAction)ghostsSeeVotesToggle, __instance);

                void ghostsSeeVotesToggle()
                {
                    ExtremeRolesPlugin.GhostsSeeVotes.Value = !ExtremeRolesPlugin.GhostsSeeVotes.Value;
                    MapOption.GhostsSeeVotes = ExtremeRolesPlugin.GhostsSeeVotes.Value;
                    UpdateButtons();
                }
            }

            if ((showRoleSummaryButton == null || showRoleSummaryButton.gameObject == null))
            {
                showRoleSummaryButton = CreateCustomToggle(
                    $"", ExtremeRolesPlugin.ShowRoleSummary.Value, new Vector2(xOffset, yOffset),
                    (UnityEngine.Events.UnityAction)showRoleSummaryToggle, __instance);

                void showRoleSummaryToggle()
                {
                    ExtremeRolesPlugin.ShowRoleSummary.Value = !ExtremeRolesPlugin.ShowRoleSummary.Value;
                    MapOption.ShowRoleSummary = ExtremeRolesPlugin.ShowRoleSummary.Value;
                    UpdateButtons();
                }
            }

            UpdateButtons();
        }
    }
}
