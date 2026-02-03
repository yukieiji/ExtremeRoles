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
	public PlayerControl? Target { private get; set; }
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
			outLine.Target = target;
			cache[target.PlayerId] = outLine;
		}
		outLine.SetOutlineColor(color);
	}

	[HideFromIl2Cpp]
	public void SetOutlineColor(Color color)
	{
		if (!TryGetValidMaterial(out var material))
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
			!TryGetValidMaterial(out var material))
		{
			this.blocked = false;
			return;
		}
		material.SetFloat("_Outline", 0f);
	}

	private bool TryGetValidMaterial(
		[NotNullWhen(true)] out Material? outLineMaterial)
	{
		outLineMaterial = null;
		if (this.Target == null ||
			this.Target.cosmetics == null ||
			this.Target.cosmetics.currentBodySprite == null ||
			this.Target.cosmetics.currentBodySprite.BodySprite == null ||
			this.Target.cosmetics.currentBodySprite.BodySprite.material == null)
		{
			return false;
		}
		outLineMaterial = this.Target.cosmetics.currentBodySprite.BodySprite.material;
		return true;
	}
}
