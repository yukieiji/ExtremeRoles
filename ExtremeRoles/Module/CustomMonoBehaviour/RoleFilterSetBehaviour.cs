using System;
using System.Linq;

using UnityEngine;

using ExtremeRoles.Module.RoleAssign;
using ExtremeRoles.Module.RoleAssign.Model;
using ExtremeRoles.Module.CustomMonoBehaviour.UIPart;

#nullable enable

namespace ExtremeRoles.Module.CustomMonoBehaviour;

[Il2CppRegister]
public sealed class RoleFilterSetBehaviour : MonoBehaviour
{
#pragma warning disable CS8618
    private ButtonWrapper addRoleButton;
    private ButtonWrapper deleteAllRoleButton;
    private ButtonWrapper deleteThisButton;

    public RoleFilterSetBehaviour(IntPtr ptr) : base(ptr) { }
#pragma warning restore CS8618
    public void Awake()
    {
        Transform trans = base.transform;

        this.addRoleButton = trans.Find(
            "Buttons/AddRoleButton").gameObject.GetComponent<ButtonWrapper>();
        this.deleteAllRoleButton = trans.Find(
            "Buttons/RemoveAllButton").gameObject.GetComponent<ButtonWrapper>();
        this.deleteThisButton = trans.Find(
            "Buttons/DeleteThisButton").gameObject.GetComponent<ButtonWrapper>();
    }
}
