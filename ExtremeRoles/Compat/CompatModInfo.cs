using System;

namespace ExtremeRoles.Compat;

#nullable enable

internal readonly record struct CompatModInfo(
	string Name,
	string Guid,
	string RepoUrl,
	bool IsRequireReactor,
	Type ModIntegratorType);
