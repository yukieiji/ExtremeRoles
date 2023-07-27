using System.IO;
using System.Text;

using UnityEngine;
using UnityEngine.AddressableAssets;

using ExtremeSkins.Core.ExtremeHats;
using ExtremeSkins.Module.Interface;

using ExtremeRoles.Performance;
using ExtremeRoles.Module;

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
        get => $"hat_{new DirectoryInfo(this.folderPath).Name}_{this.info.Author}_{this.info.Name}";
    }

    private string folderPath;
	private HatInfo info;
	private HatViewData? hatView;

    public CustomHat(string folderPath, HatInfo info)
    {
        this.folderPath = folderPath;
		this.info = info;
    }

    public override string ToString()
    {
        StringBuilder builder = new StringBuilder();
        builder
            .AppendLine($" - Name      : {this.info.Name}")
            .AppendLine($" - Author    : {this.info.Author}")
            .AppendLine($" - Load from : {this.folderPath}")
            .Append    ($" - Id        : {this.Id}");

        return builder.ToString();
    }

	public HatViewData GetViewData()
	{
		if (this.hatView == null)
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

		this.hatView = loadViewData();
		this.Data.ViewDataRef = new AssetReference(this.hatView.Pointer);

		return this.Data;
    }

	public void Release()
	{
		this.hatView = null;
	}

	private HatViewData loadViewData()
	{
		var hatView = ScriptableObject.CreateInstance<HatViewData>();

		hatView.MainImage = getSprite(Path.Combine(this.folderPath, DataStructure.FrontImageName));

		if (this.info.FrontFlip)
		{
			hatView.LeftMainImage = getSprite(
				Path.Combine(this.folderPath, DataStructure.FrontFlipImageName));
		}

		if (this.info.Back)
		{
			hatView.BackImage = getSprite(
				Path.Combine(this.folderPath, DataStructure.BackImageName));
		}
		if (this.info.BackFlip)
		{
			hatView.LeftBackImage = getSprite(
				Path.Combine(this.folderPath, DataStructure.BackFlipImageName));
		}

		if (this.info.Climb)
		{
			hatView.ClimbImage = getSprite(
				Path.Combine(this.folderPath, DataStructure.ClimbImageName));
		}

		if (this.info.Shader)
		{
			Material altShader = new Material(
				FastDestroyableSingleton<HatManager>.Instance.PlayerMaterial);
			altShader.shader = Shader.Find("Unlit/PlayerShader");

			hatView.AltShader = altShader;
		}

		this.Data!.ViewDataRef = new AssetReference(hatView.Pointer);

		return hatView;
	}

	private static Sprite? getSprite(string path)
	{
		var result = loadHatSprite(path);
		if (!result.HasValue())
		{
			ExtremeSkinsPlugin.Logger.LogError(result.Error.ToString());
		}
		return result.GetRawValue();
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
        if (sprite == null)
        {
            return new Loader.LoadError(Loader.ErrorCode.CannotCreateSprite, path);
        }
        texture.hideFlags |= HideFlags.HideAndDontSave | HideFlags.DontUnloadUnusedAsset;
        sprite.hideFlags |= HideFlags.HideAndDontSave | HideFlags.DontUnloadUnusedAsset;

        return sprite;
    }
}
#endif
