using System;

namespace ExtremeRoles.Compat;

sealed internal record CompatModInfo(
				string Name,
				string Guid,
				string RepoUrl,
				bool IsRequireReactor = false,
				Type ModIntegratorType = null);
