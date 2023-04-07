using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine.Pool;

namespace Meek.NavigationStack
{
    public class StackScreenContainer : IScreenContainer
    {
        private readonly Stack<IScreen> _screenStack = new Stack<IScreen>(32);
        private readonly Stack<IScreen> _insertOrRemoveCacheStack = new(16);

        public IReadOnlyCollection<IScreen> Screens => _screenStack;

        public ValueTask NavigateAsync(NavigationContext context)
        {
            var stackContext = context.ToStackNavigationContext();
            var screenList = ListPool<IScreen>.Get();
            screenList.AddRange(_screenStack);

            switch (stackContext.NavigatingSourceType)
            {
                case StackNavigationSourceType.Push:
                    _screenStack.Push(context.ToScreen);
                    break;
                case StackNavigationSourceType.Pop:
                    _screenStack.Pop();
                    break;
                case StackNavigationSourceType.Insert:
                    var insertionBeforeScreenType = context.GetFeatureValue<Type>(StackNavigationContextFeatureDefine.InsertionBeforeScreenType); 
                    while (_screenStack.Peek().GetType() != insertionBeforeScreenType)
                        _insertOrRemoveCacheStack.Push(_screenStack.Pop());
                    
                    // Insert
                    var insertionScreen = context.GetFeatureValue<IScreen>(StackNavigationContextFeatureDefine.InsertionScreen);
                    _screenStack.Push(insertionScreen);
                    
                    foreach (var screen in _insertOrRemoveCacheStack) _screenStack.Push(screen);
                    _insertOrRemoveCacheStack.Clear();
                    
                    break;
                case StackNavigationSourceType.Remove:
                    var removeScreenType = context.GetFeatureValue<Type>(StackNavigationContextFeatureDefine.RemoveScreenType);
                    while (_screenStack.Peek().GetType() != removeScreenType)
                        _insertOrRemoveCacheStack.Push(_screenStack.Pop());

                    // Remove
                    _screenStack.Pop();
                    
                    foreach (var screen in _insertOrRemoveCacheStack) _screenStack.Push(screen);
                    _insertOrRemoveCacheStack.Clear();
                    
                    break;
            }

            return default;
        }
    }
}