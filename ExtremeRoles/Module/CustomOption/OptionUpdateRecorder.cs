using ExtremeRoles.Module.CustomOption.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading;

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
	private readonly ConcurrentDictionary<uint, RecordResult> records = new();
	private uint nextId;

	// 初期化時に一度だけ
	public void RegisterRecordOption(int categoryId, IOption option)
	{
		option.OnValueChanged += () =>
		{
			foreach (var record in records.Values)
			{
				record.Add(categoryId, option);
			}
		};
	}

	public RecordResult StartRecord()
	{
		uint id = Interlocked.Increment(ref nextId);
		RecordResult? record = new RecordResult(() => records.TryRemove(id, out _));
		records.TryAdd(id, record);
		return record;
	}
}