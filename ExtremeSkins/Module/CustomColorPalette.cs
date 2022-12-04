using System.Collections.Generic;
using UnityEngine;

namespace ExtremeSkins.Module
{
    public static class CustomColorPalette
    {
        public struct ColorData
        {
            public string Name;
            public Color32 MainColor;
            public Color32 ShadowColor;
        }

        public static readonly List<ColorData> CustomColor = new List<ColorData>()
        {
            // from nekowa
            new ColorData()
            { 
                Name = "kusaZunGreen",
                MainColor = new Color32(17, 82, 98, byte.MaxValue),
                ShadowColor = new Color32(8, 32, 55, 0),
            },
            new ColorData()
            {
                Name = "darkRed",
                MainColor = new Color32(139, 0, 0, byte.MaxValue),
                ShadowColor = new Color32(83, 0, 0, byte.MaxValue),
            },
            new ColorData()
            {
                Name = "mediumVioletRed",
                MainColor = new Color32(199, 21, 133, byte.MaxValue),
                ShadowColor = new Color32(119, 12, 80, byte.MaxValue),
            },
            new ColorData()
            {
                Name = "mediumPurple",
                MainColor = new Color32(147, 112, 219, byte.MaxValue),
                ShadowColor = new Color32(92, 67, 131, byte.MaxValue),
            },
            new ColorData()
            {
                Name = "rosyBrown",
                MainColor = new Color32(188, 143, 143, byte.MaxValue),
                ShadowColor = new Color32(113, 86, 86, byte.MaxValue),
            },
            new ColorData()
            {
                Name = "darkMagenta",
                MainColor = new Color32(139, 0, 139, byte.MaxValue),
                ShadowColor = new Color32(83, 0, 83, byte.MaxValue),
            },
            new ColorData()
            {
                Name = "olive",
                MainColor = new Color32(128, 128, 0, byte.MaxValue),
                ShadowColor = new Color32(77, 77, 0, byte.MaxValue),
            },
            new ColorData()
            {
                Name = "steelBlue",
                MainColor = new Color32(70, 130, 180, byte.MaxValue),
                ShadowColor = new Color32(42, 78, 108, byte.MaxValue),
            },
            new ColorData()
            {
                Name = "dodgerBlue",
                MainColor = new Color32(30, 144, 255, byte.MaxValue),
                ShadowColor = new Color32(18, 86, 153, byte.MaxValue),
            },
            new ColorData()
            {
                Name = "darkSeaGreen",
                MainColor = new Color32(143, 188, 143, byte.MaxValue),
                ShadowColor = new Color32(86, 113, 86, byte.MaxValue),
            },
            new ColorData()
            {
                Name = "sikon",
                MainColor = new Color32(66, 44, 65, byte.MaxValue),
                ShadowColor = new Color32(40, 26, 39, byte.MaxValue),
            },
            new ColorData()
            {
                Name = "konjou",
                MainColor = new Color32(17, 43, 76, byte.MaxValue),
                ShadowColor = new Color32(10, 26, 52, byte.MaxValue),
            },
            new ColorData()
            {
                Name = "vermilion",
                MainColor = new Color32(253, 60, 47, byte.MaxValue),
                ShadowColor = new Color32(152, 36, 28, byte.MaxValue),
            },
            new ColorData()
            {
                Name = "ivyGreen",
                MainColor = new Color32(76, 103, 51, byte.MaxValue),
                ShadowColor = new Color32(46, 62, 31, byte.MaxValue),
            },
            new ColorData()
            {
                Name = "tilleul",
                MainColor = new Color32(186, 205, 49, byte.MaxValue),
                ShadowColor = new Color32(112, 123, 29, byte.MaxValue),
            },
            new ColorData()
            {
                Name = "ivory",
                MainColor = new Color32(255, 255, 240, byte.MaxValue),
                ShadowColor = new Color32(153, 153, 144, byte.MaxValue),
            },
            new ColorData()
            {
                Name = "kenpou",
                MainColor = new Color32(48, 47, 43, byte.MaxValue),
                ShadowColor = new Color32(29, 28, 26, byte.MaxValue),
            },
            new ColorData()
            {
                Name = "grisbleu",
                MainColor = new Color32(161, 169, 186, byte.MaxValue),
                ShadowColor = new Color32(97, 101, 112, byte.MaxValue),
            },
            new ColorData()
            {
                Name = "melon",
                MainColor = new Color32(245, 175, 78, byte.MaxValue),
                ShadowColor = new Color32(147, 105, 47, byte.MaxValue),
            },
            new ColorData()
            {
                Name = "clan",
                MainColor = new Color32(0, 109, 102, byte.MaxValue),
                ShadowColor = new Color32(0, 65, 61, byte.MaxValue),
            },
            new ColorData()
            {
                Name = "dominantRed",
                MainColor = new Color32(227, 126, 126, byte.MaxValue),
                ShadowColor = new Color32(174, 254, 255, byte.MaxValue),
            },
            new ColorData()
            {
                Name = "dominantGreen",
                MainColor = new Color32(174, 254, 173, byte.MaxValue),
                ShadowColor = new Color32(227, 125, 208, byte.MaxValue),
            },
            new ColorData()
            {
                Name = "dominantBlue",
                MainColor = new Color32(255, 210, 253, byte.MaxValue),
                ShadowColor = new Color32(234, 168, 128, byte.MaxValue),
            },
            new ColorData()
            {
                Name = "chaos",
                MainColor = new Color32(238, 239, 241, byte.MaxValue),
                ShadowColor = new Color32(47, 49, 49, byte.MaxValue),
            },
            new ColorData()
            {
                Name = "inverted",
                MainColor = new Color32(47, 49, 49, byte.MaxValue),
                ShadowColor = new Color32(238, 239, 241, byte.MaxValue),
            },
        };

        public static void AddCustomColor(ColorData data)
        {
            CustomColor.Add(data);
        }
    }
}
