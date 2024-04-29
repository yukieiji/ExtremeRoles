using System;

namespace ExtremeRoles.Module.JsonData;

public readonly record struct UserData(
	string login,
	int id,
	string node_id,
	string avatar_url,
	string gravatar_id,
	string url,
	string html_url,
	string followers_url,
	string following_url,
	string gists_url,
	string starred_url,
	string subscriptions_url,
	string organizations_url,
	string repos_url,
	string events_url,
	string received_events_url,
	string type,
	bool site_admin);

public readonly record struct GitHubAsset(
	string url,
	int id,
	string node_id,
	string name,
	string label,
	UserData uploader,
	string content_type,
	string state,
	int size,
	int download_count,
	DateTime created_at,
	DateTime updated_at,
	string browser_download_url);

public readonly record struct GitHubReleaseData(
	string url,
	string assets_url,
	string upload_url,
	string html_url,
	int id,
	UserData author,
	string node_id,
	string tag_name,
	string target_commitish,
	string name,
	bool draft,
	bool prerelease,
	DateTime created_at,
	DateTime published_at,
	GitHubAsset[] assets,
	string tarball_url,
	string zipball_url,
	string body);

