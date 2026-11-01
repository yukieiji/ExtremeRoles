using ExtremeRoles.Roles.Solo.Crewmate.Invisible;
using Xunit;

namespace ExtremeRoles.UnitTest.Roles.Solo.Crewmate.Invisible;

public sealed class InvisibleStatusModelTests
{
	[Theory]
	[InlineData(1.5f, true)]
	[InlineData(0.8f, false)]
	public void Constructor_InitializesProperties(float speedMod, bool hasHitbox)
	{
		// Arrange & Act
		var model = new InvisibleStatusModel(speedMod, hasHitbox);

		// Assert
		Assert.Equal(speedMod, model.SpeedMod);
		Assert.Equal(hasHitbox, model.HasHitbox);
	}
}
