using UnityEngine;

using Xunit;

using ExtremeRoles.Roles.Solo.Impostor.Flasher;

namespace ExtremeRoles.UnitTest.Roles.Solo.Impostor.Flasher;

[Collection(nameof(MockSetupHelper.SetupUnityCommonMocks))]
public sealed class FlasherStatusModelTests
{
	public FlasherStatusModelTests()
	{
		MockSetupHelper.SetupUnityCommonMocks();
		MockSetupHelper.SetupMathfHelpers();
	}

	private static FlasherStatusModel CreateStartedStatus(float chargeGage)
	{
		var status = new FlasherStatusModel(false, 1.5f, FlasherColor.FlasherColorWhite);
		status.StartSequence(chargeGage);

		return status;
	}

	[Fact]
	public void Constructor_KeepsRangeAndColor()
	{
		// Arrange & Act
		var status = new FlasherStatusModel(false, 2.5f, FlasherColor.FlasherColorBlack);

		// Assert
		Assert.Equal(2.5f, status.Range, 3);
		Assert.Equal(FlasherColor.FlasherColorBlack, status.FlashColor);
		Assert.False(status.IsSequenceActive);
		Assert.Equal(0.0f, status.ChargeGage, 3);
	}

	[Theory]
	[InlineData(FlasherColor.FlasherColorWhite)]
	[InlineData(FlasherColor.FlasherColorBlack)]
	public void ToColor_DependsOnFlasherColor(FlasherColor flashColor)
	{
		// Arrange
		Color expected = flashColor is FlasherColor.FlasherColorBlack
			? Color.black
			: Color.white;

		// Act
		Color screenColor = FlasherStatusModel.ToColor(flashColor);

		// Assert
		Assert.Equal(expected, screenColor);
	}

	[Theory]
	[InlineData(-1.0f, 0.0f)]
	[InlineData(0.5f, 0.5f)]
	[InlineData(1.0f, 1.0f)]
	[InlineData(5.0f, 1.0f)]
	public void StartSequence_ClampsChargeGage(float chargeGage, float expected)
	{
		// Arrange
		var status = new FlasherStatusModel(false, 1.5f, FlasherColor.FlasherColorWhite);

		// Act
		status.StartSequence(chargeGage);

		// Assert
		Assert.True(status.IsSequenceActive);
		Assert.Equal(expected, status.ChargeGage, 3);
	}

	[Fact]
	public void EndSequence_DeactivatesSequenceAndClearsChargeGage()
	{
		// Arrange
		var status = CreateStartedStatus(1.0f);

		// Act
		status.EndSequence();

		// Assert
		Assert.False(status.IsSequenceActive);
		Assert.Equal(0.0f, status.ChargeGage, 3);
	}

	[Theory]
	[InlineData(0.0f, 0.0f)]
	[InlineData(0.5f, 0.75f)]
	[InlineData(1.0f, 1.5f)]
	public void CalculateAfterglowTime_IsProportionalToChargeGage(
		float chargeGage, float expected)
	{
		// Arrange
		var status = CreateStartedStatus(chargeGage);

		// Act
		float afterglowTime = status.CalculateAfterglowTime();

		// Assert
		Assert.Equal(expected, afterglowTime, 3);
	}

	[Theory]
	[InlineData(-1.0f, 0.0f)]
	[InlineData(3.0f, FlasherStatusModel.AfterglowMaxTime)]
	public void CalculateAfterglowTime_ClampsOutOfRangeChargeGage(
		float chargeGage, float expected)
	{
		// Arrange
		var status = CreateStartedStatus(chargeGage);

		// Act
		float afterglowTime = status.CalculateAfterglowTime();

		// Assert
		Assert.Equal(expected, afterglowTime, 3);
	}

	[Fact]
	public void CalculateAfterglowTime_WithoutSequence_IsZero()
	{
		// Arrange
		var status = new FlasherStatusModel(false, 1.5f, FlasherColor.FlasherColorWhite);

		// Act
		float afterglowTime = status.CalculateAfterglowTime();

		// Assert
		Assert.Equal(0.0f, afterglowTime, 3);
	}
}
