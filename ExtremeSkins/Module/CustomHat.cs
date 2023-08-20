using System.IO;
using System.Text;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.AddressableAssets;

using ExtremeSkins.Core;
using ExtremeSkins.Core.ExtremeHats;
using ExtremeSkins.Module.Interface;

using ExtremeRoles;
using ExtremeRoles.Performance;
using ExtremeRoles.Module;

namespace ExtremeSkins.Module;

#if WITHHAT
public class CustomHat : ICustomCosmicData<HatData, HatViewData>
{
	public HatData Data
	{
		get
		{
			if (this.data == null)
			{
				this.data = this.crateData();
			}
			return this.data!;
		}
	}

	public string Author
	{
		get => this.Info.Author;
	}
	public string Name
	{
		get => this.Info.Name;
	}

	public string Id
	{
		get => $"hat_{new DirectoryInfo(this.FolderPath).Name}_{this.Author}_{this.Name}";
	}

	protected readonly string FolderPath;
	protected readonly HatInfo Info;
	protected HatViewData? HatView;

	private HatData? data;

	public CustomHat(string folderPath, HatInfo info)
	{
		this.FolderPath = folderPath;
		this.Info = info;
	}

	public override string ToString()
	{
		StringBuilder builder = new StringBuilder();
		builder
			.Append(" - Name      : ")
			.AppendLine(this.Name)
			.Append(" - Author    : ")
			.AppendLine(this.Author)
			.Append("- Load from : ")
			.AppendLine(this.FolderPath)
			.Append(" - Id        : ")
			.Append(this.Id);

		return builder.ToString();
	}

	public virtual HatViewData GetViewData()
	{
		if (this.HatView == null ||
			this.HatView.MainImage == null)
		{
			this.HatView = this.createView();
		}
		return this.HatView;
	}

	private HatViewData createView()
	{
		var view = ScriptableObject.CreateInstance<HatViewData>();
		view.MainImage = GetSprite(Path.Combine(this.FolderPath, DataStructure.FrontImageName));

		if (this.Info.FrontFlip)
		{
			view.LeftMainImage = GetSprite(
				Path.Combine(this.FolderPath, DataStructure.FrontFlipImageName));
		}

		if (this.Info.Back)
		{
			view.BackImage = GetSprite(
				Path.Combine(this.FolderPath, DataStructure.BackImageName));
		}
		if (this.Info.BackFlip)
		{
			view.LeftBackImage = GetSprite(
				Path.Combine(this.FolderPath, DataStructure.BackFlipImageName));
		}

		if (this.Info.Climb)
		{
			view.ClimbImage = GetSprite(
				Path.Combine(this.FolderPath, DataStructure.ClimbImageName));
		}
		if (this.Info.Shader)
		{
			view.AltShader = FastDestroyableSingleton<HatManager>.Instance.PlayerMaterial;
		}
		return view;
	}

	private HatData crateData()
	{
		var data = ScriptableObject.CreateInstance<HatData>();

		data.name = Helper.Translation.GetString(this.Name);
		data.displayOrder = 99;
		data.ProductId = this.Id;
		data.InFront = !this.Info.Back;
		data.NoBounce = !this.Info.Bound;
		data.ChipOffset = new Vector2(0f, 0.2f);
		data.Free = true;
		data.NotInStore = true;
		data.PreviewCrewmateColor = this.Info.Shader;

		data.SpritePreview = GetSprite(Path.Combine(this.FolderPath, DataStructure.FrontImageName));

		this.HatView = ScriptableObject.CreateInstance<HatViewData>();
		data.ViewDataRef = new AssetReference(this.HatView.Pointer);

		return data;
	}

	protected static Sprite? GetSprite(string path)
	{
		var result = loadHatSprite(path);
		if (!result.HasValue())
		{
			ExtremeSkinsPlugin.Logger.LogError(result.Error.ToString());
		}
		var sprite = result.GetRawValue();
		return sprite;
	}

	private static Expected<Sprite, Loader.LoadError> loadHatSprite(
		string path)
	{
		var result = Loader.LoadTextureFromDisk(path);
		if (!result.HasValue())
		{
			return result.Error;
		}

		Texture2D texture = result.Value;
		Sprite sprite = Sprite.Create(
			texture, new Rect(0, 0, texture.width, texture.height),
			new Vector2(0.53f, 0.575f), texture.width * 0.375f);

		texture.hideFlags |= HideFlags.HideAndDontSave | HideFlags.DontUnloadUnusedAsset;
		sprite.hideFlags |= HideFlags.HideAndDontSave | HideFlags.DontUnloadUnusedAsset;

		return sprite;
	}
}

public sealed class AnimationHat : CustomHat
{
	private const int MaxInt = 60 * 30;
	private int counter = 0;
	private Dictionary<string, Sprite?> cacheSprite = new Dictionary<string, Sprite?>();

	public AnimationHat(string folderPath, HatInfo info)
		: base(folderPath, info)
	{ }

	public override HatViewData GetViewData()
	{
		this.HatView = base.GetViewData();
		updateHatView();
		return this.HatView;
	}

	private void updateHatView()
	{
		if (CachedPlayerControl.LocalPlayer == null ||
			CachedPlayerControl.LocalPlayer.PlayerPhysics == null ||
			CachedPlayerControl.LocalPlayer.PlayerPhysics.Animations == null) { return; }

		bool isFlip = CachedPlayerControl.LocalPlayer.PlayerControl.cosmetics.FlipX;
		var animationGroup = CachedPlayerControl.LocalPlayer.PlayerPhysics.Animations;
		var animation = this.Info.Animation!;

		if (!isFlip)
		{
			if (animation.Front != null &&
				this.counter % animation.Front.FrameCount == 0)
			{
				this.HatView!.MainImage = getNextSprite(animation.Front);
			}
			if (animation.Back != null &&
				this.counter % animation.Back.FrameCount == 0)
			{
				this.HatView!.BackImage = getNextSprite(animation.Back);
			}
		}
		if (isFlip)
		{
			if (animation.FrontFlip != null &&
				this.counter % animation.FrontFlip.FrameCount == 0)
			{
				this.HatView!.LeftMainImage = getNextSprite(animation.FrontFlip);
			}
			if (animation.BackFlip != null &&
				this.counter % animation.BackFlip.FrameCount == 0)
			{
				this.HatView!.LeftBackImage = getNextSprite(animation.BackFlip);
			}
		}
		if (animationGroup.IsPlayingClimbAnimation() &&
			animation.Climb != null &&
			this.counter % animation.Climb.FrameCount == 0)
		{
			this.HatView!.ClimbImage = getNextSprite(animation.Climb);
		}

		this.counter = (this.counter + 1) % MaxInt;
	}

	private Sprite? getNextSprite(AnimationInfo animation)
	{
		int length = animation.Img.Length;
		animation.CurIndex = animation.Type switch
		{
			AnimationInfo.ImageSelection.Sequential => (animation.CurIndex + 1) % length,
			AnimationInfo.ImageSelection.Random =>
				RandomGenerator.Instance.Next(length),
			_ => 0
		};

		string path = animation.Img[animation.CurIndex];
		if (!this.cacheSprite.TryGetValue(path, out var sprite))
		{
			sprite = GetSprite(Path.Combine(this.FolderPath, path));
			if (sprite == null)
			{
				sprite = this.Data.SpritePreview;
			}
			this.cacheSprite.Add(path, sprite);
		}
		return sprite;
	}
}
#endif
