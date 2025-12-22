using System;
using System.Threading.Tasks;
using Meek.NavigationStack;
using Meek.NavigationStack.Debugs;

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
                throw new ArgumentNullException(
                    nameof(options.PrefabViewManager),
                    "PrefabViewManager must be provided in MvpNavigatorOptions."
                );
            }

            // Stack Screen Navigator Logic
            builder.ServiceCollection.AddStackNavigationService(options.InputLocker);
            builder.ServiceCollection.AddSingleton<INavigator, MvpNavigator>();

            // uGUI Logic
            builder.ServiceCollection.AddSingleton(options.PrefabViewManager);

            // Model View Presenter Logic
            builder.ServiceCollection.AddSingleton<MvpNavigatorOptions>();
            builder.ServiceCollection.AddTransient<IPresenterViewHandler, DynamicPresenterViewHandler>();
            builder.ServiceCollection.AddTransient<IPresenterViewProvider, PresenterViewProviderFromResources>(x =>
                new PresenterViewProviderFromResources("UI")
            );

            // Debug Logic
            builder.ServiceCollection.AddDebug();

            return builder;
        }

        public static IServiceProvider BuildMeekMvp(this IContainerBuilder builder)
        {
            var appService = builder.Build();
            var debugOption = appService.GetService<NavigationStackDebugOption>();
            if (debugOption.UseDebug)
            {
                appService.GetService<ServiceRegistrationHandler>();
            }

            return appService;
        }

        public static async Task<IServiceProvider> RunMeekMvpAsync<TScreen>(this IServiceProvider services) where TScreen : IScreen
        {
            await services.GetService<PushNavigation>().PushAsync<TScreen>();

            return services;
        }

        public static Task<IServiceProvider> BuildAndRunMeekMvpAsync<TScreen>(this IContainerBuilder builder) where TScreen : IScreen
        {
            var appService = builder.BuildMeekMvp();

            return appService.RunMeekMvpAsync<TScreen>();
        }
    }
}