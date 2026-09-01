using ExtremeRoles.Module.SystemType;
using ExtremeRoles.Module.SystemType.Roles;
using Xunit;
using WeaponAbility = ExtremeRoles.Roles.Solo.Impostor.Scavenger.Ability;

namespace ExtremeRoles.UnitTest.Module.SystemType.Roles;

[Collection(nameof(MockSetupHelper.SetupUnityCommonMocks))]
public class ScavengerAbilityProviderSystemTests
{
	public ScavengerAbilityProviderSystemTests()
	{
		MockSetupHelper.SetupExtremeSystemTypeManagerMock();
	}

	[Fact]
	public void BasicMethods_GetInitWeapon()
	{
		var system = new ScavengerAbilitySystem(WeaponAbility.ScavengerHandGun, false, true, null);
		Assert.False(system.IsDirty);

		var initWeapon = system.GetInitWepon();
		Assert.Equal(WeaponAbility.ScavengerHandGun, initWeapon);

		system.MarkClean();
		system.Reset(ResetTiming.MeetingStart, null);
		system.Deteriorate(1.0f);
	}
}
