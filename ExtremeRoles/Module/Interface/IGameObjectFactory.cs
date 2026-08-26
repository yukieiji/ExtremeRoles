using UnityEngine;

namespace ExtremeRoles.Module.Interface;

#nullable enable

public interface IGameObjectFactory
{
	public GameObject Create(string name);
}

public class DefaultGameObjectFactory : IGameObjectFactory
{
	public GameObject Create(string name)
	{
		return new GameObject(name);
	}
}
