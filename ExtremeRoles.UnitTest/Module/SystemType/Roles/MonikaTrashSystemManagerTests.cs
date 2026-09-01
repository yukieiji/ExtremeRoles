using ExtremeRoles.Module.SystemType;
using ExtremeRoles.Module.SystemType.Roles;
using Hazel;
using Moq;
using Xunit;

namespace ExtremeRoles.UnitTest.Module.SystemType.Roles;

[Collection(nameof(MockSetupHelper.SetupUnityCommonMocks))]
public class MonikaTrashSystemManagerTests
{
	public MonikaTrashSystemManagerTests()
	{
		MockSetupHelper.SetupExtremeSystemTypeManagerMock();
	}

	[Fact]
	public void BasicMethods_InvalidPlayer_MarkClean_Serialize_Deserialize()
	{
		var system = new MonikaTrashSystem(true);
		Assert.False(system.IsDirty);
		Assert.False(system.InvalidPlayer(1));

		system.MarkClean();
		Assert.False(system.IsDirty);

		var reader = new Mock<MessageReader>();
		reader.Setup(r => r.ReadPackedInt32()).Returns(1);
		reader.Setup(r => r.ReadByte()).Returns((byte)2);

		system.Deserialize(reader.Object, false);
		Assert.True(system.InvalidPlayer(2));

		var writer = new Mock<MessageWriter>(System.IntPtr.Zero);
		system.Serialize(writer.Object, true);
		writer.Verify(w => w.WritePacked(1), Times.Once);
		writer.Verify(w => w.Write(2), Times.Once);
	}

	[Fact]
	public void UpdateSystem_AddTrash_And_ClearTrash()
	{
		var system = new MonikaTrashSystem(true);

		// AddTrash
		var rAdd = new Mock<MessageReader>();
		rAdd.SetupSequence(r => r.ReadByte())
			.Returns((byte)MonikaTrashSystem.Ops.AddTrash)
			.Returns((byte)5);
		system.UpdateSystem(null!, rAdd.Object);
		Assert.True(system.InvalidPlayer(5));

		// ClearTrash
		var rClear = new Mock<MessageReader>();
		rClear.SetupSequence(r => r.ReadByte())
			.Returns((byte)MonikaTrashSystem.Ops.ClearTrash);
		system.UpdateSystem(null!, rClear.Object);
		Assert.False(system.InvalidPlayer(5));
	}
}
