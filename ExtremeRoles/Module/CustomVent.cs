using System.Collections.Generic;

using ExtremeRoles.Resources;

using UnityEngine;

#nullable enable

namespace ExtremeRoles.Module;

public sealed class CustomVent : NullableSingleton<CustomVent>
{
	public enum Type
	{
		MeryVent
	}

	private readonly Dictionary<int, Type> idType = new Dictionary<int, Type>();
	private readonly Dictionary<Type, List<Vent>> allVent = new Dictionary<Type, List<Vent>>();
	private readonly Dictionary<Type, Sprite[]> ventAnimation = new Dictionary<Type, Sprite[]>();

	public CustomVent()
	{
		this.idType.Clear();
		this.allVent.Clear();
		this.ventAnimation.Clear();
	}

	public bool Contains(int id) => this.idType.ContainsKey(id);

	public Sprite? GetSprite(int id, int index)
	{
		if (!this.idType.TryGetValue(id, out Type type)) { return null; }

		if (this.ventAnimation.TryGetValue(type, out var sprite) &&
			sprite != null)
		{
			return sprite[index];
		}
		else
		{
			string imgFormat = type switch
			{
				Type.MeryVent => Path.MeryCustomVentAnime,
				_ => string.Empty,
			};
			if (string.IsNullOrEmpty(imgFormat))
			{
				return null;
			}

			Sprite? newImg = Loader.CreateSpriteFromResources(
				string.Format(imgFormat, index), 125f);

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
			this.ventAnimation.Add(type, new Sprite[spriteSize]);
		}

		this.idType.Add(newVent.Id, type);
	}
}
