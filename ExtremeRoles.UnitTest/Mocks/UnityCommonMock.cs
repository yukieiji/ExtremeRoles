namespace ExtremeRoles.UnitTest.Mocks;

public class UnityCommonMock : ISerialMockSetup
{
	public UnityObjectOperatorsMock OperatorsMock { get; } = new();
	public Vector2Mock Vector2Mock { get; } = new();
	public ColorMock ColorMock { get; } = new();
	public MathfMock MathfMock { get; } = new();
	public PaletteMock PaletteMock { get; } = new();
	public GameOptionsManagerMock GameOptionsManagerMock { get; } = new();
	public CompatModManagerMock CompatModManagerMock { get; } = new();
	public TimeMock TimeMock { get; } = new();

	public void Setup()
	{
		OperatorsMock.Setup();
		Vector2Mock.Setup();
		ColorMock.Setup();
		MathfMock.Setup();
		PaletteMock.Setup();
		GameOptionsManagerMock.Setup();
		CompatModManagerMock.Setup();
		TimeMock.Setup();
	}
}
