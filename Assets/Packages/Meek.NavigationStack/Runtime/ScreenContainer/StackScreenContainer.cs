using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Meek.NavigationStack
{
    public class StackScreenContainer : IScreenContainer, IDisposable, IAsyncDisposable
    {
        private readonly Stack<IScreen> _screenStack = new Stack<IScreen>(32);
        private readonly Stack<IScreen> _insertOrRemoveCacheStack = new(16);

        private bool _disposed;

        public IReadOnlyCollection<IScreen> Screens => _screenStack;

        public ValueTask NavigateAsync(NavigationContext context)
        {
            var stackContext = context.ToStackNavigationContext();

            switch (stackContext.NavigatingSourceType)
            {
                case StackNavigationSourceType.Push:
                    _screenStack.Push(context.ToScreen);
                    break;
                case StackNavigationSourceType.Pop:
                    _screenStack.Pop();
                    break;
                case StackNavigationSourceType.Insert:
                    var insertionBeforeScreen = stackContext.GetInsertionBeforeScreen();
                    while (_screenStack.Peek() != insertionBeforeScreen)
                    {
                        _insertOrRemoveCacheStack.Push(_screenStack.Pop());
                    }

                    // Insert
                    var insertionScreen = stackContext.GetInsertionScreen();
                    _screenStack.Push(insertionScreen);

                    foreach (var screen in _insertOrRemoveCacheStack) _screenStack.Push(screen);
                    _insertOrRemoveCacheStack.Clear();

                    break;
                case StackNavigationSourceType.Remove:
                    var removeScreen = stackContext.GetRemoveScreen();
                    while (_screenStack.Peek() != removeScreen)
                    {
                        _insertOrRemoveCacheStack.Push(_screenStack.Pop());
                    }

                    // Remove
                    _screenStack.Pop();

                    foreach (var screen in _insertOrRemoveCacheStack) _screenStack.Push(screen);
                    _insertOrRemoveCacheStack.Clear();

                    break;
            }

            return default;
        }

        public void Dispose()
        {
            if (_disposed) return;

            try
            {
                foreach (var screen in _screenStack)
                {
                    if (screen is IAsyncDisposable asyncDisposable)
                    {
                        _ = asyncDisposable.SafeDisposeAsync();
                    }
                    else if (screen is IDisposable disposable)
                    {
                        disposable.SafeDispose();
                    }
                }

                _screenStack.Clear();
            }
            finally
            {
                _disposed = true;
            }
        }

        public async ValueTask DisposeAsync()
        {
            if (_disposed) return;
            try
            {
                foreach (var screen in _screenStack)
                {
                    if (screen is IAsyncDisposable asyncDisposable)
                    {
                        await asyncDisposable.SafeDisposeAsync();
                    }
                    else if (screen is IDisposable disposable)
                    {
                        disposable.SafeDispose();
                    }
                }

                _screenStack.Clear();
            }
            finally
            {
                _disposed = true;
            }
        }
    }
}