namespace ExtremeRoles.Module.InfoOverlay
{
    internal interface IShowTextBuilder
    {
        (string, string, string) GetShowText();
    }
}
