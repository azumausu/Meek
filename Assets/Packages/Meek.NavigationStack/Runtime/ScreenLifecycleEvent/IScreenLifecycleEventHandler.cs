using System.Threading.Tasks;

namespace Meek.NavigationStack
{
    public interface IScreenLifecycleEventHandler
    {
        ValueTask StartingImplAsync(StackNavigationContext context);

        ValueTask ResumingImplAsync(StackNavigationContext context);

        ValueTask PausingImplAsync(StackNavigationContext context);

        ValueTask DestroyingImplAsync(StackNavigationContext context);
    }
}