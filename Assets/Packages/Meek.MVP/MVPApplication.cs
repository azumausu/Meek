using System;
using Meek.NavigationStack;
using Meek.NavigationStack.Child;
using Meek.UGUI;
using UnityEngine;

namespace Meek.MVP
{
    public class MVPApplication
    {
        public IServiceProvider CreateApp<TBootScreen>(
            Func<IServiceProvider, IContainerBuilder> containerBuilderFactory,
            IGlobalInputLocker globalInputLocker,
            IPrefabViewManager prefabViewManager,
            Action<IServiceCollection> configure
            ) where TBootScreen : IScreen
        {
            // 依存関係順にContainerを作成する。
            
            // Global Service
            var rootContainerBuilder = containerBuilderFactory(null);
            var gameObject = new GameObject("CoroutineRunner");
            var coroutineRunner = gameObject.AddComponent<CoroutineRunner>();
            UnityEngine.Object.DontDestroyOnLoad(coroutineRunner);
            rootContainerBuilder.ServiceCollection.AddSingleton(coroutineRunner);
            var rootContainer = rootContainerBuilder.Build();
            
            // ChildStackNavigator Service
            var childStackNavigator = new NavigatorBuilder(option =>
                {
                    option.ContainerBuilder = containerBuilderFactory(rootContainer);
                    option.ScreenNavigator.Set<ChildStackScreenContainer>();
                }).ConfigureServices(serviceCollection => { }).Configure(app => { })
                .Build();
            
            // StackNavigator Service
            var stackNavigator = new NavigatorBuilder(option =>
            {
                option.ContainerBuilder = containerBuilderFactory(childStackNavigator.ServiceProvider);
                option.ScreenNavigator.Set<StackScreenContainer>();
            }).ConfigureServices(serviceCollection =>
            {
                serviceCollection.AddScreenNavigatorEvent();
                serviceCollection.AddGlobalInputLocker(x => { x.GlobalInputLocker = globalInputLocker; });
                serviceCollection.AddScreenUI();
                serviceCollection.AddNavigatorAnimation(
                    x =>
                    {
                        x.Strategies.Add<PushNavigatorAnimationStrategy>();
                        x.Strategies.Add<PopNavigatorAnimationStrategy>();
                        x.Strategies.Add<RemoveNavigatorAnimationStrategy>();
                        x.Strategies.Add<InsertNavigatorAnimationStrategy>();
                    }
                );
                serviceCollection.AddUGUIAsMVP(x =>
                {
                    x.UGUIOption.PrefabViewManager = prefabViewManager;
                    x.PresenterLoaderFactory.Set<PresenterLoaderFactoryFromResources>();
                });
                serviceCollection.AddScreenLifecycleEvent();
                serviceCollection.AddSingleton(new SyncChildScreenContainerMiddleware(childStackNavigator));
            }).Configure(app =>
            {
                app.UseScreenNavigatorEvent();
                app.UseGlobalInputLocker();
                app.UseScreenUI();
                app.UseNavigatorAnimation();
                app.UseMiddleware<SyncChildScreenContainerMiddleware>();
                app.UseUGUI();
                app.UseScreenLifecycleEvent();
            }).Build();
            
            
            // App Service
            var appBuilder = containerBuilderFactory(stackNavigator.ServiceProvider);
            appBuilder.ServiceCollection.AddSingleton(x => new StackNavigationService(stackNavigator, x));
            appBuilder.ServiceCollection.AddSingleton(x => new ChildStackNavigationService(childStackNavigator, x));
            configure(appBuilder.ServiceCollection);
            var app = appBuilder.Build();
            
            // Push Initial Screen
            var stackNavigatorService = app.GetService<StackNavigationService>();
            stackNavigatorService.PushAsync<TBootScreen>().Forget();

            return app;
        }
    }
}