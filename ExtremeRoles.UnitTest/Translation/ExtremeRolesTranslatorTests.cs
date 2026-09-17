using ExtremeRoles.Translation;
using Xunit;

namespace ExtremeRoles.UnitTest.Translation;

public class ExtremeRolesTranslatorTests
{
    [Fact]
    public void Translator_IsSupport_ReturnsTrueForSupportedLanguages()
    {
        var translator = new Translator();

        Assert.True(translator.IsSupport(SupportedLangs.English));
        Assert.True(translator.IsSupport(SupportedLangs.French));
        Assert.True(translator.IsSupport(SupportedLangs.Japanese));
        Assert.True(translator.IsSupport(SupportedLangs.SChinese));
        Assert.True(translator.IsSupport(SupportedLangs.TChinese));
    }

    [Fact]
    public void Translator_IsSupport_ReturnsFalseForUnsupportedLanguages()
    {
        var translator = new Translator();

        Assert.False(translator.IsSupport(SupportedLangs.German));
        Assert.False(translator.IsSupport(SupportedLangs.Spanish));
        Assert.False(translator.IsSupport(SupportedLangs.Russian));
    }
}
