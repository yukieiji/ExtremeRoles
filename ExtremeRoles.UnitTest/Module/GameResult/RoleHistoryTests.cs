using System;
using System.Collections.Generic;
using ExtremeRoles.Module.CustomMonoBehaviour;
using ExtremeRoles.Module.GameResult;
using Moq;
using Xunit;

using PlayerStatus = ExtremeRoles.Module.ExtremeShipStatus.ExtremeShipStatus.PlayerStatus;

namespace ExtremeRoles.UnitTest.Module.GameResult;

[Collection("UnityMock")]
public class RoleHistoryTests
{
    public RoleHistoryTests()
    {
        MockSetupHelper.SetupCommonMocks();
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
