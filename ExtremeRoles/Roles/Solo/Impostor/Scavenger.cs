using System;
using System.Collections.Generic;
using ExtremeRoles.Resources;
using ExtremeRoles.Module.Ability;
using ExtremeRoles.Module.Ability.Behavior;
using ExtremeRoles.Module.ButtonAutoActivator;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using UnityEngine;
using System.Linq;
using ExtremeRoles.Module.CustomMonoBehaviour;
using ExtremeRoles.Performance;

#nullable enable

namespace ExtremeRoles.Roles.Solo.Impostor;


public sealed class Scavenger : SingleRoleBase, IRoleUpdate, IRoleAbility
{
	private record struct CreateParam(string Name, string Path);

	private interface IWeapon
	{
		public BehaviorBase Create(in CreateParam param);
		public void Hide();
	}

	private sealed class Sword(
		in Ability type,
		float xSize) : IWeapon
	{
		private SwordBehaviour? showSword;
		private Ability type = type;
		private readonly float xSize = xSize;

		public BehaviorBase Create(in CreateParam param)
			=> new ChargingCountBehaviour(
				param.Name,
				Loader.CreateSpriteFromResources(param.Path),
				isSwordUse,
				startSwordRotation,
				startSwordCharge,
				ChargingCountBehaviour.ReduceTiming.OnActive,
				Hide);

		public void Hide()
		{
			if (this.showSword == null)
			{
				return;
			}
			this.showSword.gameObject.SetActive(false);
		}

		private bool startSwordCharge()
		{
			// Rpc処理
			if (this.showSword == null)
			{
				this.showSword = createSword(
					this.xSize, CachedPlayerControl.LocalPlayer);
			}

			this.showSword.SetRotation(
				new SwordBehaviour.RotationInfo(
					0.0f, 0.0f, false),
				true);
			this.showSword.gameObject.SetActive(true);

			return true;
		}

		private bool startSwordRotation(float chargeGauge)
		{

			// Rpc処理
			if (this.showSword == null)
			{
				return false;
			}
			this.showSword.SetRotation(
				new SwordBehaviour.RotationInfo(
					0.0f, 0.0f, true),
				false);
			return true;
		}

		private bool isSwordUse(bool isCharge, float chargeGauge)
		{
			bool isCommonUse = IRoleAbility.IsCommonUse();

			return
				isCommonUse &&
			(
				isCharge && this.showSword != null && this.showSword.gameObject.active
			) ||
			(
				!isCharge
			);
		}

		private static SwordBehaviour createSword(
			float xSize,
			in PlayerControl rolePlayer)
			=> SwordBehaviour.Create(
				"", new Vector2(),
				rolePlayer);
	}

	private sealed class NormalGun(
		in Ability type,
		in BulletBehaviour.Parameter param) : IWeapon
	{
		private Ability type = type;
		private readonly BulletBehaviour.Parameter pram = param;
		private readonly Dictionary<int, BulletBehaviour> bullet = new();
		private int id = 0;

		public BehaviorBase Create(in CreateParam param)
		{
			return new CountBehavior(
			   param.Name,
			   Loader.CreateSpriteFromResources(param.Path),
			   IRoleAbility.IsCommonUse,
			   ability);
		}

		public void Hide()
		{
			foreach (var ballet in this.bullet.Values)
			{
				if (ballet == null ||
					ballet.gameObject == null)
				{
					continue;
				}
				UnityEngine.Object.Destroy(ballet.gameObject);
			}
			this.bullet.Clear();
		}

		public void Hide(int id)
		{
			if (this.bullet.TryGetValue(id, out var bullet) &&
				bullet != null &&
				bullet.gameObject != null)
			{
				UnityEngine.Object.Destroy(bullet.gameObject);
			}
			this.bullet.Remove(id);
		}

		public void CreateBullet(
			int mngId,
			in PlayerControl? rolePlayer)
		{
			if (rolePlayer == null)
			{
				throw new ArgumentNullException("RolePlayer is null");
			}

			var bullet = BulletBehaviour.Create(
				mngId,
				rolePlayer,
				this.pram);

			this.bullet.Add(mngId, bullet);
		}

		private bool ability()
		{

			// Rpc処理
			CreateBullet(
				this.id,
				CachedPlayerControl.LocalPlayer);

			++this.id;

			return true;
		}
	}

	public ExtremeAbilityButton? Button
	{
		get => this.internalButton;
		set
		{
			if (value is not ExtremeMultiModalAbilityButton button)
			{
				throw new ArgumentException("This role using multimodal ability");
			}
			this.internalButton = button;
		}
	}

	public enum Option
	{
		InitAbility
	}

	public enum Ability : byte
	{
		// 何もないよ
		Null,

		// 落ちてるやつ
		HandGun,
		FlameThrower,
		Sword,

		// HandGun + Sword
		SniperRifle,
		// HandGun + FlameThrower
		BeamRifle,
		// FlameThrower + Sword
		BeamSaber,

		// FlameThrower + Sword + HandGun
		All,
	}

	private Ability initMode = Ability.Null;
	private IReadOnlyDictionary<Ability, IWeapon>? weapon;


	private ExtremeMultiModalAbilityButton? internalButton;

	public Scavenger() : base(
		ExtremeRoleId.Scavenger,
		ExtremeRoleType.Impostor,
		ExtremeRoleId.Scavenger.ToString(),
		Palette.ImpostorRed,
		true, false, true, true)
	{ }

	public void CreateAbility()
	{
		BehaviorBase init =
			this.weapon is null ||
			!this.weapon.TryGetValue(this.initMode, out var weapon) ?
			new NullBehaviour() :
			weapon.Create(
				createParam(this.initMode));

		this.Button = new ExtremeMultiModalAbilityButton(
			[ init ],
			new RoleButtonActivator(),
			KeyCode.F);
	}

	public void ResetOnMeetingEnd(GameData.PlayerInfo? exiledPlayer = null)
	{　}

	public void ResetOnMeetingStart()
	{　}

	public void Update(PlayerControl rolePlayer)
	{
		if (this.Button?.Behavior is NullBehaviour)
		{
			this.Button.SetButtonShow(false);
		}
	}

	protected override void CreateSpecificOption(IOptionInfo parentOps)
	{
		CreateSelectionOption(
			Option.InitAbility,
			Enum.GetValues<Ability>()
				.Select(x => x.ToString())
				.ToArray(),
			parentOps);
	}

	protected override void RoleSpecificInit()
	{
		this.weapon = new Dictionary<Ability, IWeapon>()
		{
			{
				Ability.HandGun,
				new NormalGun(Ability.HandGun, null)
			},
			{
				Ability.Sword,
				new Sword(Ability.Sword, 0.0f)
			},
			{
				Ability.SniperRifle,
				new NormalGun(Ability.SniperRifle, null)
			},
			{
				Ability.BeamRifle,
				new NormalGun(Ability.BeamRifle, null)
			}
		};
	}

	private CreateParam createParam(in Ability ability)
		=> ability switch
		{
			_ => new("", Path.TestButton)
		};
}
