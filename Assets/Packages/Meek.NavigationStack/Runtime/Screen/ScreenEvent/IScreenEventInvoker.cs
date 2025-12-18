using System;
using System.Threading.Tasks;

namespace Meek.NavigationStack
{
    public interface IScreenEventInvoker
    {
        void Invoke(string eventName, StackNavigationContext context, bool suppressException = false);

        void Invoke<TEnum>(TEnum eventName, StackNavigationContext context, bool suppressException = false) where TEnum : Enum;

        global::System.Threading.Tasks.Task InvokeAsync(string eventName, StackNavigationContext context);

        global::System.Threading.Tasks.Task InvokeAsync<TEnum>(TEnum eventName, StackNavigationContext context) where TEnum : Enum;

        bool Dispatch(string eventName, object param = null);

        Task<bool> DispatchAsync(string eventName, object param = null);
    }
}