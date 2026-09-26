using System;
using UnityEngine;

#nullable enable

namespace ExtremeRoles.Module.CustomMonoBehaviour;

[Il2CppRegister]
public sealed class EncloserPolygonMeshBehaviour : MonoBehaviour
{
#pragma warning disable CS8618
	private MeshRenderer meshRenderer;

	public EncloserPolygonMeshBehaviour(IntPtr ptr) : base(ptr) { }
#pragma warning restore CS8618

	public void Awake()
	{
		this.meshRenderer = base.GetComponent<MeshRenderer>();
	}

	public void Update()
	{
		if (this.meshRenderer != null && this.meshRenderer.material != null)
		{
			float alpha = Mathf.PingPong(Time.time * 2.0f, 0.3f) + 0.15f;
			Color color = new Color(
				ColorPalette.LiberalColor.r,
				ColorPalette.LiberalColor.g,
				ColorPalette.LiberalColor.b,
				alpha);
			this.meshRenderer.material.color = color;
		}
	}
}
