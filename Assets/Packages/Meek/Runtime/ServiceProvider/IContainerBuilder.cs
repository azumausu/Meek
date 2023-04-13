using System;

namespace Meek
{
    public interface IContainerBuilder
    {
        IServiceCollection ServiceCollection { get; }

        IServiceProvider Build();
    }
}