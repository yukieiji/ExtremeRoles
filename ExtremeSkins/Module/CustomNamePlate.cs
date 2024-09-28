using System.Text;

using UnityEngine;
using UnityEngine.AddressableAssets;

using ExtremeSkins.Module.Interface;
using ExtremeRoles.Module;
using System.IO;

namespace ExtremeSkins.Module;

#if WITHNAMEPLATE
public sealed class CustomNamePlate : ICustomCosmicData<NamePlateData, NamePlateViewData>
{

    public NamePlateData? Data { get; private set; }

    public string Author
    {
        get => this.author;
    }
    public string Name
    {
        get => this.name;
    }

    public string Id
    {
        get => $"namePlate_{this.author}_{this.name}";
    }

    private string name;
    private string author;
    private string imgPath;
	public Sprite? Preview { get; private set; }
	private NamePlateViewData? viewData;

    public CustomNamePlate(
        string imgPath,
        string author,
        string name)
    {
        this.imgPath = imgPath;
        this.author = author;
        this.name = name;
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
			.AppendLine(this.imgPath)
			.Append(" - Id        : ")
			.Append(this.Id);

        return builder.ToString();
    }

	public NamePlateViewData GetViewData()
	{
		if (this.viewData == null)
		{
			this.viewData = ScriptableObject.CreateInstance<NamePlateViewData>();
			this.viewData.Image = this.Preview;
		}
		return this.viewData;
	}

	public NamePlateData GetData()
    {
        if (this.Data != null) { return this.Data; }

        this.Data = ScriptableObject.CreateInstance<NamePlateData>();
        this.Data.name = Helper.Translation.GetString(this.Name);
        this.Data.displayOrder = 99;
        this.Data.ProductId = this.Id;
        this.Data.ChipOffset = new Vector2(0f, 0.2f);
        this.Data.Free = true;
        this.Data.NotInStore = true;
		this.Data.PreviewCrewmateColor = false;

		this.viewData = ScriptableObject.CreateInstance<NamePlateViewData>();
		this.Preview = getSprite(this.imgPath);
		this.viewData.Image = this.Preview;
		this.Data.ViewDataRef = new AssetReference(this.viewData.Pointer);

        return this.Data;
    }

	private static Sprite? getSprite(string path)
	{
		var result = loadNamePlateSprite(path);
		if (!result.HasValue())
		{
			ExtremeSkinsPlugin.Logger.LogError(result.Error.ToString());
		}
		return result.GetRawValue();
	}

	private static Expected<Sprite, Loader.LoadError> loadNamePlateSprite(
        string path)
    {

		if (LruCache<string, Sprite>.TryGetValue(path, out var sprite))
		{
			return sprite;
		}

		if (!LruCache<string, Texture2D>.TryGetValue(path, out var texture))
		{
			var result = Loader.LoadTextureFromDisk(path);
			if (!result.HasValue())
			{
				return result.Error;
			}
			texture = result.Value;
			texture.hideFlags |= HideFlags.DontUnloadUnusedAsset;

			LruCache<string, Texture2D>.Add(path, texture);
		}

		sprite = Sprite.Create(
            texture,
            new Rect(0, 0, texture.width, texture.height),
            new Vector2(0.5f, 0.5f), 100f);
		sprite.hideFlags |= HideFlags.DontUnloadUnusedAsset;

		LruCache<string, Sprite>.Add(path, sprite);

		return sprite;
    }
}

#endif
