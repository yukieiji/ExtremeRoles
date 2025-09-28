using UnityEngine;


using ExtremeRoles.Extension.Manager;
using ExtremeRoles.Extension.UnityEvents;
using ExtremeRoles.Module.Ability.Behavior;
using ExtremeRoles.Module.Ability.Behavior.Interface;
using ExtremeRoles.Module.Interface;
using ExtremeRoles.Module.SystemType;

using ArgException = System.ArgumentException;

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

	public Transform Transform => Button.transform;

	protected readonly ActionButton Button;

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

		var hud = HudManager.Instance;
		var killButton = hud.KillButton;

		this.Button = Object.Instantiate(
			killButton, killButton.transform.parent);
		PassiveButton passiveButton = this.Button.GetComponent<PassiveButton>();
		passiveButton.OnClick.RemoveAllPersistentAndListeners();
		passiveButton.OnClick.AddListener(onClick);
		passiveButton.name = Name;

		Transform info = this.Button.transform.FindChild(AditionalInfoName);
		if (info != null)
		{
			info.gameObject.SetActive(false);
		}

		SetButtonShow(true);

		Behavior.Initialize(this.Button);
		this.Button.graphic.sprite = Behavior.Graphic.Img;

		hud.ReGridButtons();
	}

	public bool IsAbilityActiveOrCharge() =>
		State is AbilityState.Activating or AbilityState.Charging;
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
		if (HudManager.Instance == null) { return; }

		var useButton = HudManager.Instance.UseButton;

		Object.Destroy(
			this.Button.buttonLabelText.fontMaterial);
		this.Button.buttonLabelText.fontMaterial = Object.Instantiate(
			useButton.buttonLabelText.fontMaterial, this.Button.transform);
	}

	public void Update()
	{
		if (!this.isShow ||
			this.Button == null ||
			IntroCutscene.Instance != null)
		{
			return;
		}

		if (ButtonLockSystem.IsAbilityButtonLock())
		{
			blockedUpdate();
			return;
		}

		this.UpdateImp();
	}

	protected virtual void UpdateImp()
	{
		bool isActive = this.activator.IsActive();
		setActive(isActive);
		if (!this.Button.isActiveAndEnabled)
		{
			return;
		}

		AbilityState newState = this.Behavior.Update(State);
		if (newState != this.State)
		{
			setStatus(newState);
		}

		var graphic = this.Button.graphic;
		graphic.sprite = this.Behavior.Graphic.Img;
		this.Button.OverrideText(this.Behavior.Graphic.Text);

		if (this.Behavior.IsUse())
		{
			graphic.color = this.Button.buttonLabelText.color = Palette.EnabledColor;
			graphic.material.SetFloat(materialName, 0f);
		}
		else
		{
			graphic.color = this.Button.buttonLabelText.color = Palette.DisabledClear;
			graphic.material.SetFloat(materialName, 1f);
		}

		// チャージ中は改行をオフにしてるので
		var cooldownTimerText = this.Button.cooldownTimerText;
		if (!cooldownTimerText.enableWordWrapping)
		{
			cooldownTimerText.enableWordWrapping = true;
		}

		float maxTimer = this.Behavior.CoolTime;
		switch (this.State)
		{
			case AbilityState.None:
				cooldownTimerText.color = Palette.EnabledColor;
				this.Button.SetCoolDown(0.0f, maxTimer);
				return;
			case AbilityState.CoolDown:
				cooldownTimerText.color = Palette.EnabledColor;

				// クールダウンが明けた
				if (this.Timer < 0.0f)
				{
					this.setStatus(AbilityState.Ready);
					return;
				}

				// クールタイムも普通に減らす
				this.Timer -= Time.deltaTime;
				break;
			case AbilityState.Charging:
				if (this.Behavior is not IChargingBehavior chargingBehavior)
				{
					throw new ArgException("Can't inject IChargingBehavior");
				}

				cooldownTimerText.color = TimerChargeColor;
				maxTimer = chargingBehavior.ChargeTime;
				chargingBehavior.ChargeGage = Mathf.Clamp(this.Timer / maxTimer, 0.0f, 1.0f);

				// チャージしてる状態で押す
				if (Input.GetKeyDown(this.HotKey))
				{
					onClick();
				}

				// チャージできない状態になったら準備状態へ
				if (!chargingBehavior.IsCharging)
				{
					chargingBehavior.ChargeGage = 0.0f;
					this.Behavior.ForceAbilityOff();
					this.setStatus(AbilityState.Ready);
					return;
				}

				// 最大までチャージして2.5f秒後経過すると失敗として再チャージを要求
				if (this.Timer > maxTimer + 2.5f)
				{
					// 能力解除 => Ready => Chargingにする
					// こうすることで能力を解除して再度チャージって形になる
					this.Behavior.ForceAbilityOff();
					this.setStatus(AbilityState.Ready);
					this.onClick();
					return;
				}

				// チャージ時間なのでタイマーを増やす
				this.Timer += Time.deltaTime;

				// そのままだと表示が分かりにくいので変える
				this.Button.isCoolingDown = true;
				this.Button.SetCooldownFill(1 - chargingBehavior.ChargeGage);
				cooldownTimerText.text = Tr.GetString(
					OptionUnit.Percentage.ToString(),
					Mathf.CeilToInt(chargingBehavior.ChargeGage * 100));
				cooldownTimerText.enableWordWrapping = false;
				cooldownTimerText.gameObject.SetActive(true);
				return;
			case AbilityState.Activating:
				if (this.Behavior is not IActivatingBehavior activatingBehavior)
				{
					throw new ArgException("Can't inject IActivatingBehavior");
				}

				cooldownTimerText.color = TimerOnColor;
				maxTimer = activatingBehavior.ActiveTime;

				// Reclick可能であれば押す
				if (Input.GetKeyDown(this.HotKey) &&
					this.Behavior is IReclickBehavior)
				{
					onClick();
				}

				// アクティブできない状態になったら解除
				if (!activatingBehavior.CanAbilityActiving)
				{
					this.Behavior.ForceAbilityOff();
					setStatus(AbilityState.Ready);
					return;
				}

				// 能力がアクティブが時間切れなので能力のリセット等を行う
				if (this.Timer < 0.0f)
				{
					this.Behavior.AbilityOff();
					setStatus(AbilityState.CoolDown);
				}

				// アクティブタイムも普通に減らす
				this.Timer -= Time.deltaTime;

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

		this.Button.SetCoolDown(this.Timer, maxTimer);
	}

	protected void AddTimerOffset(in float offsetTime)
	{
		if (this.State is AbilityState.Ready)
		{
			this.setStatus(AbilityState.CoolDown);
			this.Timer = 0;
		}
		this.Timer += offsetTime;
	}

	private void onClick()
	{
		if (ButtonLockSystem.IsAbilityButtonLock() ||
			!Behavior.IsUse() ||
			!Behavior.TryUseAbility(Timer, State, out AbilityState newState))
		{
			return;
		}

		ExtremeRolesPlugin.Logger.LogInfo(
			$"ExtremeAbilityButton : Clicking {this.Behavior.Graphic.Text}");
		if (newState == AbilityState.CoolDown)
		{
			Behavior.AbilityOff();
		}
		setStatus(newState);
	}

	private void setActive(bool active)
	{
		this.Button.gameObject.SetActive(active);
		this.Button.graphic.enabled = active;
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
				if (State is not AbilityState.Stop)
				{
					this.Timer = Behavior.CoolTime;
				}
				break;
			case AbilityState.Charging:
				if (this.Behavior is not IChargingBehavior chargingBehavior)
				{
					throw new ArgException("Can't inject IChargingBehavior");
				}
				chargingBehavior.ChargeGage = 0.0f;
				this.Timer = 0;
				break;
			case AbilityState.Activating:
				if (this.Behavior is not IActivatingBehavior activatingBehavior)
				{
					throw new ArgException("Can't inject IActivatingBehavior");
				}
				this.Timer = activatingBehavior.ActiveTime;
				break;
			case AbilityState.Reset:
				newState = AbilityState.CoolDown;
				Timer = Behavior.CoolTime;
				break;
			default:
				break;
		}
		ExtremeRolesPlugin.Logger.LogInfo(
			$"ExtremeAbilityButton : Status changed, {this.State} => {newState}");
		State = newState;
	}

	private void blockedUpdate()
	{
		var graphic = this.Button.graphic;
		graphic.color = this.Button.buttonLabelText.color = Palette.DisabledClear;
		graphic.material.SetFloat(materialName, 1f);
		switch (this.State)
		{
			case AbilityState.Activating:
			case AbilityState.Charging:
				this.Behavior.ForceAbilityOff();
				setStatus(AbilityState.Ready);
				break;
			default:
				break;
		}
	}
}
