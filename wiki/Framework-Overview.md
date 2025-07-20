# フレームワーク概要

**Meek**は、スレッドセーフなスタックナビゲーションをコアとした**ミドルウェアベース**のUnity UIフレームワークです。MVPアーキテクチャ、依存性注入、リアクティブプログラミングを統合し、拡張性と保守性を両立したアーキテクチャを提供します。

## パッケージ構成

```
Assets/Packages/
├── Meek/                    # コアインターフェースと基底クラス
├── Meek.NavigationStack/    # スタックナビゲーション実装
├── Meek.MVP/               # MVPアーキテクチャサポート
├── Meek.UGUI/              # Unity uGUI統合
└── Meek.VContainer/        # VContainer DI統合
```

### Meek (コアパッケージ)

コアインターフェースと基底クラスを提供。

**主要コンポーネント**:
- `Navigator` - ミドルウェアパイプライン実行オーケストレーター
- `NavigatorBuilder` - 入れ子構造ミドルウェアチェーン構築
- `IScreen` - 画面ライフサイクルインターフェース
- `IMiddleware` - 非同期ミドルウェアインターフェース
- `NavigationDelegate` - 関数型パイプラインデリゲート

### Meek.NavigationStack

スタックベースのナビゲーション実装。

**主要コンポーネント**:
- `StackNavigationService` - スレッドセーフナビゲーションAPI（SemaphoreSlim使用）
- `StackScreenContainer` - LIFOスタック管理（重複画面禁止）
- `StackNavigationContext` - 遷移情報・Features Dictionary
- **4つのコア操作**: Push/Pop/Insert/Remove
- `Dispatch機能` - スタック内全画面イベント送信

### Meek.MVP

MVPアーキテクチャのサポート機能。

**主要コンポーネント**:
- `MVPApplication` - DIコンテナとNavigator統合エントリポイント
- `MVPScreen<TModel>` - 非同期モデル初期化・リソース管理
- `MVPScreen<TModel, TParam>` - パラメーター付き画面基底クラス
- `Presenter<TModel>` - View-Modelバインディング・ライフサイクル管理
- `LoadPresenterAsync` - Resources自動ロード機能

### Meek.UGUI

Unity uGUIとの統合機能。

**主要コンポーネント**:
- `DefaultInputLocker` - EventSystemベース入力ロック実装
- `DefaultPrefabViewManager` - Resources/Addressableプレハブ管理
- `UGUIMiddleware` - Unity uGUI統合ミドルウェア
- `NavigatorAnimation` - 4種アニメーション戦略（Open/Close/Show/Hide）
- `ScreenUIType` - None/FullScreen/WindowOrTransparent

### Meek.VContainer

VContainerとの依存性注入統合。

**主要コンポーネント**:
- `VContainerServiceCollection` - サービス登録
- `VContainerServiceProvider` - サービス解決

## 主要特徴

### 🎯 型安全なナビゲーション

ジェネリクスによる型安全な画面遷移で実行時エラーを防止。

```csharp
// 型安全な画面遷移
await PushNavigation.PushAsync<UserProfileScreen>();

// パラメーター付き遷移
await PushNavigation.PushAsync<DetailScreen, UserModel>(userModel);
```

### 🔄 ミドルウェアパイプライン

設定可能な処理チェーンによる柔軟な拡張。

```csharp
navigatorBuilder
    .UseDebug()
    .UseInputLocker()
    .UseScreenUI()
    .UseNavigatorAnimation()
    .UseScreenLifecycleEvent();
```

### 🔒 スレッドセーフ

セマフォベースの並行制御により安全な操作を保証。

```csharp
// 並行実行されても安全
Task.Run(() => PushNavigation.PushAsync<ScreenA>());
Task.Run(() => PushNavigation.PushAsync<ScreenB>());
```

### ⚡ リアクティブプログラミング

UniRxによるモデル状態管理で宣言的UI構築。

