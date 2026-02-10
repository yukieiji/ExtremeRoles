## Release 1.0

### New Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|--------------------
ERA001 |  Usage  |  Error | ConstructorAnalyzer, Il2CppObjectの継承時のコンストラクタが間違っているかを確認する
ERA002 |  Usage  |  Error | ConstructorAnalyzer, Il2CppObjectの継承時のIl2CppRegisterの設定ミスを確認する
ERA003 |  Usage  |  Error | UnityEngine.Vector同士の比較ミスを確認する
ERA004 | Usage | Warning | Vector.IsCloseToとVector.IsNotCloseToの第2引数の値が0.1以上の場合にワーニングが出るようにする
