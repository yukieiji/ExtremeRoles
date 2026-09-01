using ExtremeRoles.Module.SystemType;
using Xunit;

namespace ExtremeRoles.UnitTest.Module.SystemType;

[Collection(nameof(MockSetupHelper.SetupUnityCommonMocks))]
public class ModedMushroomSystemTests
{
	[Fact]
	public void Constants_AreCorrect()
	{
		Assert.Equal(ExtremeSystemType.ModedMushroom, ModedMushroomSystem.Type);
		Assert.Equal("ModdedMushroom", ModedMushroomSystem.MushroomName);
	}
}
