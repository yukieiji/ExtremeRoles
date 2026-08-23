using System;
using System.Reflection;
using System.Runtime.Serialization;
using ExtremeRoles.Module;
using ExtremeRoles.Module.Interface;
using ExtremeRoles.Performance.Il2Cpp;
using Hazel;
using Moq;
using UnityEngine;
using Xunit;

namespace ExtremeRoles.UnitTest.Module;

public class MeetingReporterTests : IDisposable
{
    public MeetingReporterTests()
    {
        MockSetupHelper.SetupCommonMocks();

        var mockDeltaTime = new Mock<MockTimeget_deltaTimeHelper>();
        mockDeltaTime.Setup(h => h.Invoke()).Returns(0.1f);
        MockTimeget_deltaTimeHelper.Instance = mockDeltaTime.Object;

        MeetingReporter.Reset();
    }

    public void Dispose()
    {
        MeetingReporter.Reset();
        MockSetupHelper.SetupCommonMocks();
    }

    [Fact]
    public void Singleton_InstanceAndReset_WorksCorrectly()
    {
        Assert.False(MeetingReporter.IsExist);

        var instance = MeetingReporter.Instance;
        Assert.NotNull(instance);
        Assert.True(MeetingReporter.IsExist);

        MeetingReporter.Reset();
        Assert.False(MeetingReporter.IsExist);
    }

    [Fact]
    public void AddMeetingStartReport_AddsReportAndIgnoresDuplicates()
    {
        var reporter = MeetingReporter.Instance;

        reporter.AddMeetingStartReport("Report 1");
        reporter.AddMeetingStartReport("Report 2");
        reporter.AddMeetingStartReport("Report 1"); // Duplicate, should be ignored

        string startReport = reporter.GetMeetingStartReport();

        Assert.Contains("Report 1", startReport);
        Assert.Contains("Report 2", startReport);

        int firstIdx = startReport.IndexOf("Report 1");
        int lastIdx = startReport.LastIndexOf("Report 1");
        Assert.Equal(firstIdx, lastIdx);
    }

    [Fact]
    public void AddMeetingEndReport_AddsReportCorrectly()
    {
        var reporter = MeetingReporter.Instance;

        reporter.AddMeetingEndReport("End Report A");
        reporter.AddMeetingEndReport("End Report B");

        string endReport = reporter.GetMeetingEndReport();

        Assert.Contains("End Report A", endReport);
        Assert.Contains("End Report B", endReport);
    }

    [Fact]
    public void AddMeetingChatReport_String_EnqueuesAndUpdatesHasChatReport()
    {
        var reporter = MeetingReporter.Instance;

        Assert.False(reporter.HasChatReport);

        reporter.AddMeetingChatReport("Chat Message 1");

        Assert.True(reporter.HasChatReport);
    }

    [Fact]
    public void AddMeetingChatReport_Serializer_EnqueuesAndUpdatesHasChatReport()
    {
        var reporter = MeetingReporter.Instance;
        var mockSerializer = new Mock<IStringSerializer>();

        Assert.False(reporter.HasChatReport);

        reporter.AddMeetingChatReport(mockSerializer.Object);

        Assert.True(reporter.HasChatReport);
    }

    [Fact]
    public void ReportMeetingChat_WhenQueueIsEmpty_DoesNotThrow()
    {
        var reporter = MeetingReporter.Instance;

        Assert.False(reporter.HasChatReport);

        // Should return without doing anything
        reporter.ReportMeetingChat();

        Assert.False(reporter.HasChatReport);
    }

    [Fact]
    public void ReportMeetingChat_WhenWaitTimerPositive_DecrementsTimerWithoutDequeuing()
    {
        var reporter = MeetingReporter.Instance;

        reporter.AddMeetingChatReport("Msg 1");

        // Set waitTimer to 2.0f via reflection
        var waitTimerField = typeof(MeetingReporter).GetField("waitTimer", BindingFlags.NonPublic | BindingFlags.Instance);
        waitTimerField!.SetValue(reporter, 2.0f);

        // Calling ReportMeetingChat should decrement waitTimer (by 0.1f) and return early
        reporter.ReportMeetingChat();

        float newWaitTimer = (float)waitTimerField.GetValue(reporter)!;
        Assert.Equal(1.9f, newWaitTimer, 2);
        Assert.True(reporter.HasChatReport);
    }

    [Fact]
    public void RpcAddTargetMeetingChatReport_LocalPlayer_AddsChatDirectly()
    {
        byte localId = 0; // PlayerId field on PlayerControl defaults to 0
        var mockPlayer = new Mock<PlayerControl>();

        var mockLocalHelper = new Mock<MockPlayerControlget_LocalPlayerHelper>();
        mockLocalHelper.Setup(h => h.Invoke()).Returns(mockPlayer.Object);
        MockPlayerControlget_LocalPlayerHelper.Instance = mockLocalHelper.Object;

        var mockSerializer = new Mock<IStringSerializer>();

        MeetingReporter.RpcAddTargetMeetingChatReport(localId, mockSerializer.Object);

        Assert.True(MeetingReporter.Instance.HasChatReport);
    }

    [Fact]
    public void RpcOp_ChatSerializeDeserialize_EnqueuesReport()
    {
        var mockReader = new Mock<MessageReader>();
        int readByteCall = 0;
        mockReader.Setup(r => r.ReadByte()).Returns(() =>
        {
            readByteCall++;
            if (readByteCall == 1)
            {
                return (byte)MeetingReporter.RpcOpType.ChatSerializeDeserialize;
            }
            if (readByteCall == 2)
            {
                return (byte)StringSerializerType.ShutterPhoto;
            }
            return (byte)0;
        });
        mockReader.Setup(r => r.ReadPackedInt32()).Returns(0);
        mockReader.Setup(r => r.ReadUInt64()).Returns(0UL);

        var readerObj = mockReader.Object;
        MeetingReporter.RpcOp(ref readerObj);

        Assert.True(MeetingReporter.Instance.HasChatReport);
    }

    [Fact]
    public void RpcOp_TargetChatReport_WhenTargetIsLocalPlayer_EnqueuesReport()
    {
        byte localId = 0;
        var mockPlayer = new Mock<PlayerControl>();

        var mockLocalHelper = new Mock<MockPlayerControlget_LocalPlayerHelper>();
        mockLocalHelper.Setup(h => h.Invoke()).Returns(mockPlayer.Object);
        MockPlayerControlget_LocalPlayerHelper.Instance = mockLocalHelper.Object;

        var mockReader = new Mock<MessageReader>();
        int readByteCall = 0;
        mockReader.Setup(r => r.ReadByte()).Returns(() =>
        {
            readByteCall++;
            if (readByteCall == 1)
            {
                return (byte)MeetingReporter.RpcOpType.TargetChatReport; // 1st ReadByte in RpcOp
            }
            if (readByteCall == 2)
            {
                return (byte)StringSerializerType.ShutterPhoto; // 1st ReadByte in DeserializeStatic
            }
            if (readByteCall == 3)
            {
                return (byte)0; // ReadByte for indexer in PhotoNameGenerator.Deserialize
            }
            if (readByteCall == 4)
            {
                return localId; // 2nd ReadByte in RpcOp (targetPlayer = 0)
            }
            return (byte)0;
        });
        mockReader.Setup(r => r.ReadPackedInt32()).Returns(0);
        mockReader.Setup(r => r.ReadUInt64()).Returns(0UL);

        var readerObj = mockReader.Object;
        MeetingReporter.RpcOp(ref readerObj);

        Assert.True(MeetingReporter.Instance.HasChatReport);
    }

    [Fact]
    public void RpcOp_TargetChatReport_WhenTargetIsNotLocalPlayer_DoesNotEnqueue()
    {
        byte targetId = 7;
        var mockPlayer = new Mock<PlayerControl>();

        var mockLocalHelper = new Mock<MockPlayerControlget_LocalPlayerHelper>();
        mockLocalHelper.Setup(h => h.Invoke()).Returns(mockPlayer.Object);
        MockPlayerControlget_LocalPlayerHelper.Instance = mockLocalHelper.Object;

        var mockReader = new Mock<MessageReader>();
        int readByteCall = 0;
        mockReader.Setup(r => r.ReadByte()).Returns(() =>
        {
            readByteCall++;
            if (readByteCall == 1)
            {
                return (byte)MeetingReporter.RpcOpType.TargetChatReport; // 1st ReadByte in RpcOp
            }
            if (readByteCall == 2)
            {
                return (byte)StringSerializerType.ShutterPhoto; // 1st ReadByte in DeserializeStatic
            }
            if (readByteCall == 3)
            {
                return (byte)0; // ReadByte for indexer in PhotoNameGenerator.Deserialize
            }
            if (readByteCall == 4)
            {
                return targetId; // 2nd ReadByte in RpcOp (targetPlayer: 7 != 0)
            }
            return (byte)0;
        });
        mockReader.Setup(r => r.ReadPackedInt32()).Returns(0);
        mockReader.Setup(r => r.ReadUInt64()).Returns(0UL);

        var readerObj = mockReader.Object;
        MeetingReporter.RpcOp(ref readerObj);

        Assert.False(MeetingReporter.Instance.HasChatReport);
    }

    [Fact]
    public void RpcOp_InvalidOpType_DoesNotEnqueueReport()
    {
        var mockReader = new Mock<MessageReader>();
        int readByteCall = 0;
        mockReader.Setup(r => r.ReadByte()).Returns(() =>
        {
            readByteCall++;
            if (readByteCall == 1)
            {
                return (byte)255; // Invalid OpType
            }
            if (readByteCall == 2)
            {
                return (byte)StringSerializerType.ShutterPhoto;
            }
            return (byte)0;
        });
        mockReader.Setup(r => r.ReadPackedInt32()).Returns(0);
        mockReader.Setup(r => r.ReadUInt64()).Returns(0UL);

        var readerObj = mockReader.Object;
        MeetingReporter.RpcOp(ref readerObj);

        Assert.False(MeetingReporter.Instance.HasChatReport);
    }
}
