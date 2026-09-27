using ExtremeRoles.Roles.Solo.Crewmate;
using Xunit;

#nullable enable

namespace ExtremeRoles.UnitTest.Roles.Solo.Crewmate.CEOTests;

public class CEOStatusTests
{
	[Fact]
	public void CEOStatus_Defaults_AreCorrect()
	{
		var status = new CEOStatus();

		Assert.False(status.IsAwake);
		Assert.NotNull(status.Reviver);
	}

	[Fact]
	public void CEOStatus_IsAwake_CanBeSet()
	{
		var status = new CEOStatus();
		status.IsAwake = true;

		Assert.True(status.IsAwake);

		status.IsAwake = false;
		Assert.False(status.IsAwake);
	}
}
