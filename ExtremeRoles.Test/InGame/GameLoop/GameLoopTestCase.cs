using BepInEx.Logging;

using ExtremeRoles.Roles;

using System;
using System.Collections;
using System.Collections.Generic;

namespace ExtremeRoles.Test.InGame.GameLoop;

public sealed record GameLoopTestCase(
	string Name, int Iteration,
	HashSet<ExtremeRoleId>? Ids = null,
	Action? PreSetUp = null,
	Func<ManualLogSource, IEnumerator>? PreTestCase = null);
