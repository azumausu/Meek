using System;

namespace Meek
{
    /// <summary>
    /// 参照カウンタを用いて状態を管理するデザインパターン
    /// </summary>
    public class LockObject : ILocker, IDisposable
    {
        readonly ILockable _lockable;

        /// <summary>
        /// 参照カウント
        /// </summary>
        int _lockCount;

        /// <summary>
        /// Lockがかかっているか。
        /// </summary>
        public bool IsLock => _lockCount > 0;

        public LockObject(ILockable lockable)
        {
            _lockable = lockable;
            _lockCount = 0;
        }

        public LockObject(Action lockAction, Action unlockAction)
        {
            _lockable = new Lockable(lockAction, unlockAction);
            _lockCount = 0;
        }

        /// <summary>
        /// ロックカウントを+1します
        /// </summary>
        public IDisposable Lock()
        {
            var prevCount = _lockCount;
            _lockCount++;
            if (prevCount == 0)
            {
                _lockable.OnLock();
            }

            return this;
        }

        /// <summary>
        /// ロックカウントを-1します
        /// </summary>
        public void Unlock()
        {
            _lockCount--;
            if (_lockCount == 0)
            {
                _lockable.OnUnlock();
            }
        }

        void IDisposable.Dispose()
        {
            Unlock();
        }
    }

    class Lockable : ILockable
    {
        readonly Action _lockAction;
        readonly Action _unlockAction;

        public Lockable(Action lockAction, Action unlockAction)
        {
            _lockAction = lockAction;
            _unlockAction = unlockAction;
        }

        void ILockable.OnLock()
        {
            _lockAction?.Invoke();
        }

        void ILockable.OnUnlock()
        {
            _unlockAction?.Invoke();
        }
    }
}