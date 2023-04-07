using System;
using System.Collections.Generic;

namespace Meek
{
    public class NavigatorBuilder : INavigatorBuilder
    {
        private readonly List<Func<NavigationDelegate, NavigationDelegate>> _components = new();
        private readonly IContainerBuilder _containerBuilder;

        private Action<IServiceCollection> _configureServices;
        private Action<INavigatorBuilder> _configure;
        
        public IServiceProvider ServiceProvider { get; private set; }

        public NavigatorBuilder(Action<NavigatorBuilderOption> configure)
        {
            var option = new NavigatorBuilderOption() { };
            configure(option);
            
            _containerBuilder = option.ContainerBuilder;
            _containerBuilder.ServiceCollection.AddSingleton(typeof(IScreenContainer), option.ScreenNavigator.Get());
        }

        public NavigatorBuilder ConfigureServices(Action<IServiceCollection> configureServices)
        {
            _configureServices = configureServices;
            return this;
        }

        public NavigatorBuilder Configure(Action<INavigatorBuilder> configure)
        {
            _configure = configure;
            return this;
        }

        public INavigator Build()
        {
            // MiddlewareやDIに必要なものを登録
            _configureServices.Invoke(_containerBuilder.ServiceCollection);
            
            // Containerの作成
            ServiceProvider = _containerBuilder.Build();

            // Middlewareの実行手順の設定
            _configure.Invoke(this);
            
            // 基盤の一番最初に呼び出すMiddleware
            var stackScreenManager = ServiceProvider.GetService<IScreenContainer>();
            NavigationDelegate app = stackScreenManager.NavigateAsync;
            for (int i = _components.Count - 1; i >= 0; i--)
            {
                app = _components[i].Invoke(app);
            }

            return new Navigator(ServiceProvider, app);
        }

        public INavigatorBuilder Use(Func<NavigationDelegate, NavigationDelegate> component)
        {
            _components.Add(component);
            return this;
        } 
    }
}