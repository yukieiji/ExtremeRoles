using Hazel;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Linq;

using UnityEngine;

using ExtremeRoles.Helper;
using ExtremeRoles.GameMode;
using ExtremeRoles.Module.RoleAssign;
using ExtremeRoles.Performance;

namespace ExtremeRoles.Module.CustomOption;

#nullable enable

public sealed class OptionManager
{
    public enum ValueType : byte
    {
        Int,
        Float,
        Bool
    }

    public string ConfigPreset
    {
        get => $"Preset:{selectedPreset}";
    }

    public readonly static OptionManager Instance = new OptionManager();

    private Dictionary<int, ValueType> allOptionId = new Dictionary<int, ValueType>();
    private TypeOptionHolder<int>   intOption   = new TypeOptionHolder<int>();
    private TypeOptionHolder<float> floatOption = new TypeOptionHolder<float>();
    private TypeOptionHolder<bool>  boolOption  = new TypeOptionHolder<bool>();

    private int selectedPreset = 0;

    private const int chunkSize = 50;

	private const KeyCode maxSelectionKey = KeyCode.LeftControl;
	private const KeyCode skipSelectionKey = KeyCode.LeftShift;

	private const int defaultStep = 1;
	private const int skipStep = 10;

	// ジェネリック化をJIT化する時にtypeofの比較がどうやら定数比較になり必ずtrueになる部分とfalseになる部分が決定するらしい
	// それによってメソッドが特殊化されfalseの部分がJITの最適化時に削除、常にtrueの部分はifが消されるので非常に簡潔なILになる(ここはまぁコンパイラの授業で習った)
	// https://qiita.com/aka-nse/items/2f45f056262d2d5c6df7#comment-a8e1c1c3e9e7a0208068
	// 実際に測ったらUnsafe.Asを使わないas キャストを使ってるのに2倍近く早かった・・・・


	public void Add(int id, IValueOption<float> option)
    {
        this.floatOption.Add(id, option);
        this.allOptionId.Add(id, ValueType.Float);
    }
    public void Add(int id, IValueOption<int> option)
    {
        this.intOption.Add(id, option);
        this.allOptionId.Add(id, ValueType.Int);
    }
    public void Add(int id, IValueOption<bool> option)
    {
        this.boolOption.Add(id, option);
        this.allOptionId.Add(id, ValueType.Bool);
    }

    public void Add<SelectionType>(int id, IValueOption<SelectionType> option)
        where SelectionType :
            struct, IComparable, IConvertible,
            IComparable<SelectionType>, IEquatable<SelectionType>
    {
		if (typeof(SelectionType) == typeof(int))
		{
			Add(id, Unsafe.As<IValueOption<SelectionType>, IValueOption<int>>(ref option));
		}
		else if (typeof(SelectionType) == typeof(float))
		{
			Add(id, Unsafe.As<IValueOption<SelectionType>, IValueOption<float>>(ref option));
		}
		else if (typeof(SelectionType) == typeof(bool))
		{
			Add(id, Unsafe.As<IValueOption<SelectionType>, IValueOption<bool>>(ref option));
		}
		else
		{
			throw new ArgumentException("Cannot Add Options");
		}
	}

    public bool Contains(int id) => this.allOptionId.ContainsKey(id);

    public bool TryGet<T>(int id, out IValueOption<T>? option)
        where T :
            struct, IComparable, IConvertible,
            IComparable<T>, IEquatable<T>
    {
        option = null;
        if (!this.allOptionId.ContainsKey(id)) { return false; }

		if (typeof(T) == typeof(int))
		{
			var intOption = this.intOption.Get(id);
			option = Unsafe.As<IValueOption<int>, IValueOption<T>>(ref intOption);
			return true;
		}
		else if (typeof(T) == typeof(float))
		{
			var floatOption = this.floatOption.Get(id);
			option = Unsafe.As<IValueOption<float>, IValueOption<T>>(ref floatOption);
			return true;
		}
		else if(typeof(T) == typeof(bool))
		{
			var boolOption = this.boolOption.Get(id);
			option = Unsafe.As<IValueOption<bool>, IValueOption<T>>(ref boolOption);
			return true;
		}
		else
		{
			throw new ArgumentException("Cannot Find Options");
		}
	}

    public bool TryGetIOption(int id, out IOptionInfo? option)
    {
        option = null;
        if (!this.allOptionId.TryGetValue(id, out ValueType type)) { return false; }

        option = type switch
        {
            ValueType.Int => this.intOption.Get(id),
            ValueType.Float => this.floatOption.Get(id),
            ValueType.Bool => this.boolOption.Get(id),
            _ => null
        };
        return true;
    }

	public IValueOption<T> Get<T>(int id)
		where T :
			struct, IComparable, IConvertible,
			IComparable<T>, IEquatable<T>
	{
		if (typeof(T) == typeof(int))
		{
			var intOption = this.intOption.Get(id);
			return Unsafe.As<IValueOption<int>, IValueOption<T>>(ref intOption);
		}
		else if (typeof(T) == typeof(float))
		{
			var floatOption = this.floatOption.Get(id);
			return Unsafe.As<IValueOption<float>, IValueOption<T>>(ref floatOption);
		}
		else if (typeof(T) == typeof(bool))
		{
			var boolOption = this.boolOption.Get(id);
			return Unsafe.As<IValueOption<bool>, IValueOption<T>>(ref boolOption);
		}
		else
		{
			throw new ArgumentException("Cannot Find Options");
		}
	}

