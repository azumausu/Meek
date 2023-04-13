namespace Meek.NavigationStack
{
    public static class IServiceCollectionExtensionForStackNavigation
    {
        public static IServiceCollection AddNavigationService(this IServiceCollection self)
        {
            self.AddTransient<PushNavigation>();
            self.AddTransient<PopNavigation>();
            self.AddTransient<RemoveNavigation>();
            self.AddTransient<InsertNavigation>();
            self.AddTransient<BackToNavigation>();
            self.AddTransient<ChangeWithPopNavigation>();
            self.AddTransient<ChangeWithPushNavigation>();
            self.AddSingleton(x =>
            {
                var stackNavigatorService = x.GetService<INavigator>();
                return new StackNavigationService(stackNavigatorService, x);
            });

            return self;
        }
    }
}