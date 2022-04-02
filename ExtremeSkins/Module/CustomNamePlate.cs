using UnityEngine;

namespace ExtremeSkins.Module
{

#if WITHNAMEPLATE
    public class CustomNamePlate
    {
        public const int Order = 99;

        public string Author { get; set; }
        public string Name { get; set; }

        public NamePlateData Body { get => this.namePlate; }

        private NamePlateData namePlate;
        private Sprite image;

        private string imgPath;
        private string id;

        public CustomNamePlate(
            string id,
            string imgPath,
            string author,
            string name)
        {
            this.id = id;
            this.imgPath = imgPath;
            this.Author = author;
            this.Name = name;
        }

        public NamePlateData GetNamePlateData()
        {
            if (this.namePlate != null) { return this.namePlate; }

            this.image = loadNamePlateSprite(this.imgPath);

            this.namePlate = new NamePlateData();
            this.namePlate.Image = this.image;
            this.namePlate.name = Helper.Translation.GetString(this.Name);
            this.namePlate.Order = Order;
            this.namePlate.ProductId = this.id;
            this.namePlate.ChipOffset = new Vector2(0f, 0.2f);
            this.namePlate.Free = true;
            this.namePlate.NotInStore = true;

            return this.namePlate;
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
