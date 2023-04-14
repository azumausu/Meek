using System;
using System.Threading.Tasks;
using Meek.NavigationStack;
using Meek.NavigationStack.MVP;
using Meek.UGUI;

namespace Meek.MVP
{
    public static class MVPApplication
    {
        public static IServiceProvider CreateRootApp<TBootScreen>(
            Func<IServiceProvider, IContainerBuilder> containerBuilderFactory,
            IInputLocker inputLocker,
            IPrefabViewManager prefabViewManager,
            Action<IServiceCollection> configure
            ) where TBootScreen : IScreen
        {
            // StackNavigator
            var stackNavigator = StackNavigator.CreateAsMVP(containerBuilderFactory(null), inputLocker, prefabViewManager);
            
            // App Service
            var appBuilder = containerBuilderFactory(stackNavigator.ServiceProvider);
            appBuilder.ServiceCollection.AddSingleton<INavigator>(stackNavigator);
            appBuilder.ServiceCollection.AddNavigationService();
            
            configure(appBuilder.ServiceCollection);
            var app = appBuilder.Build();
            
            // Push Initial Screen
            app.GetService<PushNavigation>().PushAsync<TBootScreen>().Forget();

            return app;
        }

        public static async Task<IServiceProvider> CreateChildAppAsync<TBootScreen>(
            Func<IServiceProvider, IContainerBuilder> containerBuilderFactory,
            IInputLocker inputLocker,
            IPrefabViewManager prefabViewManager,
            Action<IServiceCollection> configure,
            IServiceProvider parentApp = null
        ) where TBootScreen : IScreen
        {
            var stackNavigator = StackNavigator.CreateAsMVP(containerBuilderFactory(parentApp), inputLocker, prefabViewManager);
            
            // App Service
            var appBuilder = containerBuilderFactory(stackNavigator.ServiceProvider);
            appBuilder.ServiceCollection.AddSingleton<INavigator>(stackNavigator);
            appBuilder.ServiceCollection.AddNavigationService();
            
            configure(appBuilder.ServiceCollection);
            var app = appBuilder.Build();
            
            // Push Initial Screen
            await app.GetService<PushNavigation>().UpdateSkipAnimation(true).PushAsync<TBootScreen>();

            return app; 
        }
    }
}