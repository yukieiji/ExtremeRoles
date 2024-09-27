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

namespace ExtremeRoles.Roles.Solo.Impostor;

#nullable enable

public sealed class Raider : SingleRoleBase, IRoleAutoBuildAbility, IRoleUpdate
{
    public enum Option
    {
        CanPaintDistance,
    }
    private float paintDistance;
    private byte targetDeadBodyId;

    public ExtremeAbilityButton? Button { get; set; }

	private Gui? ui;
	private float timer = 0f;

	public sealed class Gui
	{
		public bool IsOpen
		{
			get => isOpen;
			set
			{
				this.isOpen = value;

				this.ui.enabled = value;

				this.back.gameObject.SetActive(value);
				this.execute.gameObject.SetActive(value);

				this.curPos = PlayerControl.LocalPlayer.transform.position;

				bool invert = !value;
				this.camera.enabled = invert;
				this.buttonTransformObj.SetActive(invert);
			}
		}
		private bool isOpen;

		private readonly SpriteRenderer ui;
		private readonly FollowerCamera camera;
		private readonly GameObject buttonTransformObj;

		private readonly SimpleButton back;
		private readonly SimpleButton execute;
		private Vector3 curPos;

		private static HudManager hud => FastDestroyableSingleton<HudManager>.Instance;

		public Gui()
		{
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
			this.execute.Awake();
		}
		public void Update(float timer)
		{
			this.back.Text.text = $"終了\n(自動終了まで{Mathf.CeilToInt(timer)}秒)";
			if (Input.GetKeyDown(KeyCode.Escape))
			{
				this.IsOpen = false;
			}
			if (this.IsOpen && PlayerControl.LocalPlayer != null)
			{
				Vector2 cameraPos = this.camera.transform.position;
				Vector2 del = FastDestroyableSingleton<HudManager>.Instance.joystick.DeltaL.normalized;
				this.camera.transform.position = cameraPos + (del * 0.1f);
				PlayerControl.LocalPlayer.transform.position = this.curPos;
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
        if (this.ui == null)
		{
			this.ui = new Gui();
		}
		this.ui.IsOpen = true;
        return true;
    }

    protected override void CreateSpecificOption(
        AutoParentSetOptionCategoryFactory factory)
    {
        IRoleAbility.CreateAbilityCountOption(
            factory, 2, 5, 10.0f);

    }

    protected override void RoleSpecificInit()
    {
    }

	public void Update(PlayerControl rolePlayer)
	{
		if (this.ui != null &&
			this.ui.IsOpen)
		{
			this.ui.Update(this.timer);
			if (this.timer >= 0f)
			{
				this.timer -= Time.deltaTime;
				return;
			}
			this.ui.IsOpen = false;
		}
	}
}
