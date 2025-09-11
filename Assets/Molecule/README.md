# VRChat 分子可視化システム

WebソースからXYZ分子データをダウンロードし、コンピュートシェーダーを使用して3DボールアンドスティックモデルをレンダリングするUnity/VRChat分子可視化システムです。

## 必要な参考URL

- Unity Editor (Archive): https://unity.com/releases/editor/archive (2022.3.22f1を使用)
- VRChat Creator Companion (VCC) Docs: https://vcc.docs.vrchat.com/
- VRChat Creators Portal: https://creators.vrchat.com/
- VRChat Udon Docs (String Loading含む): https://creators.vrchat.com/worlds/udon/
- UdonSharp Repository: https://github.com/MerlinVR/UdonSharp
- UdonSharp VPM Repository (例): https://vpm.koyashiro.net/index.json
- Unity Compute Shaders Manual: https://docs.unity3d.com/Manual/ComputeShaders.html
- XYZ File Format: https://en.wikipedia.org/wiki/XYZ_file_format
- CCDC Structures: https://www.ccdc.cam.ac.uk/structures/
  - Example Mol endpoint: https://www.ccdc.cam.ac.uk/structures/Data/Mol?id=1133493&databaseId=0

## 機能

- **WebベースのXYZデータロード** VRChatのString Loading APIを使用
- **リアルタイム レイマーチングレンダリング** コンピュートシェーダー使用
- **ボールアンドスティック分子表現** 元素固有の色
- **インタラクティブUI** ドロップダウン分子選択
- **一般的な分子のサポート** (エタノール、水、ベンゼン、拡張可能)

## セットアップ手順

### 1. VRChat SDKセットアップ
1. UnityでVRChat SDK - Worldsをインストール
2. UdonSharpパッケージをインストール
3. koyashiroのVPMリポジトリを追加: `https://vpm.koyashiro.net/index.json`

### 2. シーンセットアップ
1. 新しいシーンを作成するか、既存のVRChatワールドを使用
2. 空のGameObjectを作成し、以下をアタッチ:
   - `MoleculeDownloader` スクリプト
   - `MoleculeRaymarchDriver` スクリプト
   - `MoleculeUI` スクリプト

### 3. UIセットアップ
1. 以下を含むCanvasを作成:
   - **TMP_Dropdown** 分子選択用
   - **RawImage** レンダリングされた分子の表示用
   - **Button** 選択された分子のロード用
2. UIコンポーネントを`MoleculeUI`スクリプトに接続
3. `MoleculeUI`をダウンローダーとドライバースクリプトに接続

### 4. コンピュートシェーダーセットアップ
1. `MoleculeRaymarch.compute`シェーダーを`MoleculeRaymarchDriver`に割り当て
2. レンダリング設定を構成（解像度、原子スケール、ライティング）
3. カメラトランスフォーム参照を設定

### 5. URL構成
`MoleculeDownloader`で分子URLを構成:
```csharp
public string[] keys = {"ethanol", "water", "benzene"};
public string[] urls = {
    // 公開ホスティングされたXYZを直接参照
    "https://example.cdn/molecules/ethanol.xyz",
    "https://example.cdn/molecules/water.xyz", 
    "https://example.cdn/molecules/benzene.xyz"
};
```

CCDCのMol（V2000）をID指定で取得して自動的にXYZへ変換することも可能です。
```csharp
// テンプレートはInspectorで編集可（既定値）
downloader.ccdcUrlTemplate = "https://www.ccdc.cam.ac.uk/structures/Data/Mol?id={id}&databaseId=0";

// 呼び出し例（UIや他スクリプトから）
downloader.LoadCcdcById("1133493"); // テンプレートからURL生成
// もしくは完全なURLを直接指定（ccdc-checkが必要な場合など）
downloader.LoadByUrl("https://www.ccdc.cam.ac.uk/structures/Data/Mol?id=1133493&databaseId=0&ccdc-check=...");
```

