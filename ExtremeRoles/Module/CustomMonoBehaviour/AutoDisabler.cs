using UnityEngine;

namespace ExtremeRoles.Module.CustomMonoBehaviour;

[Il2CppRegister]
public sealed class AutoDisabler : MonoBehaviour
{
	public void OnEnable()
	{
		this.gameObject.SetActive(false);
	}
}
