using ExtremeSkins.Module.Interface;
using UnityEngine;

namespace ExtremeSkins.Module
{

#if WITHNAMEPLATE
    public class CustomNamePlate : ICustomCosmicData<NamePlateData, NamePlateViewData>
    {

        public NamePlateData Data
        {
            get => this.namePlate;
        }
        public NamePlateViewData ViewData
        {
            get => this.namePlateView;
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

        public bool AllDataLoaded
        {
            get => true;
        }

        private NamePlateData namePlate;
        private NamePlateViewData namePlateView;

        private string id;
        private string name;
        private string author;
        private string imgPath;

        public CustomNamePlate(
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

        public NamePlateData GetData()
        {
            if (this.namePlate != null) { return this.namePlate; }

            this.namePlate = new NamePlateData();
            this.namePlate.name = Helper.Translation.GetString(this.Name);
            this.namePlate.displayOrder = 99;
            this.namePlate.ProductId = this.id;
            this.namePlate.ChipOffset = new Vector2(0f, 0.2f);
            this.namePlate.Free = true;
            this.namePlate.NotInStore = true;
            this.namePlate.viewData.viewData = this.ViewData;

            this.namePlateView = new NamePlateViewData();
            this.namePlateView.Image = loadNamePlateSprite(this.imgPath);

            return this.namePlate;

        }

        public void LoadAditionalData()
        {
            return;
        }

        private Sprite loadNamePlateSprite(
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

}
