
using System;
using System.Runtime.CompilerServices;
using System.Text;
using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.CustomOption.Implemented.Value;
using ExtremeRoles.Module.CustomOption.Interfaces;

#nullable enable

namespace ExtremeRoles.Module.CustomOption.Implemented;

public sealed class PresetOption : IOption
{
	public IOptionInfo Info { get; }

	public string TransedTitle => Tr.GetString(Info.Name);

	public string TransedValue
	{
		get
		{
			string format = Info.Format;
			string value = this.holder.StrValue;
			return string.IsNullOrEmpty(format) ?
				value : Tr.GetString(format, value);
		}
	}

	public int Range => this.holder.Range;

	public int Selection
	{
		get => this.holder.Selection;
		set
		{
			this.holder.Selection = value;

			this.OnValueChanged?.Invoke();
		}
	}

	public bool IsChangeDefault => false;

	public bool IsActive => true;
	public bool IsViewActive => true;

	private readonly IValueHolder holder;

	public event Action? OnValueChanged;

	private const int maxPresetNum = 20;
	private const int optionId = 0;
	private const int categoryId = 0;

	public enum OptionKey : int
	{
		Selection = optionId,
	}

	public PresetOption(string name)
	{
		Info = new PresetOptionInfo(categoryId, $"{name}{OptionKey.Selection}");
		this.holder = new IntOptionValue(1, 1, maxPresetNum, 1);

		this.Selection = 0;

		this.OnValueChanged += () =>
		{
			OptionManager.Instance.SwitchPreset();
		};

		ExtremeRolesPlugin.Logger.LogInfo($"---- Create new Option ----\n{this}\n--------");
	}

	public static void Create(string name)
	{
		
		using (var commonOptionFactory = OptionCategoryAssembler.CreateOptionCategory(
			categoryId, name, color: OptionCreator.DefaultOptionColor))
		{
			var presetOption = new PresetOption(name);
			commonOptionFactory.AddOption(optionId, presetOption);
		}
	}

	public static bool IsPreset(int categoryId, int optionId)
		=> categoryId == PresetOption.categoryId && optionId == PresetOption.optionId;

	public override string ToString()
	{
		var builder = new StringBuilder();
		builder
			.AppendLine(Info.ToString())
			.Append(holder.ToString());
		return builder.ToString();
	}

	public void SwitchPreset()
	{
	}

	// 基本的にPresetOptionはこのメソッドを使わないが一応実装しておく、コンストラクタ内でint型になることは証明済み
	public T Value<T>() where T : struct, IComparable, IConvertible, IComparable<T>, IEquatable<T>
	{
		if (typeof(T) == typeof(int))
		{
			int val = this.Selection;
			return Unsafe.As<int, T>(ref val);
		}
		throw new InvalidCastException($"Cannot cast PresetOption value to {typeof(T)}");
	}
}
