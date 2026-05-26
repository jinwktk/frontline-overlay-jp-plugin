# AGENTS.md

## 会話・運用ルール

- 常に日本語で会話する。
- このリポジトリは `frontline-overlay-jp` の既存 Web 版を壊さず、別管理の Dalamud プラグイン版として扱う。
- 変更があったら README.md と AGENTS.md を先に更新してからコミットする。
- サブPCでも追跡できるよう、意味のある区切りでコミット・プッシュする。
- 原則として TDD で進め、期待する入出力のテストを先に書く。
- 翻訳や FFXIV 公式用語があやふやな場合は推測で確定せず確認する。

## 目的

- ACT / OverlayPlugin を使わず、Dalamud プラグインとしてフロントライン用オーバーレイ情報を扱う。
- 既存の `frontline-overlay-jp` と互換性のあるイベント契約を Core 層に置き、プラグイン本体から同じデータを出せるようにする。

## フォルダ構成

- `src/FrontlineOverlay.Plugin.Core/`
  - Dalamud に依存しないイベントモデル、戦況スナップショット、JSON 変換処理。
- `tests/FrontlineOverlay.Plugin.Tests/`
  - Core 層の回帰テスト。
- `FrontlineOverlay.Plugin.sln`
  - .NET ソリューション。
- `global.json`
  - .NET SDK 9.0.301 を使用するための固定設定。

## 2026-05-26 作業ログ

- `frontline-overlay-jp` は既存の Web/ACT OverlayPlugin 版として維持し、新しく `frontline-overlay-jp-plugin` を作る方針にした。
- GitHub CLI はインストール済みだが、現在の Codex プロセスでは PATH へ未反映だったため `C:\Program Files\GitHub CLI\gh.exe` を直接確認した。
- `gh` は未ログインだったため、リモート作成はローカル初期化後に認証経路を確認する。
- Dalamud 公式ドキュメントを確認し、現在は `Dalamud.NET.Sdk/15.0.0` と .NET 9 系で進める方針にした。
- TDD の最初の段階として、OverlayPlugin 互換イベントと戦況スナップショットのテストを先に追加する。
