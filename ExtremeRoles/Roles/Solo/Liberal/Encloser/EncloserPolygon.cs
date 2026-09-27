using System.Collections.Generic;

using Il2CppInterop.Runtime.InteropTypes.Arrays;
using UnityEngine;

using ExtremeRoles.Core;
using ExtremeRoles.Core.Abstract;
using ExtremeRoles.Module;
using ExtremeRoles.Module.CustomMonoBehaviour;
using ExtremeRoles.Module.Interface;
using ExtremeRoles.Resources;

#nullable enable

namespace ExtremeRoles.Roles.Solo.Liberal.Encloser;

public sealed class EncloserPolygon(IUnityObjectFactory? factory = null, IIl2CppObjectProvider? il2cppProvider = null)
{
	public bool IsCompleted { get; private set; }
	public int Count => this.stakeObjects.Count;

	private readonly List<GameObject> stakeObjects = [];
	private readonly IUnityObjectFactory factory = factory ?? new DefaultUnityObjectFactory();
	private readonly IIl2CppObjectProvider il2cppProvider = il2cppProvider ?? new DefaultIl2CppObjectProvider();

	private LineRenderer? lineRenderer;
	private MeshFilter? meshFilter;

	public void AddStake(Vector2 pos, int maxStakes)
	{
		var stake = this.factory.CreateGameObject($"EncloserStake_{this.stakeObjects.Count + 1}");
		var sr = stake.AddComponent<SpriteRenderer>();
		sr.sprite = UnityObjectLoader.LoadSpriteFromResources(ObjectPath.Bomb);
		sr.color = ColorPalette.LiberalColor;
		stake.transform.position = this.factory.CreateMapPos(pos);
		this.stakeObjects.Add(stake);

		if (this.stakeObjects.Count >= maxStakes)
		{
			this.IsCompleted = true;
		}

		rebuildLinesAndMesh();
	}

	public bool IsPointInside(Vector2 point)
	{
		int count = this.stakeObjects.Count;
		if (!this.IsCompleted || count < 3)
		{
			return false;
		}

		bool inside = false;
		float px = point.x;
		float py = point.y;

		int j = count - 1;
		for (int i = 0; i < count; i++)
		{
			Vector3 posI = this.stakeObjects[i].transform.position;
			Vector3 posJ = this.stakeObjects[j].transform.position;
			float ix = posI.x;
			float iy = posI.y;
			float jx = posJ.x;
			float jy = posJ.y;

			bool intersect = ((iy > py) != (jy > py)) &&
				(px < (jx - ix) * (py - iy) / (jy - iy) + ix);

			if (intersect)
			{
				inside = !inside;
			}

			j = i;
		}

		return inside;
	}

	public void UpdateVisuals(bool isLocalPlayerEncloser)
	{
		setObjectActive(this.IsCompleted || isLocalPlayerEncloser);
	}

	public void Clear()
	{
		setObjectActive(false);

		foreach (var stake in this.stakeObjects)
		{
			Object.Destroy(stake);
		}

		this.stakeObjects.Clear();

		if (this.lineRenderer != null)
		{
			if (this.lineRenderer.gameObject != null)
			{
				Object.Destroy(this.lineRenderer.gameObject);
			}

			this.lineRenderer = null;
		}

		if (this.meshFilter != null)
		{
			if (this.meshFilter.gameObject != null)
			{
				Object.Destroy(this.meshFilter.gameObject);
			}

			this.meshFilter = null;
		}

		this.IsCompleted = false;
	}

	private void rebuildLinesAndMesh()
	{
		int count = this.stakeObjects.Count;
		if (count < 2)
		{
			return;
		}

		if (this.lineRenderer == null)
		{
			var linesObj = this.factory.CreateGameObject("EncloserLines");
			this.lineRenderer = linesObj.AddComponent<LineRenderer>();
			this.lineRenderer.material = this.factory.CreateSpriteMaterial();
			this.lineRenderer.startWidth = 0.15f;
			this.lineRenderer.endWidth = 0.15f;
			this.lineRenderer.startColor = ColorPalette.LiberalColor;
			this.lineRenderer.endColor = ColorPalette.LiberalColor;
		}

		this.lineRenderer.positionCount = count + (this.IsCompleted ? 1 : 0);

		// 線は杭より後ろに置きたいので
		for (int i = 0; i < count; i++)
		{
			Vector3 pos = this.stakeObjects[i].transform.position;
			var linePos = this.factory.CreateMapPos(pos, 975.0f);
			this.lineRenderer.SetPosition(i, linePos);
		}

		if (this.IsCompleted)
		{
			Vector3 firstPos = this.stakeObjects[0].transform.position;
			var firstLinePos = this.factory.CreateMapPos(firstPos, 975.0f);
			this.lineRenderer.SetPosition(count, firstLinePos);
			buildMesh();
		}
	}

	private void buildMesh()
	{
		int count = this.stakeObjects.Count;
		if (count < 3)
		{
			return;
		}

		if (this.meshFilter == null)
		{
			var meshObj = this.factory.CreateGameObject("EncloserPolygonMesh");
			this.meshFilter = meshObj.AddComponent<MeshFilter>();
			var meshRenderer = meshObj.AddComponent<MeshRenderer>();
			meshRenderer.material = this.factory.CreateSpriteMaterial();
			meshObj.AddComponent<EncloserPolygonMeshBehaviour>();
		}

		Mesh mesh = this.factory.CreateMesh();
		Il2CppStructArray<Vector3> vertices = this.il2cppProvider.GetStructArray<Vector3>(count);
		for (int i = 0; i < count; i++)
		{
			Vector3 pos = this.stakeObjects[i].transform.position;
			vertices[i] = this.factory.CreateMapPos(pos, 950.0f);
		}

		int triangleCount = (count - 2) * 3;
		Il2CppStructArray<int> triangles = this.il2cppProvider.GetStructArray<int>(triangleCount);
		int tIndex = 0;
		for (int i = 1; i < count - 1; i++)
		{
			triangles[tIndex++] = 0;
			triangles[tIndex++] = i;
			triangles[tIndex++] = i + 1;
		}

		Color yellowFill = new Color(
			ColorPalette.LiberalColor.r,
			ColorPalette.LiberalColor.g,
			ColorPalette.LiberalColor.b,
			0.3f);

		Il2CppStructArray<Color> colors = this.il2cppProvider.GetStructArray<Color>(count);
		for (int i = 0; i < colors.Length; i++)
		{
			colors[i] = yellowFill;
		}

		mesh.vertices = vertices;
		mesh.triangles = triangles;
		mesh.colors = colors;
		mesh.RecalculateBounds();

		this.meshFilter.mesh = mesh;
	}

	private void setObjectActive(bool showVisuals)
	{
		if (this.lineRenderer != null && this.lineRenderer.gameObject != null)
		{
			this.lineRenderer.gameObject.SetActive(showVisuals);
		}

		if (this.meshFilter != null && this.meshFilter.gameObject != null)
		{
			this.meshFilter.gameObject.SetActive(showVisuals);
		}
	}
}