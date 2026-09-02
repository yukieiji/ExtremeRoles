using System;
using ExtremeRoles.Module.CustomOption;
using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.CustomOption.Implemented;
using ExtremeRoles.Module.PRNG;
using ExtremeRoles.Performance.Il2Cpp;
using Moq;
using UnityEngine;
using Xunit;

namespace ExtremeRoles.UnitTest.Module.PRNG;


[Collection(nameof(MockSetupHelper.SetupUnityCommonMocks))]
public sealed class RngSelectorTests
{
	private const int randCategoryKey = (int)OptionCreator.CommonOption.RandomOption;

	public RngSelectorTests()
	{
		MockSetupHelper.SetupUnityCommonMocks();
		MockSetupHelper.SetupExtremeSystemTypeManagerMock();

		var mockProcFreq = new Mock<MockSystemInfoget_processorFrequencyHelper>();
		mockProcFreq.Setup(h => h.Invoke()).Returns(3000);
		MockSystemInfoget_processorFrequencyHelper.Instance = mockProcFreq.Object;

		var mockRandomInitState = new Mock<MockRandomInitStateHelper>();
		mockRandomInitState.Setup(h => h.Invoke(It.IsAny<int>()));
		MockRandomInitStateHelper.Instance = mockRandomInitState.Object;

		var mockLobby = new Mock<LobbyBehaviour>();
		var mockLobbyHelper = new Mock<MockLobbyBehaviourget_InstanceHelper>();
		mockLobbyHelper.Setup(h => h.Invoke()).Returns(mockLobby.Object);
		MockLobbyBehaviourget_InstanceHelper.Instance = mockLobbyHelper.Object;

		var mockClient = new Mock<AmongUsClient>();
		var mockClientHelper = new Mock<MockAmongUsClientget_InstanceHelper>();
		mockClientHelper.Setup(h => h.Invoke()).Returns(mockClient.Object);
		MockAmongUsClientget_InstanceHelper.Instance = mockClientHelper.Object;

		var mockClampInt = new Mock<MockMathfClampHelper2>();
		mockClampInt.Setup(h => h.Invoke(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()))
			.Returns((int v, int min, int max) => Math.Clamp(v, min, max));
		MockMathfClampHelper2.Instance = mockClampInt.Object;

		var mockClampToInt = new Mock<MockMathfClampToIntHelper>();
		mockClampToInt.Setup(h => h.Invoke(It.IsAny<long>()))
			.Returns((long v) => (int)v);
		MockMathfClampToIntHelper.Instance = mockClampToInt.Object;

		var plugin = MockSetupHelper.SetupMockExtremeRolePlugin();
		MockSetupHelper.SetupLogger();
		MockSetupHelper.SetupDebugMode();
		MockSetupHelper.SetupMockConfig(plugin);

		EnsureRandomOptionCategory(true, 0);
	}

	private static void EnsureRandomOptionCategory(bool useStrong, int algorithm)
	{
		if (ClientOption.Instance == null || !OptionManager.Instance.TryGetCategory(OptionTab.GeneralTab, randCategoryKey, out _))
		{
			OptionCreator.Create();
		}

		if (OptionManager.Instance.TryGetCategory(OptionTab.GeneralTab, randCategoryKey, out var category))
		{
			var useStrongOpt = category.Get((int)OptionCreator.RandomOptionKey.UseStrong);
			useStrongOpt.Selection = useStrong ? 1 : 0;
			var algOpt = category.Get((int)OptionCreator.RandomOptionKey.Algorithm);
			algOpt.Selection = algorithm;
		}
	}

	[Fact]
	public void Constructor_WithStrongOption_InitializesStrongRng()
	{
		// Arrange
		EnsureRandomOptionCategory(true, 0);

		// Act
		var selector = new RngSelector();
		int val1 = selector.Instance.Next();
		int val2 = selector.Instance.Next();

		// Assert
		Assert.True(selector.IsStrong);
		Assert.IsType<Pcg32XshRr>(selector.Instance);
		Assert.NotEqual(val1, val2);
	}

	[Fact]
	public void Initialize_WhenOptionNotChanged_DoesNotChangeInstance()
	{
		// Arrange
		EnsureRandomOptionCategory(true, 0);
		var selector = new RngSelector();
		var firstInstance = selector.Instance;

		// Act
		selector.Initialize();

		// Assert
		Assert.Same(firstInstance, selector.Instance);
	}

	[Fact]
	public void Initialize_WhenSwitchedToStandardGen_ChangesInstanceToSystemRandomWrapper()
	{
		// Arrange
		EnsureRandomOptionCategory(true, 0);
		var selector = new RngSelector();

		// Act: Change UseStrong to false
		EnsureRandomOptionCategory(false, 0);
		selector.Initialize();
		int val1 = selector.Instance.Next();
		int val2 = selector.Instance.Next();

		// Assert
		Assert.False(selector.IsStrong);
		Assert.IsType<SystemRandomWrapper>(selector.Instance);
		Assert.NotEqual(val1, val2);
	}

	[Theory]
	[InlineData(0, typeof(Pcg32XshRr))]
	[InlineData(1, typeof(Pcg64RxsMXs))]
	[InlineData(2, typeof(Xorshift64))]
	[InlineData(3, typeof(Xorshift128))]
	[InlineData(4, typeof(Xorshiro256StarStar))]
	[InlineData(5, typeof(Xorshiro512StarStar))]
	[InlineData(6, typeof(RomuMono))]
	[InlineData(7, typeof(RomuTrio))]
	[InlineData(8, typeof(RomuQuad))]
	[InlineData(9, typeof(Seiran128))]
	[InlineData(10, typeof(Shioi128))]
	[InlineData(11, typeof(JFT32))]
	public void Initialize_SwitchesStrongRngAlgorithmCorrectly(int selection, Type expectedType)
	{
		// Arrange
		EnsureRandomOptionCategory(true, selection);

		// Act
		var selector = new RngSelector();
		int val1 = selector.Instance.Next();
		int val2 = selector.Instance.Next();

		// Assert
		Assert.True(selector.IsStrong);
		Assert.IsType(expectedType, selector.Instance);
		Assert.NotEqual(val1, val2);
	}
}
