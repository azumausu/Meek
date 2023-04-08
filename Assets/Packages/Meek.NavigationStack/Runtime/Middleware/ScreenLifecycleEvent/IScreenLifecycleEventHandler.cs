using System.Threading.Tasks;

namespace Meek.NavigationStack
{
    public interface IScreenLifecycleEventHandler
    {
        ValueTask StartingImplAsync(NavigationContext context);

        ValueTask ResumingImplAsync(NavigationContext context);

        ValueTask PausingImplAsync(NavigationContext context);
        
        ValueTask DestroyingImplAsync(NavigationContext context); 
    }
}