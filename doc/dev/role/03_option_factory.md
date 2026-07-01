# オプションの設定

役職の動作をカスタマイズするためのオプション（設定項目）の作成方法を説明します。

## オプションIDの定義

通常、Roleクラス内に `Option` 列挙型を定義して管理します。

```csharp
public enum Option
{
    CoolDown,
    MaxCount,
    IsActive,
   SubOption,
}
```

## オプションの作成

`CreateSpecificOption` メソッド内で、`AutoParentSetOptionCategoryFactory` を使用してオプションを作成します。

```csharp
protected override void CreateSpecificOption(
    AutoParentSetOptionCategoryFactory factory)
{
    // 数値オプション (int)
    factory.CreateIntOption(Option.MaxCount, 3, 1, 10, 1);

    // 数値オプション (float)
    factory.CreateFloatOption(Option.CoolDown, 10.0f, 1.0f, 60.0f, 0.5f, format: OptionUnit.Second);

    // ON/OFFオプション
    var isShow = factory.CreateBoolOption(Option.IsActive, true);

    // 条件付きオプション（isShowがONの時だけ表示）
    factory.CreateIntOption(Option.SubOption, 1, 1, 5, 1, new ParentActive(isShow));
}
```

## オプション値の取得

`RoleSpecificInit` で`Loader` を通じて設定値を取得します。

```csharp
protected override void RoleSpecificInit()
{
    var loader = this.Loader;

    int maxCount = loader.GetValue<Option, int>(Option.MaxCount);
    float coolDown = loader.GetValue<Option, float>(Option.CoolDown);
    bool isActive = loader.GetValue<Option, bool>(Option.IsActive);

    // 取得した値を AbilityHandler 等に渡す
}
```

## 共通オプション

`RoleOptionBase` には、キルクールタイムや視界などの共通オプションを作成するためのヘルパーメソッドも用意されています。

- `CreateKillerOption`: キルクールとキル範囲のオプションをまとめて作成します。
- `CreateVisionOption`: 役職固有の視界設定オプションを作成します。
