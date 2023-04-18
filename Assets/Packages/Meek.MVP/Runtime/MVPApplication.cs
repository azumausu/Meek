using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Meek.NavigationStack;

namespace Meek.MVP
{
    public class MVPApplication : IDisposable
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IList<IDisposable> _disposables;

        public IServiceProvider AppServices => _serviceProvider;
        
        private MVPApplication(IServiceProvider serviceProvider, IList<IDisposable> disposables)
        {
            _serviceProvider = serviceProvider;
            _disposables = disposables;
        }
        
        public Task RunAsync<TBootScreen>() where TBootScreen : IScreen
        {
            return _serviceProvider.GetService<PushNavigation>().PushAsync<TBootScreen>();
        }

        public void Dispose()
        {
            foreach (var disposable in _disposables) disposable.Dispose();
            _disposables.Clear();
        }
        
        public static MVPApplication CreateRootApp(MVPRootApplicationOption option, Action<IServiceCollection> configure) 
        {
            if (option == null) throw new ArgumentNullException(nameof(option));
            if (option.ContainerBuilderFactory == null) throw new ArgumentNullException(nameof(option.ContainerBuilderFactory));
            if (option.InputLocker == null) throw new ArgumentNullException(nameof(option.InputLocker));
            if (option.PrefabViewManager == null) throw new ArgumentNullException(nameof(option.PrefabViewManager));
            
            // StackNavigator
            var stackNavigator = StackNavigator.CreateAsMVP(stackNavigatorOption =>
            {
                stackNavigatorOption.ContainerBuilder = option.ContainerBuilderFactory(null);
                stackNavigatorOption.InputLocker = option.InputLocker;
                stackNavigatorOption.PrefabViewManager = option.PrefabViewManager;
                stackNavigatorOption.PresenterLoaderFactoryType = option.PresenterLoaderFactoryType;
            });
            
            // App Service
            var appBuilder = option.ContainerBuilderFactory(stackNavigator.ServiceProvider);
            appBuilder.ServiceCollection.AddSingleton<INavigator>(stackNavigator);
            appBuilder.ServiceCollection.AddNavigationService();
            
            configure(appBuilder.ServiceCollection);
            var appServices = appBuilder.Build();

            var disposables = new List<IDisposable>();
            disposables.Add(stackNavigator);
            if (appServices is IDisposable disposable) disposables.Add(disposable);
            
            return new MVPApplication(appServices, disposables);
        }

        public static MVPApplication CreateChildAppAsync(MVPChildAppliactionOption option, Action<IServiceCollection> configure)
        {
            if (option == null) throw new ArgumentNullException(nameof(option));
            if (option.ContainerBuilderFactory == null) throw new ArgumentNullException(nameof(option.ContainerBuilderFactory));
            if (option.InputLocker == null) throw new ArgumentNullException(nameof(option.InputLocker));
            if (option.PrefabViewManager == null) throw new ArgumentNullException(nameof(option.PrefabViewManager));
            if (option.Parent == null) throw new ArgumentNullException(nameof(option.Parent));
            
            var stackNavigator = StackNavigator.CreateAsMVP(stackNavigatorOption =>
            {
                stackNavigatorOption.ContainerBuilder = option.ContainerBuilderFactory(option.Parent);
                stackNavigatorOption.InputLocker = option.InputLocker;
                stackNavigatorOption.PrefabViewManager = option.PrefabViewManager;
                stackNavigatorOption.PresenterLoaderFactoryType = option.PresenterLoaderFactoryType;
            });
            
            // App Service
            var appBuilder = option.ContainerBuilderFactory(stackNavigator.ServiceProvider);
            appBuilder.ServiceCollection.AddSingleton<INavigator>(stackNavigator);
            appBuilder.ServiceCollection.AddNavigationService();
            
            configure(appBuilder.ServiceCollection);
            var appServices = appBuilder.Build();
            
            var disposables = new List<IDisposable>();
            disposables.Add(stackNavigator);
            if (appServices is IDisposable disposable) disposables.Add(disposable); 
            
            return new MVPApplication(appServices, disposables); 
        }
    }
}