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
    /// <param name="shortCut">短いオプション識別子を指定します。</param>
    /// <param name="expression">このオプションの説明文を指定します。</param>
    /// <exception cref="ArgumentOutOfRangeException">kindに未定義の値が指定された場合にスローされます。</exception>
    /// <exception cref="ArgumentException">longNameが指定されていない場合にスローされます。</exception>
    public Option(
        string longName, char shortCut = ' ', Kind kind = Kind.NoValue, string expression = "")
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
        this.Expression = expression;
        this.OptionKind = kind;
    }

    /// <summary>
    /// Usageの出力使用される文字列表現を取得します。
    /// </summary>
    /// <param name="keyValSeparator"></param>
    /// <param name="leftColumnWidth"></param>
    /// <returns></returns>
    public string ToString(Parser parser)
    {
        var sb = new StringBuilder();

        if (this.OptionKind == Kind.Optional)
        {
            sb.Append($"{Kind.Optional} ");
        }

        sb.Append(parser.LongNameOptionSymbol).Append(this.LongName);

        if (!char.IsWhiteSpace(this.ShortName))
        {
            sb.Append(", ").Append(parser.ShortNameOptionSymbol).Append(this.ShortName);
        }
        
        switch (this.OptionKind)
        {
            case Kind.NoValue:
                sb.Append(Expression);
                break;
            case Kind.Need:
            case Kind.Optional:
                sb.Append(" [").Append($"{this.LongName}Value").Append("]").Append(this.Expression);
                break;
        }

        return sb.ToString();
    }
}
