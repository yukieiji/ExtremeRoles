using UnityEngine;
using ExtremeRoles.Resources;

namespace ExtremeRoles.Module.Interface;

#nullable enable

public interface ISpriteLoader
{
	public Sprite LoadSprite(string bundleName, string objName);
}

public class DefaultSpriteLoader : ISpriteLoader
{
	public Sprite LoadSprite(string bundleName, string objName)
	{
		return UnityObjectLoader.LoadFromResources<Sprite>(bundleName, objName);
	}
}
