using Xunit;

using ExtremeRoles.Roles.Solo.Impostor.Flasher;

namespace ExtremeRoles.UnitTest.Roles.Solo.Impostor.Flasher;

[Collection(nameof(MockSetupHelper.SetupUnityCommonMocks))]
public sealed class FlasherScreenEffectTests
{
	public FlasherScreenEffectTests()
	{
		MockSetupHelper.SetupUnityCommonMocks();
		MockSetupHelper.SetupMathfHelpers();
	}

	[Theory]
	[InlineData(0.0f, 0.0f, true)]
	[InlineData(0.5f, 0.0f, true)]
	[InlineData(0.0f, 1.0f, false)]
	[InlineData(0.99f, 1.0f, false)]
	[InlineData(1.0f, 1.0f, true)]
	[InlineData(1.5f, 1.0f, true)]
	public void IsAfterglowFinished_BecomesTrueWhenElapsedReachesLimit(
		float elapsed, float afterglowTime, bool expected)
	{
		// Arrange & Act
		bool finished = FlasherScreenTimeline.IsAfterglowFinished(elapsed, afterglowTime);

		// Assert
		Assert.Equal(expected, finished);
	}

	[Theory]
	[InlineData(0.0f, 1.0f, 0.6f)]
	[InlineData(0.5f, 1.0f, 0.3f)]
	[InlineData(0.25f, 1.0f, 0.45f)]
	public void GetAfterglowAlpha_DecaysLinearly(
		float elapsed, float afterglowTime, float expected)
	{
		// Arrange & Act
		float alpha = FlasherScreenTimeline.GetAfterglowAlpha(elapsed, afterglowTime);

		// Assert
		Assert.Equal(expected, alpha, 3);
	}

	[Theory]
	[InlineData(1.0f, 1.0f)]
	[InlineData(2.0f, 1.0f)]
	[InlineData(0.5f, 0.0f)]
	public void GetAfterglowAlpha_IsZeroWhenFinished(
		float elapsed, float afterglowTime)
	{
		// Arrange & Act
		float alpha = FlasherScreenTimeline.GetAfterglowAlpha(elapsed, afterglowTime);

		// Assert
		Assert.Equal(0.0f, alpha, 3);
	}

	[Fact]
	public void GetAfterglowShakeOffset_IsMaxDistanceAtStart()
	{
		// Arrange & Act
		(float X, float Y) offset =
			FlasherScreenTimeline.GetAfterglowShakeOffset(0.0f, 1.0f);

		// Assert
		Assert.Equal(FlasherScreenTimeline.MaxShakeDistance, offset.X, 3);
		Assert.Equal(0.0f, offset.Y, 3);
	}

	[Fact]
	public void GetAfterglowShakeOffset_HorizontalUsesPowerCurveAndVerticalIsScaledLinear()
	{
		// Arrange
		const float AfterglowTime = 1.0f;
		const float Elapsed = 0.5f; // progress = 0.5
		float progress = Elapsed / AfterglowTime;

		// Act
		(float X, float Y) offset =
			FlasherScreenTimeline.GetAfterglowShakeOffset(Elapsed, AfterglowTime);

		// Expected
		float expectedDecayX = 1.0f - (float)System.Math.Pow(progress, 6); // 1 - 0.5^6 = 0.984375
		float expectedDecayY = 1.0f - progress; // 0.5
		float expectedDistanceX = FlasherScreenTimeline.MaxShakeDistance * expectedDecayX;
		float expectedDistanceY = (FlasherScreenTimeline.MaxShakeDistance / FlasherScreenTimeline.VerticalShakeDivider) * expectedDecayY;

		float angle = Elapsed * 25.0f;
		float expectedX = (float)System.Math.Cos(angle) * expectedDistanceX;
		float expectedY = (float)System.Math.Sin(angle) * expectedDistanceY;

		// Assert
		Assert.Equal(expectedX, offset.X, 3);
		Assert.Equal(expectedY, offset.Y, 3);
	}

	[Fact]
	public void GetAfterglowShakeOffset_DecaysOverTime()
	{
		// Arrange
		const float AfterglowTime = 1.0f;
		(float X, float Y) start =
			FlasherScreenTimeline.GetAfterglowShakeOffset(0.0f, AfterglowTime);
		(float X, float Y) middle =
			FlasherScreenTimeline.GetAfterglowShakeOffset(AfterglowTime * 0.5f, AfterglowTime);

		// Act
		float startDistance = (start.X * start.X) + (start.Y * start.Y);
		float middleDistance = (middle.X * middle.X) + (middle.Y * middle.Y);

		// Assert
		Assert.True(middleDistance < startDistance);
		Assert.True(middleDistance > 0.0f);
	}

	[Theory]
	[InlineData(1.0f, 1.0f)]
	[InlineData(1.5f, 1.0f)]
	[InlineData(0.5f, 0.0f)]
	public void GetAfterglowShakeOffset_IsZeroWhenFinished(
		float elapsed, float afterglowTime)
	{
		// Arrange & Act
		(float X, float Y) offset =
			FlasherScreenTimeline.GetAfterglowShakeOffset(elapsed, afterglowTime);

		// Assert
		Assert.Equal(0.0f, offset.X, 3);
		Assert.Equal(0.0f, offset.Y, 3);
	}

	[Fact]
	public void StartAfterglow_WithoutStartHold_DoesNotRun()
	{
		// Arrange
		var effect = new FlasherScreenEffect();

		// Act
		effect.StartAfterglow(1.0f);

		// Assert
		Assert.False(effect.IsRunning);
	}

	[Fact]
	public void Stop_WithoutStartHold_DoesNotRun()
	{
		// Arrange
		var effect = new FlasherScreenEffect();

		// Act
		effect.Stop();

		// Assert
		Assert.False(effect.IsRunning);
	}
}
