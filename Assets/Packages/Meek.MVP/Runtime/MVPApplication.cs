using System;
using Meek.NavigationStack;
using Meek.NavigationStack.MVP;
using Meek.UGUI;

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
    }
}