using UnityEngine;


namespace Demo
{
    /// <summary>
    /// MonoBehaviourのSingletonパターン
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class MonoBehaviourSingleton<T> : MonoBehaviour where T : MonoBehaviourSingleton<T>
    {
        private static T instance = null;
        public static T I => instance;
        public static bool IsValid => instance != null;

        public static void Release()
        {
            if (IsValid)
            {
                Destroy(instance);
            }
			instance.Deinit();
            instance = null;
		}

        void Awake()
        {
            if (!IsValid)
            {
                instance = this as T;
                instance.Init();
            }
            else
            {
                Debug.Log("すでにSingltonのインスタンスが存在しています");
            }
        }

        void OnDestroy()
        {
            instance.Deinit();
        }

        protected virtual void Init()
        {

        }

        protected virtual void Deinit()
        {

        }
    }
}
