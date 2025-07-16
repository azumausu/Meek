using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Meek.NavigationStack
{
    public interface IViewHandlerLoader
    {
        bool IsLoaded { get; }

        Task<IViewHandler> LoadAsync([CanBeNull] object param = null);
    }
}