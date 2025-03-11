using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using UnityEngine;

using ExtremeRoles.Module;
using ExtremeRoles.Module.CustomMonoBehaviour.UIPart;
using ExtremeRoles.Extension.System.IO;

using UnityObject = UnityEngine.Object;
using Il2CppFile = Il2CppSystem.IO.File;

namespace ExtremeRoles.Resources;

#nullable enable

public static class ObjectPath
{
	public const string LangData = "ExtremeRoles.Resources.LangData.stringData.json";

	public const string CommonImagePathFormat = "assets/common/{0}.png";
	public const string CommonPrefabPath = "assets/common/{0}.prefab";

	public const string Texture = "texture";
	public const string Prefab = "commonprefab";

	public const string RoleAssignFilterPrefab = "assets/setting/RoleAssignFilter.prefab";
	public const string SettingTabImage = "assets/setting/{0}.png";
	public const string SettingTab = "settingtab";
    public const string HelpActiveImage = "HelpActive";
	public const string HelpNoneActiveImage = "HelpNoActive";

	public const string AssetPlace = "ExtremeRoles.Resources.{0}.asset";

	public const string ExtremeSelectorMinigameAssetFormat = "ExtremeRoles.Resources.RandomSpawn.{0}.asset";
	public const string ExtremeSelectorMinigameImgFormat = "assets/randomspawn/{0}/{1}.png";

	public const string RaiseHandIcon = "ExtremeRoles.Resources.RaiseHand.png";

	public static string CommonPrefabAsset => string.Format(AssetPlace, Prefab);
	public static string CommonTextureAsset => string.Format(AssetPlace, Texture);
	public static string SettingTabAsset => string.Format(AssetPlace, SettingTab);


	// !--- 役職用 ---
	public const string RolePrefabFormat = "assets/roles/{0}.prefab";
	public const string RoleVideoFormat = "assets/roles/{0}.webm";
	public const string RoleImgPathFormat = "assets/roles/{0}.png";
	public const string RoleSePathFormat = "assets/roles/{0}.mp3";
	public const string ButtonIcon = "ButtonIcon";
	public const string MapIcon = "MapIcon";
	public const string Minigame = "Minigame";
	public const string Video = "Video";
	public const string Se = "SE";

	public const string Bomb = "Bomb";
	public const string Meeting = "EmergencyMeeting";
	public const string MeetingBk = "MeetingBk";

	// !----- コンビ用 -----
	public const string HiroAcaSearch = "ExtremeRoles.Resources.Search.png";
	public const string MoverMove = "ExtremeRoles.Resources.MoverMoving.png";
	public const string AcceleratorAcceleratePanel = "Panel";
	public const string SkaterSkateOff = "ExtremeRoles.Resources.SkaterSkateOff.png";
	public const string SkaterSkateOn = "ExtremeRoles.Resources.SkaterSkateOn.png";
	// !----- コンビ終了 -----
	// !----- クルー用 -----
	public const string MaintainerRepair = "ExtremeRoles.Resources.Repair.png";
	public const string BodyGuardShield = "ExtremeRoles.Resources.Shield.png";
	public const string BodyGuardResetShield = "ExtremeRoles.Resources.ResetShield.png";
	public const string TimeMasterTimeShield = "ExtremeRoles.Resources.TimeShield.png";
	public const string AgencyTakeTask = "ExtremeRoles.Resources.TakeTask.png";
	public const string FencerCounter = "ExtremeRoles.Resources.Counter.png";
	public const string CurseMakerCurse = "ExtremeRoles.Resources.Curse.png";
	public const string OpenerOpenDoor = "ExtremeRoles.Resources.OpenDoor.png";
	public const string CarpenterSetCamera = "ExtremeRoles.Resources.SetCamera.png";
	public const string CarpenterVentSeal = "ExtremeRoles.Resources.VentSeal.png";
	public const string CaptainSpecialVote = "ExtremeRoles.Resources.SpecialVote.png";
	public const string CaptainSpecialVoteCheck =
		"ExtremeRoles.Resources.SpecialVoteCheck.png";
	public const string PhotographerPhotoCamera = "ExtremeRoles.Resources.PhotoCamera.png";
	public const string DelusionerDeflectDamage = "ExtremeRoles.Resources.DeflectDamage.png";

