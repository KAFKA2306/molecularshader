# molecularshader（日本語）

- 概要: VRChat 向け Unity プロジェクト。分子構造（XYZ 形式）を GPU の Compute Shader によるレイマーチングで描画します。UdonSharp を用いて分子データの取得と UI 操作を行います。
- Unity: 2022.3.22f1
- シーン: `Assets/Scenes/VRCDefaultWorldScene.unity`

## 主な機能
- **XYZ 読み込み**: ローカルのテストデータまたは URL から XYZ 形式の分子データを読み込み。
- **レイマーチング描画**: Compute Shader で原子と結合をスクリーンスペースにレンダリング。
- **自動結合推定**: 原子間距離のしきい値（既定 `1.8`）に基づき結合を自動生成。
- **UI 連携**: ドロップダウンで分子を選択し、ボタンで読み込み。`RawImage` に結果を表示。

## 動作環境
- **Unity**: `2022.3.22f1`
- **VRChat SDK3 / Udon / UdonSharp**: 本リポジトリに同梱されています（`Assets/UdonSharp`, `VRC.*`）。
- ネットワークダウンロードは VRChat 実行時に `VRCStringDownloader` を使用します。

## クイックスタート
1. Unity `2022.3.22f1` でプロジェクトを開く。
2. シーン `Assets/Scenes/VRCDefaultWorldScene.unity` を開く。
3. 再生して UI から分子を選択し「Load」をクリック。
4. エディタでローカルテストデータを使う場合は `MoleculeUI.useLocalTestData` を有効化（既定で有効）。

## 分子データの取得
- スクリプト: `Assets/Molecule/Scripts/MoleculeDownloader.cs`
  - `keys` と `urls` の配列で分子名と取得先 URL を対応付けます。
  - エディタでは `file://` プレフィックスのローカルパスに対応（例: `file:///.../Assets/Molecule/TestData/water.xyz`）。
  - VRChat 実行時は `VRCStringDownloader.LoadUrl` によりネットワークから取得します。
- UI スクリプト: `Assets/Molecule/Scripts/MoleculeUI.cs`
  - ドロップダウン選択と「Load」ボタンから `MoleculeDownloader` を呼び出します。

## レンダリングのカスタマイズ
- 中核スクリプト: `Assets/Molecule/Scripts/MoleculeRaymarchDriver.cs`
  - **解像度**: `resolution`
  - **スケール**: `atomScale`, `bondScale`
  - **カメラ**: `cameraTransform`, `fieldOfView`, 近遠クリップ
  - **ライティング**: `lightDirection`, `lightColor`, `ambientIntensity`
  - **結合検出**: `bondDistanceThreshold`
- Compute Shader: `Assets/Molecule/Shaders/MoleculeRaymarch.compute`
- 元素設定（色・半径・対応表）は `MoleculeRaymarchDriver` 内の `ElementMap`, `ElementColors`, `ElementRadii` を編集してください。

## ディレクトリ構成（抜粋）
- `Assets/Molecule/Scripts/` — UI・ダウンロード・描画ドライバ
- `Assets/Molecule/Shaders/` — Compute Shader
- `Assets/Molecule/TestData/` — 例: `water.xyz`, `ethanol.xyz`, `benzene.xyz`
- `Assets/Molecule/Editor/` — エディタ補助

## 既知の制限
- 対応元素は現状 `H, C, N, O, F, P, S, Cl` のみ（未定義は既定で C を使用）。
- 結合は距離しきい値による簡易推定であり、化学的厳密性は保証しません。

## ライセンス
- 本リポジトリには明示的なライセンス記載がありません。利用・再配布の前にリポジトリ所有者に確認してください。
