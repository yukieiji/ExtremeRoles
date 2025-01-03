using System.Linq;

using AmongUs.GameOptions;
using UnityEngine;

using ExtremeRoles.Helper;
using ExtremeRoles.Extension.Il2Cpp;
using ExtremeRoles.Module.Ability;
using ExtremeRoles.Module.Ability.Behavior;
using ExtremeRoles.Module.Ability.Behavior.Interface;
using ExtremeRoles.Module.Ability.AutoActivator;
using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.CustomMonoBehaviour.Overrider;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Resources;
using ExtremeRoles.Performance;
using ExtremeRoles.Extension.Player;

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
	private ShapeshifterMinigame? minigamePrefab;
	private PlayerControl? target;

	private bool opend = false;
	private bool isAbilityUse = false;

	public Hijacker() : base(
		ExtremeRoleId.Hijacker,
		ExtremeRoleType.Impostor,
		ExtremeRoleId.Hijacker.ToString(),
		Palette.ImpostorRed,
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

		var hud = FastDestroyableSingleton<HudManager>.Instance;

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
		if (this.opend)
		{
			return true;
		}

		if (this.minigamePrefab == null)
		{
			var shapeShifterBase = FastDestroyableSingleton<RoleManager>.Instance.AllRoles.FirstOrDefault(
				x => x.Role is RoleTypes.Shapeshifter);
			if (!shapeShifterBase.IsTryCast<ShapeshifterRole>(out var shapeShifter))
			{
				return false;
			}
			this.minigamePrefab = Object.Instantiate(
				shapeShifter.ShapeshifterMenu,
				PlayerControl.LocalPlayer.transform);
			this.minigamePrefab.gameObject.SetActive(false);
		}

		var game = MinigameSystem.Open(this.minigamePrefab);
		var overider = game.gameObject.TryAddComponent<ShapeshifterMinigameShapeshiftOverride>();
		overider.Add(this.overrideShapeshift);

		this.opend = true;

		return true;
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
		this.opend = false;
	}

	private void repose()
	{
		var localPlayer = PlayerControl.LocalPlayer;
		FastDestroyableSingleton<HudManager>.Instance.ShadowQuad.gameObject.SetActive(
			!localPlayer.Data.IsDead);

		this.CanMove = true;

		if (!this.isAbilityUse)
		{
			return;
		}

		if (this.camera == null)
		{
			this.camera = FastDestroyableSingleton<HudManager>.Instance.transform.parent.GetComponent<FollowerCamera>();
		}
		this.camera.Target = localPlayer;
		this.camera.enabled = true;
	}
}
