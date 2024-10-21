using System.Collections.Generic;

using UnityEngine;

using ExtremeRoles.Extension.UnityEvents;
using ExtremeRoles.Resources;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Module.Ability;

using ExtremeRoles.Performance;
using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.CustomMonoBehaviour.UIPart;
using ExtremeRoles.Module.Ability.Behavior.Interface;
using ExtremeRoles.Module.SystemType.Roles;
using ExtremeRoles.Module.SystemType;
using ExtremeRoles.Module.CustomMonoBehaviour;

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
					this.uiOpenTime = this.uiOpenTimeMax;
				}

				if (!this.isShowPlayer)
				{

					foreach (var player in PlayerCache.AllPlayerControl)
					{
						if (player == null ||
							player.Data == null ||
							player.Data.Disconnected)
						{
							continue;
						}

						var curScale = player.transform.localScale;
						byte playerId = player.PlayerId;
						if (!value && this.defaultScale.TryGetValue(playerId, out float scale))
						{
							player.transform.localScale = new Vector3(scale, curScale.y, curScale.z);
							if (player.cosmetics != null &&
								player.cosmetics.CurrentPet != null &&
								this.petScale.TryGetValue(playerId, out float petDefaultScale))
							{
								var petScale = player.cosmetics.CurrentPet.transform.localScale;
								player.cosmetics.CurrentPet.transform.localScale = new Vector3(petDefaultScale, petScale.y, petScale.z);
							}
						}
						else if (curScale.x != xScale)
						{
							this.defaultScale[playerId] = curScale.x;
							player.transform.localScale = new Vector3(xScale, curScale.y, curScale.z);
							if (player.cosmetics != null && player.cosmetics.CurrentPet != null)
							{
								var petScale = player.cosmetics.CurrentPet.transform.localScale;
								this.petScale[playerId] = player.cosmetics.CurrentPet.transform.localScale.x;
								player.cosmetics.CurrentPet.transform.localScale = new Vector3(xScale, petScale.y, petScale.z);
							}
						}
					}
				}

				this.ui.enabled = value;

				this.back.gameObject.SetActive(value);
				this.execute.gameObject.SetActive(this.num > 0 && value);

				this.curPos = PlayerControl.LocalPlayer.transform.position;

				FastDestroyableSingleton<HudManager>.Instance.ShadowQuad.gameObject.SetActive(
					!(value || PlayerControl.LocalPlayer.Data.IsDead));

				bool invert = !value;
				this.camera.enabled = invert;

				this.buttonTransformObj.SetActive(invert);
				this.button.SetButtonShow(invert);
			}
		}
		private const float xScale = 0.0001f;
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

		private readonly Dictionary<byte, float> defaultScale = new Dictionary<byte, float>();
		private readonly Dictionary<byte, float> petScale = new Dictionary<byte, float>();

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
			this.ui.sprite = UnityObjectLoader.LoadFromResources(
				ExtremeRoleId.Raider, "Gui");

			obj.transform.localScale = new Vector2(
				Screen.width / 1280.0f,
				Screen.height / 720.0f);

			int targetLayer = LayerMask.NameToLayer("UI"); ;
			obj.layer = targetLayer;

			this.ui.gameObject.SetActive(true);
			this.ui.transform.localPosition = new Vector3(0f, 0f, 20f);

			this.back = UnityObjectLoader.CreateSimpleButton(hud.transform);
			this.back.Awake();
			this.back.Layer = targetLayer;
			this.back.Scale = new Vector3(0.75f, 0.75f, 1.0f);
			this.back.Text.fontSize = this.back.Text.fontSizeMax = this.back.Text.fontSizeMin = 2.75f;
			this.back.name = "BackNormal";
			this.back.transform.localPosition = new Vector3(3.75f, -2.25f, -10.0f);
			this.back.ClickedEvent.AddListener(() =>
			{
				this.IsOpen = false;
			});

			this.execute = UnityObjectLoader.CreateSimpleButton(hud.transform);
			this.execute.Awake();
			this.execute.Layer = targetLayer;
			this.execute.Scale = new Vector3(0.75f, 0.75f, 1.0f);
			this.execute.transform.localPosition = new Vector3(3.75f, 0.0f, -10.0f);
			this.execute.name = "ExecuteBomb";
			this.execute.Text.fontSize = this.execute.Text.fontSizeMax = this.execute.Text.fontSizeMin = 4.0f;
			updateText();
			this.execute.ClickedEvent.AddListener(() =>
			{
				if (this.num <= 0)
				{
					return;
				}
				--this.num;
				RaiderBombSystem.RpcSetBomb(this.camera.gameObject.transform.position);
				updateText();
			});

		}
		public void Update(float deltaTime)
		{
			this.uiOpenTime -= deltaTime;
			this.back.Text.text = $"終了\n(自動終了まで{Mathf.CeilToInt(this.uiOpenTime)}秒)";
			if (Input.GetKeyDown(KeyCode.Escape) ||
				this.uiOpenTime <= 0.0f ||
				(
					this.num <=0 &&
					this.button.Behavior is ICountBehavior count &&
					count.AbilityCount <= 0
				))
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
			if (this.num <= 0)
			{
				this.execute.gameObject.SetActive(false);
			}
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
		var img = UnityObjectLoader.LoadSpriteFromResources(
			ObjectPath.TestButton);
		if (this.Loader.TryGetValueOption<Option, bool>(Option.IsOpenLimit, out var opt) &&
			opt.Value)
		{
			this.CreateAbilityCountButton("OpenUI", img, null, forceAbilityOff);
		}
		else
		{
			this.CreateNormalAbilityButton("OpenUI", img, null, forceAbilityOff);
		}
    }

	public bool IsAbilityUse() => IRoleAbility.IsCommonUse();

    public void ResetOnMeetingEnd(NetworkedPlayerInfo? exiledPlayer = null)
    {
        return;
    }

	public override void RolePlayerKilledAction(PlayerControl rolePlayer, PlayerControl killerPlayer)
	{
		if (rolePlayer.PlayerId == PlayerControl.LocalPlayer.PlayerId)
		{
			forceAbilityOff();
		}
	}

	public void ResetOnMeetingStart()
    {
		forceAbilityOff();
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
			25, 2, 90, 1,
			format: OptionUnit.Second);

		var limitOpt = factory.CreateBoolOption(
			Option.IsOpenLimit, true);
		factory.CreateIntOption(
			Option.LimitNum, 4, 1, 100, 1,
			limitOpt, invert: true);

		factory.CreateBoolOption(
			Option.IsHidePlayerOnOpen, true);

		var type = factory.CreateSelectionOption<Option, RaiderBombSystem.BombType>(Option.BombType);
		factory.CreateIntOption(Option.BombNum, 5, 2, 100, 1, type);
		factory.CreateFloatOption(Option.BombTargetRange, 1.7f, 0.1f, 25.0f, 0.1f, type);
		factory.CreateFloatOption(Option.BombRange, 1.7f, 0.1f, 5.0f, 0.1f);
		factory.CreateFloatOption(Option.BombAliveTime, 5.0f, 0.5f, 30.0f, 0.1f);

		factory.CreateBoolOption(Option.BombShowOtherPlayer, true);
	}

    protected override void RoleSpecificInit()
    {
		var cate = this.Loader;

		if (!cate.TryGetValueOption<RoleAbilityCommonOption, int>(
				RoleAbilityCommonOption.AbilityActiveTime,
				out var activeTimeOption))
		{
			return;
		}

		this.param = new UiParameter(
			cate.GetValue<RoleAbilityCommonOption, int>(RoleAbilityCommonOption.AbilityCount),
			activeTimeOption.Value,
			cate.GetValue<Option, bool>(Option.IsHidePlayerOnOpen));
		var _ = ExtremeSystemTypeManager.Instance.CreateOrGet(
			ExtremeSystemType.RaiderBomb,
			() => new RaiderBombSystem(
					new RaiderBombSystem.Parameter(
						(RaiderBombSystem.BombType)cate.GetValue<Option, int>(Option.BombType),
						cate.GetValue<Option, int>(Option.BombNum),
						cate.GetValue<Option, float>(Option.BombTargetRange),
						new RaiderBomb.Parameter(
							cate.GetValue<Option, float>(Option.BombRange),
							cate.GetValue<Option, float>(Option.BombAliveTime),
							cate.GetValue<Option, bool>(Option.BombShowOtherPlayer)))));
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

	private void forceAbilityOff()
	{
		if (this.ui != null &&
			this.ui.IsOpen)
		{
			this.ui.IsOpen = false;
		}
	}
}
