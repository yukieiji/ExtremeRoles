using ExtremeRoles.Module.CustomOption.Interfaces;
using System;
using System.Collections.Generic;

namespace ExtremeRoles.Module.CustomOption;

#nullable enable

public record CategoryOption(int CategoryId, IOption Option);

public sealed class RecordResult(Action disposeAction) : IDisposable
{
	public Dictionary<int, List<IOption>> Result { get; init; } = new Dictionary<int, List<IOption>>();
	private readonly Action disposeAction = disposeAction;

	public void Add(int categoryId, IOption option)
	{
		if (!this.Result.TryGetValue(categoryId, out var options))
		{
			options = [];
			this.Result[categoryId] = options;
		}
		options.Add(option);
	}

	public void Dispose()
	{
		this.Result.Clear();
		this.disposeAction.Invoke();
	}
}

public sealed class OptionUpdateRecorder : NullableSingleton<OptionUpdateRecorder>
{
	private RecordResult? @record;

	public RecordResult StartRecord()
	{
		this.record = new RecordResult(() => this.@record = null);
		return this.record;
	}

	public void RegisterRecordOption(int categoryId, IOption option)
	{
		option.OnValueChanged += () => this.record?.Add(categoryId, option);
	}
}
