# Molecule Renderer for Unity/VRChat

このリポジトリは、XYZ/MOL 形式の分子データを Unity 上でレイマーチング描画し、VRChat ワールドでも活用できるようにしたプロジェクトです。Compute Shader による球（原子）/カプセル（結合）のスクリーンスペース描画、簡易ローダー、Udon 向けダウンローダー/UI レンダラー、そして PlayMode テストを含みます。

- Unity: 2022.3.22f1
- メインシーン: `Assets/Scenes/VRCDefaultWorldScene.unity`

English follows after Japanese.

## 機能概要
- 分子データ読込: XYZ（および MOL(V2000) を XYZ へ簡易変換）
- レイマーチング描画: `MoleculeRaymarch.compute` による原子/結合の描画
- 自動結合推定: 距離しきい値（既定 `1.8Å`）で近接原子を結合
- 空間正規化: 原点センタリング/スケール合わせを `MoleculeSpatialConfig` で制御
- VRChat 連携: `MoleculeDownloader`（UdonSharp）で URL からダウンロード、`MoleculeUdonRenderer` で簡易 2D 可視化

## はじめに（クイックスタート）
1. Unity `2022.3.22f1` でプロジェクトを開く。
2. シーン `Assets/Scenes/VRCDefaultWorldScene.unity` を開く。
3. メニュー `Tools > Molecule > Validate Project Health` でセットアップを確認。
4. 再生（Play）。`EthanolMock` を使う場合は、任意の GameObject に `MoleculeRaymarchDriver` と `EthanolMock` を追加し、`EthanolMock.driver` にドライバを割り当てると `Resources/TestMolecules/ethanol.xyz` が読み込まれます。

ヒント: `Tools > Molecule > Create Demo Setup` でデモ環境（カメラ/Canvas/プレビュー）を自動生成できます。

## VRChat での利用（任意）
- `Assets/Molecule/VRChat/` 内のコンポーネントを利用します。
  - `MoleculeDownloader`: URL から XYZ/MOL を取得し、XYZ に正規化します。
  - `MoleculeUdonRenderer`: RectTransform 配下にドットを配置し、簡易 2D プロットを行います。
- 使い方の一例:
  - UI Canvas に空の `RectTransform` を用意し、`MoleculeUdonRenderer.atomContainer` に割り当て。
  - 別の GameObject に `MoleculeDownloader` を追加し、`renderer` に上記 `MoleculeUdonRenderer` を割り当て。
  - `keys`/`urls` を必要に応じて設定し、`LoadSelected()` を呼び出すとダウンロード→描画されます。
  - エディタ内でローカルファイルを試す場合は、`file://` から始まるパスを `LoadByUrl()` に渡せます（Editor 限定）。

## プロジェクト構成
- `Assets/Molecule/Scripts/`
  - `MoleculeRaymarchDriver.cs`: 解析/結合推定/ComputeBuffer 構築/描画
  - `MoleculeRenderPreview.cs`: `RenderTexture` を `RawImage`/`Material` にバインド
  - `MoleculeSpatialConfig.cs`: センタリング/スケール/結合半径などの設定（`Create > Molecule > Spatial Config`）
  - `EthanolMock.cs`: `Resources/TestMolecules` のサンプルをドライバへ投入
- `Assets/Molecule/VRChat/`
  - `MoleculeDownloader.cs`: URL 取得（VRCStringDownloader）と XYZ 正規化
  - `MoleculeUdonRenderer.cs`: 簡易 UI レンダリング
- `Assets/Molecule/Resources/Shaders/`
  - `MoleculeRaymarch.compute`: レイマーチング本体（Resources から自動参照）
- `Assets/Molecule/Resources/TestMolecules/`
  - `ethanol.xyz`, `water.xyz`, `benzene.xyz`: サンプルデータ
- `Assets/Molecule/Tests/PlayMode/`
  - `EthanolMockPlayModeTests.cs`: サンプル読込と結合生成の検証
  - `SpatialNormalizationTests.cs`: センタリング/スケールの検証

## 開発・ビルド・テスト
- セットアップ検証: Unity メニュー `Tools > Molecule > Validate Project Health`
- テスト（GUI）: Unity Test Runner → PlayMode → Run All
- テスト（CLI）: `Unity -batchmode -projectPath . -runTests -testPlatform PlayMode -logFile Logs/tests.log -testResults Logs/results.xml -quit`

## カスタマイズのポイント
- ドライバの調整: 解像度、FOV/クリップ、ライティング、結合距離、要素色/半径（`MoleculeRaymarchDriver` 内の配列）
- 空間設定: `MoleculeSpatialConfig` で原点センタリング/フィットまたは Å→m 換算を選択
- 表示先: `MoleculeRenderPreview` を使い `RawImage`/`MeshRenderer` に表示

