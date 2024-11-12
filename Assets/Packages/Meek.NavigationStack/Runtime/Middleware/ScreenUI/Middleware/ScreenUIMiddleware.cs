using System;
using System.Threading.Tasks;

namespace Meek.NavigationStack
{
    public class ScreenUIMiddleware : IMiddleware
    {
        private readonly IScreenContainer _screenContainer;

        public async ValueTask InvokeAsync(NavigationContext context, NavigationDelegate next)
        {
            var stackContext = context.ToStackNavigationContext();
            if (stackContext.NavigatingSourceType == StackNavigationSourceType.Push)
            {
                stackContext.ToScreen.Initialize(context);
            }

            if (stackContext.NavigatingSourceType == StackNavigationSourceType.Insert)
            {
                var insertionScreen = stackContext.GetFeatureValue<IScreen>(StackNavigationContextFeatureDefine.InsertionScreen);
                insertionScreen.Initialize(context);
            }


            await next(context);

            // 破棄処理をする
            if (stackContext.NavigatingSourceType == StackNavigationSourceType.Pop)
            {
                if (context.FromScreen is IAsyncDisposable asyncDisposable)
                {
                    await asyncDisposable.DisposeAsync();
                }

                if (context.FromScreen is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }

            if (stackContext.NavigatingSourceType == StackNavigationSourceType.Remove)
            {
                var removeScreen = context.GetFeatureValue<IScreen>(StackNavigationContextFeatureDefine.RemoveScreen);

                if (removeScreen is IAsyncDisposable asyncDisposable)
                {
                    await asyncDisposable.DisposeAsync();
                }

                if (removeScreen is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }
        }
    }
}