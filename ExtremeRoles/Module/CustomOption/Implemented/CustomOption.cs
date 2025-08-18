using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Runtime.CompilerServices;

using ExtremeRoles.Module.CustomOption.Interfaces;

#nullable enable

namespace ExtremeRoles.Module.CustomOption.Implemented;

public sealed class CustomOption : IOption
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

			this.OnValueChanged?.Invoke(value);

			var amongUs = AmongUsClient.Instance;
			if (amongUs != null &&
				amongUs.AmHost)
			{
				config.Value = value;
			}
		}
	}

	public bool IsEnable => this.enableCondition.IsMet;
	public IOptionCondition EnableCondition
	{
		set
		{
			enableCondition = value;
		}
	}
	private IOptionCondition enableCondition;

	public bool IsActiveAndEnable
	{
		get
		{
			if (this.Info.IsHidden)
			{
				return false;
			}
			return this.activeCondition.IsMet;
		}
	}
	public IOptionCondition ActiveCondition
	{
		set
		{
			activeCondition = value;
		}
	}
	private IOptionCondition activeCondition;

	private readonly ConfigBinder config;
	private readonly IValueHolder holder;

	public event Action<int>? OnValueChanged;

	public CustomOption(
		IOptionInfo info,
		IValueHolder value,
		IOptionCondition? activeCondition,
		IOptionCondition? enableCondition = null)
	{
		Info = info;

		this.holder = value;

#if DEBUG
		// value.以降の処理がReleaseで走らないようにするため全体的に無効化しておく
		Debug.Assert(
			value.GetType().GetInterfaces().Any(
				i => 
					i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IValue<>)),
			"holder must implement IValue<T>");
#endif

		this.activeCondition = activeCondition ?? new AlwaysTrueCondition();
		this.enableCondition = enableCondition ?? new NotDefaultValueCondition(this.holder);

		int defaultIndex = value.DefaultIndex;
		config = new ConfigBinder(Info.CodeRemovedName, defaultIndex);

		// 非表示のオプションは基本的に不具合や内部仕様のハックを元に作成されていることが多いため、デフォルト値に設定し変な挙動が起きにくくする
		this.Selection = Info.IsHidden ? defaultIndex : config.Value;

		ExtremeRolesPlugin.Logger.LogInfo($"---- Create new Option ----\n{this}\n--------");
	}

	public override string ToString()
	{
		var builder = new StringBuilder();
		builder
			.AppendLine(this.Info.ToString())
			.Append(this.holder.ToString());
		return builder.ToString();
	}

	public void SwitchPreset()
	{
		this.config.Rebind();
		Selection = this.config.Value;
	}

	public T GetValue<T>() where T :
		struct, IComparable, IConvertible,
		IComparable<T>, IEquatable<T>
	{
		var holder = this.holder;
		var value = Unsafe.As<IValueHolder, IValue<T>>(ref holder);
		return value.Value;
	}
}
