using System;
using System.Linq;
using System.Runtime.CompilerServices;

using UnityEngine;

using ExtremeRoles.Helper;
using static Il2CppSystem.Xml.Schema.FacetsChecker.FacetsCompiler;
using Rewired.Utils.Platforms.Windows;



#nullable enable

namespace ExtremeRoles.Module.CustomOption.Factorys;

public sealed class SequntialAutoParentSetFactory
{
	private IOptionInfo? parent;
	private readonly SequentialOptionFactory internalFactory;

	public SequntialAutoParentSetFactory(
		int idOffset = 0,
		string namePrefix = "",
		OptionTab tab = OptionTab.General)
	{
		this.internalFactory = new SequentialOptionFactory(idOffset, namePrefix, tab);
	}

	public FloatCustomOption CreateFloatOption(
		object option,
		float defaultValue,
		float min, float max, float step,
		IOptionInfo? parent = null,
		bool isHeader = false,
		bool isHidden = false,
		OptionUnit format = OptionUnit.None,
		bool invert = false,
		IOptionInfo? enableCheckOption = null,
		Color? color = null,
		bool ignorePrefix = false)
	{
		FloatCustomOption newOption = this.internalFactory.CreateFloatOption(
			option,
			defaultValue,
			min, max, step,
			parent is null ? this.parent : parent,
			isHeader,
			isHidden,
			format,
			invert,
			enableCheckOption,
			color,
			ignorePrefix);

		if (isHeader)
		{
			this.parent = newOption;
		}
		return newOption;
	}

	public FloatDynamicCustomOption CreateFloatDynamicOption(
		object option,
		float defaultValue,
		float min, float step,
		IOptionInfo? parent = null,
		bool isHeader = false,
		bool isHidden = false,
		OptionUnit format = OptionUnit.None,
		bool invert = false,
		IOptionInfo? enableCheckOption = null,
		Color? color = null,
		float tempMaxValue = 0.0f,
		bool ignorePrefix = false)
	{
		FloatDynamicCustomOption newOption = this.internalFactory.CreateFloatDynamicOption(
			option,
			defaultValue,
			min, step,
			parent is null ? this.parent : parent,
			isHeader,
			isHidden,
			format,
			invert,
			enableCheckOption,
			color,
			tempMaxValue,
			ignorePrefix);

		if (isHeader)
		{
			this.parent = newOption;
		}
		return newOption;
	}

	public IntCustomOption CreateIntOption(
		object option,
		int defaultValue,
		int min, int max, int step,
		IOptionInfo? parent = null,
		bool isHeader = false,
		bool isHidden = false,
		OptionUnit format = OptionUnit.None,
		bool invert = false,
		IOptionInfo? enableCheckOption = null,
		Color? color = null,
		bool ignorePrefix = false)
	{
		IntCustomOption newOption = this.internalFactory.CreateIntOption(
			option,
			defaultValue,
			min, max, step,
			parent is null ? this.parent : parent,
			isHeader,
			isHidden,
			format,
			invert,
			enableCheckOption,
			color,
			ignorePrefix);

		if (isHeader)
		{
			this.parent = newOption;
		}
		return newOption;
	}

	public IntDynamicCustomOption CreateIntDynamicOption(
		object option,
		int defaultValue,
		int min, int step,
		IOptionInfo? parent = null,
		bool isHeader = false,
		bool isHidden = false,
		OptionUnit format = OptionUnit.None,
		bool invert = false,
		IOptionInfo? enableCheckOption = null,
		Color? color = null,
		int tempMaxValue = 0,
		bool ignorePrefix = false)
	{
		IntDynamicCustomOption newOption = this.internalFactory.CreateIntDynamicOption(
			option,
			defaultValue,
			min, step,
			parent is null ? this.parent : parent,
			isHeader,
			isHidden,
			format,
			invert,
			enableCheckOption,
			color,
			tempMaxValue,
			ignorePrefix);

		if (isHeader)
		{
			this.parent = newOption;
		}
		return newOption;
	}

	public BoolCustomOption CreateBoolOption(
		object option,
		bool defaultValue,
		IOptionInfo? parent = null,
		bool isHeader = false,
		bool isHidden = false,
		OptionUnit format = OptionUnit.None,
		bool invert = false,
		IOptionInfo? enableCheckOption = null,
		Color? color = null,
		bool ignorePrefix = false)
	{
		BoolCustomOption newOption = this.internalFactory.CreateBoolOption(
			option,
			defaultValue,
			parent is null ? this.parent : parent,
			isHeader,
			isHidden,
			format,
			invert,
			enableCheckOption,
			color,
			ignorePrefix);

		if (isHeader)
		{
			this.parent = newOption;
		}
		return newOption;
	}

	public SelectionCustomOption CreateSelectionOption(
		object option,
		string[] selections,
		IOptionInfo? parent = null,
		bool isHeader = false,
		bool isHidden = false,
		OptionUnit format = OptionUnit.None,
		bool invert = false,
		IOptionInfo? enableCheckOption = null,
		Color? color = null,
		bool ignorePrefix = false)
	{
		SelectionCustomOption newOption = this.internalFactory.CreateSelectionOption(
			option,
			selections,
			parent is null ? this.parent : parent,
			isHeader,
			isHidden,
			format,
			invert,
			enableCheckOption,
			color,
			ignorePrefix);

		if (isHeader)
		{
			this.parent = newOption;
		}
		return newOption;
	}

	public SelectionCustomOption CreateSelectionOption<W>(
		object option,
		IOptionInfo? parent = null,
		bool isHeader = false,
		bool isHidden = false,
		OptionUnit format = OptionUnit.None,
		bool invert = false,
		IOptionInfo? enableCheckOption = null,
		Color? color = null,
		bool ignorePrefix = false)
		where W : struct, IConvertible
	{
		SelectionCustomOption newOption = this.internalFactory.CreateSelectionOption<W>(
			option,
			parent is null ? this.parent : parent,
			isHeader,
			isHidden,
			format,
			invert,
			enableCheckOption,
			color,
			ignorePrefix);

		if (isHeader)
		{
			this.parent = newOption;
		}
		return newOption;
	}
}
