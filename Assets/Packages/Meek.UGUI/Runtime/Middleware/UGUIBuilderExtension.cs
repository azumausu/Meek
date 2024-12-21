#if MEEK_ENABLE_UGUI
using System;

namespace Meek.UGUI
{
    public static class UGUIBuilderExtension
    {
        public static IServiceCollection AddUGUI(this IServiceCollection self, IPrefabViewManager prefabViewManager)
        {
            self.AddSingleton(prefabViewManager);
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
#endif