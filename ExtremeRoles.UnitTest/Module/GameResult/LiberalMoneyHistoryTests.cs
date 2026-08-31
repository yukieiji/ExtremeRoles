using System;
using System.Collections.Generic;
using ExtremeRoles.Module.CustomMonoBehaviour;
using ExtremeRoles.Module.GameResult;
using Moq;
using Xunit;

using PlayerStatus = ExtremeRoles.Module.ExtremeShipStatus.ExtremeShipStatus.PlayerStatus;

namespace ExtremeRoles.UnitTest.Module.GameResult;


[Collection(nameof(MockSetupHelper.SetupUnityCommonMocks))]
public class LiberalMoneyHistoryTests
{
    public LiberalMoneyHistoryTests()
    {
        MockSetupHelper.SetupUnityCommonMocks();
        var mockTranslation = MockSetupHelper.SetupDestroyableSingletonMock<TranslationController>();
        mockTranslation.Setup(t => t.GetString(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppReferenceArray<Il2CppSystem.Object>>()))
            .Returns((string id, string defaultStr, Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppReferenceArray<Il2CppSystem.Object> parts) => defaultStr ?? id);
    }

    [Fact]
    public void Add_And_SummaryBuilder_Build_AppendsLine()
    {
        LiberalMoneyHistory.Add(new LiberalMoneyHistory.MoneyHistory(LiberalMoneyHistory.Reason.AddOnKill, 1, 10f));

        var textBuilder = new SummaryTextBuilder("testHeader");

        var summaries = new Dictionary<byte, FinalSummary.PlayerSummary>
        {
            [1] = new FinalSummary.PlayerSummary(1, "Player1", null!, null!, 5, 5, PlayerStatus.Alive)
        };

        using (var builder = LiberalMoneyHistory.CreateBuiler(textBuilder))
        {
            builder.Build(summaries);
        }

        string result = textBuilder.ToString();
        Assert.Contains("Player1", result);
        Assert.Contains("+10", result);
    }
}
