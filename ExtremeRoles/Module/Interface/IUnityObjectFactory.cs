using UnityEngine;

namespace ExtremeRoles.Module.Interface;

#nullable enable

public interface IUnityObjectFactory : IGameObjectFactory
{
	public Material CreateMaterial(Shader shader);
	public Mesh CreateMesh();
}

public class DefaultUnityObjectFactory : DefaultGameObjectFactory, IUnityObjectFactory
{
	public Material CreateMaterial(Shader shader)
	{
		return new Material(shader);
	}

	public Mesh CreateMesh()
	{
		return new Mesh();
	}
}
