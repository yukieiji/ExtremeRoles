using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Reflection;

using TMPro;
using UnityEngine;

using BepInEx.Unity.IL2CPP.Utils.Collections;

using ExtremeRoles.Helper;
using ExtremeRoles.GameMode;
using ExtremeRoles.Extension.Task;
using ExtremeRoles.Module.CustomMonoBehaviour.UIPart;

using ExtremeRoles.Patches.Controller;
using ExtremeRoles.Performance;
using ExtremeRoles.Performance.Il2Cpp;

using Il2CppObject = Il2CppSystem.Object;


#nullable enable

namespace ExtremeRoles.Module.CustomMonoBehaviour.Minigames;

[Il2CppRegister]
public sealed class ExtremeSpawnSelectorMinigame : Minigame
{
	private bool selected;
	private readonly Controller controller = new Controller();
	private readonly List<SpriteButton> button = new List<SpriteButton>(3);

	private const float buttonYOffset = 0.25f;

	private readonly record struct SpawnPointInfo(string RoomName, string ImgName, float X, float Y)
	{
		[JsonIgnore(Condition = JsonIgnoreCondition.Never)]
		public Vector2 Vector => new Vector2(X, Y);
	}

	private static Dictionary<string, SpawnPointInfo[]>? spawnInfo;
	private const string jsonPath = "ExtremeRoles.Resources.JsonData.RandomSpawnPoint.json";

#pragma warning disable CS8618 // null 非許容のフィールドには、コンストラクターの終了時に null 以外の値が入っていなければなりません。Null 許容として宣言することをご検討ください。
	private TextMeshPro text;
	public ExtremeSpawnSelectorMinigame(IntPtr ptr) : base(ptr)
#pragma warning restore CS8618 // null 非許容のフィールドには、コンストラクターの終了時に null 以外の値が入っていなければなりません。Null 許容として宣言することをご検討ください。
	{ }

	public override void Begin(PlayerTask? task)
	{
		string mapKey = GameSystem.CurMapKey;
		if (mapKey == GameSystem.SubmergedKey ||
			mapKey == GameSystem.AirShipKey)
		{
			return;
		}

		if (spawnInfo == null)
		{
			var assembly = Assembly.GetCallingAssembly();
			using Stream? stream = assembly.GetManifestResourceStream(jsonPath);

			if (stream is null) { return; }

			spawnInfo = JsonSerializer.Deserialize<Dictionary<string, SpawnPointInfo[]>>(stream);
		}

		if (spawnInfo == null||
			!spawnInfo.TryGetValue(mapKey, out var usePoints) ||
			usePoints == null)
		{
			return;
		}

		this.AbstractBegin(task);

		var shuffleedPoint = usePoints.OrderBy(x => RandomGenerator.Instance.Next()).ToArray();

		for (int i = 0; i < 3; ++i)
		{
			var point = shuffleedPoint[i];

			GameObject obj = new GameObject("selector_button");
			obj.transform.SetParent(base.transform);
			obj.transform.localPosition = new Vector3(2.5f * (i - 1), buttonYOffset);
			obj.SetActive(true);
			obj.layer = 5;

			var button = obj.AddComponent<SpriteButton>();

			string roomName = point.RoomName;
			string text =
				Enum.TryParse<SystemTypes>(roomName, true, out var systemRoomName) ?
				FastDestroyableSingleton<TranslationController>.Instance.GetString(systemRoomName) :
				Translation.GetString(roomName);
			string imgName =
				string.Format(
					Resources.Path.ExtremeSelectorMinigameImgFormat,
					mapKey, roomName);

			button.Text.text = text;
			button.Rend.sprite = Resources.Loader.CreateSpriteFromResources(imgName);
			button.Colider.size = new Vector2(1.25f, 1.25f);
			button.OnClick = createSpawnAtAction(point.Vector, text);

			this.button.Add(button);
		}

		if (GameManager.Instance != null && GameManager.Instance.IsNormal())
		{
			foreach (GameData.PlayerInfo playerInfo in GameData.Instance.AllPlayers.GetFastEnumerator())
			{
				if (playerInfo != null &&
					playerInfo.Object != null &&
					!playerInfo.Object.isDummy &&
					!playerInfo.Disconnected)
				{
					var player = playerInfo.Object.NetTransform;

					player.transform.position = new Vector2(-25f, 40f);
					player.Halt();
				}
			}
		}

		if (!ExtremeGameModeManager.Instance.ShipOption.Spawn.IsAutoSelectRandom)
		{

			base.StartCoroutine(runTimer().WrapToIl2Cpp());

			PlayerControl.HideCursorTemporarily();
			ConsoleJoystick.SetMode_Menu();
		}
		else
		{
			this.Close();
		}
	}

