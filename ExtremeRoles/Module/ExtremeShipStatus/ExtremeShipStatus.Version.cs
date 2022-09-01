using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

namespace ExtremeRoles.Module.ExtremeShipStatus
{
    public sealed partial class ExtremeShipStatus
    {
        private Dictionary<int, Version> playerVersion = new Dictionary<int, Version>();

        public void AddPlayerVersion(int clientId, Version version)
        {
            playerVersion[clientId] = version;
        }

        public void AddPlayerVersion(
            int clientId, int major, int minor, int build, int rev)
        {
            this.playerVersion[clientId] =
                new Version(major, minor, build, rev);
        }

        public bool TryGetPlayerVersion(int clientId, out Version verResult)
        {
            return this.playerVersion.TryGetValue(clientId, out verResult);
        }
    }
}
