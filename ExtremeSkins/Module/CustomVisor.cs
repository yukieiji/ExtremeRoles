using ExtremeSkins.Module.Interface;
using UnityEngine;

namespace ExtremeSkins.Module
{
    public class CustomVisor : ICustomCosmicData<VisorData>
    {

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
            get => this.id;
        }

        private VisorData visor;

        private string id;
        private string name;
        private string author;
        private string imgPath;

        public CustomVisor(
            string id,
            string imgPath,
            string author,
            string name)
        {
            this.id = id;
            this.imgPath = imgPath;
            this.author = author;
            this.name = name;
        }

        public VisorData GetData()
        {
            if (this.visor != null) { return this.visor; }

            this.visor = new VisorData();
            this.visor.name = Helper.Translation.GetString(this.Name);
            this.visor.displayOrder = 99;
            this.visor.ProductId = this.id;
            this.visor.ChipOffset = new Vector2(0f, 0.2f);
            this.visor.Free = true;
            this.visor.NotInStore = true;

            this.visor.viewData.viewData = new VisorViewData();
            this.visor.viewData.viewData.IdleFrame = loadVisorSprite(this.imgPath);

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
}
