﻿using ExtremeRoles.Extension.Task;
using System;
using System.Collections.Generic;

using UnityEngine;
using TMPro;

using ExtremeRoles.Module.CustomMonoBehaviour.UIPart;
using ExtremeRoles.Module.SystemType.Roles;
using ExtremeRoles.Module.SystemType;

#nullable enable

namespace ExtremeRoles.Module.CustomMonoBehaviour.Minigames;

[Il2CppRegister]
public sealed class YokoYashiroStatusUpdateMinigame(IntPtr ptr) : Minigame(ptr)
{
	public YokoYashiroSystem.YashiroInfo? Info { private get; set; }

	private YokoYashiroLinePoint? prefab;
	private LineRenderer? line;
	private TextMeshPro? statusText;
	private Transform? anchor;

	private int curPointIndex = 0;

	private readonly List<YokoYashiroLinePoint> allPoint = new List<YokoYashiroLinePoint>();
	private readonly List<YokoYashiroLinePoint> drawPoint = new List<YokoYashiroLinePoint>();

	private YokoYashiroLinePoint curPoint => this.allPoint[this.curPointIndex];
	private bool isClose = false;

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
			Vector3 localMousePosition = this.anchor.InverseTransformPoint(mouseWorldPint.x, mouseWorldPint.y, 0.0f);
			return localMousePosition;
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

		int index = 0;
		foreach (var vec in new Vector2[]
			{
					new Vector2(0.0f, 1.5f),
					new Vector2(1.5f, 0.0f),
					new Vector2(0.0f, -1.5f),
					new Vector2(-1.5f, 0.0f) })
		{
			var newPoint = Instantiate(
				this.prefab, this.anchor);
			newPoint.transform.localPosition = vec;
			newPoint.gameObject.SetActive(true);
			newPoint.Text.text = index.ToString();
			this.allPoint.Add(newPoint);

			index++;
		}
		this.curPointIndex = 0;
		this.resetLine();
	}

	public void FixedUpdate()
	{
		if (this.Info is null ||
			this.statusText == null ||
			this.line == null ||
			this.anchor == null)
		{
			return;
		}

		this.Info.Timer -= Time.fixedDeltaTime;
		this.statusText.text = $"現在の社の状態：{this.Info.Status}\nへ以降まで{this.Info.Timer}秒";

		if (!this.isClose &&
			this.curPointIndex == this.allPoint.Count)
		{
			this.isClose = true;
			if (ExtremeSystemTypeManager.Instance.TryGet<YokoYashiroSystem>(
				YokoYashiroSystem.Type, out var system) &&
				system is not null)
			{
				system.RpcUpdateNextStatus(this.Info);
			}
			// テキストを表示
			this.AbstractClose();
			return;
		}

		if (Input.GetMouseButton(0))
		{
			Vector2 mousePosition = this.curMousePoint;

			this.updateDrawPoint(mousePosition);

			this.line.positionCount = this.drawPoint.Count + 1;
			for (int i = 0; i < this.drawPoint.Count; ++i)
			{
				Vector2 pos = this.drawPoint[i].transform.localPosition + this.anchor.localPosition;
				this.line.SetPosition(i, pos);
			}
			this.line.SetPosition(this.drawPoint.Count, mousePosition);
		}
		else
		{
			this.resetLine();
		}
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
		Vector2 pointPos = this.curPoint.transform.localPosition;
		Vector2 diff = pointPos - source;
		return diff.magnitude <= 0.45f;
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
