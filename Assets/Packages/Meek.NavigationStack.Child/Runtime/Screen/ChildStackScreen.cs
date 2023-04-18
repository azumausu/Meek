using System.Threading.Tasks;
using System;

namespace Meek.NavigationStack.Child
{
    public abstract class ChildStackScreen<TModel> : IChildScreen
    {
        protected StackScreen ParentScreen;
        protected TModel Model;
        private ICoroutineRunner _coroutineRunner;

        private IViewHandler _viewHandler;

        protected abstract TModel CreateModel(ChildStackNavigationContext context);
        protected abstract IViewHandlerLoader CreateLoader();
        protected abstract void Bind(IViewHandler viewHandler);

        void IScreen.Initialize(NavigationContext context)
        {
            var childStackContext = context.ToChildStackNavigationContext();

            Model = CreateModel(childStackContext);
            ParentScreen = childStackContext.ParentScreen;
            _coroutineRunner = childStackContext.AppServices.GetService<ICoroutineRunner>();
        }

        IScreen IChildScreen.ParentScreen => ParentScreen;

        public async ValueTask OpenChildScreenAsync(ChildStackNavigationContext context)
        {
            ParentScreen.ScreenEventInvoker.Invoke(ChildStackNavigatorScreenEvent.ChildScreenWillOpen);
            await ParentScreen.ScreenEventInvoker.InvokeAsync(ChildStackNavigatorScreenEvent.ChildScreenWillOpen);

            var loader = CreateLoader();
            var viewHandler = await ParentScreen.UI.LoadViewHandlerAsync(loader);

            Bind(viewHandler);
            viewHandler.SetLayer();
            viewHandler.Setup();
            viewHandler.SetVisibility(true);

            var openAnimationRoutine = viewHandler.PlayNavigateAnimationRoutine(
                NavigatorAnimationType.Open,
                ParentScreen.GetType(),
                GetType()
            );
            await _coroutineRunner.StartCoroutineAsTask(openAnimationRoutine);

            viewHandler.SetInteractable(true);

            _viewHandler = viewHandler;

            ParentScreen.ScreenEventInvoker.Invoke(ChildStackNavigatorScreenEvent.ChildScreenDidOpen);
            await ParentScreen.ScreenEventInvoker.InvokeAsync(ChildStackNavigatorScreenEvent.ChildScreenDidOpen);
        }

        public async ValueTask CloseChildScreenAsync(ChildStackNavigationContext context)
        {
            _viewHandler.SetInteractable(false);

            ParentScreen.ScreenEventInvoker.Invoke(ChildStackNavigatorScreenEvent.ChildScreenWillClose);
            await ParentScreen.ScreenEventInvoker.InvokeAsync(ChildStackNavigatorScreenEvent.ChildScreenWillClose);

            var closeAnimationRoutine = _viewHandler.PlayNavigateAnimationRoutine(
                NavigatorAnimationType.Close,
                GetType(),
                ParentScreen.GetType()
            );
            await _coroutineRunner.StartCoroutineAsTask(closeAnimationRoutine);

            _viewHandler.SetVisibility(false);
            ParentScreen.UI.DisposeViewHandler(_viewHandler);

            ParentScreen.ScreenEventInvoker.Invoke(ChildStackNavigatorScreenEvent.ChildScreenDidClose);
            await ParentScreen.ScreenEventInvoker.InvokeAsync(ChildStackNavigatorScreenEvent.ChildScreenDidClose);
        }
    }
}