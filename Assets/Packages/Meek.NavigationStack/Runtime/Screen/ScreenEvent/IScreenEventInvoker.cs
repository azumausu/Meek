using System;
using System.Threading.Tasks;

namespace Meek.NavigationStack
{
    public interface IScreenEventInvoker
    {
        void Invoke(string eventName);

        void Invoke<TEnum>(TEnum eventName) where TEnum : Enum;

        global::System.Threading.Tasks.Task InvokeAsync(string eventName);

        global::System.Threading.Tasks.Task InvokeAsync<TEnum>(TEnum eventName) where TEnum : Enum;

        bool Dispatch(string eventName, object param = null);

        Task<bool> DispatchAsync(string eventName, object param = null);
    }
}