    public IEnumerable<KeyValuePair<int, IOptionInfo>> GetKeyValueAllIOptions()
    {
        foreach (var (id, key) in this.allOptionId)
        {
            IOptionInfo info = key switch
            {
                ValueType.Int => this.intOption.Get(id),
                ValueType.Float => this.floatOption.Get(id),
                ValueType.Bool => this.boolOption.Get(id),
                _ => throw new ArgumentException("Invalided Option Id"),
            };
            yield return new KeyValuePair<int, IOptionInfo>(id, info);
        }
    }

    public IEnumerable<IOptionInfo> GetAllIOption()
    {
        foreach (var (id, key) in this.allOptionId)
        {
            yield return key switch
            {
                ValueType.Int => this.intOption.Get(id),
                ValueType.Float => this.floatOption.Get(id),
                ValueType.Bool => this.boolOption.Get(id),
                _ => throw new ArgumentException("Invalided Option Id"),
			};
        }
    }

    public IOptionInfo GetIOption(int id)
        => this.allOptionId[id] switch
        {
            ValueType.Int => this.intOption.Get(id),
            ValueType.Float => this.floatOption.Get(id),
            ValueType.Bool => this.boolOption.Get(id),
            _ => throw new ArgumentException("Invalided Option Id"),
		};

    public T GetValue<T>(int id)
        where T :
            struct, IComparable, IConvertible,
            IComparable<T>, IEquatable<T>
    {
		if (typeof(T) == typeof(int))
		{
			var intOption = this.intOption.Get(id);
			int intValue = intOption.GetValue();
			return Unsafe.As<int, T>(ref intValue);
		}
		else if (typeof(T) == typeof(float))
		{
			var floatOption = this.floatOption.Get(id);
			float floatValue = floatOption.GetValue();
			return Unsafe.As<float, T>(ref floatValue);
		}
		else if (typeof(T) == typeof(bool))
		{
			var boolOption = this.boolOption.Get(id);
			bool boolValue = boolOption.GetValue();
			return Unsafe.As<bool, T>(ref boolValue);
		}
		else
		{
			return default(T);
		}
	}

	public void ChangeOptionValue(int id, bool isIncrese)
	{
		var option = GetIOption(id);

		int curSelection = option.CurSelection;
		int step = Input.GetKey(skipSelectionKey) ? skipStep : defaultStep;
		int newSelection = isIncrese ? curSelection + step : curSelection - step;
		if (Input.GetKey(maxSelectionKey))
		{
			newSelection = isIncrese ? option.ValueCount - 1 : 0;
		}

		option.UpdateSelection(newSelection);

		if (id == 0)
		{
			switchPreset(newSelection);
		}

		if (AmongUsClient.Instance &&
			AmongUsClient.Instance.AmHost &&
			CachedPlayerControl.LocalPlayer)
		{
			ShareOptionSelections();// Share all selections
		}
	}

    public void ShareOptionSelections()
    {
        if (PlayerControl.AllPlayerControls.Count <= 1 ||
            !AmongUsClient.Instance ||
            !AmongUsClient.Instance.AmHost ||
            !PlayerControl.LocalPlayer) { return; }

        shareOption(this.intOption);
        shareOption(this.floatOption);
        shareOption(this.boolOption);
    }

	private void switchPreset(int newPreset)
	{
		this.selectedPreset = newPreset;

		foreach (var (_, option) in this.GetKeyValueAllIOptions())
		{
			if (option.Id == 0) { continue; }
			option.SwitchPreset();
		}

		ShareOptionSelections();
		RoleAssignFilter.Instance.SwitchPreset();
	}

	private void rpcValueSync(int id, int selection)
	{
		if (!this.allOptionId.TryGetValue(id, out ValueType type)) { return; }

		switch (type)
		{
			case ValueType.Int:
				this.intOption.Update(id, selection);
				break;
			case ValueType.Float:
				this.floatOption.Update(id, selection);
				break;
			case ValueType.Bool:
				this.boolOption.Update(id, selection);
				break;
			default:
				break;
		};
	}

	public static void Load()
    {
        // ランダム生成機を設定を読み込んで作成
        RandomGenerator.Initialize();

        // ゲームモードのオプションロード
        ExtremeGameModeManager.Instance.Load();

        // 各役職を設定を読み込んで初期化する
        Roles.ExtremeRoleManager.Initialize();
        GhostRoles.ExtremeGhostRoleManager.Initialize();

        // 各種マップモジュール等のオプション値を読み込む
        Patches.MiniGame.VitalsMinigameUpdatePatch.LoadOptionValue();
        Patches.MiniGame.SecurityHelper.LoadOptionValue();
        Patches.MapOverlay.MapCountOverlayUpdatePatch.LoadOptionValue();

        MeetingReporter.Reset();
    }

    public static void ShareOption(int numberOfOptions, MessageReader reader)
    {
        try
        {
            for (int i = 0; i < numberOfOptions; i++)
            {
                int optionId = reader.ReadPackedInt32();
                int selection = reader.ReadPackedInt32();
                Instance.rpcValueSync(optionId, selection);
            }
        }
        catch (Exception e)
        {
            Logging.Error($"Error while deserializing options:{e.Message}");
        }
    }

    private static void shareOption<T>(TypeOptionHolder<T> holder)
        where T :
            struct, IComparable, IConvertible,
            IComparable<T>, IEquatable<T>
    {
        var splitOption = holder.Select((x, i) =>
            new { data = x, indexgroup = i / chunkSize })
            .GroupBy(x => x.indexgroup, x => x.data)
            .Select(y => y.Select(x => x));

        foreach (var chunkedOption in splitOption)
        {
            using (var caller = RPCOperator.CreateCaller(
                RPCOperator.Command.ShareOption))
            {
                caller.WriteByte((byte)chunkedOption.Count());
                foreach (var (id, option) in chunkedOption)
                {
                    caller.WritePackedInt(id);
                    caller.WritePackedInt(option.CurSelection);
                }
            }
        }
    }
}
