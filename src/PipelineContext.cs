using System;
using System.Collections.Generic;
using System.Text;

namespace Jtfer.Ecp
{
    public abstract class PipelineContext : IDisposable
    {
        Domain _domain;
        Pipeline _pipeline;
        EntityOwner _entityOwner;
        ContainerPool _containerPool;
        EntityManager _entityManager;


        IContainer[] _containers = new IContainer[4];
        int _containersCount;

        internal bool _isActive;
        bool _isInitialized = false;

        public bool IsActive => _isActive;

        internal int TypeIndex;

        public PipelineContext(Domain domain, bool isActive = true, string name = null)
        {
            TypeIndex = Internals.EcpHelpers.ContextCount++;
            _entityManager = new EntityManager(domain.GetSupervisor(), TypeIndex);
            _isActive = isActive;
            _domain = domain;
            _containerPool = domain.GetContainerPool();
            _pipeline = new Pipeline(domain, _entityManager, name);
            AddOperations(_pipeline);
            _entityOwner = new EntityOwner();
            _domain.AddPipelineContext(this, _entityOwner);
        }

        public void SetActive(bool isActive)
        {
            _isActive = isActive;
        }

        protected abstract void AddOperations(Pipeline pipeline);
        protected abstract void AddContainers();
        public T AddContainer<T>()
         where T : IContainer, new()
        {
            T container;
            _containerPool.AddContainer<T>(out container);

            if (_containersCount == _containers.Length)
            {
                Array.Resize(ref _containers, _containersCount << 1);
            }
            _containers[_containersCount++] = container;

            return container;
        }

        public IContainer AddContainer(Type containerType)
        {
            IContainer container;
            _containerPool.AddContainer(containerType, out container);

            if (_containersCount == _containers.Length)
            {
                Array.Resize(ref _containers, _containersCount << 1);
            }
            _containers[_containersCount++] = container;

            return container;
        }

        public IContainer AddContainer(IContainer container)
        {
            _containerPool.AddContainer(container);

            if (_containersCount == _containers.Length)
            {
                Array.Resize(ref _containers, _containersCount << 1);
            }
            _containers[_containersCount++] = container;

            return container;
        }



        public void Initialize()
        {
            if(_isActive)
            {
                _pipeline.Initialize();
                _isInitialized = true;
            }
            
        }

        public void Initialize(Pipeline pipeline)
        {
            if (_isActive)
            {
                pipeline.Initialize();
                _isInitialized = true;
            }

        }

        public void Update()
        {
            if(_isActive)
            {
                if (!_isInitialized)
                    Initialize();

                _pipeline.Update();
            }
        }

        public void Update(Pipeline pipeline)
        {
            if (_isActive)
            {
                if (!_isInitialized)
                    Initialize();

                pipeline.Update();
            }
        }

        public void Dispose()
        {
            _containers = null;
            _containersCount = 0;
        }
    }
}
