using System;
using System.Runtime.CompilerServices;

using UnityEngine;

#nullable enable

namespace ExtremeRoles.Module.NewOption.Factory;

public sealed class ColorSyncOptionFactory(
	in Color color,
	in OptionGroupFactory factory) : IDisposable
{
	private readonly Color color = color;
	private readonly OptionGroupFactory factory = factory;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public FloatCustomOption CreateFloatOption<T>(
		T option,
		float defaultValue,
		float min, float max, float step,
		IOptionInfo? parent = null,
		bool isHeader = false,
		bool isHidden = false,
		OptionUnit format = OptionUnit.None,
		bool invert = false,
		IOptionInfo? enableCheckOption = null,
		bool ignorePrefix = false)
		where T : struct, IConvertible
		=> this.factory.CreateFloatOption(
			option,
			defaultValue,
			min, max, step,
			parent, isHeader, isHidden,
			format, invert, enableCheckOption,
			this.color, ignorePrefix);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public FloatDynamicCustomOption CreateFloatDynamicOption<T>(
		T option,
		float defaultValue,
		float min, float step,
		IOptionInfo? parent = null,
		bool isHeader = false,
		bool isHidden = false,
		OptionUnit format = OptionUnit.None,
		bool invert = false,
		IOptionInfo? enableCheckOption = null,
		float tempMaxValue = 0.0f,
		bool ignorePrefix = false)
		where T : struct, IConvertible
		=> this.factory.CreateFloatDynamicOption(
			option,
			defaultValue,
			min, step,
			parent, isHeader, isHidden,
			format, invert, enableCheckOption,
			this.color, tempMaxValue,
			ignorePrefix);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public IntCustomOption CreateIntOption<T>(
		T option,
		int defaultValue,
		int min, int max, int step,
		IOptionInfo? parent = null,
		bool isHeader = false,
		bool isHidden = false,
		OptionUnit format = OptionUnit.None,
		bool invert = false,
		IOptionInfo? enableCheckOption = null,
		bool ignorePrefix = false)
		where T : struct, IConvertible
		=> this.factory.CreateIntOption(
			option,
			defaultValue,
			min, max, step,
			parent, isHeader, isHidden,
			format, invert, enableCheckOption,
			this.color, ignorePrefix);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public IntDynamicCustomOption CreateIntDynamicOption<T>(
		T option,
		int defaultValue,
		int min, int step,
		IOptionInfo? parent = null,
		bool isHeader = false,
		bool isHidden = false,
		OptionUnit format = OptionUnit.None,
		bool invert = false,
		IOptionInfo? enableCheckOption = null,
		int tempMaxValue = 0,
		bool ignorePrefix = false)
		where T : struct, IConvertible
		=> this.factory.CreateIntDynamicOption(
			option,
			defaultValue,
			min, step,
			parent, isHeader, isHidden,
			format, invert, enableCheckOption,
			this.color, tempMaxValue,
			ignorePrefix);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public BoolCustomOption CreateBoolOption<T>(
		T option,
		bool defaultValue,
		IOptionInfo? parent = null,
		bool isHeader = false,
		bool isHidden = false,
		OptionUnit format = OptionUnit.None,
		bool invert = false,
		IOptionInfo? enableCheckOption = null,
		bool ignorePrefix = false)
		where T : struct, IConvertible
		=> this.factory.CreateBoolOption(
			option,
			defaultValue,
			parent, isHeader, isHidden,
			format, invert, enableCheckOption,
			this.color, ignorePrefix);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public SelectionCustomOption CreateSelectionOption<T>(
		T option,
		string[] selections,
		IOptionInfo? parent = null,
		bool isHeader = false,
		bool isHidden = false,
		OptionUnit format = OptionUnit.None,
		bool invert = false,
		IOptionInfo? enableCheckOption = null,
		bool ignorePrefix = false)
		where T : struct, IConvertible
		=> this.factory.CreateSelectionOption(
			option,
			selections,
			parent, isHeader, isHidden,
			format, invert, enableCheckOption,
			this.color, ignorePrefix);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public SelectionCustomOption CreateSelectionOption<T, W>(
		T option,
		IOptionInfo? parent = null,
		bool isHeader = false,
		bool isHidden = false,
		OptionUnit format = OptionUnit.None,
		bool invert = false,
		IOptionInfo? enableCheckOption = null,
		bool ignorePrefix = false)
		where T : struct, IConvertible
		where W : struct, IConvertible
		=> this.factory.CreateSelectionOption<T, W>(
			option,
			parent, isHeader, isHidden,
			format, invert, enableCheckOption,
			this.color, ignorePrefix);

	public void Dispose()
	{
		this.factory.Dispose();
	}
}
