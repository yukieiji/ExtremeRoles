using Xunit;

namespace ExtremeRoles.UnitTest.Mocks;

public abstract class SerialTestBase(
	SerialFixture fixture,
	params ISerialMockSetup[] mockSetups)
	: IAsyncLifetime
{
	private IDisposable? _lock;

	protected IReadOnlyList<ISerialMockSetup> Mocks { get; } = mockSetups;

	public async ValueTask InitializeAsync()
	{
		_lock = await fixture.AcquireAsync(
			mockSetups.Select(x => x.GetType()));

		foreach (var mock in mockSetups)
		{
			mock.Setup();
		}
	}

	public ValueTask DisposeAsync()
	{
		// SetUpすればキレイになるので
		foreach (var mock in mockSetups)
		{
			mock.Setup();
		}
		_lock?.Dispose();
		return ValueTask.CompletedTask;
	}
}
