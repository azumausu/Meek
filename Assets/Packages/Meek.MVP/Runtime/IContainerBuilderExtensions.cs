using System;
using System.Threading.Tasks;
using Meek.NavigationStack;
using Meek.NavigationStack.Debugs;
using Meek;

namespace Meek.MVP
{
    public static class IContainerBuilderExtensions
    {
        public static IContainerBuilder AddMeekMvp(this IContainerBuilder builder, MvpNavigatorOptions options)
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

            builder.ServiceCollection.AddSingleton<MvpNavigatorOptions>();
            builder.ServiceCollection.AddSingleton(options.InputLocker);
            builder.ServiceCollection.AddSingleton(options.PrefabViewManager);

            builder.ServiceCollection.AddSingleton<IScreenContainer, StackScreenContainer>();
            builder.ServiceCollection.AddSingleton<NavigationSharedSemaphore>();
            builder.ServiceCollection.AddSingleton(x => new StackNavigationService(x.GetService<INavigator>(), x));
            builder.ServiceCollection.AddSingleton<INavigator, MvpNavigator>();
            builder.ServiceCollection.TryAddSingleton<ICoroutineRunner, CoroutineRunner>();
            if (options.DebugOption.UseDebug)
            {
                builder.ServiceCollection.AddSingleton(options.DebugOption);
                builder.ServiceCollection.AddSingleton(x => new ServiceRegistrationHandler(x));
            }

            builder.ServiceCollection.AddTransient<IPresenterViewHandler, DynamicPresenterViewHandler>();
            builder.ServiceCollection.AddTransient<IPresenterViewProvider, PresenterViewProviderFromResources>(x =>
                new PresenterViewProviderFromResources("UI")
            );
            builder.ServiceCollection.AddTransient<ScreenUI>();
            builder.ServiceCollection.AddTransient<PushNavigatorAnimationStrategy>();
            builder.ServiceCollection.AddTransient<PopNavigatorAnimationStrategy>();
            builder.ServiceCollection.AddTransient<RemoveNavigatorAnimationStrategy>();
            builder.ServiceCollection.AddTransient<InsertNavigatorAnimationStrategy>();
            builder.ServiceCollection.AddTransient<PushNavigation>();
            builder.ServiceCollection.AddTransient<PopNavigation>();
            builder.ServiceCollection.AddTransient<RemoveNavigation>();
            builder.ServiceCollection.AddTransient<InsertNavigation>();
            builder.ServiceCollection.AddTransient<BackToNavigation>();

            return builder;
        }

        public static IServiceProvider BuildMeekMvp(this IContainerBuilder builder)
        {
            var appService = builder.Build();
            var options = appService.GetService<MvpNavigatorOptions>();
            if (options.DebugOption.UseDebug)
            {
                appService.GetService<ServiceRegistrationHandler>();
            }

            return appService;
        }

        public static async Task<IServiceProvider> RunMeekMvpAsync<TScreen>(this IServiceProvider services)
            where TScreen : class, IScreen
        {
            await services.GetService<PushNavigation>().PushAsync<TScreen>();

            return services;
        }

        public static Task<IServiceProvider> BuildAndRunMeekMvpAsync<TScreen>(this IContainerBuilder builder)
            where TScreen : class, IScreen
        {
            var appService = builder.BuildMeekMvp();

            return appService.RunMeekMvpAsync<TScreen>();
        }
    }
}