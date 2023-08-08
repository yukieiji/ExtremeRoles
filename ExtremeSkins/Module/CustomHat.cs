using System.IO;
using System.Text;

using UnityEngine;
using UnityEngine.AddressableAssets;

using ExtremeSkins.Core.ExtremeHats;
using ExtremeSkins.Module.Interface;

using ExtremeRoles.Performance;
using ExtremeRoles.Module;
using ExtremeSkins.SkinManager;
using ExtremeRoles;
using System.Collections.Generic;

namespace ExtremeSkins.Module;

#nullable enable

#if WITHHAT
public sealed class CustomHat : ICustomCosmicData<HatData, HatViewData>
{
    public HatData? Data { get; private set; }

    public string Author
    {
        get => this.info.Author;
    }
    public string Name
    {
        get => this.info.Name;
    }

    public string Id
    {
        get => $"hat_{new DirectoryInfo(this.folderPath).Name}_{this.Author}_{this.Name}";
    }

	private static Dictionary<string, Sprite?> spriteCache = new Dictionary<string, Sprite?>();

    private string folderPath;
	private NewHatInfo info;
	private HatViewData? hatView;

    public CustomHat(string folderPath, NewHatInfo info)
    {
        this.folderPath = folderPath;
		this.info = info;
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
			.AppendLine(this.folderPath)
			.Append(" - Id        : ")
			.Append(this.Id);

        return builder.ToString();
    }

	public HatViewData GetViewData()
	{
		if (this.hatView == null ||
			this.hatView.MainImage == null ||
			this.info.Animation != null)
		{
			this.hatView = this.loadViewData();
		}
		return this.hatView;
	}

	public HatData GetData()
    {
        if (this.Data != null) { return this.Data; }

        this.Data = ScriptableObject.CreateInstance<HatData>();

        this.Data.name = Helper.Translation.GetString(this.Name);
        this.Data.displayOrder = 99;
        this.Data.ProductId = this.Id;
        this.Data.InFront = !this.info.Back;
        this.Data.NoBounce = !this.info.Bound;
        this.Data.ChipOffset = new Vector2(0f, 0.2f);
        this.Data.Free = true;
        this.Data.NotInStore = true;
		this.Data.PreviewCrewmateColor = this.info.Shader;

		this.Data.SpritePreview = getSprite(Path.Combine(this.folderPath, DataStructure.FrontImageName));

		this.hatView = ScriptableObject.CreateInstance<HatViewData>();
		this.Data.ViewDataRef = new AssetReference(this.hatView.Pointer);

		return this.Data;
    }

	private HatViewData loadViewData()
	{
		var hatView = this.hatView == null ?
			ScriptableObject.CreateInstance<HatViewData>() :
			this.hatView;

		if (this.info.Animation == null)
		{
			createNormalView(ref hatView);
		}
		else
		{
			createVariationView(ref hatView, this.info.Animation);
		}

		if (this.info.Shader &&
			hatView.AltShader == null)
		{
			Material altShader = new Material(
				FastDestroyableSingleton<HatManager>.Instance.PlayerMaterial);
			altShader.shader = Shader.Find("Unlit/PlayerShader");

			hatView.AltShader = altShader;
		}

		this.Data!.ViewDataRef = new AssetReference(hatView.Pointer);

		return hatView;
	}

	private void createNormalView(ref HatViewData view)
	{
		view.MainImage = getSprite(Path.Combine(this.folderPath, DataStructure.FrontImageName));

		if (this.info.FrontFlip)
		{
			view.LeftMainImage = getSprite(
				Path.Combine(this.folderPath, DataStructure.FrontFlipImageName));
		}

		if (this.info.Back)
		{
			view.BackImage = getSprite(
				Path.Combine(this.folderPath, DataStructure.BackImageName));
		}
		if (this.info.BackFlip)
		{
			view.LeftBackImage = getSprite(
				Path.Combine(this.folderPath, DataStructure.BackFlipImageName));
		}

		if (this.info.Climb)
		{
			view.ClimbImage = getSprite(
				Path.Combine(this.folderPath, DataStructure.ClimbImageName));
		}
	}

	private void createVariationView(ref HatViewData view, HatAnimation animation)
	{
		if (animation.Front != null)
		{
			view.MainImage = getSprite(
				Path.Combine(
					this.folderPath,
					getVariationImgPath(variation.Front, NewHatInfo.VariationType.Random)));
		}

		if (animation.FrontFlip != null)
		{
			view.LeftMainImage = getSprite(
				Path.Combine(
					this.folderPath,
					getVariationImgPath(variation.FrontFlip, NewHatInfo.VariationType.Random)));
		}

		if (animation.Back != null)
		{
			view.BackImage = getSprite(
				Path.Combine(
					this.folderPath,
					getVariationImgPath(variation.Back, NewHatInfo.VariationType.Random)));
		}
		if (animation.BackFlip != null)
		{
			view.LeftBackImage = getSprite(
				Path.Combine(
					this.folderPath,
					getVariationImgPath(variation.BackFlip, NewHatInfo.VariationType.Random)));
		}

		if (animation.Climb != null)
		{
			view.ClimbImage = getSprite(
				Path.Combine(
					this.folderPath,
					getVariationImgPath(variation.Climb, NewHatInfo.VariationType.Random)));
		}
	}

	private static string getVariationImgPath(string[] imgArr, NewHatInfo.VariationType type)
	{
		return imgArr[RandomGenerator.Instance.Next(imgArr.Length)];
	}

	private static Sprite? getSprite(string path)
	{
		if (spriteCache.TryGetValue(path, out var sprite))
		{
			return sprite;
		}
		else
		{
			var result = loadHatSprite(path);
			if (!result.HasValue())
			{
				ExtremeSkinsPlugin.Logger.LogError(result.Error.ToString());
			}
			sprite = result.GetRawValue();
			spriteCache.Add(path, sprite);
		}
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
#endif
