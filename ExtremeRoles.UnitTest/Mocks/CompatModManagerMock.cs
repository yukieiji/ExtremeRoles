namespace ExtremeRoles.UnitTest.Mocks;

public class CompatModManagerMock : ISerialMockSetup
{
	public void Setup()
	{
		MockSetupHelper.SetupCompatModManager();
	}
}
