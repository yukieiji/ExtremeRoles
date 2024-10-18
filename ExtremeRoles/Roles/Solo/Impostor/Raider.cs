using UnityEngine;

using ExtremeRoles.Extension.UnityEvents;
using ExtremeRoles.Helper;
using ExtremeRoles.Resources;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Module.Ability;

using ExtremeRoles.Performance;
using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.CustomMonoBehaviour.UIPart;
using ExtremeRoles.Module.Ability.Behavior.Interface;
using ExtremeRoles.Module.SystemType.Roles;

namespace ExtremeRoles.Roles.Solo.Impostor;

#nullable enable

public sealed class Raider : SingleRoleBase, IRoleAutoBuildAbility, IRoleUpdate
{
    public enum Option
    {
        IsOpenLimit,
		LimitNum,
		IsHidePlayerOnOpen,
		BombType,
		BombNum,
		BombTargetRange,
		BombRange,
		BombAliveTime,
		BombShowOtherPlayer,
    }

	public enum BombType
	{
		SingleBomb,
		RandomBomb,
		CarpetHorizontalBomb,
		CarpetVerticalBomb
	}

    public ExtremeAbilityButton? Button { get; set; }

	private Gui? ui;
	private UiParameter? param;
	private float timer = 0f;

	public record UiParameter(
		int AbilityNum,
		float AbilityTime,
		bool IsShowPlayer);

	public sealed class Gui
	{
		public bool IsOpen
		{
			get => isOpen;
			set
			{
				this.isOpen = value;
				if (this.isOpen)
				{
					this.time = this.uiOpenTimeMax;
				}

				this.ui.enabled = value;

				this.back.gameObject.SetActive(value);
				this.execute.gameObject.SetActive(value);

				this.curPos = PlayerControl.LocalPlayer.transform.position;

				bool invert = !value;
				this.camera.enabled = invert;
				this.buttonTransformObj.SetActive(invert);
				this.button.SetButtonShow(invert);
			}
		}
		private bool isOpen;

		private readonly SpriteRenderer ui;
		private readonly FollowerCamera camera;
		private readonly GameObject buttonTransformObj;

		private readonly SimpleButton back;
		private readonly SimpleButton execute;
		private readonly ExtremeAbilityButton button;
		private readonly float uiOpenTimeMax;
		private readonly bool isShowPlayer;

		private Vector3 curPos;

		private static HudManager hud => FastDestroyableSingleton<HudManager>.Instance;
		private float uiOpenTime;
		private float time;
		private int num;

		public Gui(
			UiParameter parameter,
			ExtremeAbilityButton button)
		{
			this.button = button;
			this.uiOpenTimeMax = parameter.AbilityTime;
			this.isShowPlayer = parameter.IsShowPlayer;
			this.num = parameter.AbilityNum;

			this.camera = hud.transform.parent.GetComponent<FollowerCamera>();
			this.buttonTransformObj = hud.transform.Find("Buttons/BottomRight").gameObject;

			var obj = new GameObject("RaiderGUI");
			obj.transform.SetParent(hud.transform);

			this.ui = obj.AddComponent<SpriteRenderer>();
			this.ui.sprite = UnityObjectLoader.LoadSpriteFromResources(
				"ExtremeRoles.Resources.RaiderGUI.png", 120.0f);

			obj.transform.localScale = new Vector2(
				Screen.width / 1280.0f,
				Screen.height / 720.0f);

			obj.layer = LayerMask.NameToLayer("UI");

			this.ui.gameObject.SetActive(true);
			this.ui.transform.localPosition = new Vector3(0f, 0f, 20f);

			this.back = UnityObjectLoader.CreateSimpleButton(hud.transform);
			this.back.Awake();
			this.back.Scale = new Vector3(0.75f, 0.75f, 1.0f);
			this.back.Text.fontSize = this.back.Text.fontSizeMax = this.back.Text.fontSizeMin = 2.75f;
			this.back.name = "BackNormal";
			this.back.transform.localPosition = new Vector3(3.75f, -2.25f, 0.0f);
			this.back.ClickedEvent.AddListener(() =>
			{
				this.IsOpen = false;
			});

			this.execute = UnityObjectLoader.CreateSimpleButton(hud.transform);
			this.execute.Scale = new Vector3(0.75f, 0.75f, 1.0f);
			this.execute.transform.localPosition = new Vector3(3.75f, 0.0f, 0.0f);
			this.execute.name = "ExecuteBomb";
			this.execute.Text.fontSize = this.execute.Text.fontSizeMax = this.execute.Text.fontSizeMin = 4.0f;
			updateText();
			this.execute.ClickedEvent.AddListener(() =>
			{
				--this.num;
				updateText();
				RaiderBombSystem.RpcSetBomb(this.camera.centerPosition);
			});

			this.execute.Awake();
		}
		public void Update(float deltaTime)
		{
			this.uiOpenTime -= deltaTime;
			this.back.Text.text = $"終了\n(自動終了まで{Mathf.CeilToInt(this.uiOpenTime)}秒)";
			if (Input.GetKeyDown(KeyCode.Escape) ||
				this.uiOpenTime <= 0.0f)
			{
				this.IsOpen = false;
			}
			if (this.IsOpen && PlayerControl.LocalPlayer != null)
			{
				Vector2 cameraPos = this.camera.transform.position;
				this.time += deltaTime;
				if (this.time >= 0.05f)
				{
                    Vector2 del = FastDestroyableSingleton<HudManager>.Instance.joystick.DeltaL.normalized;
                    this.camera.transform.position = cameraPos + (del * 0.25f);
					this.time = 0.0f;
                }
				PlayerControl.LocalPlayer.transform.position = this.curPos;
			}
		}

