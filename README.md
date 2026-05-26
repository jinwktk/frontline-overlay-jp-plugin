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
- `tests/FrontlineOverlay.Plugin.Tests/`
  - Core のイベント契約を固定する xUnit テストを置きます。

## 開発

```powershell
dotnet test
```

このリポジトリは TDD で進めます。最初の段階では、既存 Web 版との互換性を守るイベント JSON からテストします。

## 注意

Dalamud プラグインとして動作させるため、FFXIV クライアント側では Dalamud/XIVLauncher が必要です。ACT と OverlayPlugin は不要にする方針です。
