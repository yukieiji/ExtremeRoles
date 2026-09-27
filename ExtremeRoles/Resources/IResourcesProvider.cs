using UnityEngine;

namespace ExtremeRoles.Resources;

#nullable enable

public interface IResourcesProvider
{
	public Sprite LoadSprite(string objName);
}

public class DefaultResourcesProvider : IResourcesProvider
{
	public Sprite LoadSprite(string objName)
	{
		return UnityObjectLoader.LoadFromResources<Sprite>(objName);
	}
}
