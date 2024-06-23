using UnityEngine;

using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Performance;

using ExtremeRoles.Module.CustomOption;

using ExtremeRoles.Module.NewOption;
using ExtremeRoles.Module.NewOption.Factory;

#nullable enable

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
		EnableAwakeAbility,
		AbilityTaskGage,
    }

    private string curText = string.Empty;
    private bool isAbilityOn;
    private float abilityOnTime;
    private float abilityOffTime;
    private float timer = 0f;

	private bool isEnableAwakeAbility = false;
	private bool isAwake = false;
	private float awakeTaskGage;

	private Vector2 prevPlayerPos;
    private static Vector2 defaultPos => new Vector2(100.0f, 100.0f);

#pragma warning disable CS8618
	private TMPro.TextMeshPro abilityText;
	private TextPopUpper textPopUp;
    public Whisper() : base(
        ExtremeRoleId.Whisper,
        ExtremeRoleType.Crewmate,
        ExtremeRoleId.Whisper.ToString(),
        ColorPalette.WhisperMagenta,
        false, true, false, false)
    { }
#pragma warning restore CS8618

	public void ResetOnMeetingEnd(NetworkedPlayerInfo? exiledPlayer = null)
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
                direction = Translation.GetString("right");
            }
            else if (45.0f < deg && deg <= 135.0f)
            {
                direction = Translation.GetString("up");
            }
            else if (-135.0f < deg && deg <= -45.0f)
            {
                direction = Translation.GetString("down");
            }
            else
            {
                direction = Translation.GetString("left");
            }

			string showText;
			if (this.isEnableAwakeAbility &&
				this.isAwake &&
				Player.TryGetPlayerRoom(target, out SystemTypes? room) &&
				room.HasValue)
			{
				showText = string.Format(
					Translation.GetString("killedTextWithRoom"),
					direction,
					TranslationController.Instance.GetString(room.Value),
					System.DateTime.Now);
			}
			else
			{
				showText = string.Format(
					Translation.GetString("killedText"),
					direction, System.DateTime.Now);
			}
            this.textPopUp.AddText(showText);
			Sound.PlaySound(Sound.Type.Kill, 0.3f);
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

		if (this.isEnableAwakeAbility &&
			!this.isAwake)
		{
			float curTaskGage = Player.GetPlayerTaskGage(rolePlayer);
			if (curTaskGage >= this.awakeTaskGage)
			{
				this.isAwake = true;
			}
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
        AutoParentSetOptionCategoryFactory factory)
    {

        factory.CreateFloatOption(
            WhisperOption.AbilityOffTime,
            2.0f, 1.0f, 5.0f, 0.5f,
            format: OptionUnit.Second);

        factory.CreateFloatOption(
            WhisperOption.AbilityOnTime,
            4.0f, 1.0f, 10.0f, 0.5f,
            format: OptionUnit.Second);

        factory.CreateFloatOption(
            WhisperOption.TellTextTime,
            3.0f, 1.0f, 25.0f, 0.5f,
            format: OptionUnit.Second);

        factory.CreateIntOption(
            WhisperOption.MaxTellText,
            3, 1, 10, 1);
		var awakeOpt = factory.CreateBoolOption(
			WhisperOption.EnableAwakeAbility,
			false);
		factory.CreateIntOption(
			WhisperOption.AbilityTaskGage,
			70, 0, 100, 10,
			awakeOpt,
			format: OptionUnit.Percentage);
	}

    protected override void RoleSpecificInit()
    {
        var allOption = OptionManager.Instance;

        this.textPopUp = new TextPopUpper(
            allOption.GetValue<int>(WhisperOption.MaxTellText)),
            allOption.GetValue<float>(WhisperOption.TellTextTime)),
            new Vector3(-3.75f, -2.5f, -250.0f),
            TMPro.TextAlignmentOptions.BottomLeft);

        this.abilityOffTime = allOption.GetValue<float>(
            WhisperOption.AbilityOffTime));
        this.abilityOnTime = allOption.GetValue<float>(WhisperOption.AbilityOnTime));

		this.isEnableAwakeAbility = allOption.GetValue<bool>(
			WhisperOption.EnableAwakeAbility));
		this.awakeTaskGage = allOption.GetValue<int>(
			WhisperOption.AbilityTaskGage)) / 100.0f;
		this.isAwake = this.isEnableAwakeAbility && this.awakeTaskGage <= 0.0f;

        this.prevPlayerPos = defaultPos;
    }

    private void resetAbility(PlayerControl rolePlayer)
    {
        this.prevPlayerPos = rolePlayer.GetTruePosition();
        this.abilityText.color = Palette.EnabledColor;
        this.curText = Translation.GetString("abilityRemain");
        this.isAbilityOn = false;
        this.timer = this.abilityOffTime;
    }
    private void abilityOn()
    {
        this.curText = Translation.GetString("abilityOnText");
        this.abilityText.color = new Color(0F, 0.8F, 0F);
        this.isAbilityOn = true;
        this.timer = this.abilityOnTime;
    }
}
