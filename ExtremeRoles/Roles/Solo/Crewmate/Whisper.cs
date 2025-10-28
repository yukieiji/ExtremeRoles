using UnityEngine;

using ExtremeRoles.Extension.Vector;
using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.SystemType;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;


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
		RoleCore.BuildCrewmate(
			ExtremeRoleId.Whisper,
			ColorPalette.WhisperMagenta),
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
        if (!this.isAbilityOn || MeetingHud.Instance != null)
        {
			return;
        }

		Vector2 diff = target.GetTruePosition() - PlayerControl.LocalPlayer.GetTruePosition();
		diff.Normalize();
		float rad = Mathf.Atan2(diff.y, diff.x);
		float deg = rad * Mathf.Rad2Deg;

		string direction;

		if (-45.0f < deg && deg <= 45.0f)
		{
			direction = Tr.GetString("right");
		}
		else if (45.0f < deg && deg <= 135.0f)
		{
			direction = Tr.GetString("up");
		}
		else if (-135.0f < deg && deg <= -45.0f)
		{
			direction = Tr.GetString("down");
		}
		else
		{
			direction = Tr.GetString("left");
		}

		string showText;
		if (this.isEnableAwakeAbility &&
			this.isAwake &&
			Player.TryGetPlayerRoom(target, out SystemTypes? room) &&
			room.HasValue)
		{
			showText = string.Format(
				Tr.GetString("killedTextWithRoom"),
				direction,
				TranslationController.Instance.GetString(room.Value),
				System.DateTime.Now);
		}
		else
		{
			showText = string.Format(
				Tr.GetString("killedText"),
				direction, System.DateTime.Now);
		}
		this.textPopUp.AddText(showText);
		Sound.PlaySound(Sound.Type.Kill, 0.3f);
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

        if (!GameProgressSystem.IsTaskPhase ||
			Minigame.Instance != null ||
            !rolePlayer.CanMove)
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

        if (this.prevPlayerPos.IsCloseTo(defaultPos, 0.1f))
        {
            this.prevPlayerPos = rolePlayer.GetTruePosition();
        }

        if (this.prevPlayerPos.IsNotCloseTo(rolePlayer.GetTruePosition(), 0.1f))
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
        var loader = this.Loader;

        this.textPopUp = new TextPopUpper(
            loader.GetValue<WhisperOption, int>(WhisperOption.MaxTellText),
            loader.GetValue<WhisperOption, float>(WhisperOption.TellTextTime),
            new Vector3(-3.75f, -2.5f, -250.0f),
            TMPro.TextAlignmentOptions.BottomLeft);

        this.abilityOffTime = loader.GetValue<WhisperOption, float>(
            WhisperOption.AbilityOffTime);
        this.abilityOnTime = loader.GetValue<WhisperOption, float>(WhisperOption.AbilityOnTime);

		this.isEnableAwakeAbility = loader.GetValue<WhisperOption, bool>(
			WhisperOption.EnableAwakeAbility);
		this.awakeTaskGage = loader.GetValue<WhisperOption, int>(
			WhisperOption.AbilityTaskGage) / 100.0f;
		this.isAwake = this.isEnableAwakeAbility && this.awakeTaskGage <= 0.0f;

        this.prevPlayerPos = defaultPos;
    }

    private void resetAbility(PlayerControl rolePlayer)
    {
        this.prevPlayerPos = rolePlayer.GetTruePosition();
        this.abilityText.color = Palette.EnabledColor;
        this.curText = Tr.GetString("abilityRemain");
        this.isAbilityOn = false;
        this.timer = this.abilityOffTime;
    }
    private void abilityOn()
    {
        this.curText = Tr.GetString("abilityOnText");
        this.abilityText.color = new Color(0F, 0.8F, 0F);
        this.isAbilityOn = true;
        this.timer = this.abilityOnTime;
    }
}
