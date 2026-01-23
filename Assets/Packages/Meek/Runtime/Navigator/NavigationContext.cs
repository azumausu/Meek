using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace Meek
{
    public class NavigationContext
    {
        public IDictionary<string, object> Features;

        /// <summary>
        /// The screen that was active before navigation.
        /// Note: For Remove or Insert operations, this will be the screen at the top (Peek) of the stack.
        /// Note: Will be null if the stack is empty before Push.
        /// </summary>
        [CanBeNull] public IScreen FromScreen;

        /// <summary>
        /// The screen that will be active after navigation.
        /// Note: For Remove or Insert operations, this will be the screen at the top (Peek) of the stack.
        /// Note: Will be null if the stack becomes empty after Pop.
        /// </summary>
        [CanBeNull] public IScreen ToScreen;

        public IServiceProvider AppServices;
    }
}