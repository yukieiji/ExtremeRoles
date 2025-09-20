using System;
using System.Collections.Generic;
using System.Linq;

using ExtremeRoles.GhostRoles.API;
using ExtremeRoles.GhostRoles.API.Interface;
using ExtremeRoles.GhostRoles.Crewmate.Faunus;
using ExtremeRoles.GhostRoles.Crewmate.Poltergeist;
using ExtremeRoles.GhostRoles.Crewmate.Shutter;
using ExtremeRoles.GhostRoles.Impostor.Doppelganger;
using ExtremeRoles.GhostRoles.Impostor.Igniter;
using ExtremeRoles.GhostRoles.Impostor.SaboEvil;
using ExtremeRoles.GhostRoles.Impostor.Ventgeist;
using ExtremeRoles.GhostRoles.Neutral.Foras;
using ExtremeRoles.Module;


namespace ExtremeRoles.GhostRoles;

public sealed class GhostRoleInfo : IGhostRoleInfoContainer
{
	public IReadOnlyDictionary<ExtremeGhostRoleId, GhostRoleCore> Core => new List<GhostRoleCore>()
	{
		GhostRoleCore.CreateCrewmate(ExtremeGhostRoleId.Poltergeist, ColorPalette.PoltergeistLightKenpou),
		GhostRoleCore.CreateCrewmate(ExtremeGhostRoleId.Faunus     , ColorPalette.FaunusAntiquewhite),
		GhostRoleCore.CreateCrewmate(ExtremeGhostRoleId.Shutter    , ColorPalette.PhotographerVerdeSiena),

		GhostRoleCore.CreateImpostor(ExtremeGhostRoleId.Ventgeist),
		GhostRoleCore.CreateImpostor(ExtremeGhostRoleId.SaboEvil),
		GhostRoleCore.CreateImpostor(ExtremeGhostRoleId.Igniter),
		GhostRoleCore.CreateImpostor(ExtremeGhostRoleId.Doppelganger),

		GhostRoleCore.CreateNeutral(ExtremeGhostRoleId.Foras, ColorPalette.ForasSeeSyuTin),
	}.ToDictionary(x => x.Id);

	public IReadOnlyDictionary<ExtremeGhostRoleId, Type> OptionBuilder => new Dictionary<ExtremeGhostRoleId, Type>()
	{
		{ ExtremeGhostRoleId.Poltergeist, typeof(PoltergeistOptionBuilder) },
		{ ExtremeGhostRoleId.Faunus     , typeof(FaunusOptionBuilder) },
		{ ExtremeGhostRoleId.Shutter    , typeof(ShutterOptionBuilder) },

		{ ExtremeGhostRoleId.Ventgeist, typeof(VentgeistOptionBuilder) },
		{ ExtremeGhostRoleId.SaboEvil, typeof(SaboEvilOptionBuilder) },
		{ ExtremeGhostRoleId.Igniter, typeof(IgniterOptionBuilder) },
		{ ExtremeGhostRoleId.Doppelganger, typeof(DoppelgangerOptionBuilder) },

		{ ExtremeGhostRoleId.Foras, typeof(ForasOptionBuilder) },
	};

	public IReadOnlyDictionary<ExtremeGhostRoleId, Type> Role => new Dictionary<ExtremeGhostRoleId, Type>()
	{
		{ ExtremeGhostRoleId.Poltergeist, typeof(PoltergeistRole) },
		{ ExtremeGhostRoleId.Faunus     , typeof(FaunusRole) },
		{ ExtremeGhostRoleId.Shutter    , typeof(ShutterRole) },

		{ ExtremeGhostRoleId.Ventgeist, typeof(VentgeistRole) },
		{ ExtremeGhostRoleId.SaboEvil, typeof(SaboEvilRole) },
		{ ExtremeGhostRoleId.Igniter, typeof(IgniterRole) },
		{ ExtremeGhostRoleId.Doppelganger, typeof(DoppelgangerRole) },

		{ ExtremeGhostRoleId.Foras, typeof(ForasRole) },
	};
}
