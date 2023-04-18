using System;
using System.Threading.Tasks;
using Meek.NavigationStack;
using Meek.UGUI;

namespace Meek.MVP
{
    public class StackNavigator : INavigator, IDisposable
    {
        private readonly INavigator _internalNavigator;
       
        public IScreenContainer ScreenContainer => _internalNavigator.ScreenContainer;
        public IServiceProvider ServiceProvider => _internalNavigator.ServiceProvider;
        
        private StackNavigator(INavigator internalNavigator)
        {
            _internalNavigator = internalNavigator;
        }

        public ValueTask NavigateAsync(NavigationContext context)
        {
            return _internalNavigator.NavigateAsync(context);
        }

        public static StackNavigator CreateAsMVP(Action<MVPStackNavigatorOption> configure)
        {
            var option = new MVPStackNavigatorOption();
            configure(option);
            
            option.ContainerBuilder.ServiceCollection.AddSingleton<ICoroutineRunner, CoroutineRunner>();
            
            // StackNavigator Service
            var stackNavigator = new NavigatorBuilder(navigatorBuilderOption =>
            {
                navigatorBuilderOption.ContainerBuilder = option.ContainerBuilder;
                navigatorBuilderOption.ScreenNavigator = typeof(StackScreenContainer);
            }).ConfigureServices(serviceCollection =>
            {
                serviceCollection.AddScreenNavigatorEvent();
                serviceCollection.AddInputLocker(x => { x.InputLocker = option.InputLocker; });
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
                    x.UGUIOption.PrefabViewManager = option.PrefabViewManager;
                    x.PresenterLoaderFactoryType = option.PresenterLoaderFactoryType;
                });
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

            return new StackNavigator(stackNavigator);
        }
        
        public void Dispose()
        {
            if (_internalNavigator.ServiceProvider is IDisposable disposable) disposable.Dispose();
        }
    }
}