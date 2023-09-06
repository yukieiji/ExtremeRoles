using UnityEngine;

namespace ExtremeRoles.Helper;

public static class Key
{
	public static bool IsShift()
		=> Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

	public static bool IsShiftDown()
		=> Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift);

	public static bool IsControlDown()
		=> Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightAlt);

	public static bool IsAltDown()
		=> Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt);
}
