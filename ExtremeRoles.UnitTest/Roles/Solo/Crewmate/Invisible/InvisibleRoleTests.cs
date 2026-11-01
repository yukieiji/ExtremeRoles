using System;
using ExtremeRoles.Module;
using ExtremeRoles.Module.CustomOption;
using ExtremeRoles.Module.SystemType;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.Solo.Crewmate.Invisible;
using Hazel;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Moq;
using Xunit;

namespace ExtremeRoles.UnitTest.Roles.Solo.Crewmate.Invisible;

[Collection(nameof(MockSetupHelper.SetupUnityCommonMocks))]
public sealed class InvisibleRoleTests
{
	public InvisibleRoleTests()
	{
		MockSetupHelper.SetupUnityCommonMocks();
		var plugin = MockSetupHelper.SetupMockExtremeRolePlugin();
		MockSetupHelper.SetupMockConfig(plugin);

		SetupTranslationControllerMock();
		MockSetupHelper.SetupExtremeSystemTypeManagerMock();
		MockSetupHelper.SetupOptionManager();

		var mockLocalPlayer = MockSetupHelper.SetupPlayerControlMocks();
		mockLocalPlayer.SetupGet(p => p.PlayerId).Returns((byte)1);
		mockLocalPlayer.SetupGet(p => p.NetId).Returns(100);
		mockLocalPlayer.SetupGet(p => p.CanMove).Returns(true);
		var mockData = new Mock<NetworkedPlayerInfo>(IntPtr.Zero);
		mockData.SetupGet(d => d.IsDead).Returns(false);
		mockData.SetupGet(d => d.Disconnected).Returns(false);
		mockData.SetupGet(d => d.Object).Returns(mockLocalPlayer.Object);
		mockLocalPlayer.SetupGet(p => p.Data).Returns(mockData.Object);

		var mockWriter = new Mock<Hazel.MessageWriter>(IntPtr.Zero);
		var mockClient = MockSetupHelper.SetupAmongUsClientMock();
		mockClient.SetupGet(c => c.AmHost).Returns(true);
		mockClient.Setup(c => c.StartRpcImmediately(It.IsAny<uint>(), It.IsAny<byte>(), It.IsAny<Hazel.SendOption>(), It.IsAny<int>()))
			.Returns(mockWriter.Object);
		mockWriter.Setup(w => w.Write(It.IsAny<Hazel.MessageWriter>(), It.IsAny<bool>()));
		mockWriter.Setup(w => w.ToByteArray(It.IsAny<bool>())).Returns((Il2CppStructArray<byte>)null!);

		var mockWriterGet = new Mock<Hazel.MockMessageWriterGetHelper>();
		mockWriterGet.Setup(h => h.Invoke(It.IsAny<Hazel.SendOption>())).Returns(mockWriter.Object);
		Hazel.MockMessageWriterGetHelper.Instance = mockWriterGet.Object;

		var mockWriteNetObj = new Mock<InnerNet.MockMessageExtensionsWriteNetObjectHelper>();
		mockWriteNetObj.Setup(w => w.Invoke(It.IsAny<Hazel.MessageWriter>(), It.IsAny<InnerNet.InnerNetObject>()));
		InnerNet.MockMessageExtensionsWriteNetObjectHelper.Instance = mockWriteNetObj.Object;

		var mockReaderHelper = new Mock<Hazel.MockMessageReaderGetHelper>();
		mockReaderHelper.Setup(g => g.Invoke(It.IsAny<Il2CppStructArray<byte>>()))
			.Returns(() =>
			{
				var mockReader = new Mock<Hazel.MessageReader>();
				mockReader.SetupSequence(r => r.ReadByte())
					.Returns((byte)ExtremeSystemType.InvisibleSystem)
					.Returns((byte)1);
				mockReader.Setup(r => r.ReadBoolean()).Returns(true);
				return mockReader.Object;
			});
		Hazel.MockMessageReaderGetHelper.Instance = mockReaderHelper.Object;
	}

	private static void SetupTranslationControllerMock()
	{
		var mockTranslation = MockSetupHelper.SetupDestroyableSingletonMock<TranslationController>();
		mockTranslation.Setup(t => t.GetString(It.IsAny<StringNames>())).Returns("TestText");
		mockTranslation.Setup(t => t.GetString(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Il2CppReferenceArray<Il2CppSystem.Object>>())).Returns("TestText");
		mockTranslation.Setup(t => t.GetString(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Il2CppSystem.Object[]>())).Returns("TestText");
		mockTranslation.Setup(t => t.GetString(It.IsAny<StringNames>(), It.IsAny<Il2CppReferenceArray<Il2CppSystem.Object>>())).Returns("TestText");
	}

	[Fact]
	public void Constructor_SetsRoleProperties()
	{
		// Act
		var role = new InvisibleRole();

		// Assert
		Assert.Equal(ExtremeRoleId.Invisible, role.Core.Id);
		Assert.Equal(ColorPalette.InvisibleWhite, role.Core.Color);
	}

	[Fact]
	public void CreateRoleAllOption_CreatesOptions()
	{
		// Arrange
		var role = new InvisibleRole();

		// Act
		role.CreateRoleAllOption();

		// Assert
		int groupId = ExtremeRoleManager.GetRoleGroupId(ExtremeRoleId.Invisible);
		Assert.True(OptionManager.Instance.TryGetCategory(OptionTab.CrewmateTab, groupId, out var category));
		Assert.NotNull(category);
	}

	[Fact]
	public void Initialize_ConfiguresStatusAndAbilityClass()
	{
		// Arrange
		var role = new InvisibleRole();
		role.CreateRoleAllOption();

		// Act
		role.Initialize();

		// Assert
		Assert.NotNull(role.Status);
		Assert.IsType<InvisibleStatusModel>(role.Status);
		Assert.NotNull(role.AbilityClass);
		Assert.IsType<InvisibleAbilityHandler>(role.AbilityClass);
	}

	[Fact]
	public void IsAbilityUse_ReturnsTrue()
	{
		// Arrange
		var role = new InvisibleRole();

		// Act
		bool canUse = role.IsAbilityUse();

		// Assert
		Assert.True(canUse);
	}

	[Fact]
	public void UseAbility_And_CleanUp_ExecutesWithoutError()
	{
		// Arrange
		var role = new InvisibleRole();
		role.CreateRoleAllOption();
		role.Initialize();

		// Act
		bool used = role.UseAbility();

		// Assert
		Assert.True(used);

		// CleanUp Act
		role.CleanUp();
	}

	[Fact]
	public void ResetOnMeetingStart_And_End_ExecutesWithoutError()
	{
		// Arrange
		var role = new InvisibleRole();
		role.CreateRoleAllOption();
		role.Initialize();
		role.UseAbility();

		// Act
		role.ResetOnMeetingStart();
		role.ResetOnMeetingEnd(null);

		// Assert
		Assert.NotNull(role);
	}

	[Fact]
	public void AllReset_ExecutesWithoutError()
	{
		// Arrange
		var role = new InvisibleRole();
		role.CreateRoleAllOption();
		role.Initialize();
		var mockPlayer = MockSetupHelper.SetupPlayerControlMocks();

		// Act
		role.AllReset(mockPlayer.Object);

		// Assert
		Assert.NotNull(role);
	}
}
