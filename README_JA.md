[English Documentation](README.md)

# Meek

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE.md)
[![Unity](https://img.shields.io/badge/Unity-6000.0%2B-blue.svg)](https://unity.com/)
[![Version](https://img.shields.io/badge/version-1.6.3-green.svg)](https://github.com/azumausu/Meek/releases)

Unity向けのDIベース画面管理フレームワーク。スタックナビゲーションとMVPアーキテクチャをサポートしています。

[MeekDemo](https://user-images.githubusercontent.com/19426596/232242080-f2eac6e7-e1ae-48c3-9816-8aebae1f951b.mov)

> デモで使用している画像は [Nucleus UI](https://www.nucleus-ui.com/) のフリーコンテンツです。

---

## 目次

- [特徴](#特徴)
- [アーキテクチャ概要](#アーキテクチャ概要)
- [動作要件](#動作要件)
- [インストール](#インストール)
- [クイックスタート](#クイックスタート)
- [基本コンセプト](#基本コンセプト)
  - [Screen (MVPパターン)](#screen-mvpパターン)
  - [ナビゲーション](#ナビゲーション)
  - [画面ライフサイクル](#画面ライフサイクル)
  - [DI連携](#di連携)
- [使い方](#使い方)
  - [画面の作成](#画面の作成)
  - [画面間のパラメータ受け渡し](#画面間のパラメータ受け渡し)
  - [画面間通信 (Dispatch)](#画面間通信-dispatch)
  - [遷移アニメーション](#遷移アニメーション)
- [応用的な使い方](#応用的な使い方)
  - [ネストナビゲーション (タブ)](#ネストナビゲーション-タブ)
  - [Addressablesによるプレハブ読み込み](#addressablesによるプレハブ読み込み)
  - [複数ナビゲーター](#複数ナビゲーター)
  - [ナビゲーションイベントのフック](#ナビゲーションイベントのフック)
  - [カスタムDIコンテナ](#カスタムdiコンテナ)
- [APIリファレンス](#apiリファレンス)
- [FAQ](#faq)
- [ライセンス](#ライセンス)

---

## 特徴

- **スタックベースナビゲーション** — Push、Pop、Insert、Remove、BackToの型安全なAPI
- **MVPアーキテクチャ** — Model-View-Presenterパターンを内蔵。Presenterの自動ロードとリアクティブデータバインディングに対応
- **DIコンテナ連携** — 抽象化されたDIレイヤーとVContainerアダプター。画面はDI経由で解決され、コンストラクタインジェクションをサポート
- **遷移アニメーション** — Open/Close/Show/Hideの設定可能なアニメーション。CrossFade対応とStrategyパターンによる拡張性
- **画面ライフサイクルイベント** — 豊富なライフサイクルフック（WillStart、DidStart、WillPause、DidPause、WillResume、DidResume、WillDestroy、DidDestroy）
- **入力ロック** — 遷移中の自動入力ブロックでダブルタップ問題を防止
- **静的クラス不使用** — インスタンスベース設計により、複数の独立したナビゲーターが共存可能
- **画面間通信** — 密結合なしにスタック内の画面間でDispatchイベントを送信

---

## アーキテクチャ概要

Meekは5つのモジュラーパッケージで構成されています：

```
┌─────────────────────────────────────────────────────────┐
│                 アプリケーション                          │
├───────────────┬───────────────────┬─────────────────────┤
│   Meek.MVP    │    Meek.UGUI      │  Meek.VContainer    │
├───────────────┴───────────────────┴─────────────────────┤
│                  Meek.NavigationStack                     │
├─────────────────────────────────────────────────────────┤
│                      Meek (Core)                         │
└─────────────────────────────────────────────────────────┘
```

| パッケージ | 説明 |
|---------|-------------|
| **Meek** | コアインターフェースと抽象化（`IScreen`、`INavigator`、`IServiceCollection`） |
| **Meek.NavigationStack** | スタックベースナビゲーション、ライフサイクルイベント、アニメーション戦略、入力ロック |
| **Meek.MVP** | MVPパターンサポート（`MVPScreen<TModel>`、`Presenter<TModel>`、自動ロード） |
| **Meek.UGUI** | Unity uGUI統合（アニメーションコンポーネント、`DefaultInputLocker`、`DefaultPrefabViewManager`） |
| **Meek.VContainer** | VContainer DIアダプター（`VContainerServiceCollection`、`VContainerServiceProvider`） |

---

## 動作要件

- **Unity 6000.0**（Unity 6）以降
- **[VContainer](https://github.com/hadashiA/VContainer)** 1.13.2以上
- **[UniRx](https://github.com/neuecc/UniRx)**（MVPリアクティブバインディングに推奨）

---

## インストール

`Packages/manifest.json` に以下を追加してください：

```json
{
  "dependencies": {
    "jp.amatech.meek": "https://github.com/azumausu/Meek.git?path=Assets/Packages",
    "jp.hadashikick.vcontainer": "https://github.com/hadashiA/VContainer.git?path=VContainer/Assets/VContainer"
  }
}
```
---

## クイックスタート

### 1. エントリポイントの作成

この `MonoBehaviour` をシーン内のGameObjectにアタッチします。`DefaultInputLocker` と `DefaultPrefabViewManager` コンポーネントをGameObjectに配置し、Inspectorで割り当ててください。

```csharp
using Meek;
using Meek.MVP;
using Meek.UGUI;
using UnityEngine;

public class Main : MonoBehaviour
{
    [SerializeField] private DefaultInputLocker defaultInputLocker;
    [SerializeField] private DefaultPrefabViewManager defaultPrefabViewManager;

    public void Start()
    {
        var container = new VContainerServiceCollection()
            .AddMeekMvp(new MvpNavigatorOptions()
            {
                InputLocker = defaultInputLocker,
                PrefabViewManager = defaultPrefabViewManager
            });

        // 画面を登録
        container.ServiceCollection.AddTransient<SplashScreen>();
        container.ServiceCollection.AddTransient<HomeScreen>();

        // ビルドして最初の画面に遷移
        container.BuildAndRunMeekMvpAsync<SplashScreen>().Forget();
    }
}
```

### 2. Modelの作成

```csharp
public class SplashModel
{
    // 必要に応じてリアクティブプロパティを追加
}
```

### 3. Screenの作成

```csharp
using System.Threading.Tasks;
using Meek.MVP;
using Meek.NavigationStack;
using UniRx;

public class SplashScreen : MVPScreen<SplashModel>
{
    protected override async ValueTask<SplashModel> CreateModelAsync()
    {
        return await Task.FromResult(new SplashModel());
    }

    protected override void RegisterEvents(EventHolder eventHolder, SplashModel model)
    {
        eventHolder.ScreenWillStart(async () =>
        {
            var presenter = await LoadPresenterAsync<SplashPresenter>();

            presenter.OnClickStart.Subscribe(_ => PushNavigation.PushForget<HomeScreen>());
        });
    }
}
```

### 4. Presenterの作成

Presenterプレハブを `Resources/UI/SplashPresenter` に配置してください。

```csharp
using System;
using System.Collections.Generic;
using Meek.MVP;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class SplashPresenter : Presenter<SplashModel>
{
    [SerializeField] private Button _startButton;

    public IObservable<Unit> OnClickStart => _startButton.OnClickAsObservable();

    protected override IEnumerable<IDisposable> Bind(SplashModel model)
    {
        yield break; // シンプルな画面のためバインディング不要
    }
}
```

### 5. デモの実行

`Assets/Demo/Scenes/Demo.unity` を開いてPlayボタンを押すと、すべてのナビゲーションパターンを実演する9画面の完全な動作例を確認できます。

---

## 基本コンセプト

### Screen (MVPパターン)

Meekは **Screen** がコーディネーターとして機能する、拡張されたMVP（Model-View-Presenter）パターンを採用しています：

```
┌──────────────────────┐     ┌───────────┐     ┌──────────────┐
│        Screen         │────>│   Model   │<────│  Presenter   │
│  (コーディネーター)   │     │  (状態)   │     │ (View+Bind)  │
└──────────┬────────────┘     └───────────┘     └──────────────┘
           │                                          ▲
           │  Modelを生成                             │
           │  Presenterをロード ──────────────────────┘
           │  ライフサイクルイベントを登録
           │
           ▼
  Navigation API (Push/Pop等)
```

- **Screen** (`MVPScreen<TModel>`) — Modelの生成、Presenterのロード、ライフサイクルイベントの登録、ナビゲーション処理を担当
- **Model** — `ReactiveProperty<T>` を使用してオブザーバブルなデータとして画面状態を保持
- **Presenter** (`Presenter<TModel>`) — `Bind()` メソッドでModelデータをUI要素にバインドするUnityプレハブ（`MonoBehaviour`）

### ナビゲーション

Meekは5つのナビゲーション操作を提供します：

| 操作 | メソッド | 説明 |
|-----------|--------|-------------|
| **Push** | `PushNavigation.PushAsync<T>()` | スタックの最上部に画面を追加 |
| **Pop** | `PopNavigation.PopAsync()` | スタックの最上部の画面を削除 |
| **Insert** | `InsertNavigation.InsertScreenBeforeAsync<TBefore, TInsert>()` | 指定した画面の前に画面を挿入 |
| **Remove** | `RemoveNavigation.RemoveAsync<T>()` | スタック内の特定の画面を削除 |
| **BackTo** | `BackToNavigation.BackToAsync<T>()` | 指定した画面が最上部になるまで画面をPop |

各ナビゲーションビルダーはメソッドチェーンをサポートします：

```csharp
// パラメータ付きでクロスフェードアニメーションのPush
PushNavigation
    .NextScreenParameter(new MyParam { Id = 42 })
    .IsCrossFade(true)
    .PushAsync<DetailScreen>();

// アニメーションスキップのPop
PopNavigation
    .SkipAnimation(true)
    .PopAsync();

// Fire-and-forget（非同期を待たない）バリアント
PushNavigation.PushForget<NextScreen>();
PopNavigation.PopForget();
```

### 画面ライフサイクル

```
Push                                              Pop
 │                                                 │
 ▼                                                 ▼
ScreenWillStart ──> ScreenDidStart          ScreenWillDestroy ──> ScreenDidDestroy
                         │                         ▲
                         ▼                         │
                    ScreenWillPause ─────> ScreenWillResume
                    ScreenDidPause         ScreenDidResume
                         │                         ▲
                    (別のPush)                (そのPop)
```

**アニメーションイベント**は遷移の前後に発火されます：

| イベント | タイミング |
|-------|--------|
| `ViewWillOpen` | 表示アニメーション開始前 |
| `ViewDidOpen` | 表示アニメーション完了後 |
| `ViewWillClose` | 非表示アニメーション開始前 |
| `ViewDidClose` | 非表示アニメーション完了後 |

### DI連携

Meekは `IContainerBuilder` / `IServiceCollection` / `IServiceProvider` を通じてDIを抽象化しています。VContainerアダプターは直接マッピングされます：

```csharp
// シングルトンを登録（全画面で共有）
container.ServiceCollection.AddSingleton<GlobalStore>();

// 画面を登録（解決のたびに新しいインスタンス）
container.ServiceCollection.AddTransient<HomeScreen>();

// コンストラクタインジェクションは自動的に動作
public class HomeScreen : MVPScreen<HomeModel>
{
    private readonly GlobalStore _globalStore;

    public HomeScreen(GlobalStore globalStore)  // DIによるインジェクション
    {
        _globalStore = globalStore;
    }
}
```

---

## 使い方

### 画面の作成

#### リアクティブプロパティを持つModel

```csharp
using UniRx;

public class LogInModel
{
    private ReactiveProperty<string> _email = new();
    private ReactiveProperty<string> _password = new();

    public IReadOnlyReactiveProperty<string> Email => _email;
    public IReadOnlyReactiveProperty<string> Password => _password;

    public void UpdateEmail(string value) => _email.Value = value;
    public void UpdatePassword(string value) => _password.Value = value;
}
```

#### リアクティブバインディングを持つScreen

```csharp
public class LogInScreen : MVPScreen<LogInModel>
{
    protected override async ValueTask<LogInModel> CreateModelAsync()
    {
        return await Task.FromResult(new LogInModel());
    }

    protected override void RegisterEvents(EventHolder eventHolder, LogInModel model)
    {
        eventHolder.ScreenWillStart(async () =>
        {
            var presenter = await LoadPresenterAsync<LogInPresenter>();

            presenter.OnClickBack.Subscribe(_ => PopNavigation.PopAsync().Forget());
            presenter.OnClickLogIn.Subscribe(_ => PushNavigation.PushAsync<TabScreen>().Forget());

            presenter.OnEndEditEmail.Subscribe(model.UpdateEmail);
            presenter.OnEndEditPassword.Subscribe(model.UpdatePassword);
        });
    }
}
```

#### データバインディングを持つPresenter

```csharp
public class LogInPresenter : Presenter<LogInModel>
{
    [SerializeField] private TMP_InputField _emailInputField;
    [SerializeField] private TMP_InputField _passwordInputField;
    [SerializeField] private Button _backButton;
    [SerializeField] private Button _logInButton;

    public IObservable<Unit> OnClickBack => _backButton.OnClickAsObservable();
    public IObservable<Unit> OnClickLogIn => _logInButton.OnClickAsObservable();
    public IObservable<string> OnEndEditEmail => _emailInputField.onEndEdit.AsObservable();
    public IObservable<string> OnEndEditPassword => _passwordInputField.onEndEdit.AsObservable();

    protected override IEnumerable<IDisposable> Bind(LogInModel model)
    {
        yield return model.Email.Subscribe(x => _emailInputField.text = x);
        yield return model.Password.Subscribe(x => _passwordInputField.text = x);
    }
}
```

### 画面間のパラメータ受け渡し

`MVPScreen<TModel, TParam>` を使用して型付きパラメータを受け取ります：

```csharp
// パラメータクラスを定義
public class ReviewScreenParameter
{
    public int ProductId;
}

// パラメータを受け取るScreen
public class ReviewScreen : MVPScreen<ReviewModel, ReviewScreenParameter>
{
    protected override async ValueTask<ReviewModel> CreateModelAsync(ReviewScreenParameter parameter)
    {
        return await Task.FromResult(new ReviewModel(parameter.ProductId));
    }

    protected override void RegisterEvents(EventHolder eventHolder, ReviewModel model) { /* ... */ }
}

// パラメータ付きでPush（別の画面から）
PushNavigation
    .NextScreenParameter(new ReviewScreenParameter { ProductId = 42 })
    .PushAsync<ReviewScreen>();
```

### 画面間通信 (Dispatch)

密結合なしにスタック下位の画面にイベントを送信します：

```csharp
// イベント引数クラスを定義
public class ReviewEventArgs
{
    public int ProductId;
    public bool IsGood;
}

// 送信側の画面（ReviewScreen） — Pop後にDispatch
presenter.OnClickGood.Subscribe(async _ =>
{
    await PopNavigation.PopAsync();
    Dispatch(new ReviewEventArgs { ProductId = model.ProductId.Value, IsGood = true });
});

// 受信側の画面（HomeScreen） — イベントを購読
eventHolder.SubscribeDispatchEvent<ReviewEventArgs>(args =>
{
    model.AddProduct(args.ProductId, args.IsGood);
    return true; // 伝播を停止
});
```

### 遷移アニメーション

Meekは4種類のアニメーションをサポートしています：**Open**、**Close**、**Show**、**Hide**

ナビゲーションごとにアニメーション動作を制御できます：

```csharp
// クロスフェード：旧画面と新画面が同時にアニメーション
PushNavigation.IsCrossFade(true).PushAsync<NextScreen>();

// アニメーションを完全にスキップ
PushNavigation.SkipAnimation(true).PushAsync<NextScreen>();
```

#### モーダル / 透過画面

画面全体を覆わない画面では `ScreenUIType` をオーバーライドします：

```csharp
public class ReviewScreen : MVPScreen<ReviewModel, ReviewScreenParameter>
{
    public override ScreenUIType ScreenUIType => ScreenUIType.WindowOrTransparent;
    // ...
}
```

---

## 応用的な使い方

### ネストナビゲーション (タブ)

個別の `VContainerServiceCollection` インスタンスを作成することで、各タブに独立したナビゲーターを構築できます。親の `IServiceProvider` を渡すことで、シングルトン（`GlobalStore` など）を子ナビゲーター間で共有します：

```csharp
// PresenterのLoadAsync()メソッド内
protected override async Task LoadAsync(TabModel model)
{
    var homeServices = await new VContainerServiceCollection(model.AppServices)
        .AddMeekMvp(new MvpNavigatorOptions()
        {
            InputLocker = homeDefaultInputLocker,
            PrefabViewManager = homeDefaultPrefabViewManager
        })
        .BuildAndRunMeekMvpAsync<HomeScreen>();

    // このPresenterが破棄されたときに子ナビゲーターをDispose
    if (homeServices is IDisposable disposable)
        disposable.AddTo(this); // UniRxのAddTo拡張メソッド
}
```

各タブは独自のナビゲーションスタック、入力ロッカー、ライフサイクルを持ち、完全に独立しています。4つのネストナビゲーターを持つ完全な例は `Assets/Demo/Scripts/Presenters/TabPresenter.cs` を参照してください。

### Addressablesによるプレハブ読み込み

デフォルトでは、Presenterプレハブは `PresenterViewProviderFromResources` を通じて `Resources/UI/` からロードされます。Addressablesからロードするには、`IPresenterViewProvider`（`IPrefabViewProvider` を拡張）を実装します：

```csharp
public class PresenterLoaderProviderFromAddressable : IPresenterViewProvider, IDisposable
{
    private string _prefabName;
    private AsyncOperationHandle<GameObject> _asyncOperationHandle;

    public void SetPrefabName(string prefabName)
    {
        _prefabName = prefabName;
    }

    public async ValueTask<GameObject> ProvideAsync(IScreen ownerScreen, object param = null)
    {
        _asyncOperationHandle = Addressables.LoadAssetAsync<GameObject>(_prefabName);
        var prefab = await _asyncOperationHandle.Task;

        return prefab;
    }

    public void Dispose()
    {
        if (_asyncOperationHandle.IsValid())
        {
            _asyncOperationHandle.Release();
        }
    }
}
```

デフォルトプロバイダーの代わりに登録します：

```csharp
appContainer.ServiceCollection.AddTransient<IPresenterViewProvider, PresenterLoaderProviderFromAddressable>();
```
同じインターフェースに対してAddTransientを2回呼び出すと、最後の登録が上書きされることに注意してください。

### 複数ナビゲーター

Meekは静的クラスを使用しないため、複数の独立したナビゲーションスタックを作成できます：

```csharp
// ナビゲーターA
var containerA = new VContainerServiceCollection()
    .AddMeekMvp(optionsA);
containerA.BuildAndRunMeekMvpAsync<ScreenA>();

// ナビゲーターB（完全に独立）
var containerB = new VContainerServiceCollection()
    .AddMeekMvp(optionsB);
containerB.BuildAndRunMeekMvpAsync<ScreenB>();
```

### ナビゲーションイベントのフック

アナリティクス、ログ、カスタムロジックのためにナビゲーションイベントを購読できます：

```csharp
var navigationService = appServices.GetService<StackNavigationService>();

navigationService.OnWillNavigate += context =>
{
    Debug.Log($"ナビゲーション中: {context.NavigatingSourceType}");
    return ValueTask.CompletedTask;
};

navigationService.OnDidNavigate += context =>
{
    Debug.Log($"ナビゲーション完了");
    return ValueTask.CompletedTask;
};
```

### カスタムDIコンテナ

`IContainerBuilder` と `IServiceProvider` を実装することで、別のDIフレームワークを使用できます：

```csharp
public class ZenjectServiceCollection : IContainerBuilder
{
    public IServiceCollection ServiceCollection { get; }

    public IServiceProvider Build()
    {
        // ServiceCollectionをZenjectのバインディングにマッピング
        // ZenjectServiceProviderラッパーを返す
    }
}
```

---

## APIリファレンス

### コアインターフェース (Meek)

| インターフェース | 説明 |
|-----------|-------------|
| `IScreen` | `Initialize(NavigationContext)` を持つ画面の基底インターフェース |
| `INavigator` | `NavigateAsync(NavigationContext)` を持つナビゲーションオーケストレーター |
| `IScreenContainer` | 画面コレクションを管理 |
| `IServiceCollection` | DIサービスの登録 |
| `IServiceProvider` | DIサービスの解決 |
| `IContainerBuilder` | `IServiceCollection` から `IServiceProvider` をビルド |
| `IMiddleware` | `InvokeAsync(NavigationContext, NavigationDelegate)` を持つミドルウェアインターフェース |

### ナビゲーション (Meek.NavigationStack)

| クラス | 説明 |
|-------|-------------|
| `StackNavigationService` | Push/Pop/Insert/Remove/Dispatchを持つメインナビゲーションAPI |
| `PushNavigation` | Push操作のビルダー |
| `PopNavigation` | Pop操作のビルダー |
| `InsertNavigation` | Insert操作のビルダー |
| `RemoveNavigation` | Remove操作のビルダー |
| `BackToNavigation` | BackTo操作のビルダー |
| `StackScreenContainer` | LIFOスクリーンスタック実装 |
| `StackScreen` | スタック管理画面の抽象基底クラス |
| `IInputLocker` | 遷移中の入力ロック制御 |

### MVP (Meek.MVP)

| クラス | 説明 |
|-------|-------------|
| `MVPScreen<TModel>` | Model生成とライフサイクルを持つScreen |
| `MVPScreen<TModel, TParam>` | 型付きパラメータサポート付きScreen |
| `Presenter<TModel>` | `Bind(TModel)` を持つMonoBehaviourベースのView |
| `IPresenterViewProvider` | カスタムPresenterプレハブ読み込み用インターフェース（Addressables等） |
| `MvpNavigatorOptions` | InputLockerとPrefabViewManagerの設定 |

### ライフサイクルイベント

| イベント | トリガー |
|-------|---------|
| `ScreenWillStart` / `ScreenDidStart` | 画面の初期化（Push時） |
| `ScreenWillPause` / `ScreenDidPause` | 画面の非アクティブ化（上に別の画面がPush時） |
| `ScreenWillResume` / `ScreenDidResume` | 画面の再アクティブ化（上の画面がPop時） |
| `ScreenWillDestroy` / `ScreenDidDestroy` | 画面の破棄（Pop / Remove時） |
| `ViewWillOpen` / `ViewDidOpen` | 表示アニメーションの開始 / 終了 |
| `ViewWillClose` / `ViewDidClose` | 非表示アニメーションの開始 / 終了 |

---

## FAQ

### どのDIコンテナを使うべきですか？

Meekには **VContainer** アダプターが同梱されています。VContainerは軽量でUnityに最適化されているため、推奨される選択肢です。カスタム `IContainerBuilder` を作成することで、他のDIフレームワーク（例：Zenject）のサポートを実装できます。

### 画面はPrefabですか？Sceneですか？

**PresenterはPrefab** であり、デフォルトでは `Resources/UI/` に配置します。Screen自体はDIを通じて解決される純粋なC#クラスであり、MonoBehaviourではありません。この分離により、ロジックのテスト容易性とフレームワーク非依存性が保たれます。

### Viewロジックとビジネスロジックはどう分離しますか？

MVPパターンが自然にこれを処理します：
- **Model** — 純粋なC#の状態、Unity依存なし
- **Presenter** — シリアライズされたUI参照を持つMonoBehaviour。Modelからのデータバインディングのみを担当
- **Screen** — Model生成、Presenterロード、ナビゲーション、ライフサイクルイベントをコーディネート

### 複数のナビゲーションスタックを同時に使用できますか？

はい。Meekは静的クラスを使用しないため、`VContainerServiceCollection().AddMeekMvp(...)` で完全に独立したナビゲーターが作成されます。デモの `TabPresenter` がタブコンテンツ用の4つのネストナビゲーターでこのパターンを示しています。

---

## ライセンス

Meekは [MITライセンス](LICENSE.md) の下で公開されています。

Copyright (c) 2023 Hikaru Amano