	public const string TeleporterPortalBase ="PortalBase";
	public const string TeleporterNoneActivatePortal ="NoneActivatePortal";
	public const string TeleporterFirstPortal = "FirstPortal";
	public const string TeleporterSecondPortal = "SecondPortal";

	public const string ModeratorModerate = "ExtremeRoles.Resources.Moderate.png";
	public const string PsychicPsychic = "ExtremeRoles.Resources.PsychicPsychic.png";

	public const string SummonerSummon = "Summon";
	public const string SummonerMarking = "Marking";
	// !----- クルー終了 -----
	// !----- インポスター用 -----
	public const string EvolverEvolved = "ExtremeRoles.Resources.Evolved.png";
	public const string CarrierCarry = "ExtremeRoles.Resources.Carry.png";
	public const string PainterPaintRandom = "ExtremeRoles.Resources.PaintRandom.png";
	public const string PainterPaintTrans = "ExtremeRoles.Resources.PaintTrans.png";
	public const string OverLoaderOverLoad = "ExtremeRoles.Resources.OverLoad.png";
	public const string OverLoaderDownLoad = "ExtremeRoles.Resources.DownLoad.png";
	public const string CrackerCrack = "ExtremeRoles.Resources.Crack.png";
	public const string CrackerCrackTrace = "ExtremeRoles.Resources.CrackTrace.png";
	public const string SlaveDriverHarassment = "ExtremeRoles.Resources.Harassment.png";

	public const string FakerDummyDeadBody = "DummyDeadBody";
	public const string FakerDummyPlayer = "DummyPlayer";

	// !------- メリー -------
	public const string MeryNoneActive = "NoneActivateVent";

	public const string AssaultMasterReload = "ExtremeRoles.Resources.Reload.png";
	public const string LastWolfLightOff = "ExtremeRoles.Resources.LightOff.png";

	public const string CommanderAttackCommand =
		"ExtremeRoles.Resources.AttackCommand.png";

	public const string MagicianJuggling = "ExtremeRoles.Resources.MagicianJuggling.png";
	public const string SlimeMorph = "ExtremeRoles.Resources.SlimeMorph.png";
	public const string CrewshroomSet = "ExtremeRoles.Resources.CrewshroomSetMushroom.png";

	public const string ScavengerBulletImg = "Bullet";
	public const string ScavengerBeamImg = "BeamRifle";
	// !----- インポスター終了 -----

	public const string TuckerCreateChimera = "CreateChimera";
	public const string TuckerRemoveShadow = "RemoveShadow";
	public const string TuckerShadow = "Shadow";

	public const string VigilanteEmergencyCall =
		"ExtremeRoles.Resources.EmergencyCall.png";
	public const string AliceShipBroken = "ExtremeRoles.Resources.ShipBroken.png";
	public const string JackalSidekick = "ExtremeRoles.Resources.Sidekick.png";
	public const string MissionaryPropagate = "ExtremeRoles.Resources.Propagate.png";
	public const string JesterOutburst = "ExtremeRoles.Resources.Outburst.png";
	public const string EaterDeadBodyEat = "ExtremeRoles.Resources.DeadBodyEat.png";
	public const string EaterEatKill = "ExtremeRoles.Resources.EatKil.png";
	public const string MinerSetMine = "ExtremeRoles.Resources.SetMine.png";
	public const string MinerActiveMineImg = "ExtremeRoles.Resources.MinerMineActive.png";
	public const string MinerDeactivateMineImg = "ExtremeRoles.Resources.MinerMineDeactive.png";
	public const string TotocalcioBetPlayer = "ExtremeRoles.Resources.BedPlayer.png";
	public const string QueenCharm = "ExtremeRoles.Resources.Charm.png";
	public const string SucideSprite = "ExtremeRoles.Resources.Suicide.png";
	public const string UmbrerFeatVirus = "ExtremeRoles.Resources.FeatVirus.png";
	public const string UmbrerUpgradeVirus = "ExtremeRoles.Resources.UpgradeVirus.png";
	public const string HatterTimeKill = "ExtremeRoles.Resources.HatterTimeKill.png";
	public const string ArtistArtOff = "ExtremeRoles.Resources.ArtistArtOff.png";
	public const string ArtistArtOn = "ExtremeRoles.Resources.ArtistArtOn.png";

