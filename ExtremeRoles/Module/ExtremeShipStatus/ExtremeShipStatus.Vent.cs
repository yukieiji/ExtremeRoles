using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using ExtremeRoles.Performance;

namespace ExtremeRoles.Module.ExtremeShipStatus
{
    public sealed partial class ExtremeShipStatus
    {
        public enum CustomVentType
        {
            MeryVent,
        }

        private VentContainer customVent = new VentContainer();

        public void AddVent(Vent newVent, CustomVentType type)
        {
            var allVents = CachedShipStatus.Instance.AllVents.ToList();
            allVents.Add(newVent);
            CachedShipStatus.Instance.AllVents = allVents.ToArray();
            if (this.customVent.Body.TryGetValue(type, out List<Vent> vents))
            {
                vents.Add(newVent);
            }
            else
            {
                var ventList = new List<Vent>();
                ventList.Add(newVent);
                this.customVent.Body.Add(type, ventList);
            }
            if (!this.customVent.Anime.ContainsKey(type))
            {
                this.customVent.Anime.Add(type, new Sprite[18]);
            }

            this.customVent.Type.Add(newVent.Id, type);
        }
        public List<Vent> GetCustomVent(CustomVentType type)
        {
            if (this.customVent.Body.TryGetValue(
                type, out List<Vent> vents))
            {
                return vents;
            }
            return new List<Vent>();
        }

        public Sprite GetVentSprite(int ventId, int index)
        {

            if (!this.customVent.Type.ContainsKey(ventId)) { return null; }

            CustomVentType type = this.customVent.Type[ventId];
            Sprite img = this.customVent.Anime[type][index];

            if (img != null)
            {
                return img;
            }
            else
            {
                switch (type)
                {
                    case CustomVentType.MeryVent:
                        img = Resources.Loader.CreateSpriteFromResources(
                            string.Format(Resources.Path.MeryCustomVentAnime, index), 125f);
                        break;
                    default:
                        return null;
                }

                this.customVent.Anime[type][index] = img;
                return img;
            }
        }

        public bool IsCustomVent(int ventId) => this.customVent.Type.ContainsKey(ventId);

        private void resetVent()
        {
            this.customVent.Clear();
        }

        public sealed class VentContainer
        {
            public Dictionary<int, CustomVentType> Type = 
                new Dictionary<int, CustomVentType>();
            public Dictionary<CustomVentType, List<Vent>> Body = 
                new Dictionary<CustomVentType, List<Vent>>();
            public Dictionary<CustomVentType, Sprite[]> Anime = 
                new Dictionary<CustomVentType, Sprite[]>();

            public VentContainer()
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
