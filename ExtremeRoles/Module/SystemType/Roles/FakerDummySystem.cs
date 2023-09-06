using Hazel;
using System.Collections.Generic;

using TMPro;
using AmongUs.Data;
using PowerTools;
using UnityEngine;

using HarmonyLib;

using ExtremeRoles.Helper;
using ExtremeRoles.Compat;
using ExtremeRoles.Module.Interface;

using ExtremeRoles.Performance;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.Solo.Impostor;
using Epic.OnlineServices.Presence;

namespace ExtremeRoles.Module.SystemType.Roles;

public sealed class FakerDummySystem : IExtremeSystemType
{
	private interface IFakerObject
	{
		public int ColorId { get; }

		public void Clear();
	}

	public sealed class FakeDeadBody : IFakerObject
	{
		public int ColorId { get; init; }

		private GameObject body;
		public FakeDeadBody(
			PlayerControl rolePlayer,
			PlayerControl targetPlayer)
		{
			this.body = GameSystem.CreateNoneReportableDeadbody(
				targetPlayer, rolePlayer.transform.position);

			this.ColorId = targetPlayer.Data.DefaultOutfit.ColorId;

			if (CompatModManager.Instance.TryGetModMap(out var modMap))
			{
				modMap!.AddCustomComponent(
					this.body,
					Compat.Interface.CustomMonoBehaviourType.MovableFloorBehaviour);
			}
		}

		public void Clear()
		{
			Object.Destroy(this.body);
		}
	}

	public sealed class FakePlayer : IFakerObject
	{
		public int ColorId { get; init; }

		private GameObject body;
		private GameObject colorBindText;

		private const float petOffset = 0.25f;

		private const string defaultPetName = "EmptyPet(Clone)";
		private const string nameTextObjName = "NameText_TMP";
		private const string colorBindTextName = "ColorblindName_TMP";

		private struct PlayerCosmicInfo
		{
			public CosmeticsLayer Cosmetics;
			public GameData.PlayerOutfit OutfitInfo;
			public bool FlipX;
			public int ColorInfo;
		}

		public FakePlayer(
			PlayerControl rolePlayer,
			PlayerControl targetPlayer,
			bool canSeeFake)
		{
			GameData.PlayerOutfit playerOutfit = targetPlayer.Data.DefaultOutfit;
			PlayerCosmicInfo cosmicInfo = new PlayerCosmicInfo()
			{
				Cosmetics = targetPlayer.cosmetics,
				FlipX = rolePlayer.cosmetics.currentBodySprite.BodySprite.flipX,
				OutfitInfo = playerOutfit,
				ColorInfo = playerOutfit.ColorId,
			};
			this.ColorId = cosmicInfo.ColorInfo;
			this.body = new GameObject("DummyPlayer");

			createNameTextParentObj(targetPlayer, this.body, in cosmicInfo, canSeeFake);
			SpriteRenderer baseImage = createBodyImage(in cosmicInfo);
			CosmeticsLayer cosmetics = createCosmetics(baseImage, in cosmicInfo);

			if (CompatModManager.Instance.TryGetModMap(out var modMap))
			{
				modMap!.AddCustomComponent(
					this.body, Compat.Interface.CustomMonoBehaviourType.MovableFloorBehaviour);
			}

			DataManager.Settings.Accessibility.OnChangedEvent +=
				(Il2CppSystem.Action)SwitchColorName;

			decorateDummy(cosmetics, in cosmicInfo);

			SpriteAnimNodeSync[] syncs = this.body.GetComponentsInChildren<SpriteAnimNodeSync>(true);
			for (int i = 0; i < syncs.Length; ++i)
			{
				SpriteAnimNodeSync sync = syncs[i];
				if (sync != null)
				{
					Object.Destroy(sync);
				}
			}
			this.body.transform.position = rolePlayer.transform.position;
		}

		public void SwitchColorName()
		{
			if (this.colorBindText != null)
			{
				this.colorBindText.SetActive(
					DataManager.Settings.Accessibility.ColorBlindMode);
			}
		}

		public void Clear()
		{
			Object.Destroy(this.body);
			DataManager.Settings.Accessibility.OnChangedEvent -=
				(Il2CppSystem.Action)SwitchColorName;
		}

		private SpriteRenderer createBodyImage(in PlayerCosmicInfo info)
		{
			SpriteRenderer playerImage = Object.Instantiate(
				info.Cosmetics.currentBodySprite.BodySprite,
				this.body.transform);
			playerImage.flipX = info.FlipX;
			playerImage.transform.localScale = new Vector3(0.35f, 0.35f, 0.35f);
			return playerImage;
		}

