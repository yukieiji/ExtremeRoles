using ExtremeRoles.Extension.Il2Cpp;
using Il2CppInterop.Runtime.Attributes;
using System;
using System.Collections.Generic;
using UnityEngine;

#nullable enable

namespace ExtremeRoles.Module.CustomMonoBehaviour;

[Il2CppRegister]
public sealed class PlayerOutLine(IntPtr ptr) : MonoBehaviour(ptr)
{
	public Material? OutLineMaterial { private get; set; }
	private bool blocked = false;

	private static Dictionary<byte, PlayerOutLine> cache = [];

	public static void ClearCache()
	{
		cache.Clear();
	}

	public static void SetOutline(
		PlayerControl? target,
		Color color)
	{
		if (target == null ||
			target.cosmetics == null ||
			target.cosmetics.currentBodySprite == null ||
			target.cosmetics.currentBodySprite.BodySprite == null)
		{
			return;
		}
		if (!cache.TryGetValue(target.PlayerId, out var outLine) ||
			outLine == null)
		{
			outLine = target.gameObject.TryAddComponent<PlayerOutLine>();
		}
		outLine.SetOutlineColor(color);
	}

	[HideFromIl2Cpp]
	public void SetOutlineColor(Color color)
	{
		if (this.OutLineMaterial == null)
		{
			return;
		}
		this.blocked = true;
		this.OutLineMaterial.SetFloat("_Outline", 1f);
		this.OutLineMaterial.SetColor("_OutlineColor", color);
	}

	public void LateUpdate()
	{
		if (this.OutLineMaterial == null)
		{
			return;
		}
		if (this.blocked)
		{
			this.blocked = false;
			return;
		}
		this.OutLineMaterial.SetFloat("_Outline", 0f);
	}
}
