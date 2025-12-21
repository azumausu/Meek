#if MEEK_ENABLE_UGUI
using System.Threading.Tasks;
using JetBrains.Annotations;
using Meek.NavigationStack;

namespace Meek.UGUI
{
    public interface IPrefabViewHandler : IViewHandler
    {
        ValueTask InitializeAsync(IPrefabViewProvider viewProvider, IScreen ownerScreen, [CanBeNull] object param);
    }
}
#endif