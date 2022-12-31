using System;
using System.Collections.Generic;
using System.Text;

namespace ExtremeRoles.GameMode.Factory
{
    public static class HideNSeekGameModeFactory
    {
        public static void Assemble(ExtremeGameManager mng)
        {
            mng.ShipOption = new ShipGlobalOption();
        }
    }
}
