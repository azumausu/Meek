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
                configure(mvpOption);
                mvpOption.PresenterLoaderFactoryType ??= typeof(PresenterLoaderFactoryFromResources);
                self.AddSingleton(typeof(IPresenterLoaderFactory), mvpOption.PresenterLoaderFactoryType);
            });
            
            return self;
        }
    }
}