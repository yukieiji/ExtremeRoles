using ExtremeRoles.UnitTest.Mocks;
using System.Collections.Concurrent;
using Xunit;

[assembly: AssemblyFixture<SerialFixture>]

namespace ExtremeRoles.UnitTest.Mocks;

public sealed class SerialFixture
{
	private readonly ConcurrentDictionary<Type, SemaphoreSlim> _semaphores = [];

	public async Task<IDisposable> AcquireAsync(
		IEnumerable<Type> resourceTypes)
	{
		var types = resourceTypes
			.Distinct()
			.OrderBy(x => x.FullName)
			.ToArray();

		var acquired = new List<SemaphoreSlim>(types.Length);

		try
		{
			foreach (var type in types)
			{
				var semaphore = _semaphores.GetOrAdd(
					type,
					static _ => new SemaphoreSlim(1, 1));

				await semaphore.WaitAsync();
				acquired.Add(semaphore);
			}

			return new Releaser(acquired);
		}
		catch
		{
			foreach (var semaphore in acquired.AsEnumerable().Reverse())
			{
				semaphore.Release();
			}
			throw;
		}
	}

	private sealed class Releaser(
		IReadOnlyList<SemaphoreSlim> semaphores) : IDisposable
	{
		public void Dispose()
		{
			foreach (var semaphore in semaphores.Reverse())
			{
				semaphore.Release();
			}
		}
	}
}
