using ExtremeRoles.GameMode.RoleSelector;
using ExtremeRoles.Module.GameResult;
using ExtremeRoles.Module.SystemType;
using ExtremeRoles.Module.SystemType.Roles;
using Hazel;
using Moq;
using Xunit;

namespace ExtremeRoles.UnitTest.Module.SystemType.Roles;

[Collection(nameof(MockSetupHelper.SetupUnityCommonMocks))]
public class LiberalMoneyBankSystemTests
{
	public LiberalMoneyBankSystemTests()
	{
		MockSetupHelper.SetupExtremeSystemTypeManagerMock();
	}

	[Fact]
	public void DeltaInfo_Properties_And_Serialization()
	{
		var option = new Mock<ILiberalOptionLoader>();
		option.Setup(o => o.GetValue<LiberalGlobalSetting, int>(LiberalGlobalSetting.WinMoney)).Returns(100);

		var system = new LiberalMoneyBankSystem(option.Object);
		Assert.Equal(100f, system.WinMoney);
		Assert.False(system.IsDirty);
		Assert.Equal(0f, system.Money);

		system.MarkClean();
		Assert.False(system.IsDirty);

		var reader = new Mock<MessageReader>();
		reader.SetupSequence(r => r.ReadByte())
			.Returns((byte)1) // PlayerId
			.Returns((byte)LiberalMoneyHistory.Reason.AddOnKill);
		reader.SetupSequence(r => r.ReadSingle())
			.Returns(50f)  // Money
			.Returns(1.5f); // Boost

		system.Deserialize(reader.Object, false);
		Assert.Equal(125f, system.Money); // (1.0 + 1.5) * 50 = 125

		var writer = new Mock<MessageWriter>(System.IntPtr.Zero);
		system.Serialize(writer.Object, true);
	}
}
