using System.Threading.Tasks;
using Meek.MVP;
using Meek.UGUI;
using UnityEngine;

namespace Meek.NavigationStack.MVP
{
    public class StackNavigator : INavigator
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

        public static StackNavigator CreateAsMVP(IContainerBuilder containerBuilder, IInputLocker inputLocker, IPrefabViewManager prefabViewManager)
        {
            var gameObject = new GameObject("CoroutineRunner");
            var coroutineRunner = gameObject.AddComponent<CoroutineRunner>();
            Object.DontDestroyOnLoad(coroutineRunner);
            containerBuilder.ServiceCollection.AddSingleton(coroutineRunner);
            
            // StackNavigator Service
            var stackNavigator = new NavigatorBuilder(option =>
            {
                option.ContainerBuilder = containerBuilder;
                option.ScreenNavigator.Set<StackScreenContainer>();
            }).ConfigureServices(serviceCollection =>
            {
                serviceCollection.AddScreenNavigatorEvent();
                serviceCollection.AddInputLocker(x => { x.InputLocker = inputLocker; });
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
    }
}