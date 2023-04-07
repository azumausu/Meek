using System;

namespace Meek
{
    public class ServiceType<TService>
    {
        private Type _type;

        public void Set<TImplementation>() where TImplementation : TService
        {
            _type = typeof(TImplementation);
        }

        public Type Get() => _type;
    }
}