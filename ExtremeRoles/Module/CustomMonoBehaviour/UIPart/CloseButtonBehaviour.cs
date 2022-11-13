using System;
using UnityEngine;

namespace ExtremeRoles.Module.CustomMonoBehaviour.UIPart
{
    [Il2CppRegister]
    public sealed class CloseButtonBehaviour : MonoBehaviour
    {
        private GameObject hideObj;

        private BoxCollider2D colider;

        public CloseButtonBehaviour(IntPtr ptr) : base(ptr) { }

        public void Awake()
        {
            this.colider = base.GetComponent<BoxCollider2D>();
        }

        public void OnMouseDown()
        {
            this.hideObj.SetActive(false);
        }

        public void SetHideObject(GameObject obj)
        {
            this.hideObj = obj;
        }
    }
}