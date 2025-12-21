using System.Threading.Tasks;
using UnityEngine;

namespace Meek.MVP
{
    public class PresenterViewProviderFromResources : IPresenterViewProvider
    {
        private readonly string _path;
        private string _prefabName;

        public PresenterViewProviderFromResources(string path)
        {
            _path = path;
        }

        public void SetPrefabName(string prefabName)
        {
            _prefabName = prefabName;
        }

        public ValueTask<GameObject> ProvideAsync(IScreen ownerScreen, object param = null)
        {
            var tcs = new TaskCompletionSource<GameObject>();
            var resourceRequest = Resources.LoadAsync<GameObject>($"{_path}/{_prefabName}");
            resourceRequest.completed += _ => tcs.SetResult(resourceRequest.asset as GameObject);

            if (resourceRequest.isDone) tcs.SetResult(resourceRequest.asset as GameObject);

            return new ValueTask<GameObject>(tcs.Task);
        }
    }
}