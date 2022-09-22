using System;
using System.Collections.Generic;
using UnityEngine;

using ExtremeRoles.Helper;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;

namespace ExtremeRoles.Module.CustomMonoBehaviour
{
    [Il2CppRegister]
    public class AbilityPartBase : MonoBehaviour
    {
        public AbilityPartBase(IntPtr ptr) : base(ptr) { }

        public virtual void Picup()
        {

        }
    }
}
