using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Meek.NavigationStack
{
    public class EventHolder : IScreenEventInvoker
    {
        #region Fields

        private readonly List<ScreenActionEvent> _screenActionEvents = new();
        private readonly List<ScreenTaskEvent> _screenTaskEvents = new();
        private readonly List<ScreenDispatchEvent> _screenDispatchEvents = new();

        #endregion

        #region Methods

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RegisterActionEvent(string eventName, Action action)
        {
            _screenActionEvents.Add(new ScreenActionEvent { EventName = eventName, Action = action, }); 
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RegisterTaskEvent(string eventName, Func<global::System.Threading.Tasks.Task> function)
        {
            _screenTaskEvents.Add(new ScreenTaskEvent { EventName = eventName, Function = function, }); 
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SubscribeDispatchEvent(string eventName, Func<bool> function) => _screenDispatchEvents.Add(
            new ScreenDispatchEvent
            {
                EventName = ToDispatchEventName(eventName), Function = _ => function.Invoke(),
            }
        );

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SubscribeDispatchEvent<TParam>(string eventName, Func<TParam, bool> function) => _screenDispatchEvents.Add(
            new ScreenDispatchEvent
            {
                EventName = ToDispatchEventName(eventName), Function = value => function.Invoke((TParam)value),
            }
        );

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private string ToDispatchEventName(string eventName) => $"__{eventName}__";

        #endregion

        #region Interface Implementations

        void IScreenEventInvoker.Invoke(string eventName) => _screenActionEvents.Invoke(eventName);
        
        void IScreenEventInvoker.Invoke<TEnum>(TEnum eventName) => _screenActionEvents.Invoke(eventName.ToString());

        global::System.Threading.Tasks.Task IScreenEventInvoker.InvokeAsync(string eventName) => _screenTaskEvents.InvokeAsync(eventName);
        
        global::System.Threading.Tasks.Task IScreenEventInvoker.InvokeAsync<TEnum>(TEnum eventName) => _screenTaskEvents.InvokeAsync(eventName.ToString());

        bool IScreenEventInvoker.Dispatch(string eventName, object param)
        {
            var key = ToDispatchEventName(eventName);
            var dispatchEvent = _screenDispatchEvents.FirstOrDefault(x => x.EventName == key);
            if (dispatchEvent == null) return false;

            return dispatchEvent.Function(param);
        }

        #endregion
    }
}