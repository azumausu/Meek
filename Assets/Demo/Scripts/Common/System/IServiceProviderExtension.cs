using System;
using UniRx;
using UnityEngine;

namespace Demo
{
    public static class IServiceProviderExtension
    {
        public static void AddTo(this IServiceProvider self, MonoBehaviour component)
        {
            if (self is IDisposable profileDisposable) profileDisposable.AddTo(component);
        }
    }
}