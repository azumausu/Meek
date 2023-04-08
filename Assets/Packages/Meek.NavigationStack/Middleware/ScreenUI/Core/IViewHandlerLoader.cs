using System.Threading.Tasks;

namespace Meek.NavigationStack
{
    public interface IViewHandlerLoader
    {
        bool IsLoaded { get; }
        
        Task<IViewHandler> LoadAsync();
    }
}