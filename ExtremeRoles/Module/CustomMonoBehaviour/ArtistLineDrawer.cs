using System;

using UnityEngine;

using ExtremeRoles.Extension.Vector;
using ExtremeRoles.Module.SystemType;

#nullable enable

namespace ExtremeRoles.Module.CustomMonoBehaviour;

[Il2CppRegister]
public sealed class ArtistLineDrawer :  MonoBehaviour
{

	public float Area { get; private set; } = 0;

#pragma warning disable CS8618 // null 非許容のフィールドには、コンストラクターの終了時に null 以外の値が入っていなければなりません。Null 許容として宣言することをご検討ください。
	private LineRenderer rend;
#pragma warning restore CS8618 // null 非許容のフィールドには、コンストラクターの終了時に null 以外の値が入っていなければなりません。Null 許容として宣言することをご検討ください。

	public PlayerControl ArtistPlayer
	{
		set
		{
			this.artistPlayer = value;
			this.prevPos = this.artistPlayer.transform.position;
			this.size = 1;
			addLinePos();
		}
	}
	private Vector3? prevPos = null;
	private PlayerControl? artistPlayer;
	private int size = 1;

	private const float lineSize = 0.25f;

#pragma warning disable CS8618 // null 非許容のフィールドには、コンストラクターの終了時に null 以外の値が入っていなければなりません。Null 許容として宣言することをご検討ください。
	public ArtistLineDrawer(IntPtr ptr) : base(ptr)
	{
	}
#pragma warning restore CS8618 // null 非許容のフィールドには、コンストラクターの終了時に null 以外の値が入っていなければなりません。Null 許容として宣言することをご検討ください。

	public void Awake()
	{
		this.rend = this.gameObject.AddComponent<LineRenderer>();

		this.rend.startWidth = lineSize;
		this.rend.endWidth = lineSize;

		var allColor = Palette.PlayerColors;
		int useColorIndex = RandomGenerator.Instance.Next(allColor.Count);
		var useColor = allColor[useColorIndex];

		this.rend.material = new Material(HatManager.Instance.PlayerMaterial);
		this.rend.SetColors(useColor, useColor);
	}

	public void FixedUpdate()
	{
		if (this.artistPlayer == null ||
			this.artistPlayer.Data == null ||
			this.prevPos == null ||
			!GameProgressSystem.IsTaskPhase)
		{
			return;
		}

		var curPos = this.artistPlayer.transform.position;

		if (!this.prevPos.HasValue ||
			curPos.IsCloseTo(this.prevPos.Value))
		{
			return;
		}

		if (PlayerControl.LocalPlayer.PlayerId == this.artistPlayer.PlayerId)
		{
			float distance = Vector3.Distance(curPos, this.prevPos.Value);
			this.Area += distance * lineSize;
		}

		this.prevPos = curPos;
		addLinePos();
	}

	private void addLinePos()
	{
		if (this.prevPos == null) { return; }

		this.rend.positionCount = this.size;
		this.rend.SetPosition(this.size - 1, this.prevPos.Value);

		++this.size;
	}
}
