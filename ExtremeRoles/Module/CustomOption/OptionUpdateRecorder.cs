using ExtremeRoles.Module.CustomOption.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExtremeRoles.Module.CustomOption;

#nullable enable

public sealed class RecordResult(Action disposeAction) : IDisposable
{
	public IReadOnlyList<IOption> Result => this.result;

	private readonly List<IOption> result = new List<IOption>();
	private readonly Action disposeAction = disposeAction;

	public void Add(IOption option)
	{
		this.result.Add(option); 
	}

	public void Dispose()
	{
		this.result.Clear();
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

	public void RegisterRecordOption(IOption option)
	{
		option.OnValueChanged += () => this.record?.Add(option);
	}
}
