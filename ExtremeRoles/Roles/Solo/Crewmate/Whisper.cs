using UnityEngine;

using ExtremeRoles.Module;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;

namespace ExtremeRoles.Roles.Solo.Crewmate
{
    public class Whisper : SingleRoleBase, IRoleUpdate, IRoleMurderPlayerHock, IRoleResetMeeting
    {

        public enum WhisperOption
        {
            AbilityOffTime,
            AbilityOnTime,
            TellTextTime,
            MaxTellText,
        }

        private string curText = string.Empty;
        private bool isAbilityOn;
        private float abilityOnTime;
        private float abilityOffTime;
        private float timer = 0f;
        private TMPro.TextMeshPro abilityText;
        private Vector2 prevPlayerPos;

        private TextPopUpper textPopUp;

        public Whisper() : base(
            ExtremeRoleId.Whisper,
            ExtremeRoleType.Crewmate,
            ExtremeRoleId.Whisper.ToString(),
            ColorPalette.WhisperMagenta,
            false, true, false, false)
        { }

        public void ResetOnMeetingEnd()
        {
            return;
        }

        public void ResetOnMeetingStart()
        {
            if (this.textPopUp != null)
            {
                this.textPopUp.Clear();
            }
        }

        public void HockMuderPlayer(
            PlayerControl source,
            PlayerControl target)
        {
            if (this.isAbilityOn)
            {

                Vector2 diff = target.GetTruePosition() - PlayerControl.LocalPlayer.GetTruePosition();
                diff.Normalize();
                float rad = Mathf.Atan2(diff.y, diff.x);
                float deg = rad * (360 / ((float)System.Math.PI * 2));

                string direction;
                
                if (-45.0f < deg && deg <= 45.0f )
                {
                    direction = Helper.Translation.GetString(
                        "right");
                }
                else if (45.0f < deg && deg <= 135.0f)
                {
                    direction = Helper.Translation.GetString(
                        "up");
                }
                else if (-135.0f < deg && deg <= -45.0f)
                {
                    direction = Helper.Translation.GetString(
                        "down");
                }
                else
                {
                    direction = Helper.Translation.GetString(
                        "left");
                }

                this.textPopUp.AddText(
                    string.Format(
                        Helper.Translation.GetString("killedText"),
                        direction, System.DateTime.Now));
            }
        }

        public void Update(PlayerControl rolePlayer)
        {

            if (this.abilityText == null)
            {
                this.abilityText = Object.Instantiate(
                    HudManager.Instance.KillButton.cooldownTimerText,
                    Camera.main.transform, false);
                this.abilityText.transform.localPosition = new Vector3(0.0f, 0.0f, -250.0f);
                this.abilityText.enableWordWrapping = false;
            }

            this.abilityText.gameObject.SetActive(false);

            if (Minigame.Instance != null ||
                ShipStatus.Instance == null ||
                GameData.Instance == null ||
                MeetingHud.Instance != null ||
                !rolePlayer.CanMove)
            {
                resetAbility(rolePlayer);
                return; 
            }
            if (!ShipStatus.Instance.enabled ||
                ExtremeRolesPlugin.GameDataStore.AssassinMeetingTrigger)
            {
                resetAbility(rolePlayer);
                return; 
            }

            if (prevPlayerPos == null) { this.prevPlayerPos = rolePlayer.GetTruePosition(); }

            if (this.prevPlayerPos != rolePlayer.GetTruePosition())
            {
                resetAbility(rolePlayer);
                return; 
            }

            this.timer -= Time.deltaTime;

            if (this.timer < 0)
            {
                if (this.isAbilityOn)
                {
                    resetAbility(rolePlayer);
                }
                else
                {
                    abilityOn();
                }
            }

            this.prevPlayerPos = rolePlayer.GetTruePosition();

            this.abilityText.text = string.Format(
                this.curText,
                Mathf.CeilToInt(this.timer));
            this.abilityText.gameObject.SetActive(true);
        }

        protected override void CreateSpecificOption(
            CustomOptionBase parentOps)
        {
            CustomOption.Create(
                GetRoleOptionId(WhisperOption.AbilityOffTime),
                string.Concat(
                    this.RoleName,
                    WhisperOption.AbilityOffTime.ToString()),
                3.0f, 1.0f, 5.0f, 0.5f,
                parentOps, format: OptionUnit.Second);
            CustomOption.Create(
                GetRoleOptionId(WhisperOption.AbilityOnTime),
                string.Concat(
                    this.RoleName,
                    WhisperOption.AbilityOnTime.ToString()),
                4.0f, 1.0f, 10.0f, 0.5f,
                parentOps, format: OptionUnit.Second);
            CustomOption.Create(
                GetRoleOptionId(WhisperOption.TellTextTime),
                string.Concat(
                    this.RoleName,
                    WhisperOption.TellTextTime.ToString()),
                3.0f, 1.0f, 25.0f, 0.5f,
                parentOps, format: OptionUnit.Second);
            CustomOption.Create(
                GetRoleOptionId(WhisperOption.MaxTellText),
                string.Concat(
                    this.RoleName,
                    WhisperOption.MaxTellText.ToString()),
                3, 1, 10, 1,
                parentOps);
        }

        protected override void RoleSpecificInit()
        {
            var allOption = OptionHolder.AllOption;

            this.textPopUp = new TextPopUpper(
                allOption[GetRoleOptionId(WhisperOption.MaxTellText)].GetValue(),
                allOption[GetRoleOptionId(WhisperOption.TellTextTime)].GetValue(),
                new Vector3(-4.0f, -2.75f, -250.0f),
                TMPro.TextAlignmentOptions.BottomLeft);

            this.abilityOffTime = allOption[GetRoleOptionId(WhisperOption.AbilityOffTime)].GetValue();
            this.abilityOnTime = allOption[GetRoleOptionId(WhisperOption.AbilityOnTime)].GetValue();

        }

        private void resetAbility(PlayerControl rolePlayer)
        {
            this.prevPlayerPos = rolePlayer.GetTruePosition();
            this.abilityText.color = Palette.EnabledColor;
            this.curText = Helper.Translation.GetString(
                "abilityRemain");
            this.isAbilityOn = false;
            this.timer = this.abilityOffTime;
        }
        private void abilityOn()
        {
            this.curText = Helper.Translation.GetString(
                "abilityOnText");
            this.abilityText.color = new Color(0F, 0.8F, 0F);
            this.isAbilityOn = true;
            this.timer = this.abilityOnTime;
        }
    }
}
