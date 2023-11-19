using ExtremeRoles.Performance;
using System;
using System.Collections;
using System.Threading.Tasks;

namespace ExtremeRoles.Module;

public sealed class TaskWaiter
{
	private readonly Task task;

	public TaskWaiter(Task task)
	{
		this.task = task;
	}

	public IEnumerator Wait()
	{
		while (!this.task.IsCompleted)
		{
			yield return null;
		}
	}

	public static implicit operator TaskWaiter(Task task) =>
		new TaskWaiter(task);
}

public sealed class TaskWaiter<T>
{
	public T Result => this.task.Result;
	private readonly Task<T> task;

	public TaskWaiter(Task<T> task)
	{
		this.task = task;
	}

	public IEnumerator Wait()
	{
		while (!this.task.IsCompleted)
		{
			yield return null;
		}
	}

	public static implicit operator TaskWaiter<T>(Task<T> task) =>
		new TaskWaiter<T>(task);
}

public sealed class ValueTaskWaiter<T>
{
	public T Result => this.task.Result;
	private readonly ValueTask<T> task;

	public ValueTaskWaiter(ValueTask<T> task)
	{
		this.task = task;
	}

	public IEnumerator Wait()
	{
		while (!this.task.IsCompleted)
		{
			yield return null;
		}
	}

	public static implicit operator ValueTaskWaiter<T>(ValueTask<T> task) =>
		new ValueTaskWaiter<T>(task);
}