	public override void Close()
	{
		if (!this.selected)
		{
			spawnToRandom();
		}
		this.AbstractClose();
	}

	public void Awake()
	{
		this.text = Instantiate(Prefab.Text, base.transform);
		this.text.alignment = TextAlignmentOptions.Center;
		this.text.gameObject.SetActive(true);
		this.text.transform.localPosition = new Vector3(0.0f, -1.5f);
		this.text.fontSize = text.fontSizeMin = text.fontSizeMax = 4.0f;

		this.MyTask = null;
		this.multistageMinigameChecked = true;
		this.TransType = TransitionType.SlideBottom;
	}

	public void Update()
	{
		if (this.selected ||
			FastDestroyableSingleton<HudManager>.Instance == null ||
			FastDestroyableSingleton<HudManager>.Instance.Chat.IsOpenOrOpening)
		{
			return;
		}

		this.controller.Update();

		foreach (var button in this.button)
		{
			if (Controller.currentTouchType == Controller.TouchType.Joystick &&
				this.controller.CheckHover(button.Colider))
			{
				button.OnClick?.Invoke();
				return;
			}
		}
	}

	public IEnumerator WaitForFinish()
	{
		yield return null;

		while (this.amClosing == CloseState.None)
		{
			yield return null;
		}
		yield break;
	}

	private IEnumerator runTimer()
	{
		for (float time = 10.0f; time >= 0f; time -= Time.deltaTime)
		{
			this.text.text = FastDestroyableSingleton<TranslationController>.Instance.GetString(
				StringNames.TimeRemaining, new Il2CppObject[] { Mathf.CeilToInt(time) });
			yield return null;
		}
		spawnToRandom();
		yield break;
	}

	private Action createSpawnAtAction(Vector2 pos, string name)
	{
		return () =>
		{
			if (this.amClosing != CloseState.None)
			{
				return;
			}
			ExtremeRolesPlugin.Logger.LogInfo($"Player selected spawn point {name}");

			this.selected = true;
			PlayerControl localPlayer = CachedPlayerControl.LocalPlayer;

			localPlayer.SetKinematic(true);
			localPlayer.NetTransform.SetPaused(true);
			Player.RpcUncheckSnap(localPlayer.PlayerId, pos);

			FastDestroyableSingleton<HudManager>.Instance.PlayerCam.SnapToTarget();

			base.StopAllCoroutines();
			base.StartCoroutine(coSpawnAt(localPlayer).WrapToIl2Cpp());
		};
	}

	private IEnumerator coSpawnAt(PlayerControl playerControl)
	{
		yield return new WaitForFixedUpdate();
		yield return new WaitForFixedUpdate();
		yield return new WaitForFixedUpdate();

		playerControl.SetKinematic(false);
		playerControl.NetTransform.SetPaused(false);
		this.Close();

		yield break;
	}

	private void spawnToRandom()
	{
		int index = RandomGenerator.Instance.Next(0, this.button.Count);
		this.button[index].OnClick?.Invoke();
	}

	public static IEnumerator WrapUpAndSpawn(ExileController instance)
	{
		ExileControllerWrapUpPatch.WrapUpPrefix();
		if (instance.exiled != null)
		{
			PlayerControl @object = instance.exiled.Object;
			if (@object != null)
			{
				@object.Exiled();
			}
			instance.exiled.IsDead = true;
		}
		ExileControllerWrapUpPatch.WrapUpPostfix(instance.exiled);

		bool meeting = ExtremeRolesPlugin.ShipState.AssassinMeetingTrigger;
		if (meeting)
		{
			yield break;
		}

		if (DestroyableSingleton<TutorialManager>.InstanceExists ||
			!GameManager.Instance.LogicFlow.IsGameOverDueToDeath())
		{
			yield return WaiteSpawn();
			instance.ReEnableGameplay();
		}
		Destroy(instance.gameObject);
		yield break;
	}

	public static IEnumerator WaiteSpawn()
	{
		GameObject obj = new GameObject("SpawnSelector");
		var spawnSelector = obj.AddComponent<ExtremeSpawnSelectorMinigame>();
		spawnSelector.transform.SetParent(Camera.main.transform, false);
		spawnSelector.transform.localPosition = new Vector3(0f, 0f, -600f);

		spawnSelector.Begin(null);

		yield return spawnSelector.WaitForFinish();
		yield break;
	}
}
