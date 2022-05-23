using System;
using UnityEngine;

using ExtremeRoles.Module.AbilityButton.GhostRoles;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;

namespace ExtremeRoles.GhostRoles.API.Interface
{
    internal interface IGhostRole : IRoleOption
    {
        public ExtremeRoleType Team { get; }
        public ExtremeGhostRoleId Id { get; }
        public int OptionOffset { get; }
        public string Name { get; }
        public GhostRoleAbilityButtonBase Button { get; set; }
        public Color RoleColor { get; }

        public int GetRoleOptionId<T>(T option) where T : struct, IConvertible;

        public bool IsCrewmate();

        public bool IsImpostor();

        public bool IsNeutral();

        public bool IsVanillaRole();

        public void ReseOnMeetingEnd();

        public void ReseOnMeetingStart();

    }
}
