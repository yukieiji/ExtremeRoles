using UnityEngine;

namespace ExtremeRoles.Module.Interface;

#nullable enable

public interface IUnityObjectFactory
{
	public GameObject CreateGameObject(string name);
	public Material? CreateMaterial(Shader shader);
	public Mesh CreateMesh();
}

public class DefaultUnityObjectFactory : IUnityObjectFactory
{
	public GameObject CreateGameObject(string name)
	{
		return new GameObject(name);
	}

	public Material? CreateMaterial(Shader shader)
	{
		return shader != null ? new Material(shader) : null;
	}

	public Mesh CreateMesh()
	{
		return new Mesh();
	}
}
