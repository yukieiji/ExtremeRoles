using ExtremeRoles.Roles.Solo.Impostor.RemoteKiller;
using Xunit;

namespace ExtremeRoles.UnitTest.Roles.Solo.Impostor;

public sealed class RemoteKillerStatusModelTests
{
	[Fact]
	public void Constructor_SetsOptionPropertiesCorrectly()
	{
		// Arrange & Act
		var status = new RemoteKillerStatusModel(1.5f, 5);

		// Assert
		Assert.Equal(1.5f, status.RobRange);
		Assert.Equal(5, status.ContactPlayerCount);
		Assert.True(status.CanMove);
		Assert.False(status.IsPurging);
	}

	[Fact]
	public void SetPurging_UpdatesIsPurgingProperty()
	{
		// Arrange
		var status = new RemoteKillerStatusModel(1.0f, 1);

		// Act
		status.SetPurging(true);

		// Assert
		Assert.True(status.IsPurging);

		// Act
		status.SetPurging(false);

		// Assert
		Assert.False(status.IsPurging);
	}

	[Fact]
	public void ExecutionTargets_AddAndRemove_WorksAsExpected()
	{
		// Arrange
		var status = new RemoteKillerStatusModel(1.0f, 1);
		byte targetId = 2;
		byte rolePlayerId = 1;

		// Act: Add
		status.AddExecutionTarget(targetId, rolePlayerId);

		// Assert
		Assert.True(status.HasExecutionTarget(targetId));
		Assert.Contains(targetId, status.ExecutionTargets);

		// Act: RemoveExecutionTarget
		status.RemoveExecutionTarget(targetId);

		// Assert
		Assert.False(status.HasExecutionTarget(targetId));
		Assert.DoesNotContain(targetId, status.ExecutionTargets);
	}

	[Fact]
	public void RecordContactAndClear_ManagesContactsCorrectly()
	{
		// Arrange
		var status = new RemoteKillerStatusModel(1.0f, 1);
		byte targetId = 2;
		byte contactId = 3;

		// Act: Record Contact
		status.RecordContact(targetId, contactId);

		// Assert
		Assert.True(status.TaskPhaseContacts.TryGetValue(targetId, out var contacts));
		Assert.Contains(contactId, contacts);

		// Act: Clear
		status.ClearTaskPhaseContacts();

		// Assert
		Assert.Empty(status.TaskPhaseContacts);
	}

	[Fact]
	public void Reset_ResetsAllFieldsToInitialState()
	{
		// Arrange
		var status = new RemoteKillerStatusModel(1.0f, 1);
		status.SetPurging(true);
		status.CanMove = false;
		status.AddExecutionTarget(2, 1);
		status.RecordContact(2, 3);

		// Act
		status.Reset();

		// Assert
		Assert.True(status.CanMove);
		Assert.False(status.IsPurging);
		Assert.Empty(status.ExecutionTargets);
		Assert.Empty(status.TaskPhaseContacts);
	}
}
