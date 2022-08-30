using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

using ExtremeRoles.Module.Interface;
using ExtremeRoles.Module.SpecialWinChecker;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Performance;
using ExtremeRoles.Performance.Il2Cpp;

namespace ExtremeRoles.Module.ExtremeShipStatus
{
    public sealed partial class ExtremeShipStatus
    {
        public enum CustomVentType
        {
            MeryVent,
        }

        public CustomVentContainer CustomVent = new CustomVentContainer();

        private void resetVent()
        {
            CustomVent.Clear();
        }

        public sealed class CustomVentContainer
        {
            private Dictionary<int, CustomVentType> ventType = new Dictionary<int, CustomVentType>();
            private Dictionary<CustomVentType, List<Vent>> addVent = new Dictionary<CustomVentType, List<Vent>>();
            private Dictionary<CustomVentType, Sprite[]> ventAnime = new Dictionary<CustomVentType, Sprite[]>();

            public CustomVentContainer()
            {
                Clear();
            }

            public void Clear()
            {
                addVent.Clear();
                ventType.Clear();
                ventAnime.Clear();
            }

            public void AddVent(
                Vent newVent,
                CustomVentType type)
            {
                var allVents = CachedShipStatus.Instance.AllVents.ToList();
                allVents.Add(newVent);
                CachedShipStatus.Instance.AllVents = allVents.ToArray();
                if (addVent.ContainsKey(type))
                {
                    addVent[type].Add(newVent);
                }
                else
                {
                    var ventList = new List<Vent>();
                    ventList.Add(newVent);
                    addVent.Add(type, ventList);
                }
                if (!ventAnime.ContainsKey(type))
                {
                    ventAnime.Add(type, new Sprite[18]);
                }

                ventType.Add(newVent.Id, type);
            }

            public List<Vent> GetCustomVent(CustomVentType type)
            {
                if (addVent.ContainsKey(type))
                {
                    return addVent[type];
                }
                return new List<Vent>();
            }

            public Sprite GetVentSprite(int ventId, int index)
            {

                if (!ventType.ContainsKey(ventId)) { return null; }

                CustomVentType type = ventType[ventId];
                Sprite img = ventAnime[type][index];

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

                    ventAnime[type][index] = img;
                    return img;
                }
            }

            public bool IsCustomVent(int ventId) => ventType.ContainsKey(ventId);
        }
    }

}
