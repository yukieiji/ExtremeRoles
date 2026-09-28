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
		=> UnityObjectLoader.LoadFromResources<Sprite>(objName);

	public Sprite LoadRoleSprite<TEnum>(TEnum id, string imageName) where TEnum : Enum
		=> UnityObjectLoader.LoadFromResources<Sprite, TEnum>(id, imageName);
}
