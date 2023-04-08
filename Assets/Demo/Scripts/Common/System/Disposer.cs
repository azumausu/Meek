using System;

public class Disposer : IDisposable
{
    private readonly Action _disposeAction;

    public Disposer(Action action)
    {
        _disposeAction = action;
    }

    public void Dispose()
    {
        _disposeAction.Invoke();
    }
}