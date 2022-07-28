using UnityEngine;

namespace ExtremeRoles.Module.CustomMonoBehaviour
{
    [Il2CppRegister]
    public sealed class Resizeer : MonoBehaviour
    {
        private Vector3 targetScale = Vector3.one;

        public void LateUpdate()
        {
            this.transform.localScale = Vector3.Scale(this.transform.localScale, targetScale);
        }

        public void SetScale(float targetX, float targetY, float targetZ)
        {
            this.targetScale = new Vector3(targetX, targetY, targetZ);
        }
    }
}
