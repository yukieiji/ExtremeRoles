using System;

namespace ExtremeRoles.Module.InfoOverlay
{
    internal interface IFullDecManager
    {
        void Clear();
        void ChangeRoleInfoPage(int count);

        Tuple<string, string> GetPlayerRoleText();

        Tuple<string, string> GetRoleInfoPageText();
    }
}
