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
        public const string ExtendImageName = "extend.png";


        public string Author { get; set; }
        public string Name { get; set; }

        private string folderPath;

        private bool hasFrontFlip { get; set; }
        private bool hasBack { get; set; }
        private bool hasBackFlip { get; set; }
        private bool hasClimb { get; set; }
        private bool hasExtend { get; set; }

        private Sprite frontImage;
        private Sprite frontFlipImage;
        private Sprite backImage;
        private Sprite backFlipImage;
        private Sprite climbImage;
        private Sprite extendImage;
        private HatBehaviour behaviour;


        public CustomHat(
            string folderPath,
            string author,
            string name,
            bool hasFrontFlip,
            bool hasBack,
            bool hasBackFlip,
            bool hasClimb,
            bool hasExtend)
        {
            this.folderPath = folderPath;
            this.Author = author;
            this.Name = name;
            this.hasFrontFlip = hasFrontFlip;
            this.hasBack = hasBack;
            this.hasBackFlip = hasBackFlip;
            this.hasClimb = hasClimb;
            this.hasExtend = hasExtend;
        }

        public HatBehaviour GetHatBehaviour()
        {

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
