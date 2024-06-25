using System.IO;
using System.Text;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.AddressableAssets;

using ExtremeSkins.Core.ExtremeVisor;
using ExtremeSkins.Module.Interface;
using ExtremeRoles.Performance;
using ExtremeRoles.Module;
using ExtremeRoles;
using ExtremeSkins.Core;

namespace ExtremeSkins.Module;

#if WITHVISOR
public class CustomVisor : ICustomCosmicData<VisorData, VisorViewData>
{
    public VisorData Data
	{
		get
		{
			if (this.data == null)
			{
				this.data = this.createData();
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
        get => $"visor_{new DirectoryInfo(this.FolderPath).Name}_{this.Author}_{this.Name}";
    }

	public Sprite? Preview { get; private set; }

	protected readonly string FolderPath;
	protected readonly VisorInfo Info;
	protected VisorViewData? View;

	private VisorData? data;

	public CustomVisor(
        string folderPath,
        VisorInfo info)
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

	public virtual VisorViewData GetViewData()
	{
		if (this.View == null ||
			this.View.IdleFrame == null)
		{
			this.View = this.loadViewData();
		}
		return this.View;
	}

	private VisorData createData()
	{

		var data = ScriptableObject.CreateInstance<VisorData>();
		data.name = Helper.Translation.GetString(this.Name);
		data.displayOrder = 99;
		data.ProductId = this.Id;
		data.ChipOffset = new Vector2(0f, 0.2f);
		data.Free = true;
		data.NotInStore = true;

		this.Preview = GetSprite(Path.Combine(this.FolderPath, DataStructure.IdleImageName));
		data.behindHats = this.Info.BehindHat;
		data.PreviewCrewmateColor = this.Info.Shader;

		this.View = ScriptableObject.CreateInstance<VisorViewData>();
		data.ViewDataRef = new AssetReference(this.View.Pointer);

		return data;

	}

	private VisorViewData loadViewData()
	{
		var view = ScriptableObject.CreateInstance<VisorViewData>();

		view.IdleFrame = this.Preview;

		if (this.Info.LeftIdle)
		{
			view.LeftIdleFrame = GetSprite(
				Path.Combine(this.FolderPath, DataStructure.FlipIdleImageName));
		}
		view.MatchPlayerColor = this.Info.Shader;

		return view;
	}


	protected static Sprite? GetSprite(string path)
	{
		var result = loadVisorSprite(path);
		if (!result.HasValue())
		{
			ExtremeSkinsPlugin.Logger.LogError(result.Error.ToString());
		}
		return result.GetRawValue();
	}

	private static Expected<Sprite, Loader.LoadError> loadVisorSprite(
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

public sealed class AnimationVisor : CustomVisor
{
	private const int MaxInt = 60 * 30;
	private int counter = 0;
	private Dictionary<string, Sprite?> cacheSprite = new Dictionary<string, Sprite?>();

	public AnimationVisor(string folderPath, VisorInfo info)
		: base(folderPath, info)
	{ }

	public override VisorViewData GetViewData()
	{
		this.View = base.GetViewData();
		updateVisorView();
		return this.View;
	}

	private void updateVisorView()
	{
		if (CachedPlayerControl.LocalPlayer == null ||
			CachedPlayerControl.LocalPlayer.PlayerPhysics == null ||
			CachedPlayerControl.LocalPlayer.PlayerPhysics.Animations == null) { return; }

		bool isFlip = CachedPlayerControl.LocalPlayer.PlayerControl.cosmetics.FlipX;
		var animation = this.Info.Animation!;
		bool hasLeftIdle = this.Info.LeftIdle;

		if (animation.Idle is not null &&
			(!isFlip || !hasLeftIdle) &&
			this.counter % animation.Idle.FrameCount == 0)
		{
			this.View!.IdleFrame = getNextSprite(animation.Idle);
		}
		if (hasLeftIdle &&
			animation.LeftIdle is not null &&
			isFlip &&
			this.counter % animation.LeftIdle.FrameCount == 0)
		{
			this.View!.LeftIdleFrame = getNextSprite(animation.LeftIdle);
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
				sprite = this.Preview;
			}
			this.cacheSprite.Add(path, sprite);
		}
		return sprite;
	}
}
#endif
