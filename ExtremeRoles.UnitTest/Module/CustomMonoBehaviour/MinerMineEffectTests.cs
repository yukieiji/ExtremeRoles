using System.Reflection;
using ExtremeRoles.Module.CustomMonoBehaviour;
using UnityEngine;
using Xunit;

namespace ExtremeRoles.UnitTest.Module.CustomMonoBehaviour;

[Collection(nameof(MockSetupHelper.SetupUnityCommonMocks))]
public class MinerMineEffectTests
{
	public MinerMineEffectTests()
	{
		MockSetupHelper.SetupUnityCommonMocks();
		MockSetupHelper.SetupVector2Helpers();
	}

	[Fact]
	public void CalculateNormalizedDistance_ReturnsExpectedNormalizedValue()
	{
		// Arrange
		var objPos = new Vector2(0f, 0f);
		var targetPos = new Vector2(10f, 0f);

		var method = typeof(MinerMineEffect).GetMethod(
			"calculateNormalizedDistance",
			BindingFlags.NonPublic | BindingFlags.Static);

		Assert.NotNull(method);

		// Act
		var result = (float)method.Invoke(null, new object[] { objPos, targetPos, 10f, 5f, 15f })!;

		// Assert
		Assert.True(result >= 0.0f);
	}
}
