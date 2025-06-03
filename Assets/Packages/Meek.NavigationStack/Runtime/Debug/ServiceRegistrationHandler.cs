using System;

namespace Meek.NavigationStack.Debugs
{
    public class ServiceRegistrationHandler : IDisposable
    {
        private readonly ServiceEntry _serviceEntry;

        public ServiceRegistrationHandler(IServiceProvider serviceProvider)
        {
            var option = serviceProvider.GetService<NavigationStackDebugOption>();
            _serviceEntry = new ServiceEntry()
            {
                DisplayName = option.DisplayName,
                ServiceProvider = serviceProvider,
            };

            RuntimeNavigationStackManager.Instance.RegisterServices(_serviceEntry);
        }

        public void Dispose()
        {
            RuntimeNavigationStackManager.Instance.UnregisterServices(_serviceEntry);
        }
    }
}