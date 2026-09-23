using ExtremeRoles.Roles.Solo.Crewmate.Delusioner;
using Xunit;

namespace ExtremeRoles.UnitTest.Roles.Solo.Crewmate.Delusioner;

public class DelusionerStatusModelTests
{
	[Fact]
	public void Properties_SetAndGetCorrectly()
	{
		var model = new DelusionerStatusModel(2.5f, true, 0.9f);
		Assert.Equal(2.5f, model.Range);
		Assert.True(model.IncludeSpawnPoint);
		Assert.Equal(0.9f, model.DeflectDamagePenaltyMod);

		model.CurCoolTime = 15.0f;
		Assert.Equal(15.0f, model.CurCoolTime);
	}
}
