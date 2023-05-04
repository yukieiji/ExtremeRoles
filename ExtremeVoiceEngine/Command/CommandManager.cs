using System;
using System.Collections.Generic;

using ExtremeRoles.Module;
using ExtremeRoles.Performance;

namespace ExtremeVoiceEngine.Command;

public sealed class CommandManager : NullableSingleton<CommandManager>
{
    private Dictionary<string, Parser> masterCmd = new Dictionary<string, Parser>();
    private Dictionary<string, string> subCmdLink = new Dictionary<string, string>();
    private Dictionary<string, Parser> subCmd = new Dictionary<string, Parser>();

    private const char cmdChar = '/';

    public CommandManager()
    {
        this.masterCmd.Clear();
        this.subCmdLink.Clear();
        this.subCmd.Clear();
    }

    public Result? ParseCmd(string text)
    {
        if (!text.StartsWith(cmdChar) ||
            !DestroyableSingleton<HudManager>.InstanceExists) { return null; }

        HudManager hud = FastDestroyableSingleton<HudManager>.Instance;
        PlayerControl localPlayer = CachedPlayerControl.LocalPlayer;
        string[] args = text.Split(' ');

        string masterArgs = args[0];
        if (!this.masterCmd.TryGetValue(masterArgs, out Parser? masterParser))
        {
            hud.Chat.AddChat(localPlayer, "Can't Find cmd");
            return null;
        }
        if (args.Length == 1)
        {
            hud.Chat.AddChat(localPlayer, masterParser.ToString());
            return null;
        }
        string subCmd = args[1];
        if (this.subCmdLink.TryGetValue(masterArgs, out string? hasSubCmd) &&
            !string.IsNullOrEmpty(hasSubCmd) && hasSubCmd == subCmd)
        {

            Parser subCmdParser = this.subCmd[subCmd];
            if (args.Length == 2)
            {
                hud.Chat.AddChat(localPlayer, subCmdParser.ToString());
                return null;
            }
            return subCmdParser.Parse(args[2..~0]);
        }

        return masterParser.Parse(args[1..~0]);
    }

    public void AddCommand(string cmd, Parser cmdParser)
    {
        if (!cmd.StartsWith(cmdChar))
        {
            throw new ArgumentException("cmmand start with '/'");
        }
        this.masterCmd.Add(cmd, cmdParser);
    }

    public void AddSubCommand(string masterCmd, string subCommand, Parser parser)
    {
        if (!masterCmd.StartsWith(cmdChar))
        {
            throw new ArgumentException("cmmand start with '/'");
        }
        if (!this.masterCmd.ContainsKey(masterCmd))
        {
            throw new ArgumentException("master cmmand can't find");
        }
        this.subCmdLink.Add(masterCmd, subCommand);
        this.subCmd.Add(subCmd, parser);
    }
}
