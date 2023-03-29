using System.IO;

using UnityEngine;

using ExtremeSkins.Module.Interface;
using ExtremeRoles.Performance;

namespace ExtremeSkins.Module;

#if WITHVISOR
public sealed class CustomVisor : ICustomCosmicData<VisorData>
{
    public const string IdleName = "idle.png";
    public const string FlipIdleName = "flip_idle.png";

    public VisorData Data
    {
        get => this.visor;
    }

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
        get => $"visor_{Path.GetDirectoryName(this.folderPath)}_{this.author}_{this.name}";
    }

    private VisorData visor;

    private string name;
    private string author;
    private string folderPath;

    private bool isBehindHat;
    private bool hasShader;
    private bool hasLeftImg;

    public CustomVisor(
        string folderPath,
        string author,
        string name,
        bool hasLeftImg,
        bool hasShader,
        bool isBehindHat)
    {
        this.folderPath = folderPath;
        this.author = author;
        this.name = name;

        this.isBehindHat = isBehindHat;
        this.hasLeftImg = hasLeftImg;
        this.hasShader = hasShader;
    }

    public VisorData GetData()
    {
        if (this.visor != null) { return this.visor; }

        this.visor = ScriptableObject.CreateInstance<VisorData>();
        this.visor.name = Helper.Translation.GetString(this.Name);
        this.visor.displayOrder = 99;
        this.visor.ProductId = this.Id;
        this.visor.ChipOffset = new Vector2(0f, 0.2f);
        this.visor.Free = true;
        this.visor.NotInStore = true;

        // 256×144の画像
        this.visor.viewData.viewData = ScriptableObject.CreateInstance<VisorViewData>();
        this.visor.viewData.viewData.IdleFrame = loadVisorSprite(
            string.Concat(this.folderPath, @"\", IdleName));

        if (this.hasLeftImg)
        {
            this.visor.viewData.viewData.LeftIdleFrame = loadVisorSprite(
                string.Concat(this.folderPath, @"\", FlipIdleName));
        }
        if (this.hasShader)
        {
            Material altShader = new Material(
                FastDestroyableSingleton<HatManager>.Instance.PlayerMaterial);
            altShader.shader = Shader.Find("Unlit/PlayerShader");

            this.visor.viewData.viewData.AltShader = altShader;
        }

        this.visor.behindHats = this.isBehindHat;

        return this.visor;

    }

    private Sprite loadVisorSprite(
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
