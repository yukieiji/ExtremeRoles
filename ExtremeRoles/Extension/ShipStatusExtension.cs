using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using ExtremeRoles.Resources;

namespace ExtremeRoles.Extension.Ship
{
    public static class VentExtension
    {
        public enum CustomVentType
        {
            MeryVent,
        }

        private static CustomVent customVent = new CustomVent();

        public static void AddVent(
            this ShipStatus instance, Vent newVent, CustomVentType type)
        {
            var allVents = instance.AllVents.ToList();
            allVents.Add(newVent);
            instance.AllVents = allVents.ToArray();
            if (customVent.Body.TryGetValue(type, out List<Vent> vents))
            {
                vents.Add(newVent);
            }
            else
            {
                var ventList = new List<Vent>();
                ventList.Add(newVent);
                customVent.Body.Add(type, ventList);
            }
            if (!customVent.Anime.ContainsKey(type))
            {
                customVent.Anime.Add(type, new Sprite[18]);
            }

            customVent.Type.Add(newVent.Id, type);
        }

        public static List<Vent> GetCustomVent(this ShipStatus _, CustomVentType type)
        {
            if (customVent.Body.TryGetValue(
                type, out List<Vent> vents))
            {
                return vents;
            }
            return new List<Vent>();
        }

        public static Sprite GetCustomVentSprite(this ShipStatus _, int ventId, int index)
        {
            if (!customVent.Type.ContainsKey(ventId)) { return null; }

            CustomVentType type = customVent.Type[ventId];
            Sprite img = customVent.Anime[type][index];

            if (img != null)
            {
                return img;
            }
            else
            {
                switch (type)
                {
                    case CustomVentType.MeryVent:
                        img = Loader.CreateSpriteFromResources(
                            string.Format(Path.MeryCustomVentAnime, index), 125f);
                        break;
                    default:
                        return null;
                }

                customVent.Anime[type][index] = img;
                return img;
            }
        }

        public static bool IsCustomVent(
            this ShipStatus _, int ventId) => customVent.Type.ContainsKey(ventId);

        public static void ResetCustomVent()
        {
            customVent.Clear();
        }

        public sealed class CustomVent
        {
            public Dictionary<int, CustomVentType> Type =
                new Dictionary<int, CustomVentType>();
            public Dictionary<CustomVentType, List<Vent>> Body =
                new Dictionary<CustomVentType, List<Vent>>();
            public Dictionary<CustomVentType, Sprite[]> Anime =
                new Dictionary<CustomVentType, Sprite[]>();

            public CustomVent()
            {
                Clear();
            }

            public void Clear()
            {
                this.Body.Clear();
                this.Type.Clear();
                this.Anime.Clear();
            }
        }
    }
}
