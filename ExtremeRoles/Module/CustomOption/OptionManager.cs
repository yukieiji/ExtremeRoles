using Hazel;
using System;
using System.Collections.Generic;
using System.Linq;

using ExtremeRoles.Helper;
using ExtremeRoles.GameMode;
using ExtremeRoles.Module.RoleAssign;

namespace ExtremeRoles.Module.CustomOption;

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

    private bool isBlockShare = false;
    private int selectedPreset = 0;

    private const int chunkSize = 50;

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
        if (option is IValueOption<int> intOption)
        {
            Add(id, intOption);
        }
        else if (option is IValueOption<bool> boolOption)
        {
            Add(id, boolOption);
        }
        else if (option is IValueOption<float> floatOption)
        {
            Add(id, floatOption);
        }
    }

    public bool Contains(int id) => this.allOptionId.ContainsKey(id);

    public bool TryGet<T>(int id, out IValueOption<T> option)
        where T :
            struct, IComparable, IConvertible,
            IComparable<T>, IEquatable<T>
    {
        bool result = this.allOptionId.TryGetValue(id, out ValueType type);
        option = null;

        if (!result) { return false; }

        return type switch
        {
            ValueType.Int => this.intOption.TryGetValue(id, out option),
            ValueType.Float => this.floatOption.TryGetValue(id, out option),
            ValueType.Bool => this.boolOption.TryGetValue(id, out option),
            _ => false
        };
    }

    public bool TryGetIOption(int id, out IOptionInfo option)
    {
        bool result = this.allOptionId.TryGetValue(id, out ValueType type);
        option = null;

        if (!result) { return false; }

        option = type switch
        {
            ValueType.Int => this.intOption.Get(id),
            ValueType.Float => this.floatOption.Get(id),
            ValueType.Bool => this.boolOption.Get(id),
            _ => null
        };
        return true;
    }

    public IValueOption<T> Get<T>(int id, ValueType type)
        where T :
            struct, IComparable, IConvertible,
            IComparable<T>, IEquatable<T>

        => type switch
        {
            ValueType.Int   => this.intOption.Get(id) as IValueOption<T>,
            ValueType.Float => this.floatOption.Get(id) as IValueOption<T>,
            ValueType.Bool  => this.boolOption.Get(id) as IValueOption<T>,
            _ => null
        };

    public IEnumerable<KeyValuePair<int, IOptionInfo>> GetKeyValueAllIOptions()
    {
        foreach (var (id, key) in this.allOptionId)
        {

            IOptionInfo info = key switch
            {
                ValueType.Int => this.intOption.Get(id),
                ValueType.Float => this.floatOption.Get(id),
                ValueType.Bool => this.boolOption.Get(id),
                _ => null
            };
            yield return new KeyValuePair<int, IOptionInfo>(id, info);
        }
    }

    public IEnumerable<IOptionInfo> GetAllIOption()
    {
        foreach (var (id, key) in this.allOptionId)
        {
            yield return  key switch
            {
                ValueType.Int => this.intOption.Get(id),
                ValueType.Float => this.floatOption.Get(id),
                ValueType.Bool => this.boolOption.Get(id),
                _ => null
            };
        }
    }

    public IOptionInfo GetIOption(int id)
        => this.allOptionId[id] switch
        {
            ValueType.Int => this.intOption.Get(id),
            ValueType.Float => this.floatOption.Get(id),
            ValueType.Bool => this.boolOption.Get(id),
            _ => null
        };

    public string GetHudString(int id)
    {
        ValueType type = this.allOptionId[id];

        return type switch
        {
            ValueType.Int => this.intOption.Get(id).ToHudString(),
            ValueType.Float => this.floatOption.Get(id).ToHudString(),
            ValueType.Bool => this.boolOption.Get(id).ToHudString(),
            _ => string.Empty,
        };
    }

    public string GetHudStringWithChildren(int id)
    {
        ValueType type = this.allOptionId[id];

        return type switch
        {
            ValueType.Int => this.intOption.Get(id).ToHudString(),
            ValueType.Float => this.floatOption.Get(id).ToHudString(),
            ValueType.Bool => this.boolOption.Get(id).ToHudString(),
            _ => string.Empty,
        };
    }

    public T GetValue<T>(int id)
        where T :
            struct, IComparable, IConvertible,
            IComparable<T>, IEquatable<T>
    {
        ValueType type = this.allOptionId[id];

        switch (type)
        {
            case ValueType.Int:
                var intOption = this.intOption.Get(id) as IValueOption<T>;
                return intOption!.GetValue();
            case ValueType.Float:
                var floatOption = this.floatOption.Get(id) as IValueOption<T>;
                return floatOption!.GetValue();
            case ValueType.Bool:
                var boolOption = this.boolOption.Get(id) as IValueOption<T>;
                return boolOption!.GetValue();
            default:
                return default(T);
        }
    }

    public void ExecuteWithBlockOptionShare(Action func)
    {
        this.isBlockShare = true;
        try
        {
            func.Invoke();
        }
        catch (Exception e)
        {
            ExtremeRolesPlugin.Logger.LogInfo($"BlockShareExcuteFailed!!:{e}");
        }
        this.isBlockShare = false;
    }

    public void SwitchPreset(int newPreset)
    {
        this.selectedPreset = newPreset;

        this.ExecuteWithBlockOptionShare(
            () =>
            {
                foreach (var (id, option) in this.GetKeyValueAllIOptions())
                {
					if (id == 0) { continue; }
                    option.SwitchPreset();
                }
            }
        );
        RoleAssignFilter.Instance.SwitchPreset();
    }

    public void ShareOptionSelections()
    {
        if (this.isBlockShare) { return; }

        if (PlayerControl.AllPlayerControls.Count <= 1 ||
            !AmongUsClient.Instance ||
            !AmongUsClient.Instance.AmHost ||
            !PlayerControl.LocalPlayer) { return; }

        shareOption(this.intOption);
        shareOption(this.floatOption);
        shareOption(this.boolOption);
    }

    public void Update(int id, int selection)
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
                Instance.Update(optionId, selection);
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
