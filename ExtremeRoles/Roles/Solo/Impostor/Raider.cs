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

public sealed class Raider : SingleRoleBase, IRoleAutoBuildAbility, IRoleUpdate
{
    public enum Option
    {
        CanPaintDistance,
    }
    private float paintDistance;
    private byte targetDeadBodyId;

    public ExtremeAbilityButton Button { get; set; }

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
				this.camera.enabled = value;

				this.buttonTransformObj.SetActive(value);
				this.back.gameObject.SetActive(!value);
				this.execute.gameObject.SetActive(!value);
			}
		}
		private bool isOpen;

		private readonly SpriteRenderer ui;
		private readonly FollowerCamera camera;
		private readonly GameObject buttonTransformObj;

		private readonly SimpleButton back;
		private readonly SimpleButton execute;

		private static HudManager hud => FastDestroyableSingleton<HudManager>.Instance;

		public Gui()
		{
			this.camera = hud.transform.parent.GetComponent<FollowerCamera>();
			this.buttonTransformObj = hud.transform.Find("Buttons/ButtonRight").gameObject;

			this.ui = Object.Instantiate(
				 hud.FullScreen,
				 hud.transform);

			this.back = UnityObjectLoader.CreateSimpleButton(hud.transform);
			this.back.ClickedEvent.AddListener(() =>
			{
				this.IsOpen = false;
			});

			this.execute = UnityObjectLoader.CreateSimpleButton(hud.transform);
		}
		public void Update(float timer)
		{
			this.back.Text.text = $"backUntile:{timer}";
			if (Input.GetKeyDown(KeyCode.Escape))
			{
				this.IsOpen = false;
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

    public void ResetOnMeetingEnd(NetworkedPlayerInfo exiledPlayer = null)
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