## トラブルシュート
- Compute Shader 未割当: `Resources/Shaders/MoleculeRaymarch` を自動参照します。見つからない場合は Inspector で割り当ててください。
- カメラ未割当: `cameraTransform` にカメラ Transform を割り当てるか、メインカメラを配置してください。
- サンプル未検出: `Tools > Molecule > Validate Project Health` で不足ファイルを確認。
- Compute 非対応: 環境が Compute Shader に非対応の場合、描画はスキップされます。

## 既知の制限
- 要素対応は `H, C, N, O, F, P, S, Cl`。未定義要素は Carbon として扱います。
- 結合は距離しきい値ベースの簡易推定であり、化学的厳密さは保証しません。

## コーディング/コントリビュート指針（要約）
- C#: インデント 4 スペース、1 ファイル 1 パブリック型、シリアライズは `public` もしくは `[SerializeField] private` を推奨。
- 命名: 型/メソッド/Serialized フィールドは PascalCase、ローカル/プライベートは camelCase。
- PR: 小さく焦点化、必要に応じてスクショ/短尺動画とテスト結果を添付。`Library/` や `Temp/` はコミットしないでください。
- 依存関係: VRChat SDK/Udon/UdonSharp は VCC 管理。`Packages/manifest.json` は手動編集しないでください。

---

# Molecule Renderer for Unity/VRChat (English)

This project renders molecular data (XYZ and MOL→XYZ) in Unity using a compute-shader raymarcher, and includes lightweight Udon components for use in VRChat worlds. It ships with samples, a demo menu, and PlayMode tests.

- Unity: 2022.3.22f1
- Main scene: `Assets/Scenes/VRCDefaultWorldScene.unity`

## Highlights
- Loading: XYZ, plus a simple MOL(V2000)→XYZ converter
- Rendering: `MoleculeRaymarch.compute` draws atoms/bonds via raymarching
- Bonds: proximity threshold (default `1.8 Å`)
- Spatial: centering and scaling via `MoleculeSpatialConfig`
- VRChat: `MoleculeDownloader` (Udon) pulls text from URLs; `MoleculeUdonRenderer` plots simple 2D dots

## Quick Start
1. Open with Unity `2022.3.22f1`.
2. Open `Assets/Scenes/VRCDefaultWorldScene.unity`.
3. Run `Tools > Molecule > Validate Project Health`.
4. Press Play. For a built-in sample, add `MoleculeRaymarchDriver` and `EthanolMock` to a GameObject and assign `EthanolMock.driver`.

Tip: `Tools > Molecule > Create Demo Setup` scaffolds a ready-to-run preview.

## VRChat Usage (optional)
- Place `MoleculeUdonRenderer` on a RectTransform container and `MoleculeDownloader` on another GameObject. Assign `renderer` and configure `keys`/`urls`.
- Call `LoadSelected()` or `LoadByUrl()`. Editor-only local files are supported via `file://` URLs.

## Structure
- `Assets/Molecule/Scripts/`: core driver, preview binder, spatial config, sample loader
- `Assets/Molecule/VRChat/`: URL downloader (Udon), simple UI renderer
- `Assets/Molecule/Resources/Shaders/`: compute shader
- `Assets/Molecule/Resources/TestMolecules/`: sample XYZ files
- `Assets/Molecule/Tests/PlayMode/`: PlayMode tests (UnityTest/NUnit)

## Build & Test
- Validate: `Tools > Molecule > Validate Project Health`
- Tests (GUI): Unity Test Runner → PlayMode → Run All
- Tests (CLI): `Unity -batchmode -projectPath . -runTests -testPlatform PlayMode -logFile Logs/tests.log -testResults Logs/results.xml -quit`

## Customize
- Driver: resolution, camera/FOV/clip, lighting, bond distance, element color/radius arrays
- Spatial: choose auto-fit to target size or fixed Å→m via `MoleculeSpatialConfig`
- Output: bind driver `RenderTexture` to `RawImage` or a material via `MoleculeRenderPreview`

## Troubleshooting
- Compute shader missing: ensure `Resources/Shaders/MoleculeRaymarch` is present or assign in Inspector.
- No camera: set `cameraTransform` or add a Main Camera.
- Samples missing: run the Validate menu to see what’s absent.
- No compute support: rendering is skipped on unsupported platforms.

## Contributing
- Style: 4-space indent; one public type per file; prefer `public` or `[SerializeField] private` for Inspector wiring.
- Commits/PRs: small and focused; include screenshots or clips and test results; avoid committing `Library/` or `Temp/`.
- Dependencies: manage VRChat SDK/Udon/UdonSharp via VCC; don’t hand-edit `Packages/manifest.json`.

## Acknowledgements
Thanks for contributing and testing. If anything is unclear or inaccessible, please open an issue so we can improve documentation for everyone.
