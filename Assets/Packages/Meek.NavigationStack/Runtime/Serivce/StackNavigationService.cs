using System;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Pool;

namespace Meek.NavigationStack
{
    public class StackNavigationService
    {
        private readonly INavigator _stackNavigator;
        private readonly IServiceProvider _serviceProvider;
        private event Func<StackNavigationContext, ValueTask> _willNavigate;
        private event Func<StackNavigationContext, ValueTask> _didNavigate;

        public IScreenContainer ScreenContainer => _stackNavigator.ScreenContainer;

        public event Func<StackNavigationContext, ValueTask> OnWillNavigate
        {
            add => _willNavigate += value;
            remove => _willNavigate -= value;
        }

        public event Func<StackNavigationContext, ValueTask> OnDidNavigate
        {
            add => _didNavigate += value;
            remove => _didNavigate -= value;
        }

        public StackNavigationService(INavigator stackNavigator, IServiceProvider serviceProvider)
        {
            _stackNavigator = stackNavigator;
            _serviceProvider = serviceProvider;
        }

        public Task PushAsync<TScreen>(PushContext pushContext)
            where TScreen : IScreen
        {
            return PushAsync(typeof(TScreen), pushContext);
        }

        public async Task PushAsync(Type screenClassType, PushContext pushContext)
        {
            using var poolDisposable = DictionaryPool<string, object>.Get(out var features);

            var fromScreen = _stackNavigator.ScreenContainer.Screens.FirstOrDefault() as StackScreen;
            var toScreen = _serviceProvider.GetService(screenClassType) as StackScreen;

            var context = new StackNavigationContext()
            {
                NavigatingSourceType = StackNavigationSourceType.Push,
                IsCrossFade = pushContext.IsCrossFade,
                SkipAnimation = pushContext.SkipAnimation,
                Features = features,
                FromScreen = fromScreen,
                ToScreen = toScreen,
                AppServices = _serviceProvider,
            };
            context.SetNextScreenParameter(pushContext.NextScreenParameter);

            try
            {
                await InvokeWillNavigateAsync(context);
                await _stackNavigator.NavigateAsync(context);
                await InvokeDidNavigateAsync(context);
            }
            catch
            {
                if (toScreen is IDisposable disposable) disposable.Dispose();
                if (toScreen is IAsyncDisposable asyncDisposable) await asyncDisposable.DisposeAsync();
                throw;
            }
        }

        public async Task<bool> PopAsync(PopContext popContext)
        {
            using var poolDisposable = DictionaryPool<string, object>.Get(out var features);

            var fromScreen = _stackNavigator.ScreenContainer.GetPeekScreen();
            var toScreen = _stackNavigator.ScreenContainer.Screens.Skip(1).FirstOrDefault();

            if (popContext.UseOnlyWhenScreen)
            {
                if (popContext.OnlyWhenScreen != fromScreen) return false;
            }

            var context = new StackNavigationContext()
            {
                NavigatingSourceType = StackNavigationSourceType.Pop,
                IsCrossFade = popContext.IsCrossFade,
                SkipAnimation = popContext.SkipAnimation,
                Features = features,
                FromScreen = fromScreen,
                ToScreen = toScreen,
                AppServices = _serviceProvider,
            };

            await InvokeWillNavigateAsync(context);
            await _stackNavigator.NavigateAsync(context);
            await InvokeDidNavigateAsync(context);

            return true;
        }

        /// <summary>
        ///  If two or more screens of the same type exist, the one at the top of the stack will be selected.
        /// </summary>
        public Task InsertScreenBeforeAsync<TBeforeScreen, TInsertionScreen>(InsertContext insertionContext)
            where TBeforeScreen : IScreen
            where TInsertionScreen : IScreen
        {
            return InsertScreenBeforeAsync(typeof(TBeforeScreen), typeof(TInsertionScreen), insertionContext);
        }

        /// <summary>
        ///  If two or more screens of the same type exist, the one at the top of the stack will be selected.
        /// </summary>
        public Task InsertScreenBeforeAsync(Type beforeScreenClassType, Type insertionScreenClassType, InsertContext insertionContext)
        {
            return InsertScreenBeforeAsync(
                _stackNavigator.ScreenContainer.GetScreen(beforeScreenClassType),
                insertionScreenClassType,
                insertionContext
            );
        }

        public async Task InsertScreenBeforeAsync(IScreen beforeScreen, Type insertionScreenClassType, InsertContext insertionContext)
        {
            var fromScreen = _stackNavigator.ScreenContainer.GetPeekScreen();
            if (fromScreen == beforeScreen)
            {
                Debug.LogWarning("Trying to insert into peek screen. You can convert \"Invert\" to \"Push\"");
                var pushContext = new PushContext()
                {
                    NextScreenParameter = insertionContext.NextScreenParameter,
                    IsCrossFade = insertionContext.IsCrossFade,
                    SkipAnimation = insertionContext.SkipAnimation,
                };
                await PushAsync(insertionScreenClassType, pushContext);
                return;
            }

            using var poolDisposable = DictionaryPool<string, object>.Get(out var features);

            var insertionScreen = _serviceProvider.GetService(insertionScreenClassType) as IScreen
                                  ?? throw new ArgumentException();

            var context = new StackNavigationContext()
            {
                NavigatingSourceType = StackNavigationSourceType.Insert,
                IsCrossFade = insertionContext.IsCrossFade,
                SkipAnimation = insertionContext.SkipAnimation,
                Features = features,
                FromScreen = fromScreen,
                ToScreen = fromScreen,
                AppServices = _serviceProvider,
            };
            context.SetNextScreenParameter(insertionContext.NextScreenParameter);
            context.SetInsertionBeforeScreen((beforeScreen as StackScreen));
            context.SetInsertionScreen((insertionScreen as StackScreen));


            try
            {
                await InvokeWillNavigateAsync(context);
                await _stackNavigator.NavigateAsync(context);
                await InvokeDidNavigateAsync(context);
            }
            catch
            {
                if (insertionScreen is IDisposable disposable) disposable.Dispose();
                if (insertionScreen is IAsyncDisposable asyncDisposable) await asyncDisposable.DisposeAsync();
                throw;
            }
        }

