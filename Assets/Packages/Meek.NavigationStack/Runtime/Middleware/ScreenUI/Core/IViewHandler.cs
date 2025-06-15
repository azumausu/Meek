using System;
using System.Collections;

namespace Meek.NavigationStack
{
    public interface IViewHandler : IDisposable, IAsyncDisposable
    {
        void Setup();

        void SetInteractable(bool interactable);

        void SetVisibility(bool visibility);

        void EvaluateNavigateAnimation(
            NavigatorAnimationType navigatorAnimationType,
            Type fromScreenClassType,
            Type toScreenClassType,
            float t
        );

        IEnumerator PlayNavigateAnimationRoutine(
            NavigatorAnimationType navigatorAnimationType,
            Type fromScreenClassType,
            Type toScreenClassType
        );
    }
}