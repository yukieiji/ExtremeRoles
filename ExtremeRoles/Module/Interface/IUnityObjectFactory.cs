using UnityEngine;

namespace ExtremeRoles.Module.Interface;

#nullable enable

public interface IUnityObjectFactory
{
	public GameObject CreateGameObject(string name);
	public Material CreateMaterial(Shader shader);
	public Material CreateSpriteMaterial();
	public Mesh CreateMesh();
}

public class DefaultUnityObjectFactory : IUnityObjectFactory
{
	public GameObject CreateGameObject(string name)
	{
		return new GameObject(name);
	}

	public Material CreateMaterial(Shader shader)
	{
		return new Material(shader);
	}

	public Material CreateSpriteMaterial()
	{
		return new Material(Shader.Find("Sprites/Default"));
	}

	public Mesh CreateMesh()
	{
		return new Mesh();
	}
}
