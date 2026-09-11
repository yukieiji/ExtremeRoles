using System.Reflection;
using ExtremeRoles.GameMode;
using ExtremeRoles.Module.GameEnd;
using ExtremeRoles.Module.Interface;
using ExtremeRoles.Module.SystemType;
using Moq;
using Xunit;

namespace ExtremeRoles.UnitTest.Module.GameEnd;

[Collection(nameof(MockSetupHelper.SetupUnityCommonMocks))]
public sealed class ExtremeGameEndCheckerTests
{
    public ExtremeGameEndCheckerTests()
    {
        MockSetupHelper.SetupUnityCommonMocks();
        SetupMocks();
    }

    private static void SetupMocks()
    {
        var mockShipStatus = new Mock<ShipStatus>();
        var mockShipHelper = new Mock<MockShipStatusget_InstanceHelper>();
        mockShipHelper.Setup(h => h.Invoke()).Returns(mockShipStatus.Object);
        MockShipStatusget_InstanceHelper.Instance = mockShipHelper.Object;

        var dict = new Mock<Il2CppSystem.Collections.Generic.Dictionary<SystemTypes, ISystemType>>(System.IntPtr.Zero);
        mockShipStatus.SetupGet(s => s.Systems).Returns(dict.Object);

        var mockData = new Mock<GameData>();
        var mockDataHelper = new Mock<MockGameDataget_InstanceHelper>();
        mockDataHelper.Setup(h => h.Invoke()).Returns(mockData.Object);
        MockGameDataget_InstanceHelper.Instance = mockDataHelper.Object;

        if (ExtremeGameModeManager.Instance == null)
        {
            ExtremeGameModeManager.Create(AmongUs.GameOptions.GameModes.Normal);
        }

        if (ExtremeSystemTypeManager.Instance == null)
        {
            var manager = (ExtremeSystemTypeManager)System.Runtime.CompilerServices.RuntimeHelpers.GetUninitializedObject(typeof(ExtremeSystemTypeManager));
            FieldInfo? field = typeof(ExtremeSystemTypeManager).GetField("instance", BindingFlags.NonPublic | BindingFlags.Static);
            field?.SetValue(null, manager);
        }
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
