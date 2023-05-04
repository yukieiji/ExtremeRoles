using System;
using System.Collections.Generic;

using ExtremeRoles.Module;
using ExtremeRoles.Performance;

namespace ExtremeVoiceEngine.Command;

public record class ParseActions(Parser Parser, Action<Result?> ParseAction)
{
    public void ParseAndAction(string[] args)
    {
        Result? result = null;
        try
        {
            result = this.Parser.Parse(args);
        }
        catch
        {
            FastDestroyableSingleton<HudManager>.Instance.Chat.AddChat(
                CachedPlayerControl.LocalPlayer, "Can't parse");
        }
        this.ParseAction.Invoke(result);
    }
}

public sealed class CommandManager : NullableSingleton<CommandManager>
{
    private Dictionary<string, ParseActions> masterCmd = new Dictionary<string, ParseActions>();
    private Dictionary<string, HashSet<string>> subCmdLink = new Dictionary<string, HashSet<string>>();
    private Dictionary<string, ParseActions> subCmd = new Dictionary<string, ParseActions>();

    private Dictionary<string, string> alias = new Dictionary<string, string>();

    public const char CmdChar = '/';

    public CommandManager()
    {
        this.masterCmd.Clear();
        this.subCmdLink.Clear();
        this.subCmd.Clear();
    }

    public void AddAlias(string target, params string[] newAlias)
    {
        foreach (string alias in newAlias)
        {
            this.alias.Add(alias, target);
        }
    }

    public void ExcuteCmd(string text)
    {
        if (!text.StartsWith(CmdChar) ||
            !DestroyableSingleton<HudManager>.InstanceExists) { return; }

        string cleanedText = text.Substring(1);
        HudManager hud = FastDestroyableSingleton<HudManager>.Instance;
        PlayerControl localPlayer = CachedPlayerControl.LocalPlayer;
        string[] args = cleanedText.Split(' ');

        string masterArgs = cleanedArg(args[0]);
        ExtremeVoiceEnginePlugin.Logger.LogInfo(masterArgs);
        if (!this.masterCmd.TryGetValue(masterArgs, out ParseActions? masterParser))
        {
            hud.Chat.AddChat(localPlayer, "Can't Find cmd");
            return;
        }
        if (args.Length == 1)
        {
            hud.Chat.AddChat(localPlayer, masterParser.Parser.ToString(masterArgs));
            return;
        }
        string subCmd = cleanedArg(args[1]);
        if (this.subCmdLink.TryGetValue(masterArgs, out HashSet<string>? subCmds) &&
            subCmds is not null && subCmds.Contains(subCmd))
        {

            ParseActions subCmdParser = this.subCmd[subCmd];
            if (args.Length == 2)
            {
                hud.Chat.AddChat(localPlayer, subCmdParser.Parser.ToString($"{masterArgs}{subCmd}"));
                return;
            }
            subCmdParser.ParseAndAction(args[2..~0]);
        }

        masterParser.ParseAndAction(args[1..~0]);
    }

    public void AddCommand(string cmd, ParseActions cmdParser)
    {
        this.masterCmd.Add(cmd, cmdParser);
    }

    public void AddSubCommand(string masterCmd, string subCommand, ParseActions parser)
    {
        if (!this.masterCmd.ContainsKey(masterCmd))
        {
            throw new ArgumentException("master cmmand can't find");
        }
        this.subCmdLink[masterCmd].Add(subCommand);
        this.subCmd.Add($"{masterCmd} {subCommand}", parser);
    }

    private string cleanedArg(string arg)
    {
        arg = arg.ToLower();
        if (this.alias.TryGetValue(arg, out string? newalias))
        {
            return newalias;
        }
        return arg;
    }
}
