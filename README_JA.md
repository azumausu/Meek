[English Documentation](README.md)

# Meek

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE.md)
[![Unity](https://img.shields.io/badge/Unity-6000.0%2B-blue.svg)](https://unity.com/)
[![Version](https://img.shields.io/badge/version-1.6.5-green.svg)](https://github.com/azumausu/Meek/releases)

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
  - [各ナビゲーションのイベント実行順](#各ナビゲーションのイベント実行順)
  - [DI連携](#di連携)
- [使い方](#使い方)
  - [画面の作成](#画面の作成)
  - [画面間のパラメータ受け渡し](#画面間のパラメータ受け渡し)
  - [画面間通信 (Dispatch)](#画面間通信-dispatch)
  - [遷移アニメーション](#遷移アニメーション)
- [詳細仕様](#詳細仕様)
  - [MVPScreen で使えるメンバー](#mvpscreen-で使えるメンバー)
  - [Model / Presenter の自動破棄](#model--presenter-の自動破棄)
  - [Navigation Builder のオプション](#navigation-builder-のオプション)
  - [`CustomFeature` による値渡し](#customfeature-による値渡し)
  - [ライフサイクルコールバックで `StackNavigationContext` を受け取る](#ライフサイクルコールバックで-stacknavigationcontext-を受け取る)
  - [アニメーションシステムの仕組み](#アニメーションシステムの仕組み)
  - [クラス構成: Screen, ScreenUI, IViewHandler, Presenter](#クラス構成-screen-screenui-iviewhandler-presenter)
  - [StackNavigationService によるスタック情報の取得](#stacknavigationservice-によるスタック情報の取得)
- [応用的な使い方](#応用的な使い方)
  - [ネストナビゲーション (タブ)](#ネストナビゲーション-タブ)
  - [Addressablesによるプレハブ読み込み](#addressablesによるプレハブ読み込み)
  - [複数ナビゲーター](#複数ナビゲーター)
  - [ナビゲーションイベントのフック](#ナビゲーションイベントのフック)
  - [カスタムDIコンテナ](#カスタムdiコンテナ)
- [APIリファレンス](#apiリファレンス)
- [ライセンス](#ライセンス)

---

## 特徴

- **スタックベースナビゲーション** — Push、Pop、Insert、Remove、BackToなどの画面遷移機能
- **DIコンテナ連携** — 全てのベース機能がDIコンテナに登録されているため、オーバーライドすることで容易に追加・拡張が可能
- **遷移アニメーション** — Open/Close/Show/Hideなどを簡単に設定可能。Strategyパターンによる拡張性
- **画面ライフサイクルイベント** — 豊富なライフサイクルフック（WillStart、DidStart、WillPause、DidPause、WillResume、DidResume、WillDestroy、DidDestroy）
- **入力ロック** — 遷移中の自動入力ブロックでダブルタップ問題を防止
- **静的クラス不使用** — インスタンスベース設計により、複数の独立したナビゲーターが共存可能
- **画面間通信** — 密結合なしにスタック内の画面間でDispatchイベントを送信

---

## アーキテクチャ概要

Meekは5つのモジュラーパッケージで構成されています：

```
┌─────────────────────────────────────────────────────────┐
│                      Applicaition 　　　　 　　　　　　　　 │
├───────────────┬───────────────────┬─────────────────────┤
│   Meek.MVP    │    Meek.UGUI      │  Meek.VContainer    │
├───────────────┴───────────────────┴─────────────────────┤
│                  Meek.NavigationStack                   │
├─────────────────────────────────────────────────────────┤
│                      Meek (Core)                        │
└─────────────────────────────────────────────────────────┘
```

| パッケージ | 説明 |
|---------|-------------|
| **Meek** | コアインターフェースと抽象化（`IScreen`、`INavigator`、`IServiceCollection`） |
| **Meek.NavigationStack** | スタックベースナビゲーション、ライフサイクルイベント、アニメーション、入力ロック |
| **Meek.MVP** | MVPパターンサポート（`MVPScreen<TModel>`、`Presenter<TModel>`、自動ロード） |
| **Meek.UGUI** | Unity uGUI統合（アニメーションコンポーネント、`DefaultInputLocker`、`DefaultPrefabViewManager`） |
| **Meek.VContainer** | VContainer DIアダプター（`VContainerServiceCollection`、`VContainerServiceProvider`） |

---

## 動作要件

- **Unity 6000.0**（Unity 6）以降
- **[VContainer](https://github.com/hadashiA/VContainer)** 1.13.2 以上

---

## インストール

`Packages/manifest.json` に以下を追加してください：

```json
{
  "dependencies": {
    "jp.amatech.meek": "https://github.com/azumausu/Meek.git?path=Assets/Packages"
  }
}
```

---

## クイックスタート

### 1. エントリポイントの作成

この `MonoBehaviour` をシーン内のGameObjectにアタッチします。`DefaultInputLocker` と `DefaultPrefabViewManager` コンポーネントをGameObjectに配置し、Inspectorで割り当ててください。

```csharp
using Cysharp.Threading.Tasks;
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
                // 画面遷移中の入力をロックする仕組みを実装して渡す
                InputLocker = defaultInputLocker,
                // UIプレハブを作成するノードを管理するクラスを渡す
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
    // モデルの作成
    protected override async ValueTask<SplashModel> CreateModelAsync()
    {
        return await Task.FromResult(new SplashModel());
    }

    // 画面のライフサイクルイベントに処理を登録
    protected override void RegisterEvents(EventHolder eventHolder, SplashModel model)
    {
        // 画面起動準備イベント
        eventHolder.ScreenWillStart(async () =>
        {
            // Presenterプレハブをロード
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

---

## 基本コンセプト

### Screen (MVPパターン)

Meekは **Screen** がコーディネーターとして機能する、拡張されたMVP（Model-View-Presenter）パターンを採用しています：

```
┌──────────────────────┐     ┌───────────┐     ┌──────────────┐
│        Screen        │────>│   Model   │<────│  Presenter   │
└──────────┬───────────┘     └───────────┘     └──────────────┘
           │ 
           │
           ▼
  Navigation API (Push/Pop等)
```

- **Screen** (`MVPScreen<TModel>`) — Modelの生成、Presenterのロード、ライフサイクルイベントの登録、ナビゲーション処理を担当
- **Model** — `ReactiveProperty<T>` を使用してオブザーバブルなデータとして画面状態を保持
- **Presenter** (`Presenter<TModel>`) — `Bind()` メソッドでModelデータをUI要素にバインドするUnityプレハブ（`MonoBehaviour`）

Screen 内で使える全メンバーの一覧は [MVPScreen で使えるメンバー](#mvpscreen-で使えるメンバー) を、`IDisposable` な Model や Presenter のサブスクリプションが自動で破棄される仕組みは [Model / Presenter の自動破棄](#model--presenter-の自動破棄) を参照してください。

### ナビゲーション

Meekは5つのナビゲーション操作を提供します：

| 操作 | メソッド | 説明 |
|-----------|--------|-------------|
| **Push** | `PushNavigation.PushAsync<T>()` | スタックの最上部に画面を追加 |
| **Pop** | `PopNavigation.PopAsync()` | スタックの最上部の画面を削除 |
| **Insert** | `InsertNavigation.InsertScreenBeforeAsync<TBeforeScreen, TInsertionScreen>()` | 指定した画面の前に画面を挿入 |
| **Remove** | `RemoveNavigation.RemoveAsync<T>()` | スタック内の特定の画面を削除 |
| **BackTo** | `BackToNavigation.BackToAsync<T>()` | 指定した画面が最上部になるまで画面をPop |

各ナビゲーションビルダーはメソッドチェーンをサポートし、2 種類の終端メソッドを提供します:

- **`Async`** — `Task` を返す（await 可能）
- **`Forget`** — fire-and-forget

```csharp
// await する
await PushNavigation.PushAsync<NextScreen>();

// Fire-and-forget
PushNavigation.PushForget<NextScreen>();
```

各ビルダーは固有の連結オプション（パラメータ・クロスフェード・SkipAnimation・`OnlyWhen` など）と、任意のキー/値を画面間で受け渡す `CustomFeature(key, value)` の仕組みを持ちます。全ビルダーの一覧と値渡しの詳細は [Navigation Builder のオプション](#navigation-builder-のオプション) と [`CustomFeature` による値渡し](#customfeature-による値渡し) を参照してください。

### 各ナビゲーションのイベント実行順

図は上から下へ時系列で読みます。色は「実装するときにどれだけ触るか」の優先度を表しています。

- **青** — 実装が必須（`CreateModelAsync` / `RegisterEvents`）
- **緑** — よく使うライフサイクルイベント
- **紫** — 知っておくと良いイベント（View イベント / 遷移アニメーション / ナビゲーションフック）
- **グレー** — フレームワーク内部の処理（参考）

各イベントに付いている `sync` / `sync → async` バッジは、そのイベントで呼べるハンドラの種類を表します。`sync` は `Action` 系のみ、`sync → async` は `Action` 系をすべて実行したあとに `Func<Task>` 系を登録順に await します。

#### Push

![Push のイベント実行順](Docs/Assets/PushEventOrder.png)

- **`ScreenWillStart` が Presenter をロードする場所です。** ここで `LoadPresenterAsync<T>()` を await すると、続く `UI.Setup()` で `Presenter.OnSetup` / `Bind` が走ります。
- 遷移元の `ScreenDidPause` は、遷移先の `CreateModelAsync()` よりも **前** に発火します。遷移元の後処理を遷移先の生成より先に済ませたい場合はここに書けます。
- 遷移元の入力ロックは Push 完了後も保持され、Pop で復帰するまで解除されません。

#### Pop

![Pop のイベント実行順](Docs/Assets/PopEventOrder.png)

- **`ScreenDidDestroy` はクローズアニメーションよりも前** に発火します。この時点では Model も Presenter もまだ生きているため、破棄前の値の保存などはここで行えます。
- 実際の破棄（プレハブ破棄 → `Presenter.OnDeinit` → Model / `Disposables` の破棄）はアニメーション完了後です。

<details>
<summary>Insert / Remove のイベント実行順</summary>

#### Insert

![Insert のイベント実行順](Docs/Assets/InsertEventOrder.png)

- 挿入される画面は `ScreenWillStart` → `ScreenDidPause` → `ScreenDidStart` の順に発火し、**`ScreenWillPause` は発火しません**。
- `ViewWillOpen` / `ViewDidOpen` は挿入される画面ではなく **現在のトップ画面** に対して発火します。また遷移アニメーションは再生されません。
- 挿入先が現在のトップ画面だった場合は、警告ログを出して Push にフォールバックします。

#### Remove

![Remove のイベント実行順](Docs/Assets/RemoveEventOrder.png)

- **`ScreenWillDestroy` は発火せず**、`ScreenDidDestroy` のみが発火します。
- 現在のトップ画面にはライフサイクルイベントも遷移アニメーションも一切発生しません。
- 削除対象が現在のトップ画面だった場合は、警告ログを出して Pop にフォールバックします。

</details>

※ **BackTo** は単独の遷移ではなく、上記の合成操作です。トップ画面より下の中間画面を順に Remove（既定で `SkipAnimation` が有効）してから、最後にトップ画面を 1 回 Pop します（対象の 1 つ上まで戻る場合は Pop のみ）。各サブ操作がそれぞれ入力ロックと `OnWillNavigate` / `OnDidNavigate` を発火します。

各ライフサイクルフックは `StackNavigationContext` を受け取るオーバーロードも持っています。受け取り方は [ライフサイクルコールバックで `StackNavigationContext` を受け取る](#ライフサイクルコールバックで-stacknavigationcontext-を受け取る) を参照してください。

### DI連携

ScreenはDIコンテナに登録されているのでコンストラクタで他のクラスを受け取ることができます。

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
※ 例では、ModelとPresenterのライフサイクルがScreenに内包されるため、購読の破棄（Dispose）は行っていません。

#### データバインディングを持つPresenter

```csharp
using System;
using System.Collections.Generic;
using Meek.MVP;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

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
    .PushAsync<ReviewScreen>()
    .Forget();
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

| 操作 | 前面の画面 (遷移先) | 背面の画面 (遷移元) |
|------|---------------------|---------------------|
| **Push** | `Open` | `Hide` (裏に隠れる) |
| **Pop**  | `Show` (再アクティブ化) | `Close` (Popされる画面) |

- **クロスフェード vs 順次実行** — `IsCrossFade(true)` を指定すると前面・背面のアニメーションは並列実行され、それ以外では順次実行されます。
- **View イベント** — `ViewWillOpen` / `ViewDidOpen` と `ViewWillClose` / `ViewDidClose` は `Open` / `Close` の前後でのみ発火します。`Show` / `Hide` 側には View イベントがありません。発火位置は [各ナビゲーションのイベント実行順](#各ナビゲーションのイベント実行順) を参照してください。

ナビゲーションごとにアニメーション動作を制御できます：

```csharp
// クロスフェード：旧画面と新画面が同時にアニメーション
PushNavigation.IsCrossFade(true).PushAsync<NextScreen>().Forget();

// アニメーションを完全にスキップ
PushNavigation.SkipAnimation(true).PushAsync<NextScreen>().Forget();
```

Presenter プレハブにクリップを配置する方法、`IsCrossFade` / `SkipAnimation` の内部挙動については [アニメーションシステムの仕組み](#アニメーションシステムの仕組み) を参照してください。

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

## 詳細仕様

### MVPScreen で使えるメンバー

`MVPScreen<TModel>`（および `MVPScreen<TModel, TParam>`）を継承した自前の Screen クラスからは、`StackScreen` から継承された以下のメンバーが利用できます。

| メンバー | 役割                                                                 |
|----------|--------------------------------------------------------------------|
| `Model` | `CreateModelAsync()` の戻り値（`ScreenWillStart` 直前に自動代入される）。           |
| `AppServices` | このナビゲーター用の `IServiceProvider`。任意のDIサービスを解決可能。                      |
| `UI` | 紐付いている `ScreenUI`（表示/非表示、入力ロック、ViewHandlerリストを管理）。                 |
| `NavigationService` | スタック内の参照やイベント購読に使う `StackNavigationService`。                       |
| `PushNavigation` / `PopNavigation` / `InsertNavigation` / `RemoveNavigation` / `BackToNavigation` | この画面を `Sender` として事前タグ付け済みのナビゲーションビルダー。                            |
| `Dispatch<T>(arg)` / `DispatchAsync<T>(arg)` | スタック上の全画面にイベントをブロードキャストする（受信側は `EventHolder.SubscribeDispatchEvent` で購読）。 |
| `TryGetScreen<TScreen>()` | スタック内に同じ型の画面が存在するか型で検索（無ければ `null`）。                               |
| `Disposables` / `AsyncDisposables` | 追加した `IDisposable` / `IAsyncDisposable` は画面破棄時に自動で `Dispose` される。  |
| `LoadPresenterAsync<TPresenter>(param?)` | Presenter プレハブをロードする。複数Presenterをロードすることも可能                        |
| `ScreenUIType` (override) | デフォルトは `FullScreen`。モーダル/オーバーレイ等で下の画面を残したい場合は `WindowOrTransparent` に上書き。 |
| `ForceUnlockInteractable()` / `AutoDisposeLockerOnDestroy` | 入力ロックの手動制御。                                                        |

### Model / Presenter の自動破棄

Meek は破棄処理の定型コードを Navigator パイプラインに組み込み、利用者の手書きを大幅に減らします。

- **Model** — Modelが `IDisposable` / `IAsyncDisposable` を実装していれば、`MVPScreen` が `ScreenWillStart` 中に自動的に `Disposables` / `AsyncDisposables` に登録します。画面がスタックから外れる（Pop / Remove / BackTo）と、`MvpNavigator` が `IAsyncDisposable.DisposeAsync()` を await した後 `IDisposable.Dispose()` を呼び、その流れで Model も破棄されます。
- **Presenter** — `Presenter<TModel>` は `MonoBehaviour, IAsyncDisposable` です。`Bind(TModel)` が `yield return` した `IDisposable` はすべて内部リストに保存され、Presenterの `OnDestroy()` で一括 Dispose されます。プレハブの GameObject 自体は、所属する `ScreenUI` が ViewHandler を破棄したタイミングで破棄されます。
- **Presenter の仮想フック** — `Presenter<TModel>` で上書きできるメソッド一覧:

  | フック | 呼ばれるタイミング |
  |--------|--------------------|
  | `OnInit()` | Unity `Awake()`（まだ Model は割り当てられていない）。 |
  | `LoadAsync(TModel model)` | Modelが割り当てられた直後、`Setup` の前。非同期リソースの準備に使う。 |
  | `OnSetup(TModel model)` | `Bind` の直前に走る同期セットアップ。 |
  | `Bind(TModel model)` | 購読を `IEnumerable<IDisposable>` で返す。OnDestroy で自動破棄される。 |
  | `OnDeinit(TModel model)` | Unity `OnDestroy()` の中、購読破棄の後で走る。 |
  | `DisposeAsync()` | ViewHandlerが Presenter を非同期破棄するときに呼ばれる。 |

### Navigation Builder のオプション

各ビルダーは `Async`（`Task` / `Task<T>` を返す）と `Forget`（`void` を返す）の 2 形態の終端メソッド、加えて複数の連結可能なオプションを提供します。下表はビルダー固有の機能のみで、すべてのビルダーは `CustomFeature(string key, object value)`（次節を参照）と `SetSender(object)` にも対応しています。

| Builder | 連結可能なオプション | 終端メソッド |
|---------|----------------------|-------------|
| `PushNavigation` | `NextScreenParameter(object)` / `IsCrossFade(bool)` / `SkipAnimation(bool)` | `PushAsync<T>()` → `Task<T>` / `PushForget<T>()` |
| `PopNavigation`  | `OnlyWhen(IScreen)` / `IsCrossFade(bool)` / `SkipAnimation(bool)` | `PopAsync()` → `Task` / `PopForget()` |
| `InsertNavigation` | `NextScreenParameter(object)` / `IsCrossFade(bool)` / `SkipAnimation(bool)` | `InsertScreenBeforeAsync<TBefore, TInsert>()` → `Task<IScreen>` / `InsertScreenBeforeForget<TBefore, TInsert>()` |
| `RemoveNavigation` | `IsCrossFade(bool)` / `SkipAnimation(bool)` | `RemoveAsync<T>()` / `RemoveAsync(IScreen)` / `RemoveAsync(Type)` → `Task` / `RemoveForget<T>()` |
| `BackToNavigation` | `IsCrossFade(bool)` / `SetSkipAnimation(bool)` / `SetRemoveScreenSkipAnimation(bool)` | `BackToAsync<T>()` → `Task` / `BackToForget<T>()` |

**ビルダーごとの補足:**

- **`PushNavigation`** — デフォルト: `SkipAnimation = false`, `IsCrossFade = false`。
- **`PopNavigation`** — `OnlyWhen` は指定 Screen がトップにいる時のみ Pop が走り、それ以外では no-op になる（ダブルタップ防止）。Builder ではなく `StackNavigationService.PopAsync(PopContext)` を直接呼ぶと `ValueTask<bool>` が返り、実際に Pop が走ったかを判別できる（Builder 側ではこの結果は捨てられる）。
- **`InsertNavigation`** — `SkipAnimation` のデフォルトは `true`。指定した before スクリーンがすでにトップにいる場合は通常の Push に自動格上げされる。
- **`RemoveNavigation`** — `SkipAnimation` のデフォルトは `true`。対象がトップ画面の場合は自動的に Pop へ格上げされる。
- **`BackToNavigation`** — スタックを目的画面まで巻き戻す。中間 Screen は既定で `SkipAnimation = true` で Remove（`SetRemoveScreenSkipAnimation` で変更可能）、最終遷移は通常の Pop。目的画面がすでにトップなら no-op。

### `CustomFeature` による値渡し

`NextScreenParameter(value)`（Push / Insert 限定）は遷移先 Screen に **1 つの型付き引数** を渡す正攻法です。それ以外の用途（フラグ、由来トラッキング、解析タグ、複数値）には、すべてのビルダーで使える `CustomFeature(key, value)` を利用します。値は `StackNavigationContext.Features` に格納されます。

```csharp
PushNavigation
    .CustomFeature("entry-point", "search")
    .CustomFeature("triggered-at", DateTime.UtcNow)
    .PushForget<DetailScreen>();

// 遷移先 Screen
eventHolder.ScreenWillStart(ctx =>
{
    var entry = ctx.GetFeatureNullableValue<string>("entry-point");   // 無い/型違いの場合は null
    var when  = ctx.GetFeatureValue<DateTime>("triggered-at");        // 無い/型違いの場合は例外
});
```

### ライフサイクルコールバックで `StackNavigationContext` を受け取る

`EventHolder` の各ライフサイクル拡張メソッドはオーバーロード済みで、現在の `StackNavigationContext` を受け取る形にも書けます:

```csharp
protected override void RegisterEvents(EventHolder eventHolder, DetailModel model)
{
    // 引数なし
    eventHolder.ScreenWillStart(() => Debug.Log("will start"));

    // async 版
    eventHolder.ScreenWillStart(async () => await Warmup());

    // Context 版 — FromScreen やパラメータ・カスタム値を参照
    eventHolder.ScreenWillStart(ctx =>
    {
        var param  = ctx.GetNextScreenParameter<DetailParam>();
        var source = ctx.GetFeatureNullableValue<string>("entry-point");
        var fromHome = ctx.FromScreen is HomeScreen;
    });

    // async + Context
    eventHolder.ScreenWillStart(async ctx =>
    {
        await FetchDetail(ctx.GetNextScreenParameter<DetailParam>().Id);
    });

    // エラー購読 — この遷移中のみ有効
    eventHolder.ScreenWillStart(ctx =>
    {
        ctx.OnError += ex => Debug.LogError($"Navigation failed: {ex}");
    });
}
```

非同期オーバーロードは `ScreenWillStart` / `ScreenWillResume` / `ScreenDidPause` / `ScreenDidDestroy` で利用可能です。  
それ以外のライフサイクルイベントは同期形（`Action` / `Action<StackNavigationContext>`）のみです。

`StackNavigationContext` で特に有用なメンバ:

| メンバ | 説明 |
|--------|------|
| `FromScreen` / `ToScreen` | 遷移前後の Screen。Pop でスタックが空になる時は `ToScreen` が `null` になることがある。 |
| `NavigatingSourceType` | `Push` / `Pop` / `Insert` / `Remove`。どの遷移種別かで分岐に使える。 |
| `IsCrossFade` / `SkipAnimation` | ナビゲーションビルダーで指定したフラグの値。 |
| `AppServices` | 現在のナビゲーターの `IServiceProvider`。 |
| `Features` / `GetFeatureValue<T>` / `GetFeatureNullableValue<T>` | `CustomFeature(key, value)` で渡された値の取得。 |
| `GetNextScreenParameter<T>()` | `NextScreenParameter(...)` の取得用ショートカット。 |
| `GetInsertionScreen()` / `GetInsertionBeforeScreen()` | Insert 時に、挿入された画面と挿入位置の前画面を取得。 |
| `GetRemoveScreen()` / `GetRemoveBeforeScreen()` / `GetRemoveAfterScreen()` | Remove 時に、削除画面とその前後を取得。 |
| `OnError` | この遷移パイプライン中に例外が発生した時に発火するイベント。遷移単位のクリーンアップに使う。 |

### アニメーションシステムの仕組み

アニメーションは Presenter プレハブ側に組み込みます。プレハブのルートに以下のコンポーネントを並べます。

- **`NavigatorAnimationPlayer`**（Meek.UGUI）— `Awake` 時に配下の `INavigatorAnimation` 群を取得し、`ScreenUI` から再生指示があったときに適切なアニメーションを再生します。
- **`NavigatorAnimationByAnimationClip`**（Meek.UGUI）— `INavigatorAnimation` の標準実装。Inspector で `NavigatorAnimationType`（`Open` / `Close` / `Show` / `Hide`）、`FromScreenName` / `ToScreenName` のフィルタ、`AnimationClip` を設定します。
- **`SimpleAnimationPlayer`** — Unity の `PlayableGraph` API でクリップを再生します。

**クリップ選択順序** — `ScreenUI` は再生時に以下の順序で最初に該当した非 null クリップを適応します。

1. `FromScreenName` と `ToScreenName` が両方一致するクリップ
2. `FromScreenName` のみ一致するクリップ
3. `ToScreenName` のみ一致するクリップ
4. 両方未指定（フォールバック）

複数の `NavigatorAnimationByAnimationClip` を 1 つのプレハブに置けば、コードを書かずに遷移元/遷移先ごとに細かく動きを差し替えられます。

**フラグの挙動:**

- **`IsCrossFade(true)`** — 前面の `Open` / `Close` クリップと背面の `Hide` / `Show` クリップを `StartParallelCoroutine` で並列実行します。デフォルトは順次実行です。
- **`SkipAnimation(true)`** — 内部でアニメーションを「飛ばす」のではなく、`ScreenUI` がアニメーションクリップの完了状態に即座に `Evaluate` します。

**Insert / Remove のデフォルト** — Insert / Remove 用の Strategy（`InsertNavigatorAnimationStrategy` / `RemoveNavigatorAnimationStrategy`）はスタック中段に対する操作専用で、デフォルトで `SkipAnimation = true` です。明示的に許可しない限り `Open` / `Close` は再生されません。

### クラス構成: Screen, ScreenUI, IViewHandler, Presenter

Meekは「スタック上の存在（Screen）」と「画面上の表示（Presenter）」を、`IViewHander` インターフェースで管理しています。1 Screen が複数 Presenter を持つ構成や、自前の View バックエンドを差し込みたい場合に把握しておくと役立ちます。

```
StackScreen : IScreen, IDisposable, IAsyncDisposable
   │
   ├─ ちょうど 1 個保持 ──────────────► ScreenUI
   │                                       │
   │                                       └─ 複数保持 ─────────► IViewHandler
   │                                                                ▲
   │                                                                │
   │                                                  IPrefabViewHandler : IViewHandler
   │                                                                ▲
   │                                                                │
   │                                                  IPresenterViewHandler : IPrefabViewHandler
   │                                                                ▲
   │                                                                │
   │                                                  DynamicPresenterViewHandler
   │                                                  : DynamicPrefabViewHandler, IPresenterViewHandler
   │
   ▼
MVPScreen<TModel> : StackScreen
```

押さえておきたい不変条件:

- **1 Screen に 1 `ScreenUI`**。`ScreenUI` は Screen 初期化時（`StackScreen.Initialize`）に DI で解決されます。
- **1 `ScreenUI` に複数 `IViewHandler`**。`LoadPresenterAsync<TPresenter>()` を呼ぶたびに `DynamicPresenterViewHandler` が作られ、`ScreenUI.AddViewHandler` で登録されます。
- **`ScreenUI` の依存相手は `IViewHandler`**。`Setup`、`SetInteractable`、`SetVisibility`、`EvaluateNavigateAnimation`、`PlayNavigateAnimationRoutine` を公開しており、Presenter は `GetPresenter<TPresenter>()` で取り出します。
- **破棄処理**。Screen がスタックから外れると、`StackScreen.DisposeAsync` が `UI.DisposeAsync()` を待ち、`ScreenUI` が各 ViewHandler を順に破棄し、ViewHandler が生成した GameObject の `OnDestroy` → Presenter の `OnDeinit` → 購読破棄が走ります。

### StackNavigationService によるスタック情報の取得

`StackNavigationService` はランタイム中の「読み取り・フック」用入口です:

```csharp
var nav = AppServices.GetService<StackNavigationService>();

// スタックの参照 — Screens はトップから順に列挙される
foreach (var screen in nav.ScreenContainer.Screens) { /* トップが最初 */ }
var top      = nav.ScreenContainer.GetPeekScreen();           // 空なら null
var detail   = nav.ScreenContainer.GetScreen<DetailScreen>();  // 見つからない場合は例外
bool onTop   = nav.IsActiveScreen(detail);

// 画面間イベント
nav.Dispatch(new ToastEvent("保存しました"));                  // 同期ブロードキャスト
bool handled = await nav.DispatchAsync(new SyncRequest());     // 最初に true を返した購読で停止

// グローバルフック（解析・ログ等）
nav.OnWillNavigate += ctx => { /* 各遷移の前 */ return new ValueTask(); };
nav.OnDidNavigate  += ctx => { /* 各遷移の後 */ return new ValueTask(); };
```

Screen 内部からは `this.NavigationService` と `this.TryGetScreen<TScreen>()` が同じ情報への近道として使えます。

---

## 応用的な使い方

### ネストナビゲーション (タブ)

個別のDIコンテナを作成することで、各タブに独立したナビゲーターを構築できます。親の `IServiceProvider` を渡すことで、シングルトン（`GlobalStore` など）を子ナビゲーター間で共有します。

下のサンプルでは `TabModel` が親の `IServiceProvider` を公開している前提で、Presenter から子ナビゲーターを構築しています。`TabModel` は以下のような形で定義してください:

```csharp
public class TabModel
{
    public IServiceProvider AppServices { get; }
    public TabModel(IServiceProvider appServices) => AppServices = appServices;
}
```

> Demo の `TabModel` には `GlobalStore` などの追加依存も含まれます。上のサンプルは「ネストナビゲーターを動かすために最低限必要な形」だけ抜き出したものです。

Presenter 側:

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

    homeServices.AddTo(this);
}
```

> **破棄処理について** — `BuildAndRunMeekMvpAsync` は `IDisposable` も実装した `IServiceProvider` を返します。この Presenter が破棄されるタイミングで Dispose することで、子ナビゲーターのスタック・画面・DI スコープがまとめて破棄されます。上のコードで使っている `.AddTo(this)` は Demo 側に用意した `IServiceProvider` 用ヘルパです。持っていない場合は `((IDisposable)homeServices).AddTo(this);` と書けば同等です。

各タブは独自のナビゲーションスタック、入力ロッカー、ライフサイクルを持ち、完全に独立しています。4つのネストナビゲーターを持つ完全な例は `Assets/Demo/Scripts/Presenters/TabPresenter.cs` を参照してください。

### Addressablesによるプレハブ読み込み

Addressablesからロードするには、`IPresenterViewProvider`拡張した`PresenterLoaderProviderFromAddressable`を登録します。

```csharp
container.AddMeekMvp(options);

// AddMeekMvp が登録した PresenterViewProviderFromResources を上書きする
container.ServiceCollection
    .AddTransient<IPresenterViewProvider, PresenterLoaderProviderFromAddressable>();
```
※  デフォルトでは `PresenterViewProviderFromResources` が登録されており、 `Resources/UI/` からロードされます。


```csharp
using System;
using System.Threading.Tasks;
using Meek;
using Meek.MVP;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

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

> **DI 登録の基本ルール** — `Add*`（`AddSingleton` / `AddScoped` / `AddTransient`）で同じサービス型を複数回登録した場合、通常は**後から登録したものに上書き**されます。逆に `TryAdd*`（`TryAddSingleton` / `TryAddTransient` など）は、**すでに登録があれば追加が無視**されます。`AddMeekMvp` は通常の `AddTransient` でデフォルトを登録しているので、後段から `AddTransient` し直すだけで差し替えられます。

### 複数ナビゲーター

Meekは静的クラスを使用しないため、複数の独立したナビゲーションスタックを作成できます：

```csharp
// ナビゲーターA
var containerA = new VContainerServiceCollection()
    .AddMeekMvp(optionsA);
containerA.BuildAndRunMeekMvpAsync<ScreenA>().Forget();

// ナビゲーターB（完全に独立）
var containerB = new VContainerServiceCollection()
    .AddMeekMvp(optionsB);
containerB.BuildAndRunMeekMvpAsync<ScreenB>().Forget();
```

### ナビゲーションイベントのフック

アナリティクス、ログ、カスタムロジックのためにナビゲーションイベントを購読できます：

```csharp
using System.Threading.Tasks;
using Meek;
using Meek.NavigationStack;
using UnityEngine;

var navigationService = appServices.GetService<StackNavigationService>();

navigationService.OnWillNavigate += context =>
{
    Debug.Log($"ナビゲーション中: {context.NavigatingSourceType}");
    return new ValueTask();
};

navigationService.OnDidNavigate += context =>
{
    Debug.Log($"ナビゲーション完了");
    return new ValueTask();
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

※ VContainerは軽量でUnityに最適化されているため、推奨される選択肢です。上のカスタムを作成することで、他のDIフレームワーク（例：Zenject）のサポートを実装できます。

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
| `ScreenWillStart` / `ScreenDidStart` | 画面の初期化（Push / Insert） |
| `ScreenWillPause` / `ScreenDidPause` | 画面がトップから外れる（Push で上に被さる側）。Insert で挿入される側は `ScreenDidPause` のみ発火 |
| `ScreenWillResume` / `ScreenDidResume` | 上の画面が Pop されて再アクティブ化される時 |
| `ScreenWillDestroy` / `ScreenDidDestroy` | 画面破棄（Pop）。Remove では `ScreenDidDestroy` のみ発火 |
| `ViewWillOpen` / `ViewDidOpen` | 表示アニメーションの開始 / 終了 |
| `ViewWillClose` / `ViewDidClose` | 非表示アニメーションの開始 / 終了 |

発火順の詳細は [各ナビゲーションのイベント実行順](#各ナビゲーションのイベント実行順) を参照してください。

---

## ライセンス

Meekは [MITライセンス](LICENSE.md) の下で公開されています。

Copyright (c) 2023 Hikaru Amano
