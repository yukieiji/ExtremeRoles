using System;
using UnityEngine;

namespace ExtremeRoles.Resources;

#nullable enable

public interface IResourcesProvider
{
	public Sprite LoadSprite(string objName);
	public Sprite LoadRoleSprite<TEnum>(TEnum id, string imageName) where TEnum : Enum;
}

public class DefaultResourcesProvider : IResourcesProvider
{
	public Sprite LoadSprite(string objName)
	{
		return UnityObjectLoader.LoadFromResources<Sprite>(objName);
	}

	public Sprite LoadRoleSprite<TEnum>(TEnum id, string imageName) where TEnum : Enum
	{
		return UnityObjectLoader.LoadFromResources<Sprite, TEnum>(
			id,
			ObjectPath.GetRoleImgPath(id, imageName));
	}
}
