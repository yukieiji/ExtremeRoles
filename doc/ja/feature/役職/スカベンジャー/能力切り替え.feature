# language: ja

機能: 能力の切り替え
    シナリオ: 能力がない切り替え
        前提 スカベンジャーがいる
        もし ホイールを回す
        ならば 何も起きない

    シナリオ: 能力を幾つか持っているときの切り替え
        前提 スカベンジャーがいる
        もし ホイールを回す
        ならば スカベンジャーの能力が切り替わる
