# Frontline Overlay JP Plugin

FFXIV ACT を使わず、Dalamud 上だけでフロントライン向け情報を扱うための日本語プラグインです。

## 方針

- このリポジトリは Dalamud プラグインとして完結させます。
- 既存 Web 版やローカル WebSocket ブリッジには依存しません。
- 戦況表示に必要な K/D・与ダメージは Core の戦況モデルへ集約し、Dalamud の ImGui ウィンドウ上に表示します。

## 現在の構成

- `src/FrontlineOverlay.Plugin.Core/`
  - Dalamud UI から使う戦況スナップショットと集計処理を置きます。
- `src/FrontlineOverlay.Plugin/`
  - Dalamud プラグイン本体です。`/flopjp` コマンド、設定画面、戦況ウィンドウを持ちます。
- `tests/FrontlineOverlay.Plugin.Tests/`
  - Core のイベント契約を固定する xUnit テストを置きます。

## 開発

```powershell
dotnet test
dotnet build src/FrontlineOverlay.Plugin/FrontlineOverlay.Plugin.csproj
```

このリポジトリは TDD で進めます。まず Core の戦況集計をテストし、Dalamud UI はそのスナップショットを表示します。

ローカルの Dalamud dev DLL が .NET 10 を参照しているため、ターゲットフレームワークは `net10.0` / `net10.0-windows` にしています。現時点では手元の SDK に合わせて `10.0.100-rc.2.25502.107` を `global.json` に固定しています。

プラグイン本体のビルドには Dalamud の dev DLL が必要です。既定パスにない場合は、`DALAMUD_HOME` に `Hooks/dev` 相当のディレクトリを指定してください。GitHub Actions では Core テストを常時実行し、プラグインビルドは `DALAMUD_HOME` が設定された環境だけで実行します。

## 注意

Dalamud プラグインとして動作させるため、FFXIV クライアント側では Dalamud/XIVLauncher が必要です。ACT、OverlayPlugin、既存 Web 版は不要です。

## ビルド成果物

`dotnet build src/FrontlineOverlay.Plugin/FrontlineOverlay.Plugin.csproj --configuration Release` 後、DalamudPackager が `src/FrontlineOverlay.Plugin/bin/Release/FrontlineOverlay.Plugin/latest.zip` を生成します。
