using UnityEngine;

using ExtremeRoles.Module;
using ExtremeRoles.Module.CustomOption;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Performance;

namespace ExtremeRoles.Roles.Solo.Crewmate;

public sealed class Whisper : 
    SingleRoleBase, 
    IRoleUpdate, 
    IRoleMurderPlayerHook, 
    IRoleResetMeeting
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
    private static readonly Vector2 defaultPos = new Vector2(100.0f, 100.0f);

    private TextPopUpper textPopUp;

    public Whisper() : base(
        ExtremeRoleId.Whisper,
        ExtremeRoleType.Crewmate,
        ExtremeRoleId.Whisper.ToString(),
        ColorPalette.WhisperMagenta,
        false, true, false, false)
    { }

    public void ResetOnMeetingEnd(GameData.PlayerInfo exiledPlayer = null)
    {
        return;
    }

    public void ResetOnMeetingStart()
    {
        if (this.textPopUp != null)
        {
            this.textPopUp.Clear();
        }
        if (this.abilityText != null)
        {
            this.abilityText.gameObject.SetActive(false);
        }
    }

    public void HookMuderPlayer(
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
            Helper.Sound.PlaySound(
                Helper.Sound.SoundType.Kill, 0.3f);
        }
    }

    public void Update(PlayerControl rolePlayer)
    {

        if (this.abilityText == null)
        {
            this.abilityText = Object.Instantiate(
                FastDestroyableSingleton<HudManager>.Instance.KillButton.cooldownTimerText,
                Camera.main.transform, false);
            this.abilityText.transform.localPosition = new Vector3(0.0f, 0.0f, -250.0f);
            this.abilityText.enableWordWrapping = false;
        }

        this.abilityText.gameObject.SetActive(false);

        if (Minigame.Instance != null ||
            CachedShipStatus.Instance == null ||
            GameData.Instance == null ||
            MeetingHud.Instance != null ||
            !rolePlayer.CanMove)
        {
            resetAbility(rolePlayer);
            return; 
        }
        if (!CachedShipStatus.Instance.enabled ||
            ExtremeRolesPlugin.ShipState.AssassinMeetingTrigger)
        {
            resetAbility(rolePlayer);
            return; 
        }

        if (this.prevPlayerPos == defaultPos)
        { 
            this.prevPlayerPos = rolePlayer.GetTruePosition();
        }

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
        IOptionInfo parentOps)
    {

        CreateFloatOption(
            WhisperOption.AbilityOffTime,
            2.0f, 1.0f, 5.0f, 0.5f,
            parentOps, format: OptionUnit.Second);

        CreateFloatOption(
            WhisperOption.AbilityOnTime,
            4.0f, 1.0f, 10.0f, 0.5f,
            parentOps, format: OptionUnit.Second);

        CreateFloatOption(
            WhisperOption.TellTextTime,
            3.0f, 1.0f, 25.0f, 0.5f,
            parentOps, format: OptionUnit.Second);

        CreateIntOption(
            WhisperOption.MaxTellText,
            3, 1, 10, 1,
            parentOps);
    }

    protected override void RoleSpecificInit()
    {
        var allOption = OptionManager.Instance;

        this.textPopUp = new TextPopUpper(
            allOption.GetValue<int>(GetRoleOptionId(WhisperOption.MaxTellText)),
            allOption.GetValue<float>(GetRoleOptionId(WhisperOption.TellTextTime)),
            new Vector3(-3.75f, -2.5f, -250.0f),
            TMPro.TextAlignmentOptions.BottomLeft);

        this.abilityOffTime = allOption.GetValue<float>(
            GetRoleOptionId(WhisperOption.AbilityOffTime));
        this.abilityOnTime = allOption.GetValue<float>(GetRoleOptionId(WhisperOption.AbilityOnTime));

        this.prevPlayerPos = defaultPos;
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
