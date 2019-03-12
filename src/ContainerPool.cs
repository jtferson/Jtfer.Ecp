using System;
using System.Collections.Generic;
using System.Text;

namespace Jtfer.Ecp
{
    public interface IContainer
    {

    }

    public class ContainerPool : IDisposable
    {
        IContainer[] _containers = new IContainer[4];
        int _containersCount;
        public T AddContainer<T>()
         where T : IContainer, new()
        {
            T container;
            AddContainer<T>(out container);
            return container;
        }
        public IContainer AddContainer(Type containerType)
        {
            IContainer container;
            AddContainer(containerType, out container);
            return container;
        }
        public ContainerPool AddContainer<T>(out T container)
            where T : IContainer, new()
        {
            if (_containersCount == _containers.Length)
            {
                Array.Resize(ref _containers, _containersCount << 1);
            }
            container = CreateContainer<T>();
            _containers[_containersCount++] = container;
            return this;
        }

        public ContainerPool AddContainer(Type containerType, out IContainer container)
        {
            if (_containersCount == _containers.Length)
            {
                Array.Resize(ref _containers, _containersCount << 1);
            }
            container = CreateContainer(containerType);
            _containers[_containersCount++] = container;
            return this;
        }

        public ContainerPool AddContainer(IContainer container)
        {
            if (_containersCount == _containers.Length)
            {
                Array.Resize(ref _containers, _containersCount << 1);
            }
            _containers[_containersCount++] = container;
            return this;
        }

        public IContainer GetContainer(Type containerType)
        {
#if DEBUG
            if (containerType == null) { throw new Exception("ContainerType is null"); }
            if (!containerType.IsSubclassOf(typeof(EntityFilter))) { throw new Exception(string.Format("Invalid container-type: {0}", containerType)); }
#endif
            for (int i = 0, iMax = _containersCount; i < iMax; i++)
            {
                if (_containers[i].GetType() == containerType)
                {
                    return _containers[i];
                }
            }
#if DEBUG
            throw new Exception($"Container {containerType} is not found");
#endif
            return default;
        }

        private T CreateContainer<T>()
            where T : IContainer, new()
        {
            return new T();
        }

        private IContainer CreateContainer(Type containerType)
        {
            return (IContainer)Activator.CreateInstance(containerType);
        }
        public void Dispose()
        {
        }
    }
}
