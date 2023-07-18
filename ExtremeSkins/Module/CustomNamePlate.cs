using System.Text;

using UnityEngine;
using UnityEngine.AddressableAssets;

using ExtremeSkins.Module.Interface;

namespace ExtremeSkins.Module;

#nullable enable

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
            .AppendLine($" - Name       : {this.name}")
            .AppendLine($" - Author     : {this.author}")
            .AppendLine($" - Image Path : {this.imgPath}")
            .Append    ($" - Id         : {this.Id}");

        return builder.ToString();
    }

	public NamePlateViewData GetViewData()
	{
		if (this.viewData == null)
		{
			this.viewData = ScriptableObject.CreateInstance<NamePlateViewData>();
			this.viewData.Image = loadNamePlateSprite(this.imgPath);
		}
		return this.viewData;
	}

	public void Release()
	{
		this.viewData = null;
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
		var img = loadNamePlateSprite(this.imgPath);
		this.viewData.Image = img;
		this.Data.SpritePreview = img;
		this.Data.ViewDataRef = new AssetReference(this.viewData.Pointer);

        return this.Data;
    }

    private Sprite? loadNamePlateSprite(
        string path)
    {
        Texture2D texture = Loader.LoadTextureFromDisk(path);
        if (texture == null)
        {
            return null;
        }
        Sprite sprite = Sprite.Create(
            texture,
            new Rect(0, 0, texture.width, texture.height),
            new Vector2(0.5f, 0.5f), 100f);
        if (sprite == null)
        {
            return null;
        }
        texture.hideFlags |= HideFlags.HideAndDontSave | HideFlags.DontUnloadUnusedAsset;
        sprite.hideFlags |= HideFlags.HideAndDontSave | HideFlags.DontUnloadUnusedAsset;
        return sprite;
    }
}

#endif
