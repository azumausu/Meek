using System;
using Meek.NavigationStack;
using Meek.UGUI;
using UnityEngine;

namespace Meek.MVP
{
    public class MVPApplication
    {
        public IServiceProvider CreateApp<TBootScreen>(
            Func<IServiceProvider, IContainerBuilder> containerBuilderFactory,
            IInputLocker inputLocker,
            IPrefabViewManager prefabViewManager,
            Action<IServiceCollection> configure
            ) where TBootScreen : IScreen
        {
            // Global Service
            var rootContainerBuilder = containerBuilderFactory(null);
            var gameObject = new GameObject("CoroutineRunner");
            var coroutineRunner = gameObject.AddComponent<CoroutineRunner>();
            UnityEngine.Object.DontDestroyOnLoad(coroutineRunner);
            rootContainerBuilder.ServiceCollection.AddSingleton(coroutineRunner);
            var rootContainer = rootContainerBuilder.Build();

            // StackNavigator Service
            var stackNavigator = StackNavigator.CreateAsMVP(
                containerBuilderFactory(rootContainer),
                inputLocker,
                prefabViewManager
            );
            
            // App Service
            var appBuilder = containerBuilderFactory(stackNavigator.ServiceProvider);
            appBuilder.ServiceCollection.AddSingleton(stackNavigator);
            appBuilder.ServiceCollection.AddSingleton(x =>
            {
                var stackNavigatorService = x.GetService<StackNavigator>();
                return new StackNavigationService(stackNavigatorService, x);
            });
            configure(appBuilder.ServiceCollection);
            var app = appBuilder.Build();
            
            // Push Initial Screen
            var stackNavigatorService = app.GetService<StackNavigationService>();
            stackNavigatorService.PushAsync<TBootScreen>().Forget();

            return app;
        }
    }
}