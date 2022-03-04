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
            new ColorData()
            { 
                Name = "testColor",
                MainColor = new Color32(0xF7, 0x44, 0x17, byte.MaxValue),
                ShadowColor = new Color32(0x9B, 0x2E, 0x0F, byte.MaxValue),
            }
        };
    }
}
