using System;
using System.Collections;

namespace Meek.NavigationStack
{
    public interface IViewHandler : IDisposable, IAsyncDisposable
    {
        void Setup(StackNavigationContext context);

        void SetInteractable(bool interactable);

        void SetVisibility(bool visibility);

        void EvaluateNavigateAnimation(StackNavigationContext context, NavigatorAnimationType navigatorAnimationType, float t);

        IEnumerator PlayNavigateAnimationRoutine(StackNavigationContext context, NavigatorAnimationType navigatorAnimationType);
    }
}