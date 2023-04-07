using System.Collections.Generic;

namespace Meek
{
    public class NavigationContext
    {
        public IDictionary<string, object> Features;

        public IScreen FromScreen;

        public IScreen ToScreen;

        public IServiceProvider AppServices;
    }
}