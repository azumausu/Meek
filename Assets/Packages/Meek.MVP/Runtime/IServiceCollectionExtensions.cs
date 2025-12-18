using System;
using Meek.NavigationStack;
using Meek.NavigationStack.Debugs;
using Meek.UGUI;

namespace Meek.MVP
{
    public static class IServiceCollectionExtensions
    {
        public static IServiceCollection AddMeekMvp(this IServiceCollection serviceCollection, MvpNavigatorOptions options)
        {
            if (options?.InputLocker == null)
            {
                throw new ArgumentNullException(nameof(options.InputLocker), "InputLocker must be provided in MvpNavigatorOptions.");
            }

            if (options?.PrefabViewManager == null)
            {
                throw new ArgumentNullException(nameof(options.PrefabViewManager),
                    "PrefabViewManager must be provided in MvpNavigatorOptions.");
            }

            serviceCollection.AddSingleton<MvpNavigatorOptions>();
            serviceCollection.AddSingleton(options.InputLocker);
            serviceCollection.AddSingleton(options.PrefabViewManager);

            serviceCollection.AddSingleton<IScreenContainer, StackScreenContainer>();
            serviceCollection.AddSingleton<IPresenterLoaderFactory, PresenterLoaderFactoryFromResources>();
            serviceCollection.AddSingleton<NavigationSharedSemaphore>();
            serviceCollection.AddSingleton(x => new StackNavigationService(x.GetService<INavigator>(), x));
            serviceCollection.AddSingleton<INavigator, MvpNavigator>();
            serviceCollection.TryAddSingleton<ICoroutineRunner, CoroutineRunner>();
            if (options.DebugOption.UseDebug)
            {
                serviceCollection.AddSingleton(options.DebugOption);
                serviceCollection.AddSingleton(x => new ServiceRegistrationHandler(x));
            }

            serviceCollection.AddTransient<ScreenUI>();
            serviceCollection.AddTransient<PushNavigatorAnimationStrategy>();
            serviceCollection.AddTransient<PopNavigatorAnimationStrategy>();
            serviceCollection.AddTransient<RemoveNavigatorAnimationStrategy>();
            serviceCollection.AddTransient<InsertNavigatorAnimationStrategy>();
            serviceCollection.AddTransient<PushNavigation>();
            serviceCollection.AddTransient<PopNavigation>();
            serviceCollection.AddTransient<RemoveNavigation>();
            serviceCollection.AddTransient<InsertNavigation>();
            serviceCollection.AddTransient<BackToNavigation>();

            return serviceCollection;
        }
    }
}