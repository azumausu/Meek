using System;

namespace Meek
{
    public interface IServiceProvider
    {
        T GetService<T>();

        object GetService(Type type);
    }
}