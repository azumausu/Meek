using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Meek.NavigationStack.Child
{
    public class ChildStackScreenContainer : IScreenContainer
    {
        private readonly List<Entry> _entries = new();

        public IReadOnlyCollection<IScreen> Screens => _entries
            .SelectMany(x => x.ChildScreens)
            .Cast<IScreen>()
            .ToList();
            
        async ValueTask IScreenContainer.NavigateAsync(NavigationContext context)
        {
            var childStackContext = context.ToChildStackNavigationContext();
            var entry = GetEntry(childStackContext.ParentScreen);
            
            if (childStackContext.NavigationSourceType == ChildStackNavigationSourceType.Push)
            {
                childStackContext.ToScreen.Initialize(context);
                var childScreen = childStackContext.ToScreen as IChildScreen;
                entry.ChildScreens.Push(childScreen);
                await childScreen.OpenChildScreenAsync(childStackContext);
            }

            if (childStackContext.NavigationSourceType == ChildStackNavigationSourceType.Pop)
            {
                var removeScreen = entry.ChildScreens.Pop();
                if (entry.ChildScreens.Count == 0) _entries.Remove(entry);
                await removeScreen.CloseChildScreenAsync(childStackContext);
                if (removeScreen is IDisposable disposable) disposable.Dispose();
            }
        }

        internal ValueTask RemoveChildScreenByParent(StackScreen parentScreen)
        {
            var entry = GetEntry(parentScreen);
            if (entry != null) foreach (var disposable in entry.ChildScreens.OfType<IDisposable>()) disposable.Dispose();
            _entries.Remove(entry);

            return default;
        }

        private Entry GetEntry(StackScreen screen)
        {
            var entry = _entries.FirstOrDefault(x => x.StackScreen == screen);
            if (entry != null) return entry;

            var newEntry = new Entry()
            {
                StackScreen = screen,
                ChildScreens = new Stack<IChildScreen>(),
            };
            _entries.Add(newEntry);
            return newEntry;
        }

        class Entry
        {
            public StackScreen StackScreen;

            public Stack<IChildScreen> ChildScreens;
        }
    }
}