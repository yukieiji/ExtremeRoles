using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ExtremeVoiceEngine.Command;

public class Parser
{
    /// <summary>
    /// 長い名前でオプションを指定する際のプレフィックスを取得または設定します。
    /// 既定値は"--"です。
    /// </summary>
    public string LongNameOptionSymbol { get; set; } = "--";
    /// <summary>
    /// 短い名前でオプションを指定する際のプレフィックスを取得または設定します。
    /// 既定値は"-"です。
    /// </summary>
    public string ShortNameOptionSymbol { get; set; } = "-";
    /// <summary>
    /// オプションのキーと値を区切る文字のリストを取得または設定します。
    /// 既定では'='および半角スペースを含みます。
    /// </summary>
    public IReadOnlyList<char> OptionKeyValueSeparators { get; set; } = new[] { ' ', '=' };

    private readonly Dictionary<OptionKey, Option> optionDic = new Dictionary<OptionKey, Option>();

    /// <summary>
    /// コンストラクタです。
    /// </summary>
    /// <param name="options">オプション</param>
    public Parser(params Option[] options)
    {
        foreach (var opt in options)
        {
            this.RegisterOption(opt);
        }
    }

    /// <summary>
    /// オプション情報を追加します。
    /// </summary>
    /// <param name="option"></param>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="ArgumentException">キーの重複するオプションが登録されています。</exception>
    public void RegisterOption(Option option)
    {
        this.optionDic.Add(new OptionKey(option), option);
    }

    /// <summary>
    /// 指定したコマンドライン引数を解析します。
    /// 解析不能な場合はnullを返します。
    /// </summary>
    /// <param name="args">コマンドライン引数（実行コマンド名の指定部を除く）を指定します。</param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public Result Parse(string[] args)
    {
        var optDic = new Dictionary<OptionKey, string>();
        OptionKey? currentOptionKey = null;
        var paramList = new List<string>();

        for (int index = 0; index < args.Length; ++index)
        {
            string arg = args[index];

            if (string.IsNullOrEmpty(arg))
            {
                continue;
            }

            bool optionGetResult;
            OptionKey? key;
            try
            {
                optionGetResult = tryGetOptionKey(arg, out key);
            }
            catch (Exception e)
            {
                throw new ArgumentException($"Detect unknown args : {e.Message}");
            }

            if (optionGetResult && key is null)
            {
                throw new ArgumentException($"Detect unknown options");
            }

            bool isOptionSetValue;
            string value;
            try
            {
                isOptionSetValue = tryGetOptionWithValue(arg, out value);
            }
            catch (Exception e)
            {
                throw new ArgumentException($"Detect Dupe option values : {e.Message}");
            }

            if (key is null && currentOptionKey is not null)
            {
                var optionType = this.optionDic[currentOptionKey].OptionKind;
                if (optionType == Option.Kind.NoValue)
                {
                    throw new ArgumentException($"Detect option values with no value options");
                }
                optDic[currentOptionKey] = arg;
                paramList.Add(arg);
            }
            else if (key is not null && isOptionSetValue)
            {
                var optionType = this.optionDic[key].OptionKind;
                if (optionType == Option.Kind.NoValue)
                {
                    throw new ArgumentException($"Detect option values with no value options");
                }
                optDic.Add(key, value);
                paramList.Add(value);
            }
            else if (key is not null)
            {
                var optionType = this.optionDic[key].OptionKind;
                if (optionType != Option.Kind.NoValue && index == arg.Length - 1)
                {
                    throw new ArgumentException($"Detect no option values with value options");
                }
                currentOptionKey = key;
                optDic.Add(key, string.Empty);
            }
            else
            {
                throw new ArgumentException($"Can't parse args");
            }
        }
        return new Result(paramList, optDic);
    }

    private bool tryGetOptionKey(string arg, out OptionKey? key)
    {
        key = null;
        if (arg.StartsWith(LongNameOptionSymbol))
        {
            string rmLongSym = arg.Substring(LongNameOptionSymbol.Length);
            string[] longNamePrms = rmLongSym.Split(OptionKeyValueSeparators.ToArray());
            string optionName = longNamePrms[0];
            (key, Option _) = this.optionDic.FirstOrDefault(
                p => p.Key.MatchesWithLongName(optionName));
            return key != null;
        }
        else if (arg.StartsWith(ShortNameOptionSymbol))
        {
            string rmShotSym = arg.Substring(ShortNameOptionSymbol.Length);
            string[] shortNameParms = rmShotSym.Split(OptionKeyValueSeparators.ToArray());
            string optionName = shortNameParms[0];
            if (optionName.Length > 2)
            {
                throw new ArgumentException("Shortcut options to one");
            }
            char shortcutOpt = optionName[0];
            (key, Option _) = this.optionDic.FirstOrDefault(
                p => p.Key.MatchesWithShortName(shortcutOpt));
            return key != null;
        }
        else
        {
            return false;
        }
    }

