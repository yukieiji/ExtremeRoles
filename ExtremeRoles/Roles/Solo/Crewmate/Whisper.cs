using System.Collections;

using UnityEngine;

using ExtremeRoles.Module;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;

using BepInEx.IL2CPP.Utils.Collections;

namespace ExtremeRoles.Roles.Solo.Crewmate
{
    public class Whisper : SingleRoleBase, IRoleUpdate, IRoleMurderPlayerHock
    {
        public Whisper() : base(
            ExtremeRoleId.Whisper,
            ExtremeRoleType.Crewmate,
            ExtremeRoleId.Whisper.ToString(),
            ColorPalette.WatchdogViolet,
            false, true, false, false)
        { }

        private string curText = string.Empty;
        private bool isAbilityOn;
        private float timer = 0f;
        private int showTextCount = 0;
        private TMPro.TextMeshPro abilityText;
        private TMPro.TextMeshPro tellText;
        private Vector2 prevPlayerPos;

        private TextPoper textGen;

        public void HockMuderPlayer(
            PlayerControl source,
            PlayerControl target)
        {
            /*
            if (this.isAbilityOn)
            {
                PlayerControl.LocalPlayer.StartCoroutine(
                    showText().WrapToIl2Cpp());
            }
            */
        }

        public void Update(PlayerControl rolePlayer)
        {
            this.textGen.Update();
            if (Input.GetKeyDown(KeyCode.U))
            {
                this.textGen.AddText("TEEEEEEEEEEEEEEEEEEEEEEEEEEEEEST");
            }

            if (this.abilityText == null)
            {
                this.abilityText = Object.Instantiate(
                    HudManager.Instance.KillButton.cooldownTimerText,
                    Camera.main.transform, false);
                this.abilityText.transform.localPosition = new Vector3(0.0f, 0.0f, -250.0f);
            }

            this.abilityText.gameObject.SetActive(false);

            if (ShipStatus.Instance == null ||
                GameData.Instance == null ||
                MeetingHud.Instance != null)
            {
                resetAbility();
                return; 
            }
            if (!ShipStatus.Instance.enabled ||
                ExtremeRolesPlugin.GameDataStore.AssassinMeetingTrigger)
            {
                resetAbility();
                return; 
            }

            if (prevPlayerPos == null) { this.prevPlayerPos = rolePlayer.GetTruePosition(); }

            if (this.prevPlayerPos != rolePlayer.GetTruePosition())
            {
                resetAbility();
                return; 
            }

            this.timer -= Time.deltaTime;

            if (this.timer < 0)
            {
                if (this.isAbilityOn)
                {
                    resetAbility();
                }
                else
                {
                    abilityOn();
                }
            }

            this.abilityText.text = string.Format(
                this.curText, this.timer);
            this.abilityText.gameObject.SetActive(true);

        }

        protected override void CreateSpecificOption(
            CustomOptionBase parentOps)
        {
            
        }

        protected override void RoleSpecificInit()
        {
            this.textGen = new TextPoper(
                3, 3.0f, new Vector3(-4.0f, -2.75f, -250.0f),
                TMPro.TextAlignmentOptions.BottomLeft);
        }

        private void resetAbility()
        {
            this.curText = Helper.Translation.GetString(
                "abilityRemain");
            this.isAbilityOn = false;
            this.timer = 3f;
        }
        private void abilityOn()
        {
            this.curText = Helper.Translation.GetString(
                "abilityOnText");
            this.isAbilityOn = true;
            this.timer = 3f;
        }

        private IEnumerator showText()
        {
            if (this.tellText == null)
            {
                this.tellText = Object.Instantiate(
                    Prefab.Text, Camera.main.transform, false);
                this.tellText.transform.localPosition = new Vector3(-4.0f, -2.75f, -250.0f);
                this.tellText.alignment = TMPro.TextAlignmentOptions.BottomLeft;
                this.tellText.gameObject.layer = 5;
                this.tellText.text = Helper.Translation.GetString("departureText");
            }
            this.tellText.gameObject.SetActive(true);

            yield return new WaitForSeconds(3.5f);

            this.tellText.gameObject.SetActive(false);

        }

    }
}
