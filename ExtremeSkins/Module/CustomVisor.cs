using System.IO;
using System.Text;

using UnityEngine;
using UnityEngine.AddressableAssets;

using ExtremeSkins.Core.ExtremeVisor;
using ExtremeSkins.Module.Interface;
using ExtremeRoles.Performance;
using ExtremeRoles.Module;

namespace ExtremeSkins.Module;

#if WITHVISOR
public sealed class CustomVisor : ICustomCosmicData<VisorData, VisorViewData>
{
    public VisorData? Data { get; private set; }

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
        get => $"visor_{new DirectoryInfo(this.folderPath).Name}_{this.Author}_{this.Name}";
    }

	private string folderPath;
	private VisorInfo info;
	private VisorViewData? view;

	public CustomVisor(
        string folderPath,
        VisorInfo info)
    {
        this.folderPath = folderPath;
        this.info = info;
    }

    public override string ToString()
    {
        StringBuilder builder = new StringBuilder();
        builder
            .AppendLine($" - Name      : {this.Name}")
            .AppendLine($" - Author    : {this.Author}")
            .AppendLine($" - Load from : {this.folderPath}")
            .Append    ($" - Id        : {this.Id}");

        return builder.ToString();
    }

	public VisorViewData GetViewData()
	{
		if (this.view == null ||
			this.view.IdleFrame == null)
		{
			this.view = this.loadViewData();
		}
		return this.view;
	}

	public void Release()
	{
		if (this.view != null)
		{
			Object.Destroy(this.view);
		}
	}

	public VisorData GetData()
    {
        if (this.Data != null) { return this.Data; }

		this.Data = ScriptableObject.CreateInstance<VisorData>();
		this.Data.name = Helper.Translation.GetString(this.Name);
		this.Data.displayOrder = 99;
		this.Data.ProductId = this.Id;
		this.Data.ChipOffset = new Vector2(0f, 0.2f);
		this.Data.Free = true;
		this.Data.NotInStore = true;

		this.Data.SpritePreview = getSprite(Path.Combine(this.folderPath, DataStructure.IdleImageName));
		this.Data.behindHats = this.info.BehindHat;
		this.Data.PreviewCrewmateColor = this.info.Shader;

		this.view = ScriptableObject.CreateInstance<VisorViewData>();
		this.Data.ViewDataRef = new AssetReference(this.view.Pointer);

        return this.Data;

    }

	private VisorViewData loadViewData()
	{
		var view = ScriptableObject.CreateInstance<VisorViewData>();

		view.IdleFrame = getSprite(
			Path.Combine(this.folderPath, DataStructure.IdleImageName));

		if (this.info.LeftIdle)
		{
			view.LeftIdleFrame = getSprite(
				Path.Combine(this.folderPath, DataStructure.FlipIdleImageName));
		}
		if (this.info.Shader)
		{
			Material altShader = new Material(
				FastDestroyableSingleton<HatManager>.Instance.PlayerMaterial);
			altShader.shader = Shader.Find("Unlit/PlayerShader");

			view.AltShader = altShader;
		}

		return view;
	}


	private static Sprite? getSprite(string path)
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
#endif
