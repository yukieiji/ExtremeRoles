using UnityEngine;

namespace ExtremeSkins.Module
{
    public class CustomNamePlate
    {
        public const int Order = 99;

        public string Author { get; set; }
        public string Name { get; set; }

        public NamePlateData Body { get => this.namePlate; }

        private NamePlateData namePlate;
        private Sprite image;

        private string folderPath;
        private string id;

        public CustomNamePlate(
            string id,
            string folderPath,
            string author,
            string name)
        {
            this.id = id;
            this.folderPath = folderPath;
            this.Author = author;
            this.Name = name;
        }

        public NamePlateData GetNamePlateData()
        {
            if (this.namePlate != null) { return this.namePlate; }

            this.namePlate = new NamePlateData();
            this.namePlate.Image = this.image;
            this.namePlate.name = this.Name;
            this.namePlate.Order = Order;
            this.namePlate.ProductId = this.id;
            this.namePlate.ChipOffset = new Vector2(0f, 0.2f);
            this.namePlate.Free = true;
            this.namePlate.NotInStore = true;

            return this.namePlate;
        }

    }
}
