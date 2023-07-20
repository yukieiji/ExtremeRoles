using System;

namespace ExtremeRoles.Compat;

#nullable enable

sealed internal record CompatModInfo(
	string Name,
	string Guid,
	string RepoUrl,
	bool IsRequireReactor,
	Type ModIntegratorType);
