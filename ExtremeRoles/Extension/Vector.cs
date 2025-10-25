using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ExtremeRoles.Extension.Vector;

public static class VectorExtention
{
	public static bool IsCloseTo(this Vector2 @this, Vector2 other, float eps = 0.001f)
	{
		var diff = @this - other;
		return diff.sqrMagnitude <= eps;
	}

	public static bool IsCloseTo(this Vector3 @this, Vector3 other, float eps = 0.001f)
	{
		var diff = @this - other;
		return diff.sqrMagnitude <= eps;
	}

	public static bool IsCloseTo(this Vector4 @this, Vector4 other, float eps = 0.001f)
	{
		var diff = @this - other;
		return diff.sqrMagnitude <= eps;
	}

	public static bool IsNotCloseTo(this Vector2 @this, Vector2 other, float eps = 0.001f)
	{
		var diff = @this - other;
		return diff.sqrMagnitude >= eps;
	}

	public static bool IsNotCloseTo(this Vector3 @this, Vector3 other, float eps = 0.001f)
	{
		var diff = @this - other;
		return diff.sqrMagnitude >= eps;
	}

	public static bool IsNotCloseTo(this Vector4 @this, Vector4 other, float eps = 0.001f)
	{
		var diff = @this - other;
		return diff.sqrMagnitude >= eps;
	}

}
