using System;
using System.Collections;
using System.Collections.Generic;

namespace Meek
{
    public class ServiceCollection : IServiceCollection
    {
        private readonly List<ServiceDescriptor> _serviceDescriptors = new();

        private bool _isReadOnly = false;
        
        public int Count => _serviceDescriptors.Count;

        /// <inheritdoc />
        public bool IsReadOnly => _isReadOnly;


        public ServiceDescriptor this[int index]
        {
            get => _serviceDescriptors[index];
            set
            {
                CheckReadOnly();
                _serviceDescriptors[index] = value;
            }
        }

        public void Clear()
        {
            CheckReadOnly();
            _serviceDescriptors.Clear();
        }
        
        public bool Contains(ServiceDescriptor item)
        {
            return _serviceDescriptors.Contains(item);
        }

        public void CopyTo(ServiceDescriptor[] array, int arrayIndex)
        {
            _serviceDescriptors.CopyTo(array, arrayIndex);
        }

        public bool Remove(ServiceDescriptor item)
        {
            CheckReadOnly();
            return _serviceDescriptors.Remove(item);
        }
        
        public IEnumerator<ServiceDescriptor> GetEnumerator()
        {
            return _serviceDescriptors.GetEnumerator();
        }

        void ICollection<ServiceDescriptor>.Add(ServiceDescriptor item)
        {
            CheckReadOnly();
            _serviceDescriptors.Add(item);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        
        public int IndexOf(ServiceDescriptor item)
        {
            return _serviceDescriptors.IndexOf(item);
        }

        public void Insert(int index, ServiceDescriptor item)
        {
            CheckReadOnly();
            _serviceDescriptors.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            CheckReadOnly();
            _serviceDescriptors.RemoveAt(index);
        }
        
        public void MakeReadOnly()
        {
            _isReadOnly = true;
        }

        public void CheckReadOnly()
        {
            if (_isReadOnly) throw new InvalidOperationException();
        }
    }
}