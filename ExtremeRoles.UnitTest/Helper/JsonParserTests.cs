using ExtremeRoles.Helper;
using Xunit;

namespace ExtremeRoles.UnitTest.Helper;

public class JsonParserTests
{
    [Fact]
    public void LoadJsonStructFromAssembly_WithInvalidPath_ShouldReturnDefault()
    {
        var result = JsonParser.LoadJsonStructFromAssembly<string>("NonExistentResourcePath.json");
        Assert.Null(result);
    }
}