注意:
- ダウンローダーはXYZ/MOL(V2000)を自動判定し、XYZに正規化して描画に渡します。
- CCDCはアクセス制御や一時トークン（`ccdc-check`）を用いる場合があります。URLが期限切れになる可能性があります。
- VRChat実行時のリモート文字列ロードはHTTPSと適切なヘッダ（CORS）が必要です。Unityエディタで取得できても、VRChat内ではCORSで失敗する場合があります。

## 使用方法

### ローカルテスト
1. `MoleculeUI`で`useLocalTestData = true`に設定
2. テストデータファイルは`TestData/`フォルダに含まれています
3. テストには`LoadLocalTestData()`メソッドを使用

### VRChatデプロイメント
1. CORS対応のWebサーバーでXYZファイルをホスト
2. `MoleculeDownloader`でURLを更新
3. `useLocalTestData = false`に設定
4. VRChatにビルドしてアップロード

## ファイル構造

```
Assets/Molecule/
├── Scripts/
│   ├── MoleculeDownloader.cs      # Web ダウンロード & XYZ処理
│   ├── MoleculeRaymarchDriver.cs  # コンピュートシェーダーコントローラー
│   └── MoleculeUI.cs              # UI管理
├── Shaders/
│   └── MoleculeRaymarch.compute   # レイマーチングレンダラー
└── TestData/
    ├── ethanol.xyz                # テスト分子
    ├── water.xyz
    └── benzene.xyz
```

## XYZフォーマットサポート

システムは標準のXYZ分子フォーマットをサポート:
```
[atom_count]
[comment_line]
[element] [x] [y] [z]
[element] [x] [y] [z]
...
```

例（水）:
```
3
water
O   0.000   0.000   0.000
H   0.757   0.586   0.000
H  -0.757   0.586   0.000
```

## サポートされている元素

- H (水素) - 白
- C (炭素) - 濃い灰色  
- N (窒素) - 青
- O (酸素) - 赤
- F (フッ素) - シアン
- P (リン) - オレンジ
- S (硫黄) - 黄
- Cl (塩素) - 緑

## 構成オプション

### MoleculeRaymarchDriver
- `resolution`: レンダーターゲット解像度
- `atomScale`: 原子球のスケーリング係数
- `bondScale`: 結合のスケーリング係数
- `bondDistanceThreshold`: 結合検出の最大距離
- `lightDirection`: ライティング方向ベクトル

### パフォーマンス注記
- より大きな分子はパフォーマンスに影響する可能性があります
- 複雑な分子の場合は解像度を下げることを検討
- 結合検出は距離閾値に基づいています
- コンピュートシェーダーは小〜中規模の分子用に最適化

## システムの拡張

### 新しい分子の追加
1. `keys`配列に分子名を追加
2. `urls`配列に対応するURLを追加
3. WebサーバーでXYZファイルをホスト

### 新しい元素の追加
1. `ElementMap`辞書を更新
2. `ElementColors`と`ElementRadii`配列に色と半径を追加
3. 再コンパイルしてテスト

## トラブルシューティング

- **分子が見えない**: カメラ位置と分子スケールを確認
- **ダウンロード失敗**: URLアクセス可能性とCORS設定を確認
- **パース エラー**: XYZファイル形式が正しいことを確認
- **パフォーマンス問題**: 解像度または原子数を減らす

## 依存関係

- Unity 2022.3+ (VRChat対応)
- VRChat SDK - Worlds
- UdonSharp
- VRChat String Loading API

## セットアップ補助 / ヘルスチェック

- `Tools > Molecule > Setup Test Scene`/`Setup VRChat Scene`: UIとコンポーネントを自動配置。Compute Shaderは自動検出して割当を試みます。
- `Tools > Molecule > Validate Project Health`: 依存関係（VRChat SDK/UdonSharp）、Resources配置、ComputeShader/カメラの割当状況を検査し、可能な範囲で自動修復します。
