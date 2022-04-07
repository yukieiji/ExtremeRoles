using ExtremeSkins.Module.Interface;
using UnityEngine;

namespace ExtremeSkins.Module
{

#if WITHHAT
    public class CustomHat : ICustomCosmicData<HatData>
    {
        public const string FrontImageName = "front.png";
        public const string FrontFlipImageName = "front_flip.png";
        public const string BackImageName = "back.png";
        public const string BackFlipImageName = "back_flip.png";
        public const string ClimbImageName = "climb.png";

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
            get => this.id; 
        }

        public bool HasFrontFlip { get; set; }
        public bool HasBackFlip { get; set; }
        private bool hasShader { get; set; }
        
        private bool hasBack { get; set; }
        private bool hasClimb { get; set; }

        private bool isBounce { get; set; }

        private string folderPath;

        private Sprite frontImage;
        private Sprite frontFlipImage;
        private Sprite backImage;
        private Sprite backFlipImage;
        private Sprite climbImage;

        private string id;
        private string name;
        private string author;

        private HatData hat;

        public CustomHat(
            string id,
            string folderPath,
            string author,
            string name,
            bool hasFrontFlip,
            bool hasBack,
            bool hasBackFlip,
            bool hasClimb,
            bool isBounce,
            bool hasShader)
        {
            this.id = id;
            this.folderPath = folderPath;
            this.author = author;
            this.name = name;
            
            this.HasFrontFlip = hasFrontFlip;
            this.hasBack = hasBack;
            this.HasBackFlip = hasBackFlip;
            this.hasClimb = hasClimb;
            this.hasShader = hasShader;

            this.isBounce = isBounce;
        }

        public HatData GetData()
        {
            if (this.hat != null) { return this.hat; }

            this.hat = new HatData();

            this.hat.name = Helper.Translation.GetString(this.Name);
            this.hat.displayOrder = 99;
            this.hat.ProductId = this.id;
            this.hat.InFront = !this.hasBack;
            this.hat.NoBounce = !this.isBounce;
            this.hat.ChipOffset = new Vector2(0f, 0.2f);
            this.hat.Free = true;
            this.hat.NotInStore = true;

            this.hat.hatViewData.viewData = new HatViewData();

            loadAllHatResources();

            this.hat.hatViewData.viewData.MainImage = this.frontImage;

            if (this.hasBack)
            {
                this.hat.hatViewData.viewData.BackImage = this.backImage;
            }
            if (this.hasClimb)
            {
                this.hat.hatViewData.viewData.ClimbImage = this.climbImage;
            }
            if (this.hasShader)
            {
                this.hat.hatViewData.viewData.AltShader = new Material(
                    Shader.Find("Unlit/PlayerShader"));
            }

            return this.hat;

        }

        public Sprite GetFrontImage() => this.frontImage;
        public Sprite GetFlipFrontImage() => this.frontFlipImage;
        public Sprite GetBackImage() => this.backImage;
        public Sprite GetBackFlipImage() => this.backFlipImage;

        private void loadAllHatResources()
        {

            this.frontImage = loadHatSprite(
                string.Concat(this.folderPath, @"\", FrontImageName));

            if (this.HasFrontFlip)
            {
                this.frontFlipImage = loadHatSprite(
                    string.Concat(this.folderPath, @"\", FrontFlipImageName));
            }
            if (this.hasBack)
            {
                this.backImage = loadHatSprite(
                    string.Concat(this.folderPath, @"\", BackImageName));
            }
            if (this.HasBackFlip)
            {
                this.backFlipImage = loadHatSprite(
                    string.Concat(this.folderPath, @"\", BackFlipImageName));
            }
            if (this.hasClimb)
            {
                this.climbImage = loadHatSprite(
                    string.Concat(this.folderPath, @"\", ClimbImageName));
            }
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

}
