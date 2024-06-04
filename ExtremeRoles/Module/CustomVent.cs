using System.Collections.Generic;

using ExtremeRoles.Resources;
using ExtremeRoles.Roles;

using UnityEngine;

#nullable enable

namespace ExtremeRoles.Module;

public sealed class CustomVent : NullableSingleton<CustomVent>
{
	public enum Type
	{
		Mery
	}

	private readonly Dictionary<int, Type> idType = new Dictionary<int, Type>();
	private readonly Dictionary<Type, List<Vent>> allVent = new Dictionary<Type, List<Vent>>();
	private readonly Dictionary<Type, Sprite?[]> ventAnimation = new Dictionary<Type, Sprite?[]>();

	public CustomVent()
	{
		this.idType.Clear();
		this.allVent.Clear();
		this.ventAnimation.Clear();
	}

	public bool Contains(int id) => this.idType.ContainsKey(id);

	public Sprite? GetSprite(int id, int index)
	{
		if (!this.idType.TryGetValue(id, out Type type) ||
			!this.ventAnimation.TryGetValue(type, out var sprite) ||
			sprite == null) { return null; }

		Sprite? img = sprite[index];

		if (img != null)
		{
			return img;
		}
		else
		{
			ExtremeRoleId roleId = type switch
			{
				Type.Mery => ExtremeRoleId.Mery,
				_ => ExtremeRoleId.Null,
			};
			if (roleId is ExtremeRoleId.Null)
			{
				return null;
			}

			Sprite newImg = Loader.GetUnityObjectFromResources<Sprite, ExtremeRoleId>(
				roleId, $"{index}");

			this.ventAnimation[type][index] = newImg;
			return newImg;
		}
	}

	public bool TryGet(Type type, out List<Vent>? vent)
		=> this.allVent.TryGetValue(type, out vent);

	public void Add(Type type, Vent newVent, int spriteSize = 18)
	{
		if (this.allVent.TryGetValue(type, out var teypeVent))
		{
			teypeVent.Add(newVent);
		}
		else
		{
			List<Vent> ventList = [newVent];
			this.allVent.Add(type, ventList);
		}

		if (!this.ventAnimation.ContainsKey(type))
		{
			this.ventAnimation.Add(type, new Sprite?[spriteSize]);
		}

		this.idType.Add(newVent.Id, type);
	}
}
