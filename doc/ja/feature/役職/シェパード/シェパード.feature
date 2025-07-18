# language: ja
機能: シェパード

    シェパードはジャッカル陣営の役職です。
    特定の条件下でジャッカルに変化することができます。

    シナリオ: シェパードがジャッカルを見ることができない
        前提 ゲーム開始時にジャッカルが存在している
        * ゲーム開始時にシェパードが存在している
        * シェパードがタスクを持たない
        もし ジャッカルのプレイヤーをゲームプレイ中に見る
        ならば シェパードはジャッカルを見ることができない

    シナリオ: シェパードがジャッカルを見ることができる
        前提 ゲーム開始時にジャッカルが存在している
        * ゲーム開始時にシェパードが存在している
        * シェパードがタスクを持つ
        もし シェパードが<一定値>％以上タスクを終わらせる
        * ジャッカルのプレイヤーをゲームプレイ中に見る
        ならば シェパードはジャッカルを見ることができる

        例:
            | 一定値 |
            | 30 |
            | 3 |

    シナリオ: シェパードのキルボタンが<有無>
        前提 ゲーム開始時にジャッカルが存在している
        * ゲーム開始時にシェパードが存在している
        * シェパードのオプションでキルボタンを<有無>にする
        もし シェパードプレイヤーがスポーンする
        ならば シェパードプレイヤーのキルボタンが<有無>

        例:
            | 有無 |
            | 有る |
            | なし |

    シナリオ: シェパードのベントボタンが<有無>
        前提 ゲーム開始時にジャッカルが存在している
        * ゲーム開始時にシェパードが存在している
        * シェパードのオプションでベントボタンを<有無>にする
        もし シェパードプレイヤーがスポーンする
        ならば シェパードプレイヤーのベントボタンが<有無>

        例:
            | 有無 |
            | 有る |
            | なし |

    シナリオ: シェパードの勝利
        前提 ゲーム開始時にジャッカルが存在している
        * ゲーム開始時にシェパードが存在している
        もし ジャッカルが勝利する
        ならば シェパードが勝利する

    シナリオ: フォールバック役職のシェパード
        前提: ジャッカルのスポーンが0％より高く100％より低い
        * シェパードがスポーン率が0％より高い設定になっている
        もし ジャッカルがアサインが<有無>
        ならば シェパードもアサインが<有無>

        例:
            | 有無 |
            | される |
            | されない |

     シナリオ: サブチーム役職のシェパード
        前提: ジャッカルがゲーム中に存在している
        * シェパードがゲーム中に存在している
        * シェパードのキルボタンが有効になっている
        もし シェパードのプレイヤーが勝利条件を満たしたとき
        ならば そのシェパードのプレイヤーが勝利する
        * シェパードを除くすべてのジャッカル陣営も勝利する

    シナリオ: サブチームかつフォールバック役職のシェパード
        前提: ジャッカルのスポーンが0％である
        * シェパードがスポーン率が0％より高い設定になっている
        もし シェパードのキルボタンが<有無>
        ならば シェパードのアサインが<有無>になる

        例:
            | 有無 |
            | 有効 |
            | 無効 |
