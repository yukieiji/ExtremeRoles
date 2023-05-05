using System;
using System.Collections.Generic;

using ExtremeRoles.Module;
using ExtremeRoles.Performance;

using ExtremeVoiceEngine.Extension;

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
            FastDestroyableSingleton<HudManager>.Instance.Chat.AddLocalChat("Can't parse");
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

    public bool ExcuteCmd(string text)
    {
        if (!text.StartsWith(CmdChar) ||
            !DestroyableSingleton<HudManager>.InstanceExists) { return false; }

        string cleanedText = text.Substring(1);
        ChatController chat = FastDestroyableSingleton<HudManager>.Instance.Chat;
        string[] args = cleanedText.Split(' ');

        chat.AddLocalChat(text);
        string masterArgs = cleanedArg(args[0]);
        if (!this.masterCmd.TryGetValue(masterArgs, out ParseActions? masterParser))
        {
            chat.AddLocalChat(TranslationController.Instance.GetString("CannotFindCmd"));
            return true;
        }
        if (args.Length == 1)
        {
            chat.AddLocalChat(masterParser.Parser.ToString(masterArgs));
            return true;
        }
        string subCmd = cleanedArg(args[1]);
        if (this.subCmdLink.TryGetValue(masterArgs, out HashSet<string>? subCmds) &&
            subCmds is not null && subCmds.Contains(subCmd))
        {

            ParseActions subCmdParser = this.subCmd[$"{masterArgs} {subCmd}"];
            if (args.Length == 2)
            {
                chat.AddLocalChat(subCmdParser.Parser.ToString($"{masterArgs}{subCmd}"));
            }
            else
            {
                subCmdParser.ParseAndAction(args[2..]);
            }
            return true;
        }

        masterParser.ParseAndAction(args[1..]);
        return true;
    }

    public void AddCommand(string cmd, ParseActions cmdParser)
    {
        this.masterCmd.Add(cleanedArg(cmd), cmdParser);
    }

    public void AddSubCommand(string masterCmd, string subCommand, ParseActions parser)
    {
        masterCmd = cleanedArg(masterCmd);
        subCommand = cleanedArg(subCommand);
        if (!this.masterCmd.ContainsKey(masterCmd))
        {
            throw new ArgumentException("master cmmand can't find");
        }
        if (!this.subCmdLink.TryGetValue(masterCmd, out HashSet<string>? subCmds) ||
            subCmds is null)
        {
            subCmds = new HashSet<string>();
            this.subCmdLink[masterCmd] = subCmds;
        }
        subCmds.Add(subCommand);
        this.subCmd.Add($"{masterCmd} {subCommand}", parser);
    }

    private string cleanedArg(string arg)
    {
        arg = arg.ToLower();
        if (this.alias.TryGetValue(arg, out string? newAlias))
        {
            return newAlias;
        }
        return arg;
    }
}
