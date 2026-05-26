# AGENTS.md

## 会話・運用ルール

- 常に日本語で会話する。
- このリポジトリは既存 Web 版を使わず、Dalamud 上だけで完結するプラグイン版として扱う。
- 変更があったら README.md と AGENTS.md を先に更新してからコミットする。
- サブPCでも追跡できるよう、意味のある区切りでコミット・プッシュする。
- 原則として TDD で進め、期待する入出力のテストを先に書く。
- 翻訳や FFXIV 公式用語があやふやな場合は推測で確定せず確認する。

## 目的

- ACT / OverlayPlugin / 既存 Web UI を使わず、Dalamud プラグインとしてフロントライン用情報を扱う。
- 戦況表示に必要な K/D・与ダメージは Core の戦況モデルで集計し、Dalamud の ImGui ウィンドウに表示する。

## フォルダ構成

- `src/FrontlineOverlay.Plugin.Core/`
  - Dalamud に依存しない戦況スナップショットと集計処理。
- `src/FrontlineOverlay.Plugin/`
  - Dalamud プラグイン本体。`/flopjp` コマンド、ImGui 画面、設定画面を持つ。
- `tests/FrontlineOverlay.Plugin.Tests/`
  - Core 層の回帰テスト。
- `.github/workflows/build.yml`
  - Windows 上で Core テストを実行し、`DALAMUD_HOME` がある場合だけプラグインビルドも実行する CI。
- `FrontlineOverlay.Plugin.sln`
  - .NET ソリューション。
- `global.json`
  - 手元の Dalamud dev DLL に合わせ、.NET SDK `10.0.100-rc.2.25502.107` を使用するための固定設定。

## 2026-05-26 作業ログ

- `frontline-overlay-jp` は既存の Web/ACT OverlayPlugin 版として維持し、新しく `frontline-overlay-jp-plugin` を作る方針にした。
- GitHub CLI はインストール済みだが、現在の Codex プロセスでは PATH へ未反映だったため `C:\Program Files\GitHub CLI\gh.exe` を直接確認した。
- `gh` は未ログインだったため、リモート作成はローカル初期化後に認証経路を確認する。
- Dalamud 公式ドキュメントを確認し、`Dalamud.NET.Sdk/15.0.0` を採用した。ローカルの Dalamud dev DLL が `System.Runtime 10.0` を参照しているため、ターゲットは .NET 10 系にした。
- TDD の最初の段階として、OverlayPlugin 互換イベントと戦況スナップショットのテストを先に追加する。
- Core 実装として `ChangeZone` / `ChangePrimaryPlayer` / `LogLine` 互換 JSON と、現在戦況表示向けの `BattleSnapshot` を追加した。
- Dalamud プラグイン本体を追加し、`/flopjp` コマンド、設定ウィンドウ、メインウィンドウ、ローカル WebSocket ブリッジを作成した。
- 初期ブリッジ URL は `ws://127.0.0.1:47774/frontline-overlay-jp/`。実戦ログ・K/D・与ダメージ取得は Dalamud 上で取得可能なイベントを確認してから追加する。
- DalamudPackager は `Authors` ではなく `Author` プロパティを要求したため、csproj のマニフェスト項目を修正した。
- Dalamud.NET.Sdk は既定の XIVLauncher dev ディレクトリまたは `DALAMUD_HOME` を必要とするため、GitHub Actions は Core テストを常時実行し、プラグインビルドは `DALAMUD_HOME` がある環境だけで実行する設定にした。
- `HttpListener` は Windows の URL 予約に引っかかる可能性があるため、ブリッジをループバック限定の `TcpListener` ベース WebSocket 実装へ変更した。
- ユーザー要望により方針を変更し、既存 Web 版やローカルブリッジを使わず Dalamud 上だけで完結させる。まず Core テストを戦況集計モデルへ差し替える。
