
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

using BepInEx.Configuration;

using ExtremeSkins.Module;

namespace ExtremeSkins.SkinManager
{
    public class ExtremeColorManager
    {
        public static uint ColorNum;
        public static readonly Dictionary<StringNames, string> LangData = new Dictionary<StringNames, string>();

        public static void Initialize()
        {
            Helper.Translation.CreateColorTransData();
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
                "CreateNewSkin", "NewMainColorR", 0);
            ConfigEntry<int> gMainConfig = ExtremeSkinsPlugin.Instance.Config.Bind(
                "CreateNewSkin", "NewMainColorG", 0);
            ConfigEntry<int> bMainConfig = ExtremeSkinsPlugin.Instance.Config.Bind(
                "CreateNewSkin", "NewMainColorB", 0);
            ConfigEntry<int> aMainConfig = ExtremeSkinsPlugin.Instance.Config.Bind(
                "CreateNewSkin", "NewMainColorA", 0);

            ConfigEntry<int> rShadowConfig = ExtremeSkinsPlugin.Instance.Config.Bind(
                "CreateNewSkin", "NewShadowColorR", 0);
            ConfigEntry<int> gShadowConfig = ExtremeSkinsPlugin.Instance.Config.Bind(
                "CreateNewSkin", "NewShadowColorG", 0);
            ConfigEntry<int> bShadowConfig = ExtremeSkinsPlugin.Instance.Config.Bind(
                "CreateNewSkin", "NewShadowColorB", 0);
            ConfigEntry<int> aShadowConfig = ExtremeSkinsPlugin.Instance.Config.Bind(
                "CreateNewSkin", "NewShadowColorA", 0);

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
