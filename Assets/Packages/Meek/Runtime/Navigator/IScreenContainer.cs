using System.Collections.Generic;
using System.Threading.Tasks;

namespace Meek
{
    public interface IScreenContainer
    { 
        IReadOnlyCollection<IScreen> Screens { get; }

        ValueTask NavigateAsync(NavigationContext context);
    }
}