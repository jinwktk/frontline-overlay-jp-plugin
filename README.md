# Frontline Overlay JP Plugin

FFXIV ACT を使わずに、`frontline-overlay-jp` 相当のフロントライン向け情報を扱うための Dalamud プラグイン版リポジトリです。

## 方針

- 既存の Web 版 `frontline-overlay-jp` は維持します。
- このリポジトリは Dalamud プラグインとして作成します。
- まずは既存 Web 版が受け取っていた `ChangePrimaryPlayer` / `ChangeZone` / `LogLine` 相当のイベント契約を Core で固定します。
- プラグイン本体は Dalamud から取得できるプレイヤー・ゾーン・戦闘情報を Core のイベントへ変換し、将来的に既存 UI へ流せる構成にします。

## 現在の構成

- `src/FrontlineOverlay.Plugin.Core/`
  - ACT/OverlayPlugin に依存しないイベントモデル、戦況スナップショット、JSON シリアライズを置きます。
- `src/FrontlineOverlay.Plugin/`
  - Dalamud プラグイン本体です。`/flopjp` コマンド、設定画面、ローカル WebSocket ブリッジを持ちます。
- `tests/FrontlineOverlay.Plugin.Tests/`
  - Core のイベント契約を固定する xUnit テストを置きます。

## 開発

```powershell
dotnet test
dotnet build src/FrontlineOverlay.Plugin/FrontlineOverlay.Plugin.csproj
```

このリポジトリは TDD で進めます。最初の段階では、既存 Web 版との互換性を守るイベント JSON からテストします。

ローカルの Dalamud dev DLL が .NET 10 を参照しているため、ターゲットフレームワークは `net10.0` / `net10.0-windows` にしています。現時点では手元の SDK に合わせて `10.0.100-rc.2.25502.107` を `global.json` に固定しています。

プラグイン本体のビルドには Dalamud の dev DLL が必要です。既定パスにない場合は、`DALAMUD_HOME` に `Hooks/dev` 相当のディレクトリを指定してください。GitHub Actions では Core テストを常時実行し、プラグインビルドは `DALAMUD_HOME` が設定された環境だけで実行します。

## ローカルブリッジ

プラグインは既定で `ws://127.0.0.1:47774/frontline-overlay-jp/` に WebSocket 接続を受け付けます。URL予約を避けるため、ブリッジは `HttpListener` ではなくループバック限定の `TcpListener` で動かします。現段階ではプラグイン起動状態とイベント契約の土台を用意しており、実戦ログ・K/D・与ダメージの取得は次の実装段階で Dalamud から取得可能なイベントを確認しながら追加します。

## 注意

Dalamud プラグインとして動作させるため、FFXIV クライアント側では Dalamud/XIVLauncher が必要です。ACT と OverlayPlugin は不要にする方針です。