	public const string ForasShowArrow = "ExtremeRoles.Resources.ForasArrow.png";

	public const string SoundEffect = "ExtremeRoles.Resources.soundeffect.asset";

	public const string TestButton = "ExtremeRoles.Resources.TESTBUTTON.png";

	public static string GetRoleAssetPath<W>(W id) where W : Enum
		=> string.Format(AssetPlace, id.ToString().ToLower());
	public static string GetRoleImgPath<W>(W id, string name) where W : Enum
		=> string.Format(RoleImgPathFormat, $"{id}.{name}");
	public static string GetRoleMinigamePath<W>(W id) where W : Enum
		=> GetRolePrefabPath(id, Minigame);
	public static string GetRoleVideoPath<W>(W id) where W : Enum
		=> string.Format(RoleVideoFormat, $"{id}.{Video}");
	public static string GetRoleSePath<W>(W id) where W : Enum
		=> string.Format(RoleSePathFormat, $"{id}.{Se}");
	public static string GetRolePrefabPath<W>(W id, string name) where W : Enum
		=> string.Format(RolePrefabFormat, $"{id}.{name}");
}

public static class UnityObjectLoader
{
	public static void ResetCache()
	{
		foreach(var bundle in cachedBundle.Values)
		{
			bundle.Unload(false);
		}
		cachedBundle.Clear();
	}

    private static readonly Dictionary<string, AssetBundle> cachedBundle = new Dictionary<string, AssetBundle>();

	public static SimpleButton CreateSimpleButton(Transform parent)
	{
		GameObject buuttonObj = UnityObject.Instantiate(
			LoadFromResources<GameObject>(
			ObjectPath.CommonPrefabAsset,
			string.Format(ObjectPath.CommonPrefabPath, "SimpleButton")),
			parent);

		return buuttonObj.GetComponent<SimpleButton>();
	}

	public static Sprite LoadSpriteFromResources(
		string path, float pixelsPerUnit=115f)
	{
		string key = $"{path}{pixelsPerUnit}";

		if (LruCache<string, Sprite>.TryGetValue(key, out var sprite))
		{
			return sprite;
		}

		if (!LruCache<string, Texture2D>.TryGetValue(key, out var texture))
		{
			texture = createTextureFromResources(path);
			LruCache<string, Texture2D>.Add(key, texture);
		}
		sprite = Sprite.Create(
			texture,
			new Rect(0, 0, texture.width, texture.height),
			new Vector2(0.5f, 0.5f), pixelsPerUnit);

		sprite.hideFlags |= HideFlags.DontSaveInEditor;
		LruCache<string, Sprite>.Add(key, sprite);

		return sprite;
	}

	public static Sprite LoadFromResources<W>(W id)
		where W : Enum
		=> LoadFromResources<Sprite>(
			ObjectPath.GetRoleAssetPath(id),
			ObjectPath.GetRoleImgPath(id, ObjectPath.ButtonIcon));
	public static Sprite LoadFromResources<W>(W id, string name)
		where W : Enum
		=> LoadFromResources<Sprite>(
			ObjectPath.GetRoleAssetPath(id),
			ObjectPath.GetRoleImgPath(id, name));
	public static T LoadFromResources<T, W>(W id, string path)
		where T : UnityObject
		where W : Enum
		=> LoadFromResources<T>(
			ObjectPath.GetRoleAssetPath(id),
			path);

	public static T LoadFromResources<T>(
		string bundleName, string objName) where T : UnityObject
	{
		AssetBundle bundle = GetAssetBundleFromAssembly(
			bundleName, Assembly.GetCallingAssembly());
		T result = loadObjectFromAsset<T>(bundle, objName);

		return result;
	}

	public static T LoadFromResources<T>(
		string objName) where T : UnityObject
	{
		AssetBundle bundle = GetAssetBundleFromAssembly(
			string.Format(ObjectPath.AssetPlace, objName.ToLower()),
			Assembly.GetCallingAssembly());
		T result = loadObjectFromAsset<T>(
			bundle,
			string.Format(ObjectPath.RoleImgPathFormat, objName));

		return result;
	}

#if DEBUG
	public static T GetUnityObjectFromExRResources<T>(
		string bundleName, string objName) where T : UnityObject
	{
		var assm = Assembly.GetAssembly(typeof(UnityObjectLoader));

		AssetBundle bundle = GetAssetBundleFromAssembly(
			bundleName, assm!);
		T result = loadObjectFromAsset<T>(bundle, objName);

		return result;
	}

