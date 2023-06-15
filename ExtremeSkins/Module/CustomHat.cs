using System.IO;
using System.Text;

using UnityEngine;

using ExtremeSkins.Core.ExtremeHats;
using ExtremeSkins.Module.Interface;

using ExtremeRoles.Performance;

namespace ExtremeSkins.Module;


#if WITHHAT
public sealed class CustomHat : ICustomCosmicData<HatData, HatViewData>
{
    public HatData Data
    {
        get => this.hat;
    }

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

    private HatData hat;
	private HatViewData hatView;

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
		return this.hatView;
	}

	public HatData GetData()
    {
        if (this.hat != null) { return this.hat; }

        this.hat = ScriptableObject.CreateInstance<HatData>();

        this.hat.name = Helper.Translation.GetString(this.Name);
        this.hat.displayOrder = 99;
        this.hat.ProductId = this.Id;
        this.hat.InFront = !this.info.Back;
        this.hat.NoBounce = !this.info.Bound;
        this.hat.ChipOffset = new Vector2(0f, 0.2f);
        this.hat.Free = true;
        this.hat.NotInStore = true;

        this.hat.SpritePreview = loadHatSprite(
            Path.Combine(this.folderPath, DataStructure.FrontImageName));

		this.hatView = ScriptableObject.CreateInstance<HatViewData>();

		this.hatView.MainImage = loadHatSprite(
			Path.Combine(this.folderPath, DataStructure.FrontImageName));

		if (this.info.FrontFlip)
		{
			this.hatView.LeftMainImage = loadHatSprite(
				Path.Combine(this.folderPath, DataStructure.FrontFlipImageName));
		}

		if (this.info.Back)
		{
			this.hatView.BackImage = loadHatSprite(
				Path.Combine(this.folderPath, DataStructure.BackImageName));
		}
		if (this.info.BackFlip)
		{
			this.hatView.LeftBackImage = loadHatSprite(
				Path.Combine(this.folderPath, DataStructure.BackFlipImageName));
		}

		if (this.info.Climb)
		{
			this.hatView.ClimbImage = loadHatSprite(
				Path.Combine(this.folderPath, DataStructure.ClimbImageName));
		}

		if (this.info.Shader)
		{
			Material altShader = new Material(
				FastDestroyableSingleton<HatManager>.Instance.PlayerMaterial);
			altShader.shader = Shader.Find("Unlit/PlayerShader");

			this.hatView.AltShader = altShader;
		}

		return this.hat;

    }

    private Sprite loadHatSprite(
        string path)
    {
        Texture2D texture = Loader.LoadTextureFromDisk(path);
        if (texture == null)
        {
            return null;
        }
        Sprite sprite = Sprite.Create(
            texture, new Rect(0, 0, texture.width, texture.height),
            new Vector2(0.53f, 0.575f), texture.width * 0.375f);
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
