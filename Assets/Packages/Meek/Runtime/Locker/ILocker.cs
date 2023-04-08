using System;

namespace Meek
{
    public interface ILocker
    {
        IDisposable Lock();

        void Unlock();
    }
}
