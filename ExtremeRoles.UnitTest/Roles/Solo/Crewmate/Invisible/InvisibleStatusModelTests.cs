using ExtremeRoles.Roles.Solo.Crewmate.Invisible;
using Xunit;

namespace ExtremeRoles.UnitTest.Roles.Solo.Crewmate.Invisible;

public sealed class InvisibleStatusModelTests
{
	[Fact]
	public void Constructor_InitializesProperties()
	{
		// Arrange & Act
		var model = new InvisibleStatusModel(1.5f, true);

		// Assert
		Assert.Equal(1.5f, model.SpeedMod);
		Assert.True(model.HasHitbox);
		Assert.False(model.IsActiveInvisible);
	}

	[Fact]
	public void IsActiveInvisible_CanBeModified()
	{
		// Arrange
		var model = new InvisibleStatusModel(2.0f, false);

		// Act
		model.IsActiveInvisible = true;

		// Assert
		Assert.True(model.IsActiveInvisible);
	}
}
