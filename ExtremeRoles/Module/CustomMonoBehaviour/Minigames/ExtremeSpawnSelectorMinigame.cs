using System;
using System.Collections;
using System.Linq;
using System.Text;

using TMPro;
using UnityEngine;

using BepInEx.Unity.IL2CPP.Utils.Collections;

using ExtremeRoles.Helper;
using ExtremeRoles.Performance;
using ExtremeRoles.Extension.Task;

using Il2CppObject = Il2CppSystem.Object;

#nullable enable

namespace ExtremeRoles.Module.CustomMonoBehaviour.Minigames;

[Il2CppRegister]
public sealed class ExtremeSpawnSelectorMinigame : Minigame
{
	private bool gotButton;

	private readonly record struct SpawnSelector(
		GameObject Obj,
		SpriteRenderer Renderer,
		UiElement Element);


#pragma warning disable CS8618 // null 非許容のフィールドには、コンストラクターの終了時に null 以外の値が入っていなければなりません。Null 許容として宣言することをご検討ください。
	private TextMeshPro text;
	public ExtremeSpawnSelectorMinigame(IntPtr ptr) : base(ptr)
#pragma warning restore CS8618 // null 非許容のフィールドには、コンストラクターの終了時に null 以外の値が入っていなければなりません。Null 許容として宣言することをご検討ください。
	{ }

	public void Awake()
	{
		text = Instantiate(Prefab.Text, base.transform);
		text.alignment = TextAlignmentOptions.Center;
		text.gameObject.SetActive(true);
		text.transform.localPosition = new Vector3(0.0f, -1.0f);
		text.fontSize = text.fontSizeMin = text.fontSizeMax = 4.0f;

		this.MyTask = null;
		this.multistageMinigameChecked = true;
		this.TransType = TransitionType.SlideBottom;
	}

	public override void Begin(PlayerTask? task)
	{
		this.AbstractBegin(task);

		base.StartCoroutine(runTimer().WrapToIl2Cpp());

		// ControllerManager.Instance.OpenOverlayMenu(base.name, null, this.DefaultButtonSelected, this.ControllerSelectable, false);

		PlayerControl.HideCursorTemporarily();
		ConsoleJoystick.SetMode_Menu();
	}

	public override void Close()
	{
		// ControllerManager.Instance.CloseOverlayMenu(base.name);
		if (!gotButton)
		{

		}
		this.AbstractClose();
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
		for (float time = 128f; time >= 0f; time -= Time.deltaTime)
		{
			text.text = FastDestroyableSingleton<TranslationController>.Instance.GetString(
				StringNames.TimeRemaining, new Il2CppObject[] { Mathf.CeilToInt(time) });
			yield return null;
		}
		yield break;
	}

	private void spawnAt(SpawnInMinigame.SpawnLocation spawnPoint)
	{
		if (this.amClosing != Minigame.CloseState.None)
		{
			return;
		}
		Logger.GlobalInstance.Info(string.Format("Player selected spawn point {0}", spawnPoint.Name), null);
		gotButton = true;
		PlayerControl.LocalPlayer.SetKinematic(true);
		PlayerControl.LocalPlayer.NetTransform.SetPaused(true);
		PlayerControl.LocalPlayer.NetTransform.RpcSnapTo(spawnPoint.Location);
		DestroyableSingleton<HudManager>.Instance.PlayerCam.SnapToTarget();

		base.StopAllCoroutines();
		base.StartCoroutine(
			coSpawnAt(CachedPlayerControl.LocalPlayer, spawnPoint).WrapToIl2Cpp());
	}

	private IEnumerator coSpawnAt(PlayerControl playerControl, SpawnInMinigame.SpawnLocation spawnLocation)
	{
		yield return new WaitForFixedUpdate();
		yield return new WaitForFixedUpdate();
		yield return new WaitForFixedUpdate();

		playerControl.SetKinematic(false);
		playerControl.NetTransform.SetPaused(false);
		Close();

		yield break;
	}
	/*
	private SpawnSelector createButton(string name)
	{
		var obj = new GameObject(name);
		obj.transform.SetParent(base.transform);

		var renderer =　obj.AddComponent<SpriteRenderer>();
		renderer.sprite = Resources.Loader.CreateSpriteFromResources(
			Resources.Path.MoverMove);
	}
	*/
}
