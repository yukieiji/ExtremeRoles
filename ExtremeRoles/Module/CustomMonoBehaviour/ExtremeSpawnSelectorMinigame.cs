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

namespace ExtremeRoles.Module.CustomMonoBehaviour;

[Il2CppRegister]
public sealed class ExtremeSpawnSelectorMinigame : Minigame
{
	private bool gotButton;
	private TextMeshPro text;

	private readonly record struct SpawnSelector(
		GameObject Obj,
		SpriteRenderer Renderer,
		UiElement Element);


	public ExtremeSpawnSelectorMinigame(IntPtr ptr) : base(ptr)
	{ }

	public void Awake()
	{
		this.text = Instantiate(Prefab.Text, base.transform);
		this.text.alignment = TextAlignmentOptions.Center;
		this.text.gameObject.SetActive(true);
		this.text.transform.localPosition = new Vector3(0.0f, 0.0f);

		this.MyTask = null;
		this.multistageMinigameChecked = true;
		this.TransType = TransitionType.SlideBottom;
	}

	public override void Begin(PlayerTask? task)
	{
		this.AbstractBegin(task);

		base.StartCoroutine(this.runTimer().WrapToIl2Cpp());

		// ControllerManager.Instance.OpenOverlayMenu(base.name, null, this.DefaultButtonSelected, this.ControllerSelectable, false);

		PlayerControl.HideCursorTemporarily();
		ConsoleJoystick.SetMode_Menu();
	}

	public override void Close()
	{
		ControllerManager.Instance.CloseOverlayMenu(base.name);
		if (!this.gotButton)
		{

		}
		base.Close();
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
			this.text.text = DestroyableSingleton<TranslationController>.Instance.GetString(
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
		this.gotButton = true;
		PlayerControl.LocalPlayer.SetKinematic(true);
		PlayerControl.LocalPlayer.NetTransform.SetPaused(true);
		PlayerControl.LocalPlayer.NetTransform.RpcSnapTo(spawnPoint.Location);
		DestroyableSingleton<HudManager>.Instance.PlayerCam.SnapToTarget();

		base.StopAllCoroutines();
		base.StartCoroutine(
			this.coSpawnAt(CachedPlayerControl.LocalPlayer, spawnPoint).WrapToIl2Cpp());
	}

	private IEnumerator coSpawnAt(PlayerControl playerControl, SpawnInMinigame.SpawnLocation spawnLocation)
	{
		yield return new WaitForFixedUpdate();
		yield return new WaitForFixedUpdate();
		yield return new WaitForFixedUpdate();

		playerControl.SetKinematic(false);
		playerControl.NetTransform.SetPaused(false);
		this.Close();

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