```csharp
protected override IEnumerable<IDisposable> Bind(UserModel model)
{
    yield return model.Name
        .Subscribe(name => _nameText.text = name);
    
    yield return model.IsLoading
        .Subscribe(loading => _loadingSpinner.SetActive(loading));
}
```

### 🏗️ 依存性注入 + 自動ローダー

VContainer統合とResources自動ロードで簡単設定。

```csharp
// DI登録
services.AddSingleton<IUserRepository, UserRepository>();
services.AddTransient<UserProfileScreen>();

// Presenter自動ロード（Resources/UI/から型名で推定）
var presenter = await LoadPresenterAsync<UserProfilePresenter>();

// カスタムローダーも可能
services.AddSingleton<IPresenterLoaderFactory, AddressablePresenterLoaderFactory>();
```

### 📨 Dispatch機能

スタック内全画面へのイベント配信システム。

```csharp
// 全画面にイベント送信（最初にtrueを返した画面で停止）
StackNavigationService.Dispatch(new RefreshEvent());
await StackNavigationService.DispatchAsync(new SaveDataEvent());

// 受信側（画面で実装）
eventHolder.Dispatch<RefreshEvent>(evt => {
    model.Refresh();
    return true; // 処理完了
});
```

### 🎬 アニメーション戦略パターン

4種類のアニメーション（Open/Close/Show/Hide）を操作別に自動選択。

```csharp
// ScreenUIType別の動作
ScreenUIType.None          // アニメーションなし
ScreenUIType.FullScreen    // 背後画面自動非表示
ScreenUIType.WindowOrTransparent // 背後画面表示維持

// CrossFade/順次実行の選択
await PushNavigation.IsCrossFade(true).PushAsync<T>(); // 並列実行
await PushNavigation.IsCrossFade(false).PushAsync<T>(); // 順次実行
```

### 🔧 高度なスタック操作

Push/Pop以外の柔軟なスタック操作。

```csharp
// 特定画面の直前に挿入
await InsertNavigation.InsertScreenBeforeAsync<TargetScreen, NewScreen>();

// スタック中の特定画面を削除
await RemoveNavigation.RemoveAsync<UnwantedScreen>();

// 条件付きPop
await PopNavigation.OnlyWhen(currentScreen).PopAsync();
```

## アーキテクチャ概要

### ミドルウェア入れ子構造

```
ナビゲーションリクエスト
       ┃
   ┌───┴───┐
   │ Debug │ ← 最外層（前処理）
   │   ┌───┴───┐
   │   │SemSlim│ ← スレッド制御
   │   │ ┌───┴───┐
   │   │ │ Lock  │ ← 入力ロック
   │   │ │ ┌───┴───┐
   │   │ │ │ UI   │ ← 初期化/破棄
   │   │ │ │ ┌───┴───┐
   │   │ │ │ │ Anim │ ← アニメーション
   │   │ │ │ │ ┌───┴───┐
   │   │ │ │ │ │Stack│ ← 最内層（実行）
   │   │ │ │ │ └───┬───┘
   │   │ │ │ └───┬───┘ ← 後処理
   │   │ │ └───┬───┘
   │   │ └───┬───┘
   │   └───┬───┘
   └───┬───┘ ← 最外層（後処理）
       ┃
レスポンス返却
```

**重要ポイント**:
- 各ミドルウェアは`await next(context)`で次を呼び出し
- 前処理→内層実行→後処理の順で実行
- SemaphoreSlimで全ナビゲーションを排他制御

### ナビゲーションフロー詳細

1. **API呼び出し**: `PushNavigation.PushAsync<T>()`等
2. **セマフォ取得**: `SemaphoreSlim.WaitAsync()`で排他制御
3. **コンテキスト生成**: `StackNavigationContext`+Features Dictionary
4. **ミドルウェアチェーン**: 入れ子構造で順次実行
   - 前処理: 初期化・ロック・イベント
   - スタック操作: `StackScreenContainer`の実際の操作
   - 後処理: アニメーション・イベント・破棄
