using System;
using UnityEngine;

namespace ExtremeRoles.Module.CustomMonoBehaviour.UIPart
{
    [Il2CppRegister]
    public sealed class HideUIBehaviour : MonoBehaviour
    {
        private GameObject hideObj;

        private BoxCollider2D colider;

        private bool enable;

        public HideUIBehaviour(IntPtr ptr) : base(ptr) { }

        public void Awake()
        {
            this.colider = base.gameObject.GetComponent<BoxCollider2D>();
        }

        public void OnMouseDown()
        {
            if (this.enable)
            {
                this.hideObj.SetActive(false);
            }
        }
        public void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape) && this.enable)
            {
                this.hideObj.SetActive(false);
            }
        }

        public void SetActive(bool active)
        {
            this.enable = active;
        }

        public void SetHideObject(GameObject obj)
        {
            this.hideObj = obj;
        }
    }
}