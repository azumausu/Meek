using Meek.UGUI;

namespace Meek.MVP
{
    public interface IPresenterViewProvider : IPrefabViewProvider
    {
        void SetPrefabName(string prefabName);
    }
}