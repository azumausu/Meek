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

        public StackNavigationService(INavigator stackNavigator, IServiceProvider serviceProvider)
        {
            _stackNavigator = stackNavigator;
            _serviceProvider = serviceProvider;
        }

        public Task PushAsync<TScreen>(object nextScreenParameter = null, bool enableNavigationAnimation = true, bool isCrossFade = false) 
            where TScreen : IScreen
        {
            return PushAsync(typeof(TScreen), nextScreenParameter, enableNavigationAnimation, isCrossFade);
        }
        
        public async Task PushAsync(Type screenClassType, object nextScreenParameter = null, bool enableNavigationAnimation = true, bool isCrossFade = false)
        {
            await _semaphoreSlim.WaitAsync();
            try
            {
                DictionaryPool<string, object>.Get(out var features);
                
                features.Add(StackNavigationContextFeatureDefine.NextScreenParameter, nextScreenParameter);
                
                var fromScreen = _stackNavigator.ScreenContainer.Screens.FirstOrDefault();
                var toScreen = _serviceProvider.GetService(screenClassType) as IScreen
                               ?? throw new ArgumentException();
                
                var context = new StackNavigationContext()
                {
                    NavigatingSourceType = StackNavigationSourceType.Push,
                    IsCrossFade = isCrossFade,
                    EnableNavigateAnimation = enableNavigationAnimation,
                    Features = features,
                    FromScreen = fromScreen,
                    ToScreen = toScreen,
                    AppServices = _serviceProvider,
                };

                await _stackNavigator.NavigateAsync(context);
                
                DictionaryPool<string, object>.Release(features);
            }
            finally { _semaphoreSlim.Release(); }
        }

        public async Task PopAsync(bool enableNavigationAnimation = true, bool isCrossFade = false)
        {
            await _semaphoreSlim.WaitAsync();
            try
            {
                DictionaryPool<string, object>.Get(out var features);
            
                var fromScreen = _stackNavigator.ScreenContainer.GetPeekScreen();
                var toScreen = _stackNavigator.ScreenContainer.Screens.Skip(1).FirstOrDefault();
            
                var context = new StackNavigationContext()
                {
                    NavigatingSourceType = StackNavigationSourceType.Pop,
                    IsCrossFade = isCrossFade,
                    EnableNavigateAnimation = enableNavigationAnimation,
                    Features = features,
                    FromScreen = fromScreen,
                    ToScreen = toScreen,
                    AppServices = _serviceProvider,
                };

                await _stackNavigator.NavigateAsync(context);
            
                DictionaryPool<string, object>.Release(features);
                
            }
            finally { _semaphoreSlim.Release(); } 
        }

        public Task InsertScreenBeforeAsync<TBeforeScreen, TInsertionScreen>(object nextScreenParameter = null)
            where TBeforeScreen : IScreen
            where TInsertionScreen : IScreen
        {
            return InsertScreenBeforeAsync(typeof(TBeforeScreen), typeof(TInsertionScreen), nextScreenParameter);
        }
        
        public async Task InsertScreenBeforeAsync(Type beforeScreenClassType, Type insertionScreenClassType, object nextScreenParameter = null)
        {
            var fromScreen = _stackNavigator.ScreenContainer.GetPeekScreen();
            if (fromScreen?.GetType() == beforeScreenClassType)
            {
                Debug.LogWarning("Trying to insert into peek screen. You can convert \"Invert\" to \"Push\"");
                await PushAsync(insertionScreenClassType, nextScreenParameter);
                return;
            }
            
            await _semaphoreSlim.WaitAsync();
            try
            {
                DictionaryPool<string, object>.Get(out var features);
                
                var insertionScreen = _serviceProvider.GetService(insertionScreenClassType) as IScreen
                                      ?? throw new ArgumentException();
                
                features.Add(StackNavigationContextFeatureDefine.InsertionBeforeScreenType, beforeScreenClassType);
                features.Add(StackNavigationContextFeatureDefine.InsertionScreen, insertionScreen);
                features.Add(StackNavigationContextFeatureDefine.NextScreenParameter, nextScreenParameter);
                
                var context = new StackNavigationContext()
                {
                    NavigatingSourceType = StackNavigationSourceType.Insert,
                    IsCrossFade = false,
                    EnableNavigateAnimation = false,
                    Features = features,
                    FromScreen = fromScreen,
                    ToScreen = fromScreen,
                    AppServices = _serviceProvider,
                };

                await _stackNavigator.NavigateAsync(context);
            
                DictionaryPool<string, object>.Release(features); 
            }
            finally { _semaphoreSlim.Release(); } 
        }

        public Task RemoveAsync<TScreen>() where TScreen : IScreen
        {
            return RemoveAsync(typeof(TScreen));
        }
        
        public async Task RemoveAsync(Type screenClassType)
        {
            var fromScreen = _stackNavigator.ScreenContainer.GetPeekScreen();
            if (fromScreen?.GetType() == screenClassType)
            {
                Debug.LogWarning("Trying to remove into peek screen. You can convert \"Remove\" to \"Pop\"");
                await PopAsync();
                return;
            }
            
            await _semaphoreSlim.WaitAsync();
            try
            {
                DictionaryPool<string, object>.Get(out var features);
            
                features.Add(StackNavigationContextFeatureDefine.RemoveScreenType, screenClassType);
                features.Add(StackNavigationContextFeatureDefine.RemoveScreen, _stackNavigator.ScreenContainer.GetScreen(screenClassType) as IScreen);
                features.Add(StackNavigationContextFeatureDefine.RemoveBeforeScreen, _stackNavigator.ScreenContainer.GetScreenBefore(screenClassType));
                features.Add(StackNavigationContextFeatureDefine.RemoveAfterScreen, _stackNavigator.ScreenContainer.GetScreenAfter(screenClassType));
            
                var context = new StackNavigationContext()
                {
                    NavigatingSourceType = StackNavigationSourceType.Remove,
                    IsCrossFade = false,
                    EnableNavigateAnimation = false,
                    Features = features,
                    FromScreen = fromScreen,
                    ToScreen = fromScreen,
                    AppServices = _serviceProvider,
                };

                await _stackNavigator.NavigateAsync(context);
            
                DictionaryPool<string, object>.Release(features);  
            }
            finally { _semaphoreSlim.Release(); }  
        }

        public void Dispatch(string eventName, object parameter)
        {
            DispatchAsync(eventName, parameter).Forget();
        }

        private async Task DispatchAsync(string eventName, object parameter)
        {
            await _semaphoreSlim.WaitAsync();
            try { DispatchInternal(eventName, parameter); }
            finally { _semaphoreSlim.Release(); }
        }

        private bool DispatchInternal(string eventValue, object param = null)
        {
            foreach (var screen in _stackNavigator.ScreenContainer.Screens.OfType<StackScreen>())
            {
                // まだ存在してないタイミングにDispatchが呼ばれたら無視する。
                if (screen.ScreenEventInvoker == null) continue;

                // trueが返されたら終了
                if (screen.ScreenEventInvoker.Dispatch(eventValue, param)) return true;
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