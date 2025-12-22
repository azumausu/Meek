using Meek.NavigationStack.Debugs;

namespace Meek.NavigationStack
{
    public static class IServiceCollectionExntensions
    {
        public static IServiceCollection AddStackNavigationService(this IServiceCollection self, IInputLocker inputLocker)
        {
            self.AddSingleton(inputLocker);
            self.AddSingleton<IScreenContainer, StackScreenContainer>();
            self.AddSingleton<NavigationSharedSemaphore>();
            self.AddSingleton(x => new StackNavigationService(x));
            self.TryAddSingleton<ICoroutineRunner, CoroutineRunner>();

            self.AddTransient<ScreenUI>();
            self.AddTransient<PushNavigatorAnimationStrategy>();
            self.AddTransient<PopNavigatorAnimationStrategy>();
            self.AddTransient<RemoveNavigatorAnimationStrategy>();
            self.AddTransient<InsertNavigatorAnimationStrategy>();
            self.AddTransient<PushNavigation>();
            self.AddTransient<PopNavigation>();
            self.AddTransient<RemoveNavigation>();
            self.AddTransient<InsertNavigation>();
            self.AddTransient<BackToNavigation>();

            return self;
        }

        public static IServiceCollection AddDebug(this IServiceCollection self)
        {
            var debugOptions = new NavigationStackDebugOption();
            self.AddSingleton(debugOptions);
            self.AddSingleton(x => new ServiceRegistrationHandler(x));

            return self;
        }
    }
}