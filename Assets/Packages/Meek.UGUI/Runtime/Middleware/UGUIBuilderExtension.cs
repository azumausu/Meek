using System;

namespace Meek.UGUI
{
    public static class UGUIBuilderExtension
    {
        public static IServiceCollection AddUGUI(this IServiceCollection self, Action<UGUIOption> configure)
        {
            var option = new UGUIOption();
            configure(option);

            self.AddSingleton(option);
            self.AddSingleton<IPrefabViewManager>(option.PrefabViewManager);
            self.AddSingleton<UGUIMiddleware>();

            return self;
        }
        
        public static INavigatorBuilder UseUGUI(this INavigatorBuilder navigatorBuilder)
        {
            navigatorBuilder.UseMiddleware<UGUIMiddleware>();
            return navigatorBuilder;
        }
    }
}