		private void updateText()
		{
			this.execute.Text.text = $"爆撃\n残り{Mathf.CeilToInt(this.num)}回";
		}
	}

    public Raider() : base(
        ExtremeRoleId.Raider,
        ExtremeRoleType.Impostor,
        ExtremeRoleId.Raider.ToString(),
        Palette.ImpostorRed,
        true, false, true, true)
    { }

    public void CreateAbility()
    {


    }

	public bool IsAbilityUse() => IRoleAbility.IsCommonUse();

    public void ResetOnMeetingEnd(NetworkedPlayerInfo? exiledPlayer = null)
    {
        return;
    }

	public override void RolePlayerKilledAction(PlayerControl rolePlayer, PlayerControl killerPlayer)
	{
		if (rolePlayer.PlayerId == PlayerControl.LocalPlayer.PlayerId &&
			this.ui != null)
		{
			this.ui.IsOpen = false;
		}
	}

	public void ResetOnMeetingStart()
    {
		if (this.ui != null)
		{
			this.ui.IsOpen = false;
		}
    }

    public bool UseAbility()
    {
		if (this.param is null ||
			this.Button is null)
		{
			return false;
		}
        if (this.ui == null)
		{
			this.ui = new Gui(this.param, this.Button);
		}
		this.ui.IsOpen = true;
        return true;
    }

    protected override void CreateSpecificOption(
        AutoParentSetOptionCategoryFactory factory)
    {
        IRoleAbility.CreateAbilityCountOption(
            factory, 2, 10);

		factory.CreateIntOption(
			RoleAbilityCommonOption.AbilityActiveTime,
			10, 2, 60, 1,
			format: OptionUnit.Second);

		var limitOpt = factory.CreateBoolOption(
			Option.IsOpenLimit, true);
		factory.CreateIntOption(
			Option.LimitNum, 4, 1, 100, 1,
			invert: true);

		factory.CreateBoolOption(
			Option.IsHidePlayerOnOpen, true);

		var type = factory.CreateSelectionOption<Option, BombType>(Option.BombType);
		factory.CreateIntOption(Option.BombNum, 5, 2, 100, 1, type);
		factory.CreateFloatOption(Option.BombTargetRange, 1.7f, 0.1f, 25.0f, 0.1f, type);
		factory.CreateFloatOption(Option.BombRange, 1.7f, 0.1f, 5.0f, 0.1f);
		factory.CreateFloatOption(Option.BombAliveTime, 5.0f, 0.5f, 30.0f, 0.1f);

		factory.CreateBoolOption(Option.BombShowOtherPlayer, true);
	}

    protected override void RoleSpecificInit()
    {
		var cate = this.Loader;

		if (cate.TryGetValueOption<RoleAbilityCommonOption, float>(
				RoleAbilityCommonOption.AbilityActiveTime,
				out var activeTimeOption))
		{
			this.param = new UiParameter(
				cate.GetValue<RoleAbilityCommonOption, int>(RoleAbilityCommonOption.AbilityCount),
				activeTimeOption.Value,
				cate.GetValue<Option, bool>(Option.IsHidePlayerOnOpen));
		}
    }

	public void RoleAbilityInit()
	{
		if (this.Button == null) { return; }

		var cate = this.Loader;
		this.Button.Behavior.SetCoolTime(
			cate.GetValue<RoleAbilityCommonOption, float>(RoleAbilityCommonOption.AbilityCoolTime));

		if (this.Button.Behavior is IActivatingBehavior activatingBehavior &&
			cate.TryGetValueOption<RoleAbilityCommonOption, float>(
				RoleAbilityCommonOption.AbilityActiveTime,
				out var activeTimeOption))
		{
			activatingBehavior.ActiveTime = activeTimeOption.Value;
		}

		if (cate.TryGetValueOption<Option, bool>(Option.IsOpenLimit, out var limitOpt) &&
			limitOpt.Value &&
			this.Button.Behavior is ICountBehavior countBehavior &&
			cate.TryGetValueOption<Option, int>(
				Option.LimitNum,
				out var countOption))
		{
			countBehavior.SetAbilityCount(countOption.Value);
		}

		this.Button.OnMeetingEnd();
	}

	public void Update(PlayerControl rolePlayer)
	{
		if (this.ui != null &&
			this.ui.IsOpen)
		{
			this.ui.Update(Time.deltaTime);
		}
	}
}
