using AmongUs.GameOptions;
using ExtremeRoles.Extension.Il2Cpp;
using ExtremeRoles.Extension.Player;
using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Module.Ability;
using ExtremeRoles.Module.Ability.AutoActivator;
using ExtremeRoles.Module.Ability.Behavior;
using ExtremeRoles.Module.Ability.Behavior.Interface;
using ExtremeRoles.Module.CustomMonoBehaviour.Overrider;
using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Performance;
using ExtremeRoles.Resources;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using System.Linq;
using UnityEngine;


#nullable enable

namespace ExtremeRoles.Roles.Solo.Impostor;

public sealed class Hijacker : SingleRoleBase, IRoleAbility, IRoleMovable
{
	public enum Option
	{
		IsRandomPlayer,
	}

	public ExtremeAbilityButton? Button { get; set; }
	public bool CanMove { get; private set; } = true;

	private FollowerCamera? camera;

	private ShapeShiftMinigameWrapper? minigame;
	private PlayerControl? target;

	private bool isAbilityUse = false;

	public Hijacker() : base(
		RoleCore.BuildImpostor(ExtremeRoleId.Hijacker),
		true, false, true, true)
	{ }

	public void CreateAbility()
	{
		var img = UnityObjectLoader.LoadFromResources(
			ExtremeRoleId.Hijacker);
		string name = Tr.GetString("hijackerHijack");

		BehaviorBase beha = this.Loader.TryGetValueOption<Option, bool>(
			Option.IsRandomPlayer, out var opt) &&
			opt.Value ?
				new ReclickCountBehavior(
					name, img,
					IsAbilityUse,
					UseAbility,
					abilityOff: repose)
				:
				new ChargingAndReclickCountBehavior(
					name, img,
					(isCharge, _) => {
						if (isCharge)
						{
							return IRoleAbility.IsCommonUseWithMinigame();
						}
						return IsAbilityUse();
					},
					openUI,
					(_) => UseAbility(),
					isCharge: () => this.minigame is not null && this.minigame.IsOpen,
					abilityOff: repose,
					reduceOnCharge: false);
		if (beha is IChargingBehavior charging)
		{
			charging.ChargeTime = float.MaxValue;
		}
		this.Button = new ExtremeAbilityButton(
			beha,
			new RoleButtonActivator(),
			KeyCode.F);
		((IRoleAbility)this).RoleAbilityInit();
	}

	public bool IsAbilityUse()
		=> IRoleAbility.IsCommonUse();

	public void ResetOnMeetingEnd(NetworkedPlayerInfo? exiledPlayer = null)
	{ }

	public void ResetOnMeetingStart()
	{
		this.minigame?.Reset();
		repose();
	}

	public bool UseAbility()
	{
		if (this.target == null)
		{
			var alive = PlayerCache.AllPlayerControl.Where(
				x => x.IsValid() && x.PlayerId != PlayerControl.LocalPlayer.PlayerId);
			this.target = alive.OrderBy(
				x => RandomGenerator.Instance.Next()).First();
		}

		var hud = HudManager.Instance;

		if (this.camera == null)
		{
			this.camera = hud.transform.parent.GetComponent<FollowerCamera>();
		}

		this.CanMove = false;

		this.camera.Target = this.target;
		this.isAbilityUse = true;

		hud.ShadowQuad.gameObject.SetActive(false);

		return true;
	}

	protected override void CreateSpecificOption(
		AutoParentSetOptionCategoryFactory factory)
	{
		IRoleAbility.CreateAbilityCountOption(
			factory, 3, 10, 10f);
		factory.CreateBoolOption(Option.IsRandomPlayer, true);
	}

	protected override void RoleSpecificInit()
	{
	}

	private bool openUI()
	{
		this.minigame ??= new ShapeShiftMinigameWrapper();
		return this.minigame.IsOpen || this.minigame.OpenUi(this.overrideShapeshift);
	}

	private void overrideShapeshift(PlayerControl player)
	{
		if (this.Button is null ||
			!this.Button.Transform.TryGetComponent<PassiveButton>(out var button))
		{
			return;
		}
		this.target = player;
		button.OnClick.Invoke();
	}

	private void repose()
	{
		var localPlayer = PlayerControl.LocalPlayer;
		HudManager.Instance.ShadowQuad.gameObject.SetActive(
			!localPlayer.Data.IsDead);

		this.CanMove = true;
		this.target = null;

		if (!this.isAbilityUse)
		{
			return;
		}

		if (this.camera == null)
		{
			this.camera = HudManager.Instance.transform.parent.GetComponent<FollowerCamera>();
		}
		this.camera.Target = localPlayer;
		this.camera.enabled = true;
	}
}
