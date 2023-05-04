using System;
using System.Collections.Generic;
using System.Linq;

namespace ExtremeVoiceEngine.Command;

/// <summary>
/// 解析されたコマンドの情報を格納するクラスです。
/// </summary>
public sealed class Result
{
    private readonly IReadOnlyDictionary<OptionKey, string> optionDic;
    /// <summary>
    /// コマンドに指定されたオプション以外の引数を取得します。
    /// </summary>
    public IReadOnlyList<string> CommandParameters { get; private set; }
    /// <summary>
    /// 指定した長い名称のオプションが指定されたかどうかを取得します。
    /// </summary>
    /// <param name="longName"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="ArgumentException">longNameに空文字列を指定した際にスローされます。</exception>
    public bool HasOption(string longName)
    {
        if (string.IsNullOrEmpty(longName))
        {
            throw new ArgumentException("longNameを空にすることはできません");
        }

        return this.optionDic.Keys.Any(k => k.MatchesWithLongName(longName));
    }
    /// <summary>
    /// 指定した短い名称のオプションが指定されたかどうかを取得します。
    /// </summary>
    /// <param name="shortName"></param>
    /// <returns></returns>
    public bool HasOption(char shortName)
    {
        return this.optionDic.Keys.Any(k => k.MatchesWithShortName(shortName));
    }
    /// <summary>
    /// 指定したオプションが指定されたかどうかを取得します。
    /// </summary>
    /// <param name="option"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public bool HasOption(Option option)
    {
        return this.optionDic.ContainsKey(new OptionKey(option));
    }

    /// <summary>
    /// 指定した長い名称のオプションとその値が指定されていれば、その値を取得します。
    /// </summary>
    /// <param name="longName"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="ArgumentException">longNameに空文字列を指定した際にスローされます。</exception>
    public string GetOptionValue(string longName)
    {
        if (string.IsNullOrEmpty(longName))
        {
            throw new ArgumentException("longNameを空にすることはできません");
        }

        return this.optionDic.FirstOrDefault(p => p.Key.MatchesWithLongName(longName)).Value;
    }
    /// <summary>
    /// 指定した短い名称のオプションとその値が指定されていれば、その値を取得します。
    /// </summary>
    /// <param name="shortName"></param>
    /// <returns></returns>
    public string GetOptionValue(char shortName)
    {
        return this.optionDic.FirstOrDefault(p => p.Key.MatchesWithShortName(shortName)).Value;
    }
    /// <summary>
    /// 指定したオプションとその値が指定されていれば、その値を取得します。
    /// </summary>
    /// <param name="option"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public string GetOptionValue(Option option)
    {
        return this.optionDic[new OptionKey(option)];
    }

    public bool TryGetOptionValue(string longName, out string value)
    {
        value = string.Empty;
        
        if (string.IsNullOrEmpty(longName)) { return false; }

        bool result = HasOption(longName);
        if (result)
        {
            value = GetOptionValue(longName);
        }
        return result;
    }

    public bool TryGetOptionValue(char shortcutName, out string value)
    {
        value = string.Empty;

        bool result = HasOption(shortcutName);
        if (result)
        {
            value = GetOptionValue(shortcutName);
        }
        return result;
    }

    /// <summary>
    /// プロテクトコンストラクタです。
    /// </summary>
    /// <param name="ignoresCase"></param>
    /// <param name="commandParams"></param>
    /// <param name="optionDic"></param>
    /// <exception cref="ArgumentNullException"></exception>
    internal Result(
        IReadOnlyList<string> commandParams,
        Dictionary<OptionKey, string> optionDic)
    {
        this.CommandParameters = commandParams;
        this.optionDic = optionDic;
    }

}
