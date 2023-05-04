using System;
using System.Linq;
using System.Text;

namespace ExtremeVoiceEngine.Command;

public sealed class Option
{
    /// <summary>
    /// コマンドオプション種別を表す列挙型です。
    /// </summary>
    public enum Kind
    {
        /// <summary>
        /// 値を取らないオプションを表します。
        /// </summary>
        NoValue,
        /// <summary>
        /// 値を必要とするオプションを表します。
        /// </summary>
        Need,
        /// <summary>
        /// 任意で値を指定できるオプションを表します。
        /// </summary>
        Optional,
    }

    /// <summary>
    /// 短いオプション識別子を取得します。
    /// </summary>
    public char ShortName { get; private set; } = ' ';
    /// <summary>
    /// 長いオプション識別子を取得します。
    /// </summary>
    public string LongName { get; private set; }
    /// <summary>
    /// 値をとるオプションであれば、その値の名称を取得します。
    /// </summary>
    public string ValueName { get; private set; }
    /// <summary>
    /// このオプションの説明文を取得します。
    /// </summary>
    public string Expression { get; private set; }
    /// <summary>
    /// オプション種別を取得します。
    /// </summary>
    public Kind OptionKind { get; private set; }
    /// <summary>
    /// このオプション情報をUsageに表示するかどうかを取得または設定します。
    /// 既定値はTrueです。
    /// </summary>
    public bool ShowsInUsage { get; set; } = true;

    /// <summary>
    /// コンストラクタです。
    /// </summary>
    /// <param name="longName">長いオプション識別子を指定します。</param>
    /// <param name="valueName">値をとるオプションであれば、その値の名称を指定します。</param>
    /// <param name="kind">オプション種別を指定します。</param>
    /// <param name="shortName">短いオプション識別子を指定します。</param>
    /// <param name="expression">このオプションの説明文を指定します。</param>
    /// <exception cref="ArgumentOutOfRangeException">kindに未定義の値が指定された場合にスローされます。</exception>
    /// <exception cref="ArgumentException">longNameが指定されていない場合にスローされます。</exception>
    public Option(
        string longName, string valueName, Kind kind,
        char shortCut = ' ', string expression = "")
    {
        if (!Enum.IsDefined(typeof(Kind), kind))
        {
            throw new ArgumentOutOfRangeException(nameof(kind));
        }

        if (string.IsNullOrEmpty(longName))
        {
            throw new ArgumentException("You should set at least either longName.");
        }

        this.ShortName = char.ToLower(shortCut);
        this.LongName = longName.ToLower();
        this.ValueName = valueName;
        this.Expression = expression;
        this.OptionKind = kind;
    }

    /// <summary>
    /// Usage出力時に必要となる、コマンド部の半角文字数を取得します。
    /// </summary>
    internal int NeededLeftColumnLength
    {
        get
        {
            int rtn = "-x".Length;

            if (!string.IsNullOrEmpty(this.LongName))
            {
                rtn += ", ".Length;
                rtn += ("--" + this.LongName).Length;
            }

            if (!string.IsNullOrEmpty(this.ValueName))
            {
                rtn += (' ' + this.ValueName).Length;
                if (this.OptionKind == Kind.Optional)
                {
                    rtn += ("[]").Length;
                }
            }

            return rtn;
        }
    }

    /// <summary>
    /// Usageの出力使用される文字列表現を取得します。
    /// </summary>
    /// <param name="keyValSeparator"></param>
    /// <param name="leftColumnWidth"></param>
    /// <returns></returns>
    internal string ToString(char keyValSeparator, int leftColumnWidth)
    {
        var sb = new StringBuilder();

        sb.Append("--").Append(this.LongName);

        if (!char.IsWhiteSpace(this.ShortName))
        {
            sb.Append(", ").Append("-").Append(this.ShortName);
        }
        
        if (!string.IsNullOrEmpty(this.ValueName))
        {
            if (char.IsWhiteSpace(keyValSeparator))
            {
                // "KEY [VAL]" となるように
                sb.Append(keyValSeparator);
                if (this.OptionKind == Kind.Optional)
                {
                    sb.Append("[");
                }
            }
            else
            {
                // "KEY[=VAL]" となるように
                if (this.OptionKind == Kind.Optional)
                {
                    sb.Append("[");
                }

                sb.Append(keyValSeparator);
            }
            sb.Append(this.ValueName);
            if (OptionKind == Kind.Optional)
            {
                sb.Append("]");
            }
        }

        int restLeft = leftColumnWidth - sb.Length;
        if (0 < restLeft)
        {
            sb.Append(Enumerable.Repeat(' ', restLeft).ToArray());
        }

        return string.Format("  {0}  {1}", sb.ToString(), Expression);
    }
}