        /// <summary>
        /// Removes the specified screen type from the navigation stack.
        /// If two or more screens of the same type exist, the one at the top of the stack will be selected.
        /// </summary>
        public Task RemoveAsync<TScreen>(RemoveContext removeContext) where TScreen : IScreen
        {
            return RemoveAsync(typeof(TScreen), removeContext);
        }

        /// <summary>
        /// Removes the specified screen type from the navigation stack.
        /// If two or more screens of the same type exist, the one at the top of the stack will be selected.
        /// </summary>
        public Task RemoveAsync(Type removeScreenClassType, RemoveContext removeContext)
        {
            return RemoveAsync(_stackNavigator.ScreenContainer.GetScreen(removeScreenClassType), removeContext);
        }

        public async Task RemoveAsync(IScreen removeScreen, RemoveContext removeContext)
        {
            var fromScreen = _stackNavigator.ScreenContainer.GetPeekScreen();
            if (fromScreen == removeScreen)
            {
                Debug.LogWarning("Trying to remove into peek screen. You can convert \"Remove\" to \"Pop\"");
                var popContext = new PopContext()
                {
                    IsCrossFade = removeContext.IsCrossFade,
                    SkipAnimation = removeContext.SkipAnimation,
                };
                await PopAsync(popContext);
                return;
            }

            using var poolDisposable = DictionaryPool<string, object>.Get(out var features);
            var beforeScreen = _stackNavigator.ScreenContainer.GetScreenBefore(removeScreen);
            var afterScreen = _stackNavigator.ScreenContainer.GetScreenAfter(removeScreen);
            var context = new StackNavigationContext()
            {
                NavigatingSourceType = StackNavigationSourceType.Remove,
                IsCrossFade = false,
                SkipAnimation = false,
                Features = features,
                FromScreen = fromScreen,
                ToScreen = fromScreen,
                AppServices = _serviceProvider,
            };
            context.SetRemoveScreen((removeScreen as StackScreen));
            context.SetRemoveAfterScreen((afterScreen as StackScreen));
            context.SetRemoveBeforeScreen((beforeScreen as StackScreen));

            await InvokeWillNavigateAsync(context);
            await _stackNavigator.NavigateAsync(context);
            await InvokeDidNavigateAsync(context);
        }

        public void Dispatch<T>(T args)
        {
            DispatchInternal(typeof(T).FullName, args);
        }

        public void Dispatch(string eventName, object parameter)
        {
            DispatchInternal(eventName, parameter);
        }

        public Task DispatchAsync<T>(T args)
        {
            return DispatchInternalAsync(typeof(T).FullName, args);
        }

        public Task DispatchAsync(string eventName, object parameter)
        {
            return DispatchInternalAsync(eventName, parameter);
        }

        private bool DispatchInternal(string eventValue, object param = null)
        {
            foreach (var screen in _stackNavigator.ScreenContainer.Screens)
            {
                if (screen is not StackScreen stackScreen) continue;

                // まだ存在してないタイミングにDispatchが呼ばれたら無視する。
                if (stackScreen.ScreenEventInvoker == null) continue;

                // trueが返されたら終了
                if (stackScreen.ScreenEventInvoker.Dispatch(eventValue, param)) return true;
            }

            return false;
        }

        private async Task<bool> DispatchInternalAsync(string eventValue, object param = null)
        {
            foreach (var screen in _stackNavigator.ScreenContainer.Screens)
            {
                if (screen is not StackScreen stackScreen) continue;

                // まだ存在してないタイミングにDispatchが呼ばれたら無視する。
                if (stackScreen.ScreenEventInvoker == null) continue;

                // trueが返されたら終了
                var result = await stackScreen.ScreenEventInvoker.DispatchAsync(eventValue, param);
                if (result) return true;
            }

            return false;
        }

        public bool IsActiveScreen(IScreen screen)
        {
            var peekScreen = _stackNavigator.ScreenContainer.GetPeekScreen();
            return peekScreen == screen;
        }

        private async ValueTask InvokeWillNavigateAsync(StackNavigationContext context)
        {
            if (_willNavigate != null)
            {
                await _willNavigate.Invoke(context);
            }
        }

        private async ValueTask InvokeDidNavigateAsync(StackNavigationContext context)
        {
            if (_didNavigate != null)
            {
                await _didNavigate.Invoke(context);
            }
        }
    }
}