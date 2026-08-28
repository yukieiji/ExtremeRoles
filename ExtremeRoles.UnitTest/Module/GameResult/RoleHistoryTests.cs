using ExtremeRoles.UnitTest.Mocks;
using System;
using System.Collections.Generic;
using ExtremeRoles.Module.CustomMonoBehaviour;
using ExtremeRoles.Module.GameResult;
using Moq;
using Xunit;

using PlayerStatus = ExtremeRoles.Module.ExtremeShipStatus.ExtremeShipStatus.PlayerStatus;

namespace ExtremeRoles.UnitTest.Module.GameResult;

public class RoleHistoryTests : SerialTestBase, IClassFixture<SerialFixture>, IClassFixture<UnityCommonMock>
{
    public RoleHistoryTests(SerialFixture fixture, UnityCommonMock unityCommonMock)
        : base(fixture, unityCommonMock.OperatorsMock, unityCommonMock.Vector2Mock, unityCommonMock.ColorMock, unityCommonMock.MathfMock, unityCommonMock.PaletteMock, unityCommonMock.GameOptionsManagerMock, unityCommonMock.CompatModManagerMock, unityCommonMock.TimeMock)
    {
        var mockTranslation = MockSetupHelper.SetupDestroyableSingletonMock<TranslationController>();
        mockTranslation.Setup(t => t.GetString(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppReferenceArray<Il2CppSystem.Object>>()))
            .Returns((string id, string defaultStr, Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppReferenceArray<Il2CppSystem.Object> parts) => defaultStr ?? id);
    }

    [Fact]
    public void RoleHistoryContainer_Add_And_SummaryBuilder_Build_AppendsHistory()
    {
        var textBuilder = new SummaryTextBuilder("testHeader");

        var hist = new RoleHistory(1, "Imp", "Crewmate", "Sheriff");
        RoleHistoryContainer.Add(2, hist);

        var summaries = new Dictionary<byte, FinalSummary.PlayerSummary>
        {
            [1] = new FinalSummary.PlayerSummary(1, "CausePlayer", null!, null!, 5, 5, PlayerStatus.Alive),
            [2] = new FinalSummary.PlayerSummary(2, "TargetPlayer", null!, null!, 5, 5, PlayerStatus.Alive)
        };

        using (var builder = RoleHistoryContainer.CreateBuiler(textBuilder))
        {
            builder.Build(summaries);
        }

        string result = textBuilder.ToString();
        Assert.Contains("TargetPlayer", result);
        Assert.Contains("Crewmate => Sheriff", result);
    }
}