		private CosmeticsLayer createCosmetics(
			SpriteRenderer playerImage, in PlayerCosmicInfo info)
		{
			CosmeticsLayer cosmetic = Object.Instantiate(
				info.Cosmetics,
				this.body.transform);

			PlayerBodySprite basePayerBodySprite = info.Cosmetics.currentBodySprite;
			PlayerBodySprite playerBodySprite = new PlayerBodySprite()
			{
				BodySprite = playerImage,
				Type = basePayerBodySprite.Type,
				flippedCosmeticOffset = basePayerBodySprite.flippedCosmeticOffset,
			};
			cosmetic.currentBodySprite = playerBodySprite;
			cosmetic.hat.Parent = playerImage;
			cosmetic.petParent = this.body.transform;
			cosmetic.transform.localScale = new Vector3(0.35f, 0.35f, 0.35f);
			cosmetic.ResetCosmetics();

			return cosmetic;
		}

		private void decorateDummy(
			CosmeticsLayer cosmetics, in PlayerCosmicInfo cosmicInfo)
		{
			int colorId = cosmicInfo.ColorInfo;
			bool flipX = cosmicInfo.FlipX;

			cosmetics.SetHat(cosmicInfo.OutfitInfo.HatId, colorId);
			cosmetics.SetVisor(cosmicInfo.OutfitInfo.VisorId, colorId);
			cosmetics.SetSkin(cosmicInfo.OutfitInfo.SkinId, colorId);
			cosmetics.SetFlipX(flipX);

			Transform emptyPet = this.body.transform.Find(defaultPetName);

			if (emptyPet != null)
			{
				Object.Destroy(emptyPet.gameObject);
			}
			string petId = cosmicInfo.OutfitInfo.PetId;
			if (petId != PetData.EmptyId &&
				CachedShipStatus.Instance.CosmeticsCache.pets.TryGetValue(
					petId, out var targetPet) &&
				targetPet != null)
			{
				PetBehaviour pet = Object.Instantiate(
					targetPet.GetAsset(), this.body.transform);
				pet.SetColor(colorId);
				pet.transform.localPosition =
					new Vector2(flipX ? 0.5f : -0.5f, -0.15f);
				pet.transform.localScale = new Vector3(0.35f, 0.35f, 0.35f);
				pet.FlipX = flipX;
				destroyAllColider(pet.gameObject);
			}
			cosmetics.SetColor(colorId);

			cosmetics.hat.transform.localPosition = cosmicInfo.Cosmetics.hat.transform.localPosition;
			cosmetics.visor.transform.localPosition = cosmicInfo.Cosmetics.visor.transform.localPosition;
		}

		private void removeRoleInfo(GameObject nameTextObjct)
		{
			Transform info = nameTextObjct.transform.FindChild(
				Patches.Manager.HudManagerUpdatePatch.RoleInfoObjectName);
			if (info != null)
			{
				Object.Destroy(info.gameObject);
			}
		}

		private void updateColorName(
			TextMeshPro colorText, TextMeshPro baseColorText, int colorId)
		{
			char[] array = FastDestroyableSingleton<TranslationController>.Instance.GetString(
					Palette.ColorNames[colorId],
					System.Array.Empty<Il2CppSystem.Object>()).ToCharArray();
			if (array.Length != 0)
			{
				array[0] = char.ToUpper(array[0]);
				for (int i = 1; i < array.Length; i++)
				{
					array[i] = char.ToLower(array[i]);
				}
			}

			fitTextMeshPro(colorText, baseColorText);

			colorText.text = new string(array);
		}
		private void createNameTextParentObj(
			PlayerControl player, GameObject parent, in PlayerCosmicInfo info, bool canSeeFake)
		{
			Transform baseParentTrans = player.gameObject.transform.FindChild("Names");

			if (baseParentTrans == null) { return; }

			GameObject baseObject = baseParentTrans.gameObject;
			GameObject nameObj = Object.Instantiate(
				baseObject, parent.transform);

			nameObj.transform.localScale = player.gameObject.transform.localScale;
			nameObj.transform.localPosition = baseObject.transform.localPosition;
			nameObj.transform.localPosition -= new Vector3(0.0f, 0.3f, 0.0f);

			TextMeshPro nameText = nameObj.transform.FindChild(
				nameTextObjName).GetComponent<TextMeshPro>();
			TextMeshPro baseNameText = baseObject.transform.FindChild(
				nameTextObjName).GetComponent<TextMeshPro>();

			this.colorBindText = nameObj.transform.FindChild(
				colorBindTextName).gameObject;
			TextMeshPro baseColorBindText = baseObject.transform.FindChild(
				colorBindTextName).GetComponent<TextMeshPro>();

			if (nameText != null && baseNameText != null)
			{
				changeDummyName(nameText, baseNameText, info, canSeeFake);
			}
			if (this.colorBindText != null && baseColorBindText != null)
			{
				updateColorName(
					this.colorBindText.GetComponent<TextMeshPro>(),
					baseColorBindText, info.ColorInfo);
			}
			removeRoleInfo(nameObj);

		}

