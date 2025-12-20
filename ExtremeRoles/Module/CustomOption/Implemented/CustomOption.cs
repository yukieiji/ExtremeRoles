using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

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

			var amongUs = AmongUsClient.Instance;
			if (LobbyBehaviour.Instance != null &&
				amongUs != null && amongUs.AmHost)
			{
				config.Value = value;
			}
		}
	}

	public bool IsChangeDefault => this.Selection != this.holder.DefaultIndex;

	public bool IsViewActive => !this.Info.IsHidden && this.IsActive;

	public bool IsActive => this.Activator.IsActive;

	public IOptionActivator Activator { get; init; }

	private readonly ConfigBinder config;
	private readonly IValueHolder holder;

	public event Action OnValueChanged
	{
		add
		{
			this.holder.OnValueChanged += value;
		}
		remove
		{
			this.holder.OnValueChanged -= value;
		}
	}

	public CustomOption(
		IOptionInfo info,
		IValueHolder value,
		IOptionActivator? activator)
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

		this.Activator = activator ?? new AlwaysActive();

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

	public T Value<T>() where T :
		struct, IComparable, IConvertible,
		IComparable<T>, IEquatable<T>
	{
		var holder = this.holder;
		// コンストラクタでIValueを継承していることは確定しているのでUnsafe.Asで高速にキャストする
		var value = Unsafe.As<IValueHolder, IValue<T>>(ref holder);
		return value.Value;
	}
}
