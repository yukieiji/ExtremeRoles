using System;
using ExtremeRoles.Compat;
using ExtremeRoles.Module.JsonData;
using Xunit;

namespace ExtremeRoles.UnitTest.Compat;

public class CompatModInfoTests
{
    [Fact]
    public void CompatModInfo_PropertiesSetCorrectly()
    {
        var info = new CompatModInfo(
            "TestMod",
            "com.test.mod",
            "https://github.com/test/mod",
            true,
            typeof(string)
        );

        Assert.Equal("TestMod", info.Name);
        Assert.Equal("com.test.mod", info.Guid);
        Assert.Equal("https://github.com/test/mod", info.RepoUrl);
        Assert.True(info.IsRequireReactor);
        Assert.Equal(typeof(string), info.ModIntegratorType);
    }
}

public class CompatModRepoDataTests
{
    [Fact]
    public void GetDownloadUrl_ReturnsMatchingAssetUrl()
    {
        var asset1 = new GitHubAsset(
            "application/x-zip-compressed",
            "https://github.com/test/mod/releases/download/v1.0.0/test.zip"
        );
        var asset2 = new GitHubAsset(
            "application/octet-stream",
            "https://github.com/test/mod/releases/download/v1.0.0/test.dll"
        );

        var releaseData = new GitHubReleaseData(
            "v1.0.0",
            new[] { asset1, asset2 }
        );

        var repoData = new CompatModRepoData(releaseData, "test.dll");

        Assert.Equal("https://github.com/test/mod/releases/download/v1.0.0/test.dll", repoData.GetDownloadUrl());
    }

    [Fact]
    public void GetDownloadUrl_ReturnsEmptyStringWhenNoMatch()
    {
        var releaseData = new GitHubReleaseData(
            "v1.0.0",
            Array.Empty<GitHubAsset>()
        );

        var repoData = new CompatModRepoData(releaseData, "test.dll");

        Assert.Equal(string.Empty, repoData.GetDownloadUrl());
    }

    [Fact]
    public void IsNewer_ReturnsTrueWhenTagIsNewer()
    {
        var releaseData = new GitHubReleaseData(
            "v2.0.0",
            Array.Empty<GitHubAsset>()
        );

        var repoData = new CompatModRepoData(releaseData, "test.dll");
        var currentVersion = new SemanticVersioning.Version("1.0.0");

        Assert.True(repoData.IsNewer(currentVersion));
    }

    [Fact]
    public void IsNewer_ReturnsFalseWhenTagIsOlderOrInvalid()
    {
        var releaseData1 = new GitHubReleaseData(
            "v0.5.0",
            Array.Empty<GitHubAsset>()
        );
        var repoData1 = new CompatModRepoData(releaseData1, "test.dll");
        var currentVersion = new SemanticVersioning.Version("1.0.0");

        Assert.False(repoData1.IsNewer(currentVersion));

        var releaseData2 = new GitHubReleaseData(
            "invalid_version",
            Array.Empty<GitHubAsset>()
        );
        var repoData2 = new CompatModRepoData(releaseData2, "test.dll");

        Assert.False(repoData2.IsNewer(currentVersion));
    }
}
