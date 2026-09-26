using ExtremeRoles.Roles.Solo.Impostor.RemoteKiller;
using Xunit;

namespace ExtremeRoles.UnitTest.Roles.Solo.Impostor;

public sealed class RemoteKillerStatusModelTests
{
	[Fact]
	public void Constructor_SetsOptionPropertiesCorrectly()
	{
		// Arrange & Act
		var status = new RemoteKillerStatusModel(1.5f, 3.0f, 5, 4.0f);

		// Assert
		Assert.Equal(1.5f, status.RobRange);
		Assert.Equal(3.0f, status.RobActiveTime);
		Assert.Equal(5, status.ContactPlayerCount);
		Assert.Equal(4.0f, status.PurgeTime);
		Assert.True(status.CanMove);
		Assert.False(status.IsPurging);
	}

	[Fact]
	public void SetPurging_UpdatesIsPurgingProperty()
	{
		// Arrange
		var status = new RemoteKillerStatusModel(1.0f, 2.0f, 1, 5.0f);

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
		var status = new RemoteKillerStatusModel(1.0f, 2.0f, 1, 5.0f);
		byte targetId = 2;

		// Act: Add
		status.AddExecutionTarget(targetId);

		// Assert
		Assert.True(status.HasExecutionTarget(targetId));
		Assert.Contains(targetId, status.ExecutionTargets);
		Assert.True(status.IsPendingReport(targetId));

		// Act: RemovePendingReport
		status.RemovePendingReport(targetId);

		// Assert
		Assert.False(status.IsPendingReport(targetId));
		Assert.True(status.HasExecutionTarget(targetId));

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
		var status = new RemoteKillerStatusModel(1.0f, 2.0f, 1, 5.0f);
		byte targetId = 2;
		byte contactId = 3;

		// Act: Record Contact
		status.RecordContact(targetId, contactId);

		// Assert
		Assert.True(status.TaskPhaseContacts.TryGetValue(targetId, out var contacts));
		Assert.Contains(contactId, contacts);

		// Act: Clear
		status.ClearTaskPhaseContacts(targetId);

		// Assert
		Assert.Empty(status.TaskPhaseContacts[targetId]);
	}

	[Fact]
	public void Reset_ResetsAllFieldsToInitialState()
	{
		// Arrange
		var status = new RemoteKillerStatusModel(1.0f, 2.0f, 1, 5.0f);
		status.SetPurging(true);
		status.CanMove = false;
		status.AddExecutionTarget(2);
		status.RecordContact(2, 3);

		// Act
		status.Reset();

		// Assert
		Assert.True(status.CanMove);
		Assert.False(status.IsPurging);
		Assert.Empty(status.ExecutionTargets);
		Assert.Empty(status.PendingReports);
		Assert.Empty(status.TaskPhaseContacts);
	}
}