	public static T GetUnityObjectFromExRResources<T>(
		string objName) where T : UnityObject
	{
		var assm = Assembly.GetAssembly(typeof(UnityObjectLoader));

		AssetBundle bundle = GetAssetBundleFromAssembly(
			string.Format(ObjectPath.AssetPlace, objName.ToLower()),
			assm!);
		T result = loadObjectFromAsset<T>(
			bundle,
			string.Format(ObjectPath.RoleImgPathFormat, objName));

		return result;
	}

#endif

	public static T LoadFromPath<T>(
		string path, string objName) where T : UnityObject
	{
		AssetBundle bundle = getAssetBundleFromFilePath(path);
		T result = loadObjectFromAsset<T>(bundle, objName);

		return result;
	}

	public static void LoadCommonAsset()
	{
		Assembly assembly = Assembly.GetExecutingAssembly();
		foreach (string path in new string[]
		{
			ObjectPath.Texture, "fonts", "eventsystem", ObjectPath.Prefab
		})
		{
			GetAssetBundleFromAssembly(string.Format(ObjectPath.AssetPlace, path), assembly);
		}
	}

	private static T loadObjectFromAsset<T>(AssetBundle bundle, string objName) where T : UnityObject
	{
		var obj = bundle.LoadAsset(objName, Il2CppType.Of<T>());
		return obj.Cast<T>();
	}

	private static AssetBundle getAssetBundleFromFilePath(
		string filePath)
	{
		lock (cachedBundle)
		{
			if (cachedBundle.TryGetValue(filePath, out AssetBundle? bundle) ||
				bundle != null)
			{
				bundle.Unload(true);
				cachedBundle.Remove(filePath);
			}
			var byteArray = Il2CppFile.ReadAllBytes(filePath);
			bundle = loadAssetFromByteArray(byteArray);

			cachedBundle.Add(filePath, bundle);

			return bundle;
		}
	}

	private static AssetBundle GetAssetBundleFromAssembly(
		string bundleName, Assembly assembly)
	{
		lock (cachedBundle)
		{
			if (!cachedBundle.TryGetValue(bundleName, out AssetBundle? bundle) ||
				bundle == null)
			{
				using var stream = getStreamFromResource(assembly, bundleName);
				var byteArray = getBytedArryFrom(stream);
				bundle = loadAssetFromByteArray(byteArray);

				cachedBundle.Add(bundleName, bundle);
			}
			return bundle;
		}
	}

	private static AssetBundle loadAssetFromByteArray(Il2CppStructArray<byte> byteArray)
	{
		AssetBundle bundle = AssetBundle.LoadFromMemory(byteArray);
		bundle.hideFlags |= HideFlags.DontSaveInEditor;
		return bundle;
	}

	private static Texture2D createTextureFromResources(string path)
	{
		Assembly assembly = Assembly.GetExecutingAssembly();
		using Stream stream = getStreamFromResource(assembly, path);

		var byteTexture = getBytedArryFrom(stream);

		Texture2D texture = new Texture2D(2, 2, TextureFormat.ARGB32, true);
		ImageConversion.LoadImage(texture, byteTexture, false);
		return texture;
	}

	private static Stream getStreamFromResource(Assembly assembly, string path)
	{
		Stream? stream = assembly.GetManifestResourceStream(path);

		if (stream is null)
		{
			throw new ArgumentException($"Can't find {path} in resorces");
		}
		return stream;
	}

	private static unsafe Il2CppStructArray<byte> getBytedArryFrom(Stream stream)
	{
		int length = (int)stream.Length;
		var byteTexture = new Il2CppStructArray<byte>(length);
		var span = new Span<byte>(
			IntPtr.Add(byteTexture.Pointer, IntPtr.Size * 4).ToPointer(),
			length);

		stream.ReadExactly(span);
		return byteTexture;
	}
}

