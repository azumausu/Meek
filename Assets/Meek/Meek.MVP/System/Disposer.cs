using System;

namespace Meek.MVP
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