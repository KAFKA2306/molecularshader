# molecularshader（日本語）

- 概要: VRChat 向け Unity プロジェクト。分子構造（XYZ 形式）を GPU の Compute Shader によるレイマーチングで描画します。UdonSharp を用いて分子データの取得と UI 操作を行います。
- Unity: 2022.3.22f1
- シーン: `Assets/Scenes/VRCDefaultWorldScene.unity`

## 必須参照URL
- Unity エディタ（アーカイブ）: https://unity.com/releases/editor/archive （使用バージョン: 2022.3.22f1）
- VRChat クリエイター向けポータル: https://creators.vrchat.com/
- VRChat Creator Companion (VCC) ドキュメント: https://vcc.docs.vrchat.com/
- VRChat Udon ドキュメント（Udon/SDK3 一式）: https://creators.vrchat.com/worlds/udon/
- UdonSharp リポジトリ: https://github.com/MerlinVR/UdonSharp
- UdonSharp VPM リポジトリ（例）: https://vpm.koyashiro.net/index.json
- Unity Compute Shader 解説: https://docs.unity3d.com/Manual/ComputeShaders.html
- XYZ ファイル形式: https://en.wikipedia.org/wiki/XYZ_file_format
- CCDC 構造検索: https://www.ccdc.cam.ac.uk/structures/
  - 例: Mol 取得エンドポイント: https://www.ccdc.cam.ac.uk/structures/Data/Mol?id=1133493&databaseId=0

## 主な機能
- **XYZ 読み込み**: ローカルのテストデータまたは URL から XYZ 形式の分子データを読み込み。
- **レイマーチング描画**: Compute Shader で原子と結合をスクリーンスペースにレンダリング。
- **自動結合推定**: 原子間距離のしきい値（既定 `1.8`）に基づき結合を自動生成。
- **UI 連携**: ドロップダウンで分子を選択し、ボタンで読み込み。`RawImage` に結果を表示。

## 動作環境
- **Unity**: `2022.3.22f1`
- **VRChat SDK3 / Udon / UdonSharp**: 本リポジトリに同梱されています（`Assets/UdonSharp`, `VRC.*`）。
- ネットワークダウンロードは VRChat 実行時に `VRCStringDownloader` を使用します。

## VRChat SDK の導入（VCC 推奨）
- VRChat Creator Companion (VCC) で本プロジェクトを追加し、以下のパッケージを追加してください。
  - `VRChat Worlds (com.vrchat.worlds)`
  - `Udon (com.vrchat.udon)`
  - `UdonSharp (com.vrchat.udonsharp)`
  - 任意: `ClientSim` などの補助パッケージ
- 直接 `Packages/manifest.json` を編集するのではなく、VCC 経由を推奨します。

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
  - CCDC の Mol(V2000) もサポート（自動で XYZ に変換）。`ccdcUrlTemplate` に ID をはめ込み `LoadCcdcById("1133493")` などで取得可能。`ccdc-check` 付与が必要な場合は `LoadByUrl()` で完全URLを指定してください。
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
- VRChat のリモート文字列ロードでは HTTPS と許可されたヘッダが必要です。

## ヘルスチェックと自動修復
- メニュー `Tools > Molecule > Validate Project Health` で以下を検査・一部自動修正します。
  - VRChat SDK/UdonSharp の導入確認（未導入なら CRITICAL を表示）
  - `Resources/TestMolecules/ethanol.xyz` の存在確認
  - シーン内 `MoleculeRaymarchDriver` の `computeShader`/`cameraTransform` の未設定を検知し、可能なら自動割当
- 必要に応じて `Tools > Molecule > Auto-Assign Compute Shader to Drivers` を実行してください。
