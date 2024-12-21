#if MEEK_ENABLE_UGUI
using System;

namespace Meek.UGUI
{
    internal class Disposer : IDisposable
    {
        private readonly Action _disposeAction;
        
        public Disposer(Action disposeAction)
        {
            _disposeAction = disposeAction;
        }

        public void Dispose()
        {
            _disposeAction.Invoke();
        }
    }
}
#endif