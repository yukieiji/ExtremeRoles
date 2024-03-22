using System;
using System.Text;
using System.Text.Json;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using TMPro;
using BepInEx.Unity.IL2CPP.Utils;
using Il2CppInterop.Runtime.Attributes;

using ExtremeRoles.Helper;
using ExtremeRoles.Extension.Task;
using ExtremeRoles.Extension.Linq;
using ExtremeRoles.Module.SystemType;
using ExtremeRoles.Module.SystemType.Roles;
using ExtremeRoles.Module.CustomMonoBehaviour.UIPart;

#nullable enable

namespace ExtremeRoles.Module.CustomMonoBehaviour.Minigames;

[Il2CppRegister]
public sealed class YokoYashiroStatusUpdateMinigame(IntPtr ptr) : Minigame(ptr)
{
	[HideFromIl2Cpp]
	public YokoYashiroSystem.YashiroInfo? Info { get; set; }

	private YokoYashiroLinePoint? prefab;
	private LineRenderer? line;
	private TextMeshPro? statusText;
	private Transform? anchor;

	private int curPointIndex = 0;

	private record VectorClass(float X, float Y)
	{
		public Vector2 Vector => new Vector2(X, Y);
	}
	private readonly StringBuilder builder = new StringBuilder();
	private readonly List<YokoYashiroLinePoint> allPoint = new List<YokoYashiroLinePoint>();
	private readonly List<YokoYashiroLinePoint> drawPoint = new List<YokoYashiroLinePoint>();

	private YokoYashiroLinePoint curPoint => this.allPoint[this.curPointIndex];
	private bool isClose = false;

	private static Dictionary<string, VectorClass>[]? yokoJsonData = null;

	private Vector2 curMousePoint
	{
		get
		{
			if (this.anchor == null)
			{
				return Vector2.zero;
			}
			Vector3 mousePosition = Input.mousePosition;
			Vector3 mouseWorldPint = Camera.main.ScreenToWorldPoint(mousePosition);
			return mouseWorldPint;
		}
	}

	public override void Begin(PlayerTask? task)
	{
		this.AbstractBegin(task);

		var trans = base.transform;

		this.anchor = trans.Find("Anchor");
		this.prefab = trans.Find("PointPrefab").GetComponent<YokoYashiroLinePoint>();
		this.statusText = trans.Find("StatusText").GetComponent<TextMeshPro>();
		this.line = this.anchor.Find("Line").GetComponent<LineRenderer>();

		if (yokoJsonData is null)
		{
			yokoJsonData = JsonParser.LoadJsonStructFromAssembly<Dictionary<string, VectorClass>[]>(
				"ExtremeRoles.Resources.JsonData.YokoYahiroStatusUpdateMinigamePoint.json");
			if (yokoJsonData is null)
			{
				return;
			}
		}

		Dictionary<string, VectorClass> data = yokoJsonData.GetRandomItem();

		foreach (var (index, vec) in data)
		{
			var newPoint = Instantiate(
				this.prefab, this.anchor);
			newPoint.transform.localPosition = vec.Vector;
			newPoint.gameObject.SetActive(true);
			newPoint.Text.text = index;
			this.allPoint.Add(newPoint);
		}
		this.curPointIndex = 0;
		this.resetLine();
	}

	public override void Close()
		=> this.AbstractClose();

	public void FixedUpdate()
	{
		if (this.Info is null ||
			this.statusText == null ||
			this.line == null ||
			this.anchor == null)
		{
			return;
		}

		if (this.curPointIndex == this.allPoint.Count)
		{
			if (!this.isClose)
			{
				this.isClose = true;
				updateNextStatus();
				this.StartCoroutine(this.closeAndShowText());
			}
			return;
		}

		var nextStatus = YokoYashiroSystem.GetNextStatus(this.Info.Status);

		this.builder.Clear();
		this.builder.Append(
			Translation.GetString("yokoYashiroStatusText"));
		string statusText = Translation.GetString(this.Info.Status.ToString());

		if (this.Info.Status is YokoYashiroSystem.YashiroInfo.StatusType.YashiroDeactive ||
			this.Info.Timer == float.MaxValue)
		{
			this.statusText.text = string.Format(
				this.builder.ToString(), statusText);
		}
		else
		{
			this.Info.Timer -= Time.fixedDeltaTime;

			this.builder.AppendLine();
			this.builder.Append(
				Translation.GetString("yokoYashiroNextStatusText"));

			this.statusText.text = string.Format(
				this.builder.ToString(),
				statusText,
				Translation.GetString(nextStatus.ToString()),
				Mathf.Ceil(this.Info.Timer));

			if (this.Info.Timer < 0.0f)
			{
				updateNextStatus();
			}
		}

		if (Input.GetMouseButton(0) &&
			this.Info.Status is not YokoYashiroSystem.YashiroInfo.StatusType.YashiroSeal)
		{
			Vector2 mousePosition = this.curMousePoint;

			this.updateDrawPoint(mousePosition);

			this.line.positionCount = this.drawPoint.Count + 1;
			for (int i = 0; i < this.drawPoint.Count; ++i)
			{
				Vector2 pos = this.drawPoint[i].transform.position;
				this.line.SetPosition(i, pos);
			}
			this.line.SetPosition(this.drawPoint.Count, mousePosition);
		}
		else
		{
			this.resetLine();
		}
	}

	[HideFromIl2Cpp]
	private IEnumerator closeAndShowText()
	{
		if (this.Info is null) { yield break; }

		string statusText = Translation.GetString(this.Info.Status.ToString());
		string closeText = Translation.GetString("yokoYashiroCloseText");

		var text = this.transform.Find("CloseText").GetComponent<TextMeshPro>();

		text.text = string.Format(closeText, statusText);
		text.gameObject.SetActive(true);

		var waiter = new WaitForFixedUpdate();
		float timer = 0.0f;
		var farstPos = new Vector2(0.0f, -2.0f);
		while (timer <= 0.5f)
		{
			timer += (Time.fixedDeltaTime * 2);
			var newPos = Vector2.Lerp(farstPos, Vector2.zero, timer);
			text.transform.localPosition = newPos;

			yield return waiter;
		}
		text.transform.localPosition = Vector2.zero;

		yield return new WaitForSeconds(0.5f);

		this.AbstractClose();
	}

	private void updateDrawPoint(in Vector2 localMousePosition)
	{
		bool isNearCurPoint = this.isNearCurPoint(localMousePosition);

		if (!isNearCurPoint) { return; }

		this.drawPoint.Add(this.curPoint);
		this.curPointIndex++;
	}

	private bool isNearCurPoint(in Vector2 source)
	{
		Vector2 pointPos = this.curPoint.transform.position;
		Vector2 diff = pointPos - source;
		return diff.magnitude <= 0.35f;
	}

	private void updateNextStatus()
	{
		if (ExtremeSystemTypeManager.Instance.TryGet<YokoYashiroSystem>(
				YokoYashiroSystem.Type, out var system) &&
			system is not null &&
			this.Info is not null)
		{
			system.RpcUpdateNextStatus(this.Info);
		}
	}

	private void resetLine()
	{
		this.drawPoint.Clear();
		if (this.line != null)
		{
			this.line.positionCount = 0;
		}
		this.curPointIndex = 0;
	}
}
