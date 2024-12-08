using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Roles.API;

using System.Linq;

using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.Ability;
using ExtremeRoles.Resources;
using ExtremeRoles.Module.Ability.Behavior;
using ExtremeRoles.Performance;
using ExtremeRoles.Extension.Player;

using AmongUs.GameOptions;
using UnityEngine;

using ExtremeRoles.Helper;
using ExtremeRoles.Module.CustomMonoBehaviour.Overrider;
using ExtremeRoles.Extension.Il2Cpp;
using ExtremeRoles.Module.Ability.Behavior.Interface;
using ExtremeRoles.Module.Ability.AutoActivator;



#nullable enable

namespace ExtremeRoles.Roles.Solo.Impostor;

public sealed class Hijacker : SingleRoleBase, IRoleAbility
{
	public enum Option
	{
		IsRandomPlayer,
	}

	public ExtremeAbilityButton? Button { get; set; }
	private FollowerCamera? camera;
	private ShapeshifterMinigame? minigamePrefab;
	private PlayerControl? target;

	private bool isAbilityUse = true;

	public void CreateAbility()
	{
		var img = UnityObjectLoader.LoadSpriteFromResources(
			ObjectPath.TestButton);
		string name = Tr.GetString("OpenBombUI");

		BehaviorBase beha = this.Loader.TryGetValueOption<Option, bool>(
			Option.IsRandomPlayer, out var opt) &&
			opt.Value ?
				new ActivatingCountBehavior(
					name, img,
					IsAbilityUse,
					UseAbility,
					abilityOff: repose,
					forceAbilityOff: repose)
				:
				new ChargingAndActivatingCountBehaviour(
					name, img,
					(_, _) => IsAbilityUse(),
					(_) => UseAbility(),
					openUI,
					reduceTiming: ChargingAndActivatingCountBehaviour.ReduceTiming.OnActive,
					abilityOff: repose,
					forceAbilityOff: repose);
		if (beha is IChargingBehavior charging)
		{
			charging.ChargeTime = 1.0f;
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
	{ }

	public bool UseAbility()
	{
		if (this.target == null)
		{
			var alive = PlayerCache.AllPlayerControl.Where(
				x => x.IsValid());
			this.target = alive.OrderBy(
				x => RandomGenerator.Instance.Next()).First();
		}
		if (this.camera == null)
		{
			this.camera = FastDestroyableSingleton<HudManager>.Instance.transform.parent.GetComponent<FollowerCamera>();
		}

		PlayerControl.LocalPlayer.moveable = false;
		this.camera.Target = this.target;
		this.isAbilityUse = true;
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
	}

	private void repose()
	{
		if (!this.isAbilityUse)
		{
			return;
		}

		if (this.camera == null)
		{
			this.camera = FastDestroyableSingleton<HudManager>.Instance.transform.parent.GetComponent<FollowerCamera>();
		}
		if (!PlayerControl.LocalPlayer.moveable &&
			MeetingHud.Instance != null)
		{
			PlayerControl.LocalPlayer.moveable = true;
		}
		this.camera.Target = PlayerControl.LocalPlayer;
	}
}
