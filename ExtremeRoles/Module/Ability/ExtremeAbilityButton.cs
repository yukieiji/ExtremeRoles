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
	Charging,
	Activating,
	Reset,
}

#nullable enable

public class ExtremeAbilityButton
{
	public const string Name = "ExRAbilityButton";
	public const string AditionalInfoName = "ExRKillButtonAditionalInfo";

	public BehaviorBase Behavior { get; protected set; }

	public AbilityState State { get; private set; }

	public float Timer { get; private set; } = 10.0f;

	public KeyCode HotKey { private get; set; } = KeyCode.F;

	public Transform Transform => button.transform;

	private readonly ActionButton button;
	private readonly IButtonAutoActivator activator;

	private bool isShow = true;

	private const string materialName = "_Desat";
	private static readonly Color TimerOnColor = new Color(0f, 0.8f, 0f);
	private static readonly Color TimerChargeColor = Color.yellow;

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
		if (!this.isShow ||
			this.button == null ||
			IntroCutscene.Instance != null)
		{
			return;
		}
		this.UpdateImp();
	}

	protected virtual void UpdateImp()
	{
		bool isActive = this.activator.IsActive();
		setActive(isActive);
		if (!this.button.isActiveAndEnabled) { return; }

		AbilityState newState = this.Behavior.Update(State);
		if (newState != this.State)
		{
			setStatus(newState);
		}

		this.button.graphic.sprite = this.Behavior.Graphic.Img;
		this.button.OverrideText(this.Behavior.Graphic.Text);

		if (this.Behavior.IsUse())
		{
			this.button.graphic.color = this.button.buttonLabelText.color = Palette.EnabledColor;
			this.button.graphic.material.SetFloat(materialName, 0f);
		}
		else
		{
			this.button.graphic.color = this.button.buttonLabelText.color = Palette.DisabledClear;
			this.button.graphic.material.SetFloat(materialName, 1f);
		}

		float maxTimer = this.Behavior.CoolTime;
		switch (this.State)
		{
			case AbilityState.None:
				this.button.cooldownTimerText.color = Palette.EnabledColor;
				this.button.SetCoolDown(0, maxTimer);
				return;
			case AbilityState.CoolDown:
				// 白色でタイマーをすすめる
				this.Timer -= Time.deltaTime;
				this.button.cooldownTimerText.color = Palette.EnabledColor;

				// クールダウンが明けた
				if (this.Timer <= 0.0f)
				{
					setStatus(AbilityState.Ready);
				}
				break;
			case AbilityState.Charging:
				// 黄色でタイマーを進める
				this.Timer -= Time.deltaTime;
				this.button.cooldownTimerText.color = TimerChargeColor;

				// 能力がチャージングが時間切れなのでチャージング等を行う
				if (this.Timer <= 0.0f)
				{
					this.setStatus(AbilityState.Charging);
					return;
				}
				// チャージしてる状態で押す
				if (Input.GetKeyDown(this.HotKey))
				{
					onClick();
				}
				break;
			case AbilityState.Activating:
				// 緑色でタイマーをすすめる
				this.Timer -= Time.deltaTime;
				this.button.cooldownTimerText.color = TimerOnColor;

				maxTimer = this.Behavior.ActiveTime;

				if (!this.Behavior.IsCanAbilityActiving())
				{
					this.Behavior.ForceAbilityOff();
					setStatus(AbilityState.Ready);
					return;
				}
				// 能力がアクティブが時間切れなので能力のリセット等を行う
				if (this.Timer <= 0.0f)
				{
					this.Behavior.AbilityOff();
					setStatus(AbilityState.CoolDown);
				}
				break;
			case AbilityState.Ready:
				this.Timer = 0.0f;
				if (Input.GetKeyDown(HotKey))
				{
					onClick();
				}
				break;
			default:
				break;
		}

		this.button.SetCoolDown(this.Timer, maxTimer);
	}

	protected void AddTimerOffset(in float offsetTime)
	{
		this.Timer += offsetTime;
	}

	private void onClick()
	{
		if (Behavior.IsUse() &&
			Behavior.TryUseAbility(Timer, State, out AbilityState newState))
		{
			ExtremeRolesPlugin.Logger.LogInfo($"ExtremeAbilityButton : Clicking {this.Behavior.Graphic.Text}");
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
