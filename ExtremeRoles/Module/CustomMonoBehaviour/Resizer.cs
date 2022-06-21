using UnityEngine;

namespace ExtremeRoles.Module.CustomMonoBehaviour
{
    [Il2CppRegister]
    public class Resizeer : MonoBehaviour
    {
        public float Scale = 0.5f;

        public Resizeer(System.IntPtr ptr) : base(ptr) { }

        public void LateUpdate()
        {
            this.transform.localScale *= Scale;
        }
    }
}
