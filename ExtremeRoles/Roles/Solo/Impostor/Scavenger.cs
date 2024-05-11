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
using ExtremeRoles.Helper;
using ExtremeRoles.Performance.Il2Cpp;


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
			=> new ChargingAndActivatingCountBehaviour(
				param.Name,
				Loader.CreateSpriteFromResources(param.Path),
				isSwordUse,
				startSwordRotation,
				startSwordCharge,
				ChargingAndActivatingCountBehaviour.ReduceTiming.OnActive,
				isValidSword, isValidSword);

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
			=> IRoleAbility.IsCommonUse();
		private bool isValidSword()
			=> this.showSword != null && this.showSword.gameObject.active;

		private static SwordBehaviour createSword(
			float xSize,
			in PlayerControl rolePlayer)
			=> SwordBehaviour.Create(
				"", new Vector2(),
				rolePlayer);
	}

	private sealed class BeamSaber(
		float range,
		bool isAutoDetect) : IWeapon
	{
		private readonly float range = range;
		private readonly bool isIgnoreAutoDetect = !isAutoDetect;
		private byte targetPlayerId;
		private Vector2 chargePos = Vector2.zero;
		private readonly List<PlayerControl> cacheResult = new List<PlayerControl>();

		public BehaviorBase Create(in CreateParam param)
			=> new ChargingCountBehaviour(
				param.Name,
				Loader.CreateSpriteFromResources(param.Path),
				isIaiOk,
				tryIai,
				startIai,
				ChargingCountBehaviour.ReduceTiming.OnActive,
				iaiCheck);

		public void Hide()
		{ }

		private bool iaiCheck()
		{
			var curPos = CachedPlayerControl.LocalPlayer.PlayerControl.GetTruePosition();
			return curPos == this.chargePos;
		}

		private bool startIai()
		{
			this.cacheResult.Clear();
			this.targetPlayerId = byte.MaxValue;
			this.chargePos = CachedPlayerControl.LocalPlayer.PlayerControl.GetTruePosition();
			return true;
		}

		private bool tryIai(float chargeGauge)
		{
			if (this.targetPlayerId == byte.MaxValue)
			{
				return this.isIgnoreAutoDetect;
			}


			Player.RpcUncheckMurderPlayer(
				CachedPlayerControl.LocalPlayer.PlayerId,
				this.targetPlayerId,
				byte.MinValue);
			//　おとならす

			return true;
		}

		private bool isIaiOk(bool isCharge, float chargeGauge)
		{
			this.targetPlayerId = byte.MaxValue;

			bool isCommonUse = IRoleAbility.IsCommonUse();

			if (!isCommonUse)
			{
				return false;
			}

			if (!isCharge)
			{
				return true;
			}

			float searchRange = this.range * chargeGauge * chargeGauge;

			var allPlayer = GameData.Instance.AllPlayers;
			this.cacheResult.Clear();

			if (!ShipStatus.Instance)
			{
				return false;
			}

			PlayerControl pc = CachedPlayerControl.LocalPlayer;
			Vector2 truePosition = pc.GetTruePosition();
			var role = ExtremeRoleManager.GetLocalPlayerRole();

			foreach (GameData.PlayerInfo playerInfo in
				GameData.Instance.AllPlayers.GetFastEnumerator())
			{
				if (!Player.IsValidPlayer(role, pc, playerInfo))
				{
					continue;
				}
				PlayerControl target = playerInfo.Object;

				Vector2 vector = target.GetTruePosition() - truePosition;
				float magnitude = vector.magnitude;
				if (magnitude <= searchRange)
				{
					this.cacheResult.Add(target);
				}
			}

			this.cacheResult.Sort(delegate (PlayerControl a, PlayerControl b)
			{
				float magnitude2 = (a.GetTruePosition() - truePosition).magnitude;
				float magnitude3 = (b.GetTruePosition() - truePosition).magnitude;
				if (magnitude2 > magnitude3)
				{
					return 1;
				}
				if (magnitude2 < magnitude3)
				{
					return -1;
				}
				return 0;
			});

			if (this.cacheResult.Count <= 0)
			{
				return this.isIgnoreAutoDetect;
			}
			this.targetPlayerId = this.cacheResult[0].PlayerId;
			return true;
		}
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
		InitAbility,


		BeamSaberCount,
		BeamSaberChargeTime,
		BeamSaberRange,
		BeamSaberAutoDetect,
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
		this.initMode = (Ability)OptionManager.Instance.GetValue<int>(
			this.GetRoleOptionId(Option.InitAbility));
		this.createWeapon();

		BehaviorBase init = this.getAbilityBehavior(this.initMode);

		this.Button = new ExtremeMultiModalAbilityButton(
			[ init ],
			new RoleButtonActivator(),
			KeyCode.F);
	}

	public void RoleAbilityInit()
	{ }

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

		CreateFloatOption(
			RoleAbilityCommonOption.AbilityCoolTime,
			IRoleAbilityMixin.DefaultCoolTime,
			IRoleAbilityMixin.MinCoolTime,
			IRoleAbilityMixin.MaxCoolTime,
			IRoleAbilityMixin.Step,
			parentOps,
			format: OptionUnit.Second);


		CreateIntOption(
			Option.BeamSaberCount,
			1, 0, 10, 1, parentOps);
		CreateIntOption(
			Option.BeamSaberChargeTime,
			5, 1, 60, 1, parentOps,
			format: OptionUnit.Second);
		CreateFloatOption(
			Option.BeamSaberRange,
			3.5f, 0.1f, 7.5f, 0.1f, parentOps);
		CreateBoolOption(
			Option.BeamSaberAutoDetect,
			false, parentOps);
	}

	protected override void RoleSpecificInit()
	{

	}

	private void createWeapon()
	{
		var mng = OptionManager.Instance;

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
			},
			{
				Ability.BeamSaber,
				new BeamSaber(
					mng.GetValue<float>(
						this.GetRoleOptionId(Option.BeamSaberRange)),
					mng.GetValue<bool>(
						this.GetRoleOptionId(Option.BeamSaberAutoDetect)))
			}
		};
	}

	private CreateParam createParam(in Ability ability)
		=> ability switch
		{
			_ => new("", Path.TestButton)
		};

	private BehaviorBase getAbilityBehavior(in Ability ability)
	{
		BehaviorBase? result;

		ExtremeRolesPlugin.Logger.LogInfo(ability.ToString());

		if (this.weapon is null ||
			!this.weapon.TryGetValue(ability, out var weapon))
		{
			result = new NullBehaviour();
		}
		else
		{
			var weaponParam = createParam(ability);
			result = weapon.Create(weaponParam);
			this.loadAbilityOption(result, ability);
		}
		ExtremeRolesPlugin.Logger.LogInfo(result.ToString());
		return result;
	}

	private void loadAbilityOption(in BehaviorBase behavior, in Ability ability)
	{
		var mng = OptionManager.Instance;

		float coolTime = mng.GetValue<float>(
			this.GetRoleOptionId(RoleAbilityCommonOption.AbilityCoolTime));

		behavior.SetCoolTime(coolTime);

		switch (ability)
		{
			case Ability.HandGun:
				//behavior.SetCount(3);
				break;
			case Ability.Sword:
				// behavior.SetCount(3);
				break;
			case Ability.SniperRifle:
				// behavior.SetCount(3);
				break;
			case Ability.BeamRifle:
				// behavior.SetCount(3);
				break;
			case Ability.BeamSaber:
				if (behavior is not ChargingCountBehaviour beamSaber)
				{
					throw new ArgumentException("BeamSaber Behavior is not ChargingCountBehaviour");
				}

				beamSaber.SetAbilityCount(
					mng.GetValue<int>(
						this.GetRoleOptionId(Option.BeamSaberCount)));
				beamSaber.ChargeTime = mng.GetValue<int>(
					this.GetRoleOptionId(Option.BeamSaberChargeTime));

				break;
			case Ability.All:
				// behavior.SetCount(3);
				break;
		}
	}
}
