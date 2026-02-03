using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

using UnityEngine;
using Il2CppInterop.Runtime.Attributes;

using ExtremeRoles.Extension.Il2Cpp;

#nullable enable

namespace ExtremeRoles.Module.CustomMonoBehaviour;

[Il2CppRegister]
public sealed class PlayerOutLine(IntPtr ptr) : MonoBehaviour(ptr)
{
	private PlayerControl? target { get; set; }
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
		if (target == null)
		{
			return;
		}
		if (!cache.TryGetValue(target.PlayerId, out var outLine) ||
			outLine == null)
		{
			outLine = target.gameObject.TryAddComponent<PlayerOutLine>();
			outLine.target = target;
			cache[target.PlayerId] = outLine;
		}
		outLine.SetOutlineColor(color);
	}

	[HideFromIl2Cpp]
	public void SetOutlineColor(Color color)
	{
		if (!tryGetValidMaterial(out var material))
		{
			return;
		}
		this.blocked = true;
		material.SetFloat("_Outline", 1f);
		material.SetColor("_OutlineColor", color);
	}

	public void LateUpdate()
	{
		if (this.blocked ||
			!tryGetValidMaterial(out var material))
		{
			this.blocked = false;
			return;
		}
		material.SetFloat("_Outline", 0f);
	}

	private bool tryGetValidMaterial(
		[NotNullWhen(true)] out Material? outLineMaterial)
	{
		if (this.target == null ||
			this.target.cosmetics == null ||
			this.target.cosmetics.currentBodySprite == null ||
			this.target.cosmetics.currentBodySprite.BodySprite == null ||
			this.target.cosmetics.currentBodySprite.BodySprite.material == null)
		{
			outLineMaterial = null;
			return false;
		}
		else
		{
			outLineMaterial = this.target.cosmetics.currentBodySprite.BodySprite.material;
			return true;
		}
	}
}
