using System;
using System.Collections.Generic;

using UnityEngine;

using ExtremeRoles.Module.Interface;
using UnhollowerBaseLib.Attributes;

namespace ExtremeRoles.Module.CustomMonoBehaviour
{
    [Il2CppRegister]
    public sealed class HostObjectUpdater : MonoBehaviour
    {
        private List<IUpdatableObject> updateObject = new List<IUpdatableObject>();

        public HostObjectUpdater(IntPtr ptr) : base(ptr) { }

        public void Awake()
        {
            updateObject.Clear();
        }

        [HideFromIl2Cpp]
        public void AddObject(IUpdatableObject obj)
        {
            updateObject.Add(obj);
        }

        public void RemoveObject(int index)
        {
            updateObject[index].Clear();
            updateObject.RemoveAt(index);
        }

        [HideFromIl2Cpp]
        public void RemoveObject(IUpdatableObject obj)
        {
            obj.Clear();
            updateObject.Remove(obj);
        }

        [HideFromIl2Cpp]
        public IUpdatableObject GetObject(int index) => updateObject[index];

        public void Update()
        {
            if (!AmongUsClient.Instance.AmHost) { return; }

            for (int i = 0; i < updateObject.Count; i++)
            {
                updateObject[i].Update(i);
            }
        }
    }
}
