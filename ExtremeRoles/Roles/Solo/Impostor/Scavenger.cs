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

	public enum BulletType : byte
	{
		HandGun,
		SniperRifle,
		BeamRifle,
	}

	private int id = 0;

	private Dictionary<int, BulletBehaviour> allShowBullet = new();
	private SwordBehaviour? showSword;


	private Ability initMode = Ability.Null;
	private IReadOnlyDictionary<Ability, BehaviorBase>? allAbility;

	private BulletBehaviour.Parameter? handGunParam;
	private BulletBehaviour.Parameter? sniperRifleParam;
	private BulletBehaviour.Parameter? beamRifleParam;
	private float swordSize;


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
		if (this.allAbility is null ||
			!this.allAbility.TryGetValue(this.initMode, out var ability))
		{
			return;
		}

		this.Button = new ExtremeMultiModalAbilityButton(
			[ ability ],
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
		this.allAbility = new Dictionary<Ability, BehaviorBase>()
		{
			{ Ability.Null, new NullBehaviour() },
			{
				Ability.HandGun,
				new CountBehavior(
					"",
					Loader.CreateSpriteFromResources(Path.TestButton),
					IRoleAbility.IsCommonUse,
					handGunAbility)
			},
			{
				Ability.Sword,
				new ChargingCountBehaviour(
					"",
					Loader.CreateSpriteFromResources(Path.TestButton),
					isSwordUse,
					startSwordRotation,
					startSwordCharge,
					ChargingCountBehaviour.ReduceTiming.OnActive,
					hideSword)
			},
			{
				Ability.SniperRifle,
				new CountBehavior(
					"",
					Loader.CreateSpriteFromResources(Path.TestButton),
					IRoleAbility.IsCommonUse,
					sniperRifleAbility)
			},
			{
				Ability.BeamRifle,
				new CountBehavior(
					"",
					Loader.CreateSpriteFromResources(Path.TestButton),
					IRoleAbility.IsCommonUse,
					beamRifleAbility)
			}
		};
	}

	private bool startSwordCharge()
	{
		if (this.showSword == null)
		{
			this.showSword = createSword(
				this, CachedPlayerControl.LocalPlayer);
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

	private void hideSword()
	{
		// ToDo Hide処理
		if (this.showSword == null)
		{
			return;
		}
		this.showSword.gameObject.SetActive(false);
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


	private bool handGunAbility()
	{
		createBullet(this.handGunParam);
		return true;
	}

	private bool sniperRifleAbility()
	{
		createBullet(this.sniperRifleParam);
		return true;
	}

	private bool beamRifleAbility()
	{
		createBullet(this.beamRifleParam);
		return true;
	}

	private void createBullet(
		in BulletBehaviour.Parameter? param)
	{
		if (param is null)
		{
			throw new ArgumentNullException("Bullet parameter is null");
		}

		// Rpc処理

		createBulletStatic(
			in this.allShowBullet,
			this.id,
			this.beamRifleParam,
			CachedPlayerControl.LocalPlayer);

		++this.id;
	}

	private static SwordBehaviour createSword(
		in Scavenger scavenger,
		in PlayerControl rolePlayer)
		=> SwordBehaviour.Create(
			"", new Vector2(),
			rolePlayer);

	private static void createBulletStatic(
		in Dictionary<int, BulletBehaviour> mngContainer,
		int mngId,
		in BulletBehaviour.Parameter? param,
		in PlayerControl? rolePlayer)
	{
		if (rolePlayer == null)
		{
			throw new ArgumentNullException("RolePlayer is null");
		}

		if (param is null)
		{
			throw new ArgumentNullException("Bullet parameter is null");
		}

		var bullet = BulletBehaviour.Create(
			mngId,
			rolePlayer,
			param);

		mngContainer.Add(mngId, bullet);
	}
}
