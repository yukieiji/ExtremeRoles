
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

using BepInEx.Configuration;

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

            ConfigEntry<int> rMainConfig = ExtremeSkinsPlugin.Instance.Config.Bind(
                "NewSkinCreate", "NewMainColorR", 0);
            ConfigEntry<int> gMainConfig = ExtremeSkinsPlugin.Instance.Config.Bind(
                "NewSkinCreate", "NewMainColorG", 0);
            ConfigEntry<int> bMainConfig = ExtremeSkinsPlugin.Instance.Config.Bind(
                "NewSkinCreate", "NewMainColorB", 0);
            ConfigEntry<int> aMainConfig = ExtremeSkinsPlugin.Instance.Config.Bind(
                "NewSkinCreate", "NewMainColorA", 0);

            ConfigEntry<int> rShadowConfig = ExtremeSkinsPlugin.Instance.Config.Bind(
                "NewSkinCreate", "NewShadowColorR", 0);
            ConfigEntry<int> gShadowConfig = ExtremeSkinsPlugin.Instance.Config.Bind(
                "NewSkinCreate", "NewShadowColorG", 0);
            ConfigEntry<int> bShadowConfig = ExtremeSkinsPlugin.Instance.Config.Bind(
                "NewSkinCreate", "NewShadowColorB", 0);
            ConfigEntry<int> aShadowConfig = ExtremeSkinsPlugin.Instance.Config.Bind(
                "NewSkinCreate", "NewShadowColorA", 0);

            if (ExtremeSkinsPlugin.CreatorMode.Value)
            {
                longlist.Add((StringNames)id);
                colorlist.Add(
                    new Color32(
                        (byte)rMainConfig.Value,
                        (byte)gMainConfig.Value,
                        (byte)bMainConfig.Value,
                        (byte)aMainConfig.Value));
                shadowlist.Add(
                    new Color32(
                        (byte)rShadowConfig.Value,
                        (byte)gShadowConfig.Value,
                        (byte)bShadowConfig.Value,
                        (byte)aShadowConfig.Value));
                LangData.Add((StringNames)id, "configAddColor");
                ColorNum += 1;
            }

            Palette.ColorNames = longlist.ToArray();
            Palette.PlayerColors = colorlist.ToArray();
            Palette.ShadowColors = shadowlist.ToArray();

        }
    }
}
