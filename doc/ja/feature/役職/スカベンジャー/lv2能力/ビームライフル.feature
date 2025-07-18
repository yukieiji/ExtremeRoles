# language: ja

機能: ビームライフル能力

    シナリオ: ビームライフルの発動条件
        前提 スカベンジャーが存在しており
        * ビームライフルが合成済み
        * ビームライフル能力が選択済み
        * クールタイムが終了している
        もし プレイヤーがボタンをクリックする
        * もしくは、Fキーを押す
        ならば ビームがスカベンジャープレイヤーの前方に<速度>で現れる

        例:
            | スカベンジャープレイヤー | 速度 |
            | yukieiji | 5 |
            | player1 | 3 |


    シナリオ: ビームの動作
        前提 ビームが前進中である
        もし ビームが<特定距離>まで進む
        ならば ビームは自動的に消滅する

        例:
            | 一定距離 |
            | 10 |
            | 25 |

    シナリオ: ビームのキル
        前提「ビーム」が前進中である
        もし <プレイヤー>が「ビーム」にふれる
        ならば <プレイヤー>がキルされる

        例:
            | プレイヤー |
            | yukieiji |
            | player1 |