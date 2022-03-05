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
                Name = "nekowasaColor",
                MainColor = new Color32(17, 82, 98, byte.MaxValue),
                ShadowColor = new Color32(8, 32, 55, 0),
            }
        };
    }
}
