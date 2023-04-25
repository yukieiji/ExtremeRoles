using System.IO;
using System.Text;

using UnityEngine;

using ExtremeSkins.Core.ExtremeHats;
using ExtremeSkins.Module.Interface;

using ExtremeRoles.Performance;

namespace ExtremeSkins.Module;


#if WITHHAT
public sealed class CustomHat : ICustomCosmicData<HatData>
{
    public HatData Data
    { 
        get => this.hat; 
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
        get => $"hat_{new DirectoryInfo(this.folderPath).Name}_{this.author}_{this.name}"; 
    }

    private bool hasFrontFlip;
    private bool hasBackFlip;

    private bool hasShader;
    private bool hasBack;
    private bool hasClimb;
    private bool isBounce;

    private string folderPath;

    private string name;
    private string author;

    private HatData hat;

    public CustomHat(string folderPath, HatInfo info)
    {
        this.folderPath = folderPath;
        this.author = info.Author;
        this.name = info.Name;
        
        this.hasFrontFlip = info.FrontFlip;
        this.hasBack = info.Back;
        this.hasBackFlip = info.BackFlip;
        this.hasClimb = info.Climb;
        this.hasShader = info.Shader;

        this.isBounce = info.Bound;
    }

    public override string ToString()
    {
        StringBuilder builder = new StringBuilder();
        builder
            .AppendLine($" - Name      : {this.name}")
            .AppendLine($" - Author    : {this.author}")
            .AppendLine($" - Load from : {this.folderPath}")
            .Append    ($" - Id        : {this.Id}");

        return builder.ToString();
    }

    public HatData GetData()
    {
        if (this.hat != null) { return this.hat; }

        this.hat = ScriptableObject.CreateInstance<HatData>();

        this.hat.name = Helper.Translation.GetString(this.Name);
        this.hat.displayOrder = 99;
        this.hat.ProductId = this.Id;
        this.hat.InFront = !this.hasBack;
        this.hat.NoBounce = !this.isBounce;
        this.hat.ChipOffset = new Vector2(0f, 0.2f);
        this.hat.Free = true;
        this.hat.NotInStore = true;

        this.hat.hatViewData.viewData = ScriptableObject.CreateInstance<HatViewData>();

        this.hat.hatViewData.viewData.MainImage = loadHatSprite(
            Path.Combine(this.folderPath, DataStructure.FrontImageName));
        
        if (this.hasFrontFlip)
        {
            this.hat.hatViewData.viewData.LeftMainImage = loadHatSprite(
                Path.Combine(this.folderPath, DataStructure.FrontFlipImageName));
        }

        if (this.hasBack)
        {
            this.hat.hatViewData.viewData.BackImage = loadHatSprite(
                Path.Combine(this.folderPath, DataStructure.BackImageName));
        }
        if (this.hasBackFlip)
        {
            this.hat.hatViewData.viewData.LeftBackImage = loadHatSprite(
                Path.Combine(this.folderPath, DataStructure.BackFlipImageName));
        }

        if (this.hasClimb)
        {
            this.hat.hatViewData.viewData.ClimbImage = loadHatSprite(
                Path.Combine(this.folderPath, DataStructure.ClimbImageName));
        }

        if (this.hasShader)
        {
            Material altShader = new Material(
                FastDestroyableSingleton<HatManager>.Instance.PlayerMaterial);
            altShader.shader = Shader.Find("Unlit/PlayerShader");

            this.hat.hatViewData.viewData.AltShader = altShader;
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
