#if UNITY_EDITOR
using System;
using System.Collections.Generic;

namespace Meek.NavigationStack.Debugs
{
    public class RuntimeNavigationStackManager
    {
        public static RuntimeNavigationStackManager Instance =>
            _instance ??= new RuntimeNavigationStackManager();

        private static RuntimeNavigationStackManager _instance;


        private readonly List<ServiceEntry> _serviceEntries = new();

        public List<ServiceEntry> ServiceEntries => _serviceEntries;

        public event Action<ServiceEntry> OnRegisterServices;
        public event Action<ServiceEntry> OnUnregisterServices;
        public event Action<StackNavigationContext> ScreenWillNavigate;
        public event Action<StackNavigationContext> ScreenDidNavigate;


        public void RegisterServices(ServiceEntry serviceEntry)
        {
            _serviceEntries.Add(serviceEntry);
            OnRegisterServices?.Invoke(serviceEntry);
        }

        public void UnregisterServices(ServiceEntry serviceEntry)
        {
            _serviceEntries.Remove(serviceEntry);
            OnUnregisterServices?.Invoke(serviceEntry);
        }

        public void FireScreenWillNavigate(StackNavigationContext context)
        {
            ScreenWillNavigate?.Invoke(context);
        }

        public void FireScreenDidNavigate(StackNavigationContext context)
        {
            ScreenDidNavigate?.Invoke(context);
        }
    }
}
#endif