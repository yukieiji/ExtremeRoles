using UnityEngine;

namespace ExtremeRoles.Module.Interface;

#nullable enable

public interface IUnityObjectFactory
{
	public GameObject CreateGameObject(string name);
	public Material CreateSpriteMaterial();
	public Vector3 CreateMapPos(Vector2 target, float offset = 1000.0f);
	public Mesh CreateMesh();
}

public class DefaultUnityObjectFactory : IUnityObjectFactory
{
	public GameObject CreateGameObject(string name)
		=> new GameObject(name);

	public Vector3 CreateMapPos(Vector2 target, float offset = 1000.0f)
		=> new Vector3(target.x, target.y, target.y / offset);

	public Material CreateSpriteMaterial()
		=> new Material(Shader.Find("Sprites/Default"));

	public Mesh CreateMesh()
		=> new Mesh();
}
