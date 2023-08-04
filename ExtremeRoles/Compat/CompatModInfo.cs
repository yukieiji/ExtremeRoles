using System;

namespace ExtremeRoles.Compat;

#nullable enable

internal record struct CompatModInfo(
	string Name,
	string Guid,
	string RepoUrl,
	bool IsRequireReactor,
	Type ModIntegratorType);
