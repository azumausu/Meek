namespace Meek
{
    /// <summary>
    /// ロック機能を提供するクラスに実装するinterfaceです。
    /// </summary>
    public interface ILockable
    {
        /// <summary>
        /// ロック
        /// </summary>
        void OnLock();

        void OnUnlock();
    }
}