5. **セマフォ解放**: `finally`ブロックで確実に解放
6. **リソース管理**: IDisposable/IAsyncDisposable自動管理

### MVPパターンの実装

```
┌─────────────────────────────────────────────┐
│ Screen (MVPScreen<TModel>)                      │
│ ・ CreateModelAsync() - 非同期モデル初期化             │
│ ・ RegisterEvents() - ライフサイクルイベント登録        │
│ ・ LoadPresenterAsync() - Presenter自動ロード          │
│ ・ リソース自動管理(IDisposable)                   │
├─────────────────────────────────────────────┤
│ Model (ReactiveProperty)                        │
│ ・ ReactiveProperty<T> - 変更通知                      │
│ ・ CompositeDisposable - メモリリーク防止            │
│ ・ ビジネスロジックカプセル化                     │
├─────────────────────────────────────────────┤
│ Presenter (Presenter<TModel>)                   │
│ ・ OnInit() → LoadAsync() → OnSetup() → Bind()     │
│ ・ Bind() - Model→View リアクティブバインディング      │
│ ・ Viewイベント公開(OnClickAsObservable等)          │
│ ・ 自動Dispose管理                               │
└─────────────────────────────────────────────┘
```

**ライフサイクルフロー**:
1. Screen初期化 → CreateModelAsync() → Model生成
2. Presenterロード → LoadPresenterAsync() → Resourcesから自動取得
3. Presenterライフサイクル → OnInit → LoadAsync → OnSetup → Bind
4. リアクティブバインディング → Model変更→View更新
5. 破棄時 → OnDeinit → Dispose自動実行

## 設計哲学と実装特徴

### 1. 型安全性 + 実行時エラー防止

- ジェネリクスによるコンパイル時型チェック
- スタック内重複画面の禁止（同じ型は1つのみ）
- 強い型付けによるパラメーター受け渡し

### 2. 宣言的UI + 自動リソース管理

- ReactivePropertyによる宣言的バインディング
- IDisposable/IAsyncDisposable自動管理
- メモリリーク防止の仕組み

### 3. ミドルウェアベース拡張性

- 入れ子構造による明確な責任分離
- カスタムミドルウェアによる機能拡張
- プラグイン的なアーキテクチャ

### 4. Unityネイティブ最適化

- ObjectPool使用によるGC負荷軽減
- Unity Coroutineとasync/awaitの統合
- Resources/Addressable両対応

### 5. スレッドセーフティ

- SemaphoreSlimによる確実な排他制御
- 並行ナビゲーション操作の安全性保証
- デッドロック防止設計

## 使用シーン

### モバイルアプリ開発

- **タブ画面**: ScreenUIType.FullScreenで背後自動非表示
- **モーダル画面**: ScreenUIType.WindowOrTransparentで透過表示
- **ウィザード形式**: Insert/Remove操作で柔軟なフロー制御
- **複雑なナビゲーション**: Dispatch機能でグローバル状態管理

### ゲームUI

- **メニューシステム**: 階層的スタック管理で戻る操作が直感的
- **インベントリ/ショップ**: リアクティブなアイテム管理
- **設定画面**: 型安全なパラメーター受け渡し
- **チュートリアル**: アニメーション戦略とスキップ機能

### エンタープライズアプリ

- **業務フロー**: スレッドセーフな並行操作対応
- **データ入力**: MVP分離でテスタブルなビジネスロジック
- **ダッシュボード**: 自動リソース管理でメモリ効率が良い

## 次のステップ

1. [ナビゲーションシステム](Navigation-System) - 画面遷移の詳細
2. [MVPアーキテクチャ](MVP-Architecture) - 画面とプレゼンターの実装
3. [ミドルウェアシステム](Middleware-System) - カスタムミドルウェアの作成

---

[[← Home]](Home) | [[Navigation System →]](Navigation-System)