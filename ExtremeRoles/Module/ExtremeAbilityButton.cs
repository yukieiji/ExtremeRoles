using UnityEngine;

using ExtremeRoles.Extension.UnityEvents;
using ExtremeRoles.Extension.Manager;
using ExtremeRoles.Module.AbilityBehavior;
using ExtremeRoles.Module.Interface;
using ExtremeRoles.Performance;

namespace ExtremeRoles.Module;

public enum AbilityState : byte
{
	None = 0,
	Stop,
	CoolDown,
	Ready,
	Activating,
	Reset,
}

#nullable enable

public sealed class ExtremeAbilityButton
{
	public const string Name = "ExRAbilityButton";
	public const string AditionalInfoName = "ExRKillButtonAditionalInfo";

	public AbilityBehaviorBase Behavior { get; private set; }

	public AbilityState State { get; private set; }

	public float Timer { get; private set; } = 10.0f;

	public KeyCode HotKey { private get; set; } = KeyCode.F;

	public Transform Transform => this.button.transform;

	private ActionButton button;

	private bool isShow = true;

	private IButtonAutoActivator activator;

	private readonly Color TimerOnColor = new Color(0f, 0.8f, 0f);

	public ExtremeAbilityButton(
		AbilityBehaviorBase behavior,
		IButtonAutoActivator activator,
		KeyCode hotKey)
	{
		this.State = AbilityState.CoolDown;
		this.activator = activator;
		this.Behavior = behavior;
		this.HotKey = hotKey;

		var hud = FastDestroyableSingleton<HudManager>.Instance;
		var killButton = hud.KillButton;

		this.button = Object.Instantiate(
			killButton, killButton.transform.parent);
		PassiveButton passiveButton = button.GetComponent<PassiveButton>();
		passiveButton.OnClick.RemoveAllPersistentAndListeners();
		passiveButton.OnClick.AddListener(onClick);
		passiveButton.name = Name;

		Transform info = this.button.transform.FindChild(AditionalInfoName);
		if (info != null)
		{
			info.gameObject.SetActive(false);
		}

		this.SetButtonShow(true);

		this.Behavior.Initialize(this.button);
		this.button.graphic.sprite = this.Behavior.Graphic.Img;

		hud.ReGridButtons();
	}

	public bool IsAbilityActive() =>
		this.State == AbilityState.Activating;
	public bool IsAbilityReady() =>
		this.State == AbilityState.Ready;

	public void OnMeetingStart()
	{
		this.Behavior.ForceAbilityOff();
		this.SetButtonShow(false);
	}

	public void OnMeetingEnd()
	{
		this.setStatus(AbilityState.Reset);
		this.SetButtonShow(true);
	}

	public void SetButtonShow(bool isShow)
	{
		this.isShow = isShow;
		setActive(isShow);
	}

	public void SetLabelToCrewmate()
	{
		if (FastDestroyableSingleton<HudManager>.Instance == null) { return; }

		var useButton = FastDestroyableSingleton<HudManager>.Instance.UseButton;

		Object.Destroy(
			this.button.buttonLabelText.fontMaterial);
		this.button.buttonLabelText.fontMaterial = UnityEngine.Object.Instantiate(
			useButton.buttonLabelText.fontMaterial, this.button.transform);
	}

	public void Update()
	{
		if (!this.isShow || this.button == null) { return; }

		setActive(this.activator.IsActive());
		if (!this.button.isActiveAndEnabled) { return; }

		AbilityState newState = this.Behavior.Update(this.State);
		if (newState != this.State)
		{
			setStatus(newState);
		}

		this.button.graphic.sprite = this.Behavior.Graphic.Img;
		this.button.OverrideText(this.Behavior.Graphic.Text);

		if (this.Behavior.IsUse())
		{
			this.button.graphic.color = this.button.buttonLabelText.color = Palette.EnabledColor;
			this.button.graphic.material.SetFloat("_Desat", 0f);
		}
		else
		{
			this.button.graphic.color = this.button.buttonLabelText.color = Palette.DisabledClear;
			this.button.graphic.material.SetFloat("_Desat", 1f);
		}

		switch (this.State)
		{
			case AbilityState.None:
				this.button.cooldownTimerText.color = Palette.EnabledColor;
				this.button.SetCoolDown(0, this.Behavior.CoolTime);
				return;
			case AbilityState.CoolDown:
				// 白色でタイマーをすすめる
				this.Timer -= Time.deltaTime;
				this.button.cooldownTimerText.color = Palette.EnabledColor;

				// クールダウンが明けた
				if (this.Timer <= 0.0f)
				{
					this.setStatus(AbilityState.Ready);
				}
				break;
			case AbilityState.Activating:
				// 緑色でタイマーをすすめる
				this.Timer -= Time.deltaTime;
				this.button.cooldownTimerText.color = TimerOnColor;

				if (!this.Behavior.IsCanAbilityActiving())
				{
					this.Behavior.ForceAbilityOff();
					this.setStatus(AbilityState.Ready);
					return;
				}
				// 能力がアクティブが時間切れなので能力のリセット等を行う
				if (this.Timer <= 0.0f)
				{
					this.Behavior.AbilityOff();
					this.setStatus(AbilityState.CoolDown);
				}
				break;
			case AbilityState.Ready:
				this.Timer = 0.0f;
				if (Input.GetKeyDown(this.HotKey))
				{
					onClick();
				}
				break;
			default:
				break;
		}

		this.button.SetCoolDown(
			this.Timer,
			this.State != AbilityState.Activating ?
			this.Behavior.CoolTime : this.Behavior.ActiveTime);
	}

	private void onClick()
	{
		if (this.Behavior.IsUse() &&
			this.Behavior.TryUseAbility(this.Timer, this.State, out AbilityState newState))
		{
			ExtremeRolesPlugin.Logger.LogInfo($"ExtremeAbilityButton : Using {this.Behavior.Graphic.Text}");
			if (newState == AbilityState.CoolDown)
			{
				this.Behavior.AbilityOff();
			}
			this.setStatus(newState);
		}
	}

	private void setActive(bool active)
	{
		this.button.gameObject.SetActive(active);
		this.button.graphic.enabled = active;
	}

	private void setStatus(AbilityState newState)
	{
		switch (newState)
		{
			case AbilityState.None:
			case AbilityState.Ready:
				this.Timer = 0.0f;
				break;
			case AbilityState.CoolDown:
				if (this.State != AbilityState.Stop)
				{
					this.Timer = this.Behavior.CoolTime;
				}
				break;
			case AbilityState.Activating:
				this.Timer = this.Behavior.ActiveTime;
				break;
			case AbilityState.Reset:
				newState = AbilityState.CoolDown;
				this.Timer = this.Behavior.CoolTime;
				break;
			default:
				break;
		}
		ExtremeRolesPlugin.Logger.LogInfo($"ExtremeAbilityButton : Change To {this.State} => {newState}");
		this.State = newState;
	}
}
