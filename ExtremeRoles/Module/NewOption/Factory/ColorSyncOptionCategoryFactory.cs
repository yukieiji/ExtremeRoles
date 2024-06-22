using System;
using System.Runtime.CompilerServices;

using UnityEngine;

using OptionUnit = ExtremeRoles.Module.CustomOption.OptionUnit;

using ExtremeRoles.Module.NewOption.Interfaces;
using ExtremeRoles.Module.NewOption.Implemented;

#nullable enable

namespace ExtremeRoles.Module.NewOption.Factory;

public sealed class ColorSyncOptionCategoryFactory(
	in Color color,
	in OptionCategoryFactory factory) : IDisposable
{
	private readonly Color color = color;
	private readonly OptionCategoryFactory factory = factory;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public BoolCustomOption CreateBoolOption<T>(
		T option,
		bool defaultValue,
		IOption? parent = null,
		bool isHidden = false,
		OptionUnit format = OptionUnit.None,
		bool invert = false,
		bool ignorePrefix = false)
		where T : struct, IConvertible
		=> this.factory.CreateBoolOption(
			option,
			defaultValue,
			parent, isHidden,
			format, invert,
			this.color, ignorePrefix);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public FloatCustomOption CreateFloatOption<T>(
		T option,
		float defaultValue,
		float min, float max, float step,
		IOption? parent = null,
		bool isHidden = false,
		OptionUnit format = OptionUnit.None,
		bool invert = false,
		bool ignorePrefix = false)
		where T : struct, IConvertible
		=> this.factory.CreateFloatOption(
			option,
			defaultValue,
			min, max, step,
			parent, isHidden,
			format, invert,
			this.color, ignorePrefix);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public FloatDynamicCustomOption CreateFloatDynamicOption<T>(
		T option,
		float defaultValue,
		float min, float step,
		IOption? parent = null,
		bool isHidden = false,
		OptionUnit format = OptionUnit.None,
		bool invert = false,
		float tempMaxValue = 0.0f,
		bool ignorePrefix = false)
		where T : struct, IConvertible
		=> this.factory.CreateFloatDynamicOption(
			option,
			defaultValue,
			min, step,
			parent, isHidden,
			format, invert,
			this.color, tempMaxValue,
			ignorePrefix);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public IntCustomOption CreateIntOption<T>(
		T option,
		int defaultValue,
		int min, int max, int step,
		IOption? parent = null,
		bool isHidden = false,
		OptionUnit format = OptionUnit.None,
		bool invert = false,
		bool ignorePrefix = false)
		where T : struct, IConvertible
		=> this.factory.CreateIntOption(
			option,
			defaultValue,
			min, max, step,
			parent, isHidden,
			format, invert,
			this.color, ignorePrefix);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public IntDynamicCustomOption CreateIntDynamicOption<T>(
		T option,
		int defaultValue,
		int min, int step,
		IOption? parent = null,
		bool isHidden = false,
		OptionUnit format = OptionUnit.None,
		bool invert = false,
		int tempMaxValue = 0,
		bool ignorePrefix = false)
		where T : struct, IConvertible
		=> this.factory.CreateIntDynamicOption(
			option,
			defaultValue,
			min, step,
			parent, isHidden,
			format, invert,
			this.color, tempMaxValue,
			ignorePrefix);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public SelectionCustomOption CreateSelectionOption<T>(
		T option,
		string[] selections,
		IOption? parent = null,
		bool isHidden = false,
		OptionUnit format = OptionUnit.None,
		bool invert = false,
		bool ignorePrefix = false)
		where T : struct, IConvertible
		=> this.factory.CreateSelectionOption(
			option,
			selections,
			parent, isHidden,
			format, invert,
			this.color, ignorePrefix);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public SelectionCustomOption CreateSelectionOption<T, W>(
		T option,
		IOption? parent = null,
		bool isHeader = false,
		bool isHidden = false,
		OptionUnit format = OptionUnit.None,
		bool invert = false,
		bool ignorePrefix = false)
		where T : struct, IConvertible
		where W : struct, Enum
		=> this.factory.CreateSelectionOption<T, W>(
			option,
			parent, isHidden,
			format, invert,
			this.color, ignorePrefix);

	public void Dispose()
	{
		this.factory.Dispose();
	}
}
