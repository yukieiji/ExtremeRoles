namespace ExtremeRoles.Module.JsonData;

public readonly record struct GitHubAsset(
	string content_type,
	string browser_download_url);

public readonly record struct GitHubReleaseData(
	string tag_name,
	GitHubAsset[] assets);

