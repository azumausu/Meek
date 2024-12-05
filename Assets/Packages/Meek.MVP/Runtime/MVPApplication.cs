using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Meek.NavigationStack;
using Meek.UGUI;

namespace Meek.MVP
{
    public class MVPApplication : IDisposable
    {
        private readonly IServiceProvider _appServices;
        public IServiceProvider AppServices => _appServices;

        private MVPApplication(IServiceProvider appServices)
        {
            _appServices = appServices;
        }

        public Task RunAsync<TBootScreen>() where TBootScreen : IScreen
        {
            return _appServices.GetService<PushNavigation>().PushAsync<TBootScreen>();
        }

        public void Dispose()
        {
            if (_appServices is IDisposable disposable) disposable.Dispose();
        }

        public static MVPApplication CreateApp(MVPApplicationOption option, Action<IServiceCollection> configure)
        {
            if (option == null) throw new ArgumentNullException(nameof(option));
            if (option.ContainerBuilderFactory == null) throw new ArgumentNullException(nameof(option.ContainerBuilderFactory));
            if (option.InputLocker == null) throw new ArgumentNullException(nameof(option.InputLocker));
            if (option.PrefabViewManager == null) throw new ArgumentNullException(nameof(option.PrefabViewManager));

            // Create Navigator
            var navigator = new NavigatorBuilder(option.ContainerBuilderFactory(option.Parent))
                .ConfigureServices(serviceCollection =>
                {
                    serviceCollection.AddSingleton<ICoroutineRunner, CoroutineRunner>();
                    serviceCollection.AddSingleton<IScreenContainer, StackScreenContainer>();
                    serviceCollection.AddSingleton<IPresenterLoaderFactory, PresenterLoaderFactoryFromResources>();

                    serviceCollection.AddScreenNavigatorEvent();
                    serviceCollection.AddInputLocker(option.InputLocker);
                    serviceCollection.AddScreenUI();
                    serviceCollection.AddNavigatorAnimation();
                    serviceCollection.AddUGUI(option.PrefabViewManager);
                    serviceCollection.AddScreenLifecycleEvent();
                }).Configure(app =>
                {
                    app.UseScreenNavigatorEvent();
                    app.UseInputLocker();
                    app.UseScreenUI();
                    app.UseNavigatorAnimation();
                    app.UseUGUI();
                    app.UseScreenLifecycleEvent();
                }).Build();

            // Create App
            var appBuilder = option.ContainerBuilderFactory(navigator.ServiceProvider);
            appBuilder.ServiceCollection.AddSingleton(navigator);
            appBuilder.ServiceCollection.AddNavigationService();
            configure(appBuilder.ServiceCollection);

            return new MVPApplication(appBuilder.Build());
        }
    }
}