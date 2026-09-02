using System.Text.Json;
using System.Text.Json.Serialization;
using ExtremeRoles.Helper;
using Xunit;

namespace ExtremeRoles.UnitTest.Helper;

public class JsonParserTests
{
    public class SampleData
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }
    }

    [Fact]
    public void LoadJsonStructFromAssembly_WithInvalidResourcePath_ShouldReturnDefault()
    {
        var result = JsonParser.LoadJsonStructFromAssembly<SampleData>("Invalid.Resource.Path.json");

        Assert.Null(result);
    }

    [Fact]
    public void LoadJsonStructFromAssembly_WithCustomOptionsAndInvalidPath_ShouldReturnDefault()
    {
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        var result = JsonParser.LoadJsonStructFromAssembly<SampleData>("Invalid.Resource.Path.json", options);

        Assert.Null(result);
    }

    [Fact]
    public void GetJObjectFromAssembly_WithInvalidResourcePath_ShouldReturnNull()
    {
        var result = JsonParser.GetJObjectFromAssembly("Invalid.Resource.Path.json");

        Assert.Null(result);
    }
}
