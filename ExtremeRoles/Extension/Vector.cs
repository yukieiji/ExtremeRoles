using UnityEngine;

namespace ExtremeRoles.Extension.Vector;

public static class VectorExtension
{
	public static bool IsCloseTo(this Vector2 @this, Vector2 other, float sqrEps = 0.001f)
	{
		var diff = @this - other;
		return diff.sqrMagnitude <= sqrEps;
	}

	public static bool IsCloseTo(this Vector3 @this, Vector3 other, float sqrEps = 0.001f)
	{
		var diff = @this - other;
		return diff.sqrMagnitude <= sqrEps;
	}

	public static bool IsCloseTo(this Vector4 @this, Vector4 other, float sqrEps = 0.001f)
	{
		var diff = @this - other;
		return diff.sqrMagnitude <= sqrEps;
	}

	public static bool IsNotCloseTo(this Vector2 @this, Vector2 other, float sqrEps = 0.001f)
	{
		var diff = @this - other;
		return diff.sqrMagnitude > sqrEps;
	}

	public static bool IsNotCloseTo(this Vector3 @this, Vector3 other, float sqrEps = 0.001f)
	{
		var diff = @this - other;
		return diff.sqrMagnitude > sqrEps;
	}

	public static bool IsNotCloseTo(this Vector4 @this, Vector4 other, float sqrEps = 0.001f)
	{
		var diff = @this - other;
		return diff.sqrMagnitude > sqrEps;
	}

}
