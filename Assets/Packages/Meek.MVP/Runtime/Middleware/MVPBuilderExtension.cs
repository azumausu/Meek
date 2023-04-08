using System;
using Meek.UGUI;

namespace Meek.MVP
{
    public static class MVPBuilderExtension
    {
        public static IServiceCollection AddUGUIAsMVP(this IServiceCollection self, Action<MVPOption> configure)
        {
            self.AddUGUI(uguiOption =>
            {
                var mvpOption = new MVPOption() { UGUIOption = uguiOption, };
                mvpOption.PresenterLoaderFactory.Set<PresenterLoaderFactoryFromResources>();
                configure(mvpOption);
                self.AddSingleton(typeof(IPresenterLoaderFactory), mvpOption.PresenterLoaderFactory.Get());
            });
            
            return self;
        }
    }
}