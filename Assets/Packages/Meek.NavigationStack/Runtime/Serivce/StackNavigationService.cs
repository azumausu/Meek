using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Pool;

namespace Meek.NavigationStack
{
    public class StackNavigationService
    {
        private readonly INavigator _stackNavigator;
        private readonly IServiceProvider _serviceProvider;
        private readonly SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1, 1);

        public IScreenContainer ScreenContainer => _stackNavigator.ScreenContainer;

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
            await _semaphoreSlim.WaitAsync();
            try
            {
                DictionaryPool<string, object>.Get(out var features);

                features.Add(StackNavigationContextFeatureDefine.NextScreenParameter, pushContext.NextScreenParameter);

                var fromScreen = _stackNavigator.ScreenContainer.Screens.FirstOrDefault();
                var toScreen = _serviceProvider.GetService(screenClassType) as IScreen
                               ?? throw new ArgumentException();

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

                await _stackNavigator.NavigateAsync(context);

                DictionaryPool<string, object>.Release(features);
            }
            finally
            {
                _semaphoreSlim.Release();
            }
        }

        public async Task<bool> PopAsync(PopContext popContext)
        {
            await _semaphoreSlim.WaitAsync();
            try
            {
                DictionaryPool<string, object>.Get(out var features);

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

                await _stackNavigator.NavigateAsync(context);

                DictionaryPool<string, object>.Release(features);

                return true;
            }
            finally
            {
                _semaphoreSlim.Release();
            }
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

            await _semaphoreSlim.WaitAsync();
            try
            {
                DictionaryPool<string, object>.Get(out var features);

                var insertionScreen = _serviceProvider.GetService(insertionScreenClassType) as IScreen
                                      ?? throw new ArgumentException();

                features.Add(StackNavigationContextFeatureDefine.InsertionBeforeScreen, beforeScreen);
                features.Add(StackNavigationContextFeatureDefine.InsertionScreen, insertionScreen);
                features.Add(StackNavigationContextFeatureDefine.NextScreenParameter, insertionContext.NextScreenParameter);

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

                await _stackNavigator.NavigateAsync(context);

                DictionaryPool<string, object>.Release(features);
            }
            finally
            {
                _semaphoreSlim.Release();
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

            await _semaphoreSlim.WaitAsync();
            try
            {
                DictionaryPool<string, object>.Get(out var features);
                var beforeScreen = _stackNavigator.ScreenContainer.GetScreenBefore(removeScreen);
                var afterScreen = _stackNavigator.ScreenContainer.GetScreenAfter(removeScreen);
                features.Add(StackNavigationContextFeatureDefine.RemoveScreenType, removeScreen.GetType());
                features.Add(StackNavigationContextFeatureDefine.RemoveScreen, removeScreen);
                features.Add(StackNavigationContextFeatureDefine.RemoveBeforeScreen, beforeScreen);
                features.Add(StackNavigationContextFeatureDefine.RemoveAfterScreen, afterScreen);

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

                await _stackNavigator.NavigateAsync(context);

                DictionaryPool<string, object>.Release(features);
            }
            finally
            {
                _semaphoreSlim.Release();
            }
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
    }
}