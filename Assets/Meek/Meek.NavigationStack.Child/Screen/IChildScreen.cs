using System.Threading.Tasks;

namespace Meek.NavigationStack.Child
{
    public interface IChildScreen : IScreen
    {
        IScreen ParentScreen { get; }
        
        ValueTask OpenChildScreenAsync(ChildStackNavigationContext context);
        
        ValueTask CloseChildScreenAsync(ChildStackNavigationContext context);
    }
}