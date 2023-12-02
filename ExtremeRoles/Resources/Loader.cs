using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using UnityEngine;

using ExtremeRoles.Module.CustomMonoBehaviour.UIPart;

using UnityObject = UnityEngine.Object;
using Il2CppFile = Il2CppSystem.IO.File;

namespace ExtremeRoles.Resources;

#nullable enable

public static class Path
{
	public const string InfoOverlayResources = "ExtremeRoles.Resources.Asset.infooverlay.asset";
	public const string InfoOverlayPrefab = "assets/roles/infooverlay.prefab";

	public const string VideoAsset = "ExtremeRoles.Resources.Asset.video.asset";
    public const string VideoAssetPlaceHolder = "assets/video/{0}.webm";

    public const string LangData = "ExtremeRoles.Resources.LangData.stringData.json";

    public const string HelpImage = "ExtremeRoles.Resources.Help.png";
    public const string CompatModMenuImage = "ExtremeRoles.Resources.CompatModMenu.png";

    public const string TitleBurner = "ExtremeRoles.Resources.TitleBurner.png";

    public const string TabImagePathFormat = "ExtremeRoles.Resources.SettingTab.{0}.png";
	public const string ExtremeSelectorMinigameImgFormat = "ExtremeRoles.Resources.RandomSpawn.{0}.{1}.png";

	public const string HiroAcaSearch = "ExtremeRoles.Resources.Search.png";
    public const string GuesserGuess = "ExtremeRoles.Resources.GuesserGuess.png";
    public const string GusserUiResources = "ExtremeRoles.Resources.Asset.guesserui.asset";
    public const string GusserUiPrefab = "assets/roles/guesserui.prefab";
    public const string DelinquentScribe =
        "ExtremeRoles.Resources.DelinquentScribe.{0}.png";
    public const string WispTorch = "ExtremeRoles.Resources.torch.png";
    public const string MoverMove = "ExtremeRoles.Resources.MoverMoving.png";
	public const string AcceleratorAcceleratePanel = "ExtremeRoles.Resources.AcceleratorAcceleratePanel.png";

	public const string MaintainerRepair = "ExtremeRoles.Resources.Repair.png";
    public const string BodyGuardShield = "ExtremeRoles.Resources.Shield.png";
    public const string BodyGuardResetShield = "ExtremeRoles.Resources.ResetShield.png";
    public const string TimeMasterTimeShield = "ExtremeRoles.Resources.TimeShield.png";
    public const string AgencyTakeTask = "ExtremeRoles.Resources.TakeTask.png";
    public const string FencerCounter = "ExtremeRoles.Resources.Counter.png";
    public const string CurseMakerCurse = "ExtremeRoles.Resources.Curse.png";
    public const string OpenerOpenDoor = "ExtremeRoles.Resources.OpenDoor.png";
    public const string DetectiveApprenticeEmergencyMeeting =
        "ExtremeRoles.Resources.EmergencyMeeting.png";
    public const string CarpenterSetCamera = "ExtremeRoles.Resources.SetCamera.png";
    public const string CarpenterVentSeal = "ExtremeRoles.Resources.VentSeal.png";
    public const string CaptainSpecialVote = "ExtremeRoles.Resources.SpecialVote.png";
    public const string CaptainSpecialVoteCheck =
        "ExtremeRoles.Resources.SpecialVoteCheck.png";
    public const string PhotographerPhotoCamera = "ExtremeRoles.Resources.PhotoCamera.png";
    public const string DelusionerDeflectDamage = "ExtremeRoles.Resources.DeflectDamage.png";
    public const string TeleporterPortalBase =
        "ExtremeRoles.Resources.TeleporterPortalBase.png";
    public const string TeleporterNoneActivatePortal =
        "ExtremeRoles.Resources.TeleportNoneActivatePortal.png";
    public const string TeleporterFirstPortal =
        "ExtremeRoles.Resources.TeleporterFirstPortal.png";
    public const string TeleporterSecondPortal =
        "ExtremeRoles.Resources.TeleporterSecondPortal.png";
	public const string ModeratorModerate = "ExtremeRoles.Resources.Moderate.png";

	public const string EvolverEvolved = "ExtremeRoles.Resources.Evolved.png";
    public const string CarrierCarry = "ExtremeRoles.Resources.Carry.png";
    public const string PainterPaintRandom = "ExtremeRoles.Resources.PaintRandom.png";
    public const string PainterPaintTrans = "ExtremeRoles.Resources.PaintTrans.png";
    public const string OverLoaderOverLoad = "ExtremeRoles.Resources.OverLoad.png";
    public const string OverLoaderDownLoad = "ExtremeRoles.Resources.DownLoad.png";
    public const string FakerDummyDeadBody = "ExtremeRoles.Resources.DummyDeadBody.png";
    public const string FakerDummyPlayer = "ExtremeRoles.Resources.DummyPlayer.png";
    public const string CrackerCrack = "ExtremeRoles.Resources.Crack.png";
    public const string CrackerCrackTrace = "ExtremeRoles.Resources.CrackTrace.png";
    public const string BomberSetBomb = "ExtremeRoles.Resources.SetBomb.png";
	public const string SlaveDriverHarassment = "ExtremeRoles.Resources.Harassment.png";
	public const string MeryNoneActiveVent = "ExtremeRoles.Resources.NoneActivateVent.png";
    public const string MeryCustomVentAnime =
        "ExtremeRoles.Resources.MeryVentAnimation.{0}.png";
    public const string AssaultMasterReload = "ExtremeRoles.Resources.Reload.png";
    public const string LastWolfLightOff = "ExtremeRoles.Resources.LightOff.png";
    public const string HypnotistHypnosis = "ExtremeRoles.Resources.Hypnosis.png";
    public const string CommanderAttackCommand =
        "ExtremeRoles.Resources.AttackCommand.png";
    public const string HypnotistRedAbilityPart =
        "ExtremeRoles.Resources.RedAbilityPart.png";
    public const string HypnotistBlueAbilityPart =
        "ExtremeRoles.Resources.BlueAbilityPart.png";
    public const string HypnotistGrayAbilityPart =
        "ExtremeRoles.Resources.GrayAbilityPart.png";
    public const string MagicianJuggling = "ExtremeRoles.Resources.MagicianJuggling.png";
    public const string ZombieMagicCircle = "ExtremeRoles.Resources.ZombieMagicCircle.png";
    public const string ZombieMagicCircleButton =
        "ExtremeRoles.Resources.ZombieMagicCircleButton.png";
    public const string ZombieMagicCircleVideo = "zombiemagiccircle";
    public const string SlimeMorph = "ExtremeRoles.Resources.SlimeMorph.png";
	public const string TheifTimeParts = "ExtremeRoles.Resources.TheifTimePart.png";
	public const string TheifMagicCircle = "ExtremeRoles.Resources.TheifMagicCircle.png";
	public const string TheifMagicCircleVideo = "theifmagiccircle";

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

    public const string ForasShowArrow = "ExtremeRoles.Resources.ForasArrow.png";

    public const string XionMapZoomIn = "ExtremeRoles.Resources.ZoomIn.png";
    public const string XionMapZoomOut = "ExtremeRoles.Resources.ZoomOut.png";
    public const string XionSpeedUp = "ExtremeRoles.Resources.SpeedUp.png";
    public const string XionSpeedDown = "ExtremeRoles.Resources.SpeedDown.png";

    public const string SoundEffect = "ExtremeRoles.Resources.Asset.soundeffect.asset";

    public const string TestButton = "ExtremeRoles.Resources.TESTBUTTON.png";
}

public static class Loader
{

    private static readonly Dictionary<string, Sprite> cachedSprite = new Dictionary<string, Sprite> ();
    private static readonly Dictionary<string, AssetBundle> cachedBundle = new Dictionary<string, AssetBundle>();

	public static SimpleButton CreateSimpleButton(Transform parent)
	{
		GameObject buuttonObj = UnityObject.Instantiate(
			GetUnityObjectFromResources<GameObject>(
			"ExtremeRoles.Resources.Asset.simplebutton.asset",
			"assets/common/simplebutton.prefab"),
			parent);

		return buuttonObj.GetComponent<SimpleButton>();
	}

	public static Sprite CreateSpriteFromResources(
		string path, float pixelsPerUnit=115f)
	{
		string key = $"{path}{pixelsPerUnit}";

		if (cachedSprite.TryGetValue(key, out Sprite? sprite) ||
			sprite != null) { return sprite; }

		Texture2D texture = createTextureFromResources(path);
		sprite = Sprite.Create(
			texture,
			new Rect(0, 0, texture.width, texture.height),
			new Vector2(0.5f, 0.5f), pixelsPerUnit);

		sprite.hideFlags |= HideFlags.HideAndDontSave | HideFlags.DontSaveInEditor;
		cachedSprite.Add(key, sprite);

		return sprite;
	}

	public static T GetUnityObjectFromResources<T>(
		string bundleName, string objName) where T : UnityObject
	{
		AssetBundle bundle = getAssetBundleFromAssembly(
			bundleName, Assembly.GetCallingAssembly());
		T result = getObjectFromAsset<T>(bundle, objName);

		return result;
	}

	public static T GetUnityObjectFromPath<T>(
		string path, string objName) where T : UnityObject
	{
		AssetBundle bundle = getAssetBundleFromFilePath(path);
		T result = getObjectFromAsset<T>(bundle, objName);

		return result;
	}

	public static void LoadCommonAsset()
	{
		Assembly assembly = Assembly.GetExecutingAssembly();
		foreach (string path in new string[]
		{
			"texture", "fonts", "eventsystem",
			"simplebutton", "closebutton", "confirmmenu"
		})
		{
			getAssetBundleFromAssembly($"ExtremeRoles.Resources.Asset.{path}.asset", assembly);
		}
	}

	private static T getObjectFromAsset<T>(AssetBundle bundle, string objName) where T : UnityObject
	{
		var obj = bundle.LoadAsset(objName, Il2CppType.Of<T>());
		return obj.Cast<T>();
	}

	private static AssetBundle getAssetBundleFromFilePath(
		string filePath)
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

	private static AssetBundle getAssetBundleFromAssembly(
		string bundleName, Assembly assembly)
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

	private static AssetBundle loadAssetFromByteArray(Il2CppStructArray<byte> byteArray)
	{
		AssetBundle bundle = AssetBundle.LoadFromMemory(byteArray);
		bundle.hideFlags |= HideFlags.HideAndDontSave | HideFlags.DontSaveInEditor;
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
		long length = stream.Length;
		var byteTexture = new Il2CppStructArray<byte>(length);
		var span = new Span<byte>(
			IntPtr.Add(byteTexture.Pointer, IntPtr.Size * 4).ToPointer(),
			(int)length);

		stream.Read(span);

		return byteTexture;
	}
}

