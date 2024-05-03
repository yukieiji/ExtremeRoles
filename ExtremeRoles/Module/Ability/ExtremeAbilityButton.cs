using UnityEngine;

using ExtremeRoles.Extension.UnityEvents;
using ExtremeRoles.Extension.Manager;
using ExtremeRoles.Module.Interface;
using ExtremeRoles.Performance;
using ExtremeRoles.Module.Ability.Behavior;

namespace ExtremeRoles.Module.Ability;

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

	public BehaviorBase Behavior { get; private set; }

	public AbilityState State { get; private set; }

	public float Timer { get; private set; } = 10.0f;

	public KeyCode HotKey { private get; set; } = KeyCode.F;

	public Transform Transform => button.transform;

	private readonly ActionButton button;
	private readonly IButtonAutoActivator activator;

	private bool isShow = true;

	private readonly Color TimerOnColor = new Color(0f, 0.8f, 0f);

	public ExtremeAbilityButton(
		BehaviorBase behavior,
		IButtonAutoActivator activator,
		KeyCode hotKey)
	{
		State = AbilityState.CoolDown;
		this.activator = activator;
		Behavior = behavior;
		HotKey = hotKey;

		var hud = FastDestroyableSingleton<HudManager>.Instance;
		var killButton = hud.KillButton;

		button = Object.Instantiate(
			killButton, killButton.transform.parent);
		PassiveButton passiveButton = button.GetComponent<PassiveButton>();
		passiveButton.OnClick.RemoveAllPersistentAndListeners();
		passiveButton.OnClick.AddListener(onClick);
		passiveButton.name = Name;

		Transform info = button.transform.FindChild(AditionalInfoName);
		if (info != null)
		{
			info.gameObject.SetActive(false);
		}

		SetButtonShow(true);

		Behavior.Initialize(button);
		button.graphic.sprite = Behavior.Graphic.Img;

		hud.ReGridButtons();
	}

	public bool IsAbilityActive() =>
		State == AbilityState.Activating;
	public bool IsAbilityReady() =>
		State == AbilityState.Ready;

	public void OnMeetingStart()
	{
		Behavior.ForceAbilityOff();
		SetButtonShow(false);
	}

	public void OnMeetingEnd()
	{
		setStatus(AbilityState.Reset);
		SetButtonShow(true);
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
			button.buttonLabelText.fontMaterial);
		button.buttonLabelText.fontMaterial = Object.Instantiate(
			useButton.buttonLabelText.fontMaterial, button.transform);
	}

	public void Update()
	{
		if (!isShow ||
			button == null ||
			IntroCutscene.Instance != null)
		{
			return;
		}

		bool isActive = activator.IsActive();

		setActive(isActive);
		if (!button.isActiveAndEnabled) { return; }

		AbilityState newState = Behavior.Update(State);
		if (newState != State)
		{
			setStatus(newState);
		}

		button.graphic.sprite = Behavior.Graphic.Img;
		button.OverrideText(Behavior.Graphic.Text);

		if (Behavior.IsUse())
		{
			button.graphic.color = button.buttonLabelText.color = Palette.EnabledColor;
			button.graphic.material.SetFloat("_Desat", 0f);
		}
		else
		{
			button.graphic.color = button.buttonLabelText.color = Palette.DisabledClear;
			button.graphic.material.SetFloat("_Desat", 1f);
		}

		switch (State)
		{
			case AbilityState.None:
				button.cooldownTimerText.color = Palette.EnabledColor;
				button.SetCoolDown(0, Behavior.CoolTime);
				return;
			case AbilityState.CoolDown:
				// 白色でタイマーをすすめる
				Timer -= Time.deltaTime;
				button.cooldownTimerText.color = Palette.EnabledColor;

				// クールダウンが明けた
				if (Timer <= 0.0f)
				{
					setStatus(AbilityState.Ready);
				}
				break;
			case AbilityState.Activating:
				// 緑色でタイマーをすすめる
				Timer -= Time.deltaTime;
				button.cooldownTimerText.color = TimerOnColor;

				if (!Behavior.IsCanAbilityActiving())
				{
					Behavior.ForceAbilityOff();
					setStatus(AbilityState.Ready);
					return;
				}
				// 能力がアクティブが時間切れなので能力のリセット等を行う
				if (Timer <= 0.0f)
				{
					Behavior.AbilityOff();
					setStatus(AbilityState.CoolDown);
				}
				break;
			case AbilityState.Ready:
				Timer = 0.0f;
				if (Input.GetKeyDown(HotKey))
				{
					onClick();
				}
				break;
			default:
				break;
		}

		button.SetCoolDown(
			Timer,
			State != AbilityState.Activating ?
			Behavior.CoolTime : Behavior.ActiveTime);
	}

	private void onClick()
	{
		if (Behavior.IsUse() &&
			Behavior.TryUseAbility(Timer, State, out AbilityState newState))
		{
			ExtremeRolesPlugin.Logger.LogInfo($"ExtremeAbilityButton : Using {Behavior.Graphic.Text}");
			if (newState == AbilityState.CoolDown)
			{
				Behavior.AbilityOff();
			}
			setStatus(newState);
		}
	}

	private void setActive(bool active)
	{
		button.gameObject.SetActive(active);
		button.graphic.enabled = active;
	}

	private void setStatus(AbilityState newState)
	{
		switch (newState)
		{
			case AbilityState.None:
			case AbilityState.Ready:
				Timer = 0.0f;
				break;
			case AbilityState.CoolDown:
				if (State != AbilityState.Stop)
				{
					Timer = Behavior.CoolTime;
				}
				break;
			case AbilityState.Activating:
				Timer = Behavior.ActiveTime;
				break;
			case AbilityState.Reset:
				newState = AbilityState.CoolDown;
				Timer = Behavior.CoolTime;
				break;
			default:
				break;
		}
		State = newState;
	}
}
