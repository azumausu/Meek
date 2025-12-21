#if MEEK_ENABLE_UGUI

using System.Threading.Tasks;
using JetBrains.Annotations;
using UnityEngine;

namespace Meek.UGUI
{
    public interface IPrefabViewProvider
    {
        ValueTask<GameObject> ProvideAsync(IScreen ownerScreen, [CanBeNull] object param = null);
    }
}

#endif