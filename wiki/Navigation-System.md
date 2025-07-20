# ナビゲーションシステム

Meekのナビゲーションシステムは、スタックベースの画面管理を提供し、型安全で直感的な画面遷移を実現します。

## StackNavigationService

### 概要

`StackNavigationService`は、スタックナビゲーションのメインAPIです。スレッドセーフな設計により、並行して実行される複数のナビゲーション操作を安全に処理します。

**場所**: `Meek.NavigationStack/Runtime/Serivce/StackNavigationService.cs:10`

```csharp
public class StackNavigationService
{
    private readonly SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1, 1);
    // スレッドセーフな操作を保証
}
```

## スタック操作

### PushAsync - 画面追加

新しい画面をスタックのトップに追加します。

```csharp
// 基本的な使用方法
await PushNavigation.PushAsync<UserProfileScreen>();

// パラメーター付き
var userModel = new UserModel { Id = 123 };
await PushNavigation
    .NextScreenParameter(userModel)
    .PushAsync<UserDetailScreen>();

// オプション指定
await PushNavigation
    .IsCrossFade(true)      // クロスフェードアニメーション
    .SkipAnimation(false)   // アニメーション実行
    .PushAsync<ModalScreen>();
```

**実装詳細**:
```csharp
// StackNavigationService.cs:24
public Task PushAsync<TScreen>(PushContext pushContext) where TScreen : IScreen
{
    return PushAsync(typeof(TScreen), pushContext);
}
```

### PopAsync - 画面削除

スタックトップの画面を削除します。

```csharp
// 無条件でPop
await PopNavigation.PopAsync();

// 条件付きPop（特定の画面の時のみ）
await PopNavigation
    .OnlyWhen(currentScreen)
    .PopAsync();
```

**実装詳細**:
```csharp
// StackNavigationService.cs:64
public async Task<bool> PopAsync(PopContext popContext)
{
    // 戻り値でPop成功/失敗を判定
    if (popContext.UseOnlyWhenScreen)
    {
        if (popContext.OnlyWhenScreen != fromScreen) return false;
    }
    return true;
}
```

PopNavigation は fluent インターフェースで使用できます：
```csharp
// PopNavigation.cs
public PopNavigation OnlyWhen(IScreen screen)
{
    Context.OnlyWhenScreen = screen;
    return this;
}
```

### InsertScreenBeforeAsync - 画面挿入

指定した画面の直前に新しい画面を挿入します。

```csharp
// 特定画面の前に挿入
await InsertNavigation.InsertScreenBeforeAsync<TargetScreen, NewScreen>();

// パラメーター付き挿入
await InsertNavigation
    .NextScreenParameter(parameter)
    .IsCrossFade(false)
    .InsertScreenBeforeAsync<TargetScreen, NewScreen>();
```

**特殊処理**:
- Peek画面（スタックトップ）への挿入は自動的にPushに変換
- `StackNavigationService.cs:127-137`で実装

### RemoveAsync - 画面削除

スタック中の特定の画面を削除します。

```csharp
// 型指定で削除
await RemoveNavigation.RemoveAsync<UnwantedScreen>();

// インスタンス指定で削除
await RemoveNavigation.RemoveAsync(screenInstance);
```

**特殊処理**:
- Peek画面の削除は自動的にPopに変換
- `StackNavigationService.cs:194`で実装

## パラメーター渡し

### NextScreenParameter

画面間でデータを受け渡しする仕組み。

```csharp
// 送信側
var userData = new UserModel { Name = "John", Age = 30 };
await PushNavigation
    .NextScreenParameter(userData)
    .PushAsync<UserDetailScreen>();

// 受信側 (MVPScreen<TModel, TParam>)
public class UserDetailScreen : MVPScreen<UserDetailModel, UserModel>
{
    protected override ValueTask<UserDetailModel> CreateModelAsync(UserModel parameter)
    {
        // parameterにUserModelが渡される
        return new ValueTask<UserDetailModel>(new UserDetailModel(parameter));
    }
}
```

### Features Dictionary

より複雑なデータ受け渡しには Features を使用。

```csharp
// StackNavigationContext.cs
public class StackNavigationContext : NavigationContext
{
    public IDictionary<string, object> Features { get; set; }
}

// カスタムデータの格納
features.Add("CustomData", complexObject);
var customData = context.GetFeatureValue<ComplexObject>("CustomData");
```

## Dispatch機能

### 概要

スタック内の全画面にイベントを送信します。内部では最初にtrueを返した画面で処理を停止しますが、publicメソッドには返り値がありません。

```csharp
// 同期的なDispatch（返り値なし）
StackNavigationService.Dispatch(new RefreshEvent());

// 非同期Dispatch（返り値なし）
await StackNavigationService.DispatchAsync(new SaveDataEvent());

// 文字列ベース
StackNavigationService.Dispatch("UserChanged", userData);
```

### 実装詳細

```csharp
// StackNavigationService.cs:257
private bool DispatchInternal(string eventValue, object param = null)
{
    foreach (var screen in _stackNavigator.ScreenContainer.Screens)
    {
        if (screen is not StackScreen stackScreen) continue;
        
        // trueが返されたら終了
        if (stackScreen.ScreenEventInvoker.Dispatch(eventValue, param)) 
            return true;
    }
    return false;
}
```

### 受信側の実装

```csharp
public class SomeScreen : MVPScreen<SomeModel>
{
    protected override void RegisterEvents(EventHolder eventHolder, SomeModel model)
    {
        // Dispatchイベントの受信
        eventHolder.Dispatch<RefreshEvent>(refreshEvent =>
        {
            // イベント処理
            model.Refresh();
            return true; // 処理完了を示すためtrueを返す
        });
    }
}
```

## StackScreenContainer

### スタック管理の詳細

`StackScreenContainer`はLIFO（後入れ先出し）構造で画面を管理します。

**場所**: `Meek.NavigationStack/Runtime/ScreenContainer/StackScreenContainer.cs:9`

```csharp
public class StackScreenContainer : IScreenContainer, IDisposable
{
    private readonly Stack<IScreen> _screenStack = new Stack<IScreen>(32);
    private readonly Stack<IScreen> _insertOrRemoveCacheStack = new(16);
}
```

### Insert/Remove操作

複雑なスタック操作では一時キャッシュを使用：

```csharp
// StackScreenContainer.cs:31 - Insert操作
case StackNavigationSourceType.Insert:
    var insertionBeforeScreen = context.GetFeatureValue<IScreen>(...);
    
    // 対象画面まで一時的にPop
    while (_screenStack.Peek() != insertionBeforeScreen)
    {
        _insertOrRemoveCacheStack.Push(_screenStack.Pop());
    }
    
    // 新しい画面を挿入
    _screenStack.Push(insertionScreen);
    
    // 一時的にPopした画面を戻す
    foreach (var screen in _insertOrRemoveCacheStack) 
        _screenStack.Push(screen);
    _insertOrRemoveCacheStack.Clear();
```

## 使用例

### 基本的な画面遷移

```csharp
public class MainMenuScreen : MVPScreen<MainMenuModel>
{
    protected override void RegisterEvents(EventHolder eventHolder, MainMenuModel model)
    {
        eventHolder.ScreenWillStart(async () =>
        {
            var presenter = await LoadPresenterAsync<MainMenuPresenter>();
            
            // 設定画面へ
            presenter.OnSettingsClick.Subscribe(_ => 
                PushNavigation.PushAsync<SettingsScreen>());
            
            // ユーザープロフィールへ（パラメーター付き）
            presenter.OnProfileClick.Subscribe(_ => 
                PushNavigation
                    .NextScreenParameter(model.CurrentUser)
                    .PushAsync<ProfileScreen>());
        });
    }
}
```

### 複雑なナビゲーション

```csharp
// ウィザード形式での画面挿入
public class WizardStep2Screen : MVPScreen<WizardStep2Model>
{
    private async Task GoToStep4()
    {
        // Step3とStep4の間にStep3.5を挿入
        await InsertNavigation.InsertScreenBeforeAsync<WizardStep4Screen, WizardStep3_5Screen>();
        
        // 現在のステップをスキップしてStep4へ
        await PushNavigation.PushAsync<WizardStep4Screen>();
    }
}
```


## 次のステップ

- [MVPアーキテクチャ](MVP-Architecture) - 画面とプレゼンターの実装
- [ミドルウェアシステム](Middleware-System) - アニメーションと拡張

---

[[← Framework Overview]](Framework-Overview) | [[MVP Architecture →]](MVP-Architecture)