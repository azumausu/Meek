using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine.Pool;

namespace Meek.NavigationStack
{
    public class StackScreenContainer : IScreenContainer, IDisposable
    {
        private readonly Stack<IScreen> _screenStack = new Stack<IScreen>(32);
        private readonly Stack<IScreen> _insertOrRemoveCacheStack = new(16);

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
            foreach (var screen in _screenStack.OfType<IDisposable>()) screen.Dispose();
            _screenStack.Clear();
        }
    }
}