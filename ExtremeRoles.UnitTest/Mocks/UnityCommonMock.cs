namespace ExtremeRoles.UnitTest.Mocks;

public class UnityCommonMock : ISerialMockSetup
{
	public UnityObjectOperatorsMock OperatorsMock { get; } = new();
	public Vector2Mock Vector2Mock { get; } = new();
	public ColorMock ColorMock { get; } = new();
	public MathfMock MathfMock { get; } = new();
	public TimeMock TimeMock { get; } = new();

	public void Setup()
	{
		OperatorsMock.Setup();
		Vector2Mock.Setup();
		ColorMock.Setup();
		MathfMock.Setup();
		TimeMock.Setup();
	}
}
