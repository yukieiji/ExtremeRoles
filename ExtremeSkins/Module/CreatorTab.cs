using System.Collections.Generic;
using UnityEngine;

using ExtremeRoles.Module;
using ExtremeRoles.Module.CustomMonoBehaviour.UIPart;

namespace ExtremeSkins.Module
{
    [Il2CppRegister]
    public sealed class CreatorTab : MonoBehaviour
    {
        private ButtonWrapper button;

        private List<ButtonWrapper> selectButton = new List<ButtonWrapper>();

        
        public void Awake()
        {
            Transform trans = base.transform;

            this.button = trans.Find(
                "Scroll View/Viewport/Content/Button").gameObject.GetComponent<ButtonWrapper>();
            this.button.Awake();
        }

        public void SetUpButtons()
        {

        }
    }
}
