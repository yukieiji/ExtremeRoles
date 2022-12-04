using UnityEngine;

namespace ExtremeRoles
{
    public sealed class ExtremeRolePluginBehavior : MonoBehaviour
    {
        public void Update()
        {
            if (Input.GetKeyDown(KeyCode.F8))
            {
                Helper.Logging.Dump();
            }
        }
    }
}
