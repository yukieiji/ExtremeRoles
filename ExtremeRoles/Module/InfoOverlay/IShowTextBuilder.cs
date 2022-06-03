using System;

namespace ExtremeRoles.Module.InfoOverlay
{
    internal interface IShowTextBuilder
    {
        Tuple<string, string> GetShowText();
    }
}