		private static void changeDummyName(
			TextMeshPro nameText,
			TextMeshPro baseNameText,
			PlayerCosmicInfo info,
			bool canSeeFake)
		{
			fitTextMeshPro(nameText, baseNameText);
			nameText.text = canSeeFake ?
					Translation.GetString("DummyPlayerName") : info.OutfitInfo.PlayerName;
			nameText.color = canSeeFake ? Palette.ImpostorRed : Palette.White;
		}


		private static void destroyAllColider(GameObject obj)
		{
			destroyCollider<Collider2D>(obj);
			destroyCollider<PolygonCollider2D>(obj);
			destroyCollider<BoxCollider2D>(obj);
			destroyCollider<CircleCollider2D>(obj);
		}

		private static void destroyCollider<T>(GameObject obj) where T : Collider2D
		{
			T component = obj.GetComponent<T>();
			if (component != null)
			{
				Object.Destroy(component);
			}
		}
		private static void fitTextMeshPro(TextMeshPro a, TextMeshPro b)
		{
			a.transform.localPosition = b.transform.localPosition;
			a.transform.localScale = b.transform.localScale;
			a.fontSize = a.fontSizeMax = a.fontSizeMin =
				b.fontSizeMax = b.fontSizeMin = b.fontSize;
		}
	}

	public bool IsDirty => false;

	private List<IFakerObject> dummy = new List<IFakerObject> ();
	private Dictionary<SystemTypes, List<int>> dummyRoomInfo = new Dictionary<SystemTypes, List<int>>();

	public bool TryGetDummyColors(SystemTypes room, out List<int> color)
		=> this.dummyRoomInfo.TryGetValue(room, out color);

	public void Deserialize(MessageReader reader, bool initialState)
	{ }

	public void Detoriorate(float deltaTime)
	{ }

	public void Reset(ResetTiming timing, PlayerControl resetPlayer = null)
	{
		if (timing == ResetTiming.MeetingStart)
		{
			this.dummy.Do(x => x.Clear());
			this.dummy.Clear();
			this.dummyRoomInfo.Clear();
		}
	}

	public void Serialize(MessageWriter writer, bool initialState)
	{ }

	public void UpdateSystem(PlayerControl player, MessageReader msgReader)
	{
		byte fakerPlayerId = msgReader.ReadByte();
		byte dummyTargetId = msgReader.ReadByte();
		byte fakerOps = msgReader.ReadByte();

		PlayerControl rolePlyaer = Player.GetPlayerControlById(fakerPlayerId);
		PlayerControl targetPlyaer = Player.GetPlayerControlById(dummyTargetId);

		IFakerObject fake;
		switch ((Faker.FakerDummyOps)fakerOps)
		{
			case Faker.FakerDummyOps.DeadBody:
				fake = new FakeDeadBody(rolePlyaer, targetPlyaer);
				break;
			case Faker.FakerDummyOps.Player:
				SingleRoleBase role = ExtremeRoleManager.GetLocalPlayerRole();
				fake = new FakePlayer(
					rolePlyaer, targetPlyaer,
					role.IsImpostor() || role.Id == ExtremeRoleId.Marlin);
				break;
			default:
				return;
		}
		this.dummy.Add(fake);

		if (Player.TryGetPlayerRoom(rolePlyaer, out var room) &&
			room.HasValue)
		{
			if (!this.dummyRoomInfo.TryGetValue(room.Value, out var colorInfo))
			{
				colorInfo = new List<int>();
			}
			colorInfo.Add(fake.ColorId);
			this.dummyRoomInfo[room.Value] = colorInfo;
		}
	}
}
