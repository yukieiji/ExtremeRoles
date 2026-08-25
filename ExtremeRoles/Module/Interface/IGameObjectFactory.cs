using UnityEngine;
using ExtremeRoles.Resources;

namespace ExtremeRoles.Module.Interface;

#nullable enable

public interface IGameObjectFactory
{
	public GameObject Create(string name);
	public Sprite LoadMultiAbilitySprite();
}

public class DefaultGameObjectFactory : IGameObjectFactory
{
	public GameObject Create(string name)
	{
		return new GameObject(name);
	}

	public Sprite LoadMultiAbilitySprite()
	{
		return UnityObjectLoader.LoadFromResources<Sprite>(
			ObjectPath.CommonTextureAsset,
			string.Format(ObjectPath.CommonImagePathFormat, "MultiAbility"));
	}
}