    private bool tryGetOptionWithValue(string arg, out string value)
    {
        value = string.Empty;

        //longname
        if (arg.StartsWith(LongNameOptionSymbol))
        {
            string rmSym = arg.Substring(LongNameOptionSymbol.Length);
            string[] longNamePrms = rmSym.Split(OptionKeyValueSeparators.ToArray());
            if (longNamePrms.Length > 2)
            {
                //※"--hoge=huga=piyo" みたいな形は許可しないものとする
                throw new ArgumentException($"Detect Dupe values");
            }
            if (longNamePrms.Length == 2)
            {
                value = longNamePrms[1];
                return true;
            }
            else
            {
                return false;
            }
        }

        //shortname
        if (arg.StartsWith(ShortNameOptionSymbol))
        {
            string rmSym = arg.Substring(ShortNameOptionSymbol.Length);
            string[] shortNamePrms = rmSym.Split(OptionKeyValueSeparators.ToArray());
            if (shortNamePrms.Length > 2)
            {
                throw new ArgumentException("Shortcut options to one");
            }
            if (shortNamePrms.Length == 2)
            {
                value = shortNamePrms[1];
                return true;
            }
            else
            {
                return false;
            }
        }
        else
        {
            return false;
        }
    }

    public string ToString(string cmd)

    {
        var sb = new StringBuilder();
        sb.AppendLine(cmd);

        sb.AppendLine("Arguments:     ");

        foreach (var opt in this.optionDic.Values)
        {
            sb.AppendLine(opt.ToString(this));
        }

        return sb.ToString();
    }
}

/// <summary>
/// コマンドのオプションを識別するための情報を格納するクラスです。
/// </summary>
public sealed class OptionKey
{
    private readonly char shortName;
    private readonly string longName;

    public OptionKey(Option option): this(option.LongName, option.ShortName)
    { }

    /// <summary>
    /// コンストラクタです。
    /// </summary>
    /// <param name="shortName"></param>
    /// <param name="longName"></param>
    /// <param name="ignoresCase"></param>
    /// <exception cref="ArgumentException"></exception> 
    public OptionKey(string longName, char shortName = ' ')
    {
        if (string.IsNullOrEmpty(longName))
        {
            throw new ArgumentException("You should set at least either shortName or longName.");
        }

        this.longName = longName.ToLower();
        this.shortName = char.ToLower(shortName);
    }

    /// <summary>
    /// 指定した短い名称がこのインスタンスと一致するかどうかを取得します。
    /// </summary>
    /// <param name="shortName"></param>
    /// <returns></returns>
    public bool MatchesWithShortName(char shortName)
        => this.shortName == char.ToLower(shortName);

    /// <summary>
    /// 指定した長い名称がこのインスタンスと一致するかどうかを取得します。
    /// </summary>
    /// <param name="longName"></param>
    /// <returns></returns>
    public bool MatchesWithLongName(string longName)
        => this.longName == longName.ToLower();

    /// <summary>
    /// 指定したオプションのキーがこのインスタンスと一致するかどうかを取得します。
    /// </summary>
    /// <param name="option"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public bool Matches(Option option)
        => MatchesWithLongName(option.LongName) && MatchesWithShortName(option.ShortName);

    /// <summary>
    /// 指定したオブジェクトがこのオブジェクトと等価かどうかを取得します。
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public override bool Equals(object? obj)
    {
        if (obj is not OptionKey other)
        {
            return false;
        }

        return MatchesWithShortName(other.shortName) && MatchesWithLongName(other.longName);
    }
    /// <summary>
    /// ハッシュコードを取得します。
    /// </summary>
    /// <returns></returns>
    public override int GetHashCode()
        => this.shortName.GetHashCode() ^ this.longName.GetHashCode();

    /// <summary>
    /// このオブジェクトの文字列表現を返します。
    /// </summary>
    /// <param name="shortNameSymbol"></param>
    /// <param name="longNameSymbol"></param>
    /// <returns></returns>
    public string ToString(string shortNameSymbol, string longNameSymbol)
    {
        string result = $"{longNameSymbol}{this.longName}";
        if (!char.IsWhiteSpace(this.shortName))
        {
            result = $"{result}, {shortNameSymbol} {this.shortName}";
        }

        return result;
    }
}
