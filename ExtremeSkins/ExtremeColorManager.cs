
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using ExtremeSkins.Module;

namespace ExtremeSkins
{
    public class ExtremeColorManager
    {
        public static uint ColorNum;
        public static Dictionary<StringNames, string> LangData = new Dictionary<StringNames, string>();

        public static void Initialize()
        {
            ColorNum = (uint)Palette.ColorNames.Length;
            LangData.Clear();
            load();
        }

        private static void load()
        {
            var customColor = CustomColorPalette.CustomColor;

            if (CustomColorPalette.CustomColor.Count == 0) { return; }

            List<StringNames> longlist = Enumerable.ToList(Palette.ColorNames);
            List<Color32> colorlist = Enumerable.ToList(Palette.PlayerColors);
            List<Color32> shadowlist = Enumerable.ToList(Palette.ShadowColors);

            ColorNum += (uint)customColor.Count;

            int id = 50000;
            foreach (var cc in customColor)
            {
                longlist.Add((StringNames)id);
                colorlist.Add(cc.MainColor);
                shadowlist.Add(cc.ShadowColor);
                LangData.Add((StringNames)id, cc.Name);
                ++id;
            }

            Palette.ColorNames = longlist.ToArray();
            Palette.PlayerColors = colorlist.ToArray();
            Palette.ShadowColors = shadowlist.ToArray();

        }
    }
}
