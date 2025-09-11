# molecularshader（ミニマル版）

- 概要: Unity 上で XYZ 形式の分子データを Compute Shader によりレイマーチング描画する、最小構成のプロジェクトです。VRChat/Udon/UI などの付加機能は削除し、コア描画と簡易モック、PlayMode テストのみを残しています。
- Unity: 2022.3.22f1
- シーン: `Assets/Scenes/VRCDefaultWorldScene.unity`

## 主な機能（最小）
- XYZ ローディング（簡易モック）: `Resources/TestMolecules` のサンプルを `EthanolMock` 経由で `MoleculeRaymarchDriver` にセット。
- レイマーチング描画: Compute Shader で原子（球）と結合（カプセル）をスクリーンスペースにレンダリング。
- 自動結合推定: 距離しきい値（既定 `1.8`）による簡易結合生成。

## クイックスタート
1. Unity `2022.3.22f1` でプロジェクトを開く。
2. シーン `Assets/Scenes/VRCDefaultWorldScene.unity` を開く。
3. シーン内の任意の GameObject に `MoleculeRaymarchDriver` と `EthanolMock` を追加（`EthanolMock.driver` にドライバを割当）。
4. 再生（Play）。自動で `Resources/TestMolecules/ethanol` が読み込まれ、1 フレーム後にパース・描画されます。

## ディレクトリ構成（最小）
- `Assets/Molecule/Scripts/`
  - `MoleculeRaymarchDriver.cs` — レンダリング・XYZ パース・バッファ管理
  - `EthanolMock.cs` — `Resources/TestMolecules` から簡易ロード
- `Assets/Molecule/Resources/Shaders/`
  - `MoleculeRaymarch.compute` — 描画用 Compute Shader（Resources 配下の単一コピー）
- `Assets/Molecule/Resources/TestMolecules/`
  - `ethanol.xyz`, `water.xyz`, `benzene.xyz` — サンプルデータ
- `Assets/Molecule/Tests/PlayMode/`
  - `EthanolMockPlayModeTests.cs` — 読み込みと結合生成の基本検証

## カスタマイズ
- ドライバ（`MoleculeRaymarchDriver`）のパラメータで解像度・スケール・カメラ・ライティング・しきい値を調整。
- 元素の色・半径は `MoleculeRaymarchDriver` 内の `ElementColors` / `ElementRadii` を編集。

## テスト
- Unity Test Runner → PlayMode → Run All
- CLI 例: `Unity -batchmode -projectPath . -runTests -testPlatform PlayMode -logFile Logs/tests.log -testResults Logs/results.xml -quit`

## 変更点（このミニマル化で削除したもの）
- VRChat/Udon 関連コードと Editor ユーティリティ（メニュー、ヘルスチェック等）
- 重複していた Compute Shader の片方（`Assets/Molecule/Shaders/`）
- `Assets/Molecule/TestData/`（Resources に統合）
- `Molecule` アセンブリの TextMeshPro 参照（不要のため除去）

## 既知の制限
- 対応元素は `H, C, N, O, F, P, S, Cl`（未定義は `C` を使用）。
- 結合は距離しきい値による簡易推定であり、化学的厳密性は保証しません。

## 参考
- Unity Compute Shader: https://docs.unity3d.com/Manual/ComputeShaders.html
- XYZ ファイル形式: https://en.wikipedia.org/wiki/XYZ_file_format
