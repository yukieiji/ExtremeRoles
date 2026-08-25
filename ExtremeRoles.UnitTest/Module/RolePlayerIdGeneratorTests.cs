using System;
using ExtremeRoles.Module;
using Hazel;
using Moq;
using Xunit;

namespace ExtremeRoles.UnitTest.Module;

public class RolePlayerIdGeneratorTests
{
    [Fact]
    public void Generate_SingleControlId_IncrementsInternalId()
    {
        var generator = new RolePlayerIdGenerator();

        var id0 = generator.Generate(10);
        var id1 = generator.Generate(10);
        var id2 = generator.Generate(10);

        Assert.Equal("InternalId:0  GameId:10", id0.ToString());
        Assert.Equal("InternalId:1  GameId:10", id1.ToString());
        Assert.Equal("InternalId:2  GameId:10", id2.ToString());
    }

    [Fact]
    public void Generate_MultipleControlIds_MaintainsIndependentCounts()
    {
        var generator = new RolePlayerIdGenerator();

        var idA0 = generator.Generate(100);
        var idB0 = generator.Generate(200);
        var idA1 = generator.Generate(100);
        var idB1 = generator.Generate(200);

        Assert.Equal(new RolePlayerId(0, 100), idA0);
        Assert.Equal(new RolePlayerId(0, 200), idB0);
        Assert.Equal(new RolePlayerId(1, 100), idA1);
        Assert.Equal(new RolePlayerId(1, 200), idB1);
    }

    [Fact]
    public void RolePlayerId_ToString_ReturnsExpectedFormat()
    {
        var id = new RolePlayerId(5, 42);

        Assert.Equal("InternalId:5  GameId:42", id.ToString());
    }

    [Fact]
    public void RolePlayerId_Equals_And_GetHashCode_Behavior()
    {
        var id1 = new RolePlayerId(1, 2);
        var id2 = new RolePlayerId(1, 2);
        var id3 = new RolePlayerId(1, 3);
        var id4 = new RolePlayerId(2, 2);

        Assert.True(id1.Equals(id2));
        Assert.True(id2.Equals(id1));
        Assert.Equal(id1.GetHashCode(), id2.GetHashCode());

        Assert.False(id1.Equals(id3));
        Assert.False(id1.Equals(id4));
        Assert.False(id1.Equals(null));
        Assert.False(id1.Equals("not a RolePlayerId"));
    }

    [Fact]
    public void RolePlayerId_SerializeAndDeserializeConstruct_Roundtrip()
    {
        var original = new RolePlayerId(123, 456);

        var mockWriter = new Mock<MessageWriter>(IntPtr.Zero);
        int writtenInternalId = -1;
        int writtenGameId = -1;

        mockWriter.Setup(w => w.WritePacked(It.IsAny<int>()))
            .Callback<int>(val =>
            {
                if (writtenInternalId == -1)
                {
                    writtenInternalId = val;
                }
                else
                {
                    writtenGameId = val;
                }
            });

        original.Serialize(mockWriter.Object);

        Assert.Equal(123, writtenInternalId);
        Assert.Equal(456, writtenGameId);

        var mockReader = new Mock<MessageReader>(IntPtr.Zero);
        int readCallIndex = 0;
        mockReader.Setup(r => r.ReadPackedInt32())
            .Returns(() =>
            {
                readCallIndex++;
                return readCallIndex == 1 ? writtenInternalId : writtenGameId;
            });

        var deserialized = RolePlayerId.DeserializeConstruct(mockReader.Object);

        Assert.Equal(original, deserialized);
        Assert.Equal(original.ToString(), deserialized.ToString());
        Assert.Equal(original.GetHashCode(), deserialized.GetHashCode());
    }
}
