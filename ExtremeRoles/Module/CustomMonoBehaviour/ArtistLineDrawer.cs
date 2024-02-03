using ExtremeRoles.Performance;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

#nullable enable

namespace ExtremeRoles.Module.CustomMonoBehaviour;

public sealed class ArtistLineDrawer :  MonoBehaviour
{

	public float Area
	{
		get
		{
			float area = 0f;
			for (int i = 0; i < this.pos.Count - 1; ++i)
			{
				var prev = this.pos[i];
				var cur = this.pos[i + 1];

				var diff = prev - cur;
				area += diff.sqrMagnitude * lineSize;
			}
			return area;
		}
	}

#pragma warning disable CS8618 // null 非許容のフィールドには、コンストラクターの終了時に null 以外の値が入っていなければなりません。Null 許容として宣言することをご検討ください。
	private LineRenderer rend;
#pragma warning restore CS8618 // null 非許容のフィールドには、コンストラクターの終了時に null 以外の値が入っていなければなりません。Null 許容として宣言することをご検討ください。

	private readonly List<Vector3> pos = new List<Vector3>();
	private Vector3? prevPos = null;
	public PlayerControl? ArtistPlayer { private get; set; }

	private const float lineSize = 0.25f;

	public void Awake()
	{
		this.rend = this.gameObject.AddComponent<LineRenderer>();

		this.rend.startWidth = lineSize;
		this.rend.endWidth = lineSize;

		var allColor = Palette.PlayerColors;
		int useColorIndex = RandomGenerator.Instance.Next(allColor.Count);
		var useColor = allColor[useColorIndex];

		this.rend.startColor = useColor;
		this.rend.endColor = useColor;

		this.pos.Clear();
	}

	public void FixedUpdate()
	{
		if (this.ArtistPlayer == null ||
			this.ArtistPlayer.Data != null)
		{
			return;
		}

		var curPos = this.ArtistPlayer.transform.position;

		if (curPos == this.prevPos) { return; }

		this.prevPos = curPos;
		this.pos.Add(curPos);

		this.rend.positionCount = this.pos.Count;
		this.rend.SetPositions(this.pos.ToArray());
	}
}
