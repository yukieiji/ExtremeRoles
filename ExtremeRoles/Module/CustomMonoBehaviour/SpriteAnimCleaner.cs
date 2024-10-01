using PowerTools;
using UnityEngine;

namespace ExtremeRoles.Module.CustomMonoBehaviour;

[Il2CppRegister]
public sealed class SpriteAnimCleaner : MonoBehaviour
{
	public SpriteAnim Anim { private get; set; }
	public void OnDestroy()
	{
		if (Anim == null ||
			!Anim.m_initialized ||
			Anim.m_controller == null)
		{
			return;
		}
		Destroy(Anim.m_controller);
		Anim.m_controller = null;
	}
}
