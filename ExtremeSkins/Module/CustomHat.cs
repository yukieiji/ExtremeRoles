using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using UnityEngine;

namespace ExtremeSkins.Module
{
    public class CustomHat
    {
        public const string FrontImageName = "front.png";
        public const string FrontFlipImageName = "front_flip.png";
        public const string BackImageName = "back.png";
        public const string BackFlipImageName = "back_flip.png";
        public const string ClimbImageName = "climb.png";

        public int Order = 99;


        public string Author { get; set; }
        public string Name { get; set; }

        private string folderPath;

        private bool hasFrontFlip { get; set; }
        private bool hasBack { get; set; }
        private bool hasBackFlip { get; set; }
        private bool hasClimb { get; set; }

        private bool hasShader { get; set; }

        private bool isBounce { get; set; }

        private Sprite frontImage;
        private Sprite frontFlipImage;
        private Sprite backImage;
        private Sprite backFlipImage;
        private Sprite climbImage;
        private HatBehaviour behaviour;


        public CustomHat(
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
            this.folderPath = folderPath;
            this.Author = author;
            this.Name = name;
            
            this.hasFrontFlip = hasFrontFlip;
            this.hasBack = hasBack;
            this.hasBackFlip = hasBackFlip;
            this.hasClimb = hasClimb;
            this.hasShader = hasShader;

            this.isBounce = isBounce;
        }

        public HatBehaviour GetHatBehaviour()
        {
            if (this.behaviour != null) { return this.behaviour; }

            loadAllHatResources();

            this.behaviour = new HatBehaviour();

            this.behaviour.name = this.Name;
            this.behaviour.Order = Order;
            this.behaviour.ProductId = string.Concat(
                "hat_", this.Name.Replace(' ', '_'));
            this.behaviour.InFront = this.hasBack;
            this.behaviour.NoBounce = !this.isBounce;
            this.behaviour.ChipOffset = new Vector2(0f, 0.2f);
            this.behaviour.Free = true;
            this.behaviour.NotInStore = true;

            this.behaviour.MainImage = this.frontImage;

            if (this.hasBack)
            {
                this.behaviour.BackImage = this.backImage;
            }
            if (this.hasClimb)
            {
                this.behaviour.ClimbImage = this.climbImage;
            }
            if (this.hasShader)
            {
                foreach (HatBehaviour h in DestroyableSingleton<HatManager>.Instance.AllHats)
                {
                    if (h.AltShader != null)
                    {
                        this.behaviour.AltShader = h.AltShader;
                        break;
                    }
                }
            }

            return this.behaviour;
        }

        public Sprite GetFrontImage() => this.frontImage;
        public Sprite GetFlipFrontImage() => this.frontFlipImage;
        public Sprite GetBackImage() => this.backImage;
        public Sprite GetBackFlipImage() => this.backFlipImage;

        private void loadAllHatResources()
        {

            this.frontImage = loadHatSprite(
                string.Concat(this.folderPath, FrontImageName));

            if (this.hasFrontFlip)
            {
                this.frontFlipImage = loadHatSprite(
                    string.Concat(this.folderPath, FrontFlipImageName));
            }
            if (this.hasBack)
            {
                this.backImage = loadHatSprite(
                    string.Concat(this.folderPath, BackImageName));
            }
            if (this.hasBackFlip)
            {
                this.backFlipImage = loadHatSprite(
                    string.Concat(this.folderPath, BackFlipImageName));
            }
            if (this.hasClimb)
            {
                this.climbImage = loadHatSprite(
                    string.Concat(this.folderPath, ClimbImageName));
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
}
