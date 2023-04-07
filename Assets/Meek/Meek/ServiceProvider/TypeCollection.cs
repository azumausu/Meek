using System;
using System.Collections;
using System.Collections.Generic;

namespace Meek
{
    public class TypeCollection<TService> : IReadOnlyCollection<Type>
    {
        private readonly List<Type> _types = new List<Type>();

        public int Count => _types.Count;
        
        public void Add<T>() where T : TService
        {
            _types.Add(typeof(T));
        }

        public void Remove<T>() where T : TService
        {
            _types.Remove(typeof(T));
        }

        public void Clear()
        {
            _types.Clear();
        }
        
        public IEnumerator<Type> GetEnumerator()
        {
            return _types.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}