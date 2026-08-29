using System.Reflection;
using ExtremeRoles.GameMode;
using ExtremeRoles.Module.GameEnd;
using ExtremeRoles.Module.Interface;
using ExtremeRoles.UnitTest.Mocks;
using Moq;
using Xunit;

namespace ExtremeRoles.UnitTest.Module.GameEnd;

public sealed class ExtremeGameEndCheckerTests : SerialTestBase
{
    public ExtremeGameEndCheckerTests(SerialFixture fixture)
        : base(fixture, new ShipStatusMock(), new AmongUsClientMock())
    {
        SetupMocks();
    }

    private static void SetupMocks()
    {
        var mockData = new Mock<GameData>();
        var mockDataHelper = new Mock<MockGameDataget_InstanceHelper>();
        mockDataHelper.Setup(h => h.Invoke()).Returns(mockData.Object);
        MockGameDataget_InstanceHelper.Instance = mockDataHelper.Object;

        if (ExtremeGameModeManager.Instance == null)
        {
            ExtremeGameModeManager.Create(AmongUs.GameOptions.GameModes.Normal);
        }

        MockSetupHelper.SetupExtremeSystemTypeManagerMock();
    }

    [Fact]
    public void Constructor_ConstructsSuccessfullyWithRealCheckers()
    {
        ExtremeGameEndChecker checker = new ExtremeGameEndChecker();

        FieldInfo? checkersField = typeof(ExtremeGameEndChecker).GetField("checkers", BindingFlags.NonPublic | BindingFlags.Instance);
        var checkers = checkersField?.GetValue(checker) as System.Collections.Generic.IReadOnlyList<IGameEndChecker>;

        Assert.NotNull(checkers);
        Assert.NotEmpty(checkers);
    }
}
