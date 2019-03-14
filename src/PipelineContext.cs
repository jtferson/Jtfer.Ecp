using System;
using System.Collections.Generic;
using System.Text;

namespace Jtfer.Ecp
{
    public abstract class PipelineContext : IDisposable
    {
        Domain _domain;
        EntityOwner _entityOwner;
        ContainerPool _containerPool;
        EntityManager _entityManager;

        Pipeline[] _pipelines = new Pipeline[1];
        int _pipelineCount;

        IContainer[] _containers = new IContainer[4];
        int _containersCount;

        IInitContainer[] _initContainers = new IInitContainer[4];
        int _initContainerCount;

        internal bool _isActive;
        bool _isInitialized = false;

        public bool IsActive => _isActive;

        internal int TypeIndex;

        System.Collections.Generic.Dictionary<Type, object> _injections = new System.Collections.Generic.Dictionary<Type, object>(32);

        public PipelineContext(Domain domain, bool isActive = true, string name = null)
        {
            TypeIndex = Internals.EcpHelpers.ContextCount++;
            _entityManager = new EntityManager(domain.GetSupervisor(), TypeIndex);
            _isActive = isActive;
            _domain = domain;
            _containerPool = domain.GetContainerPool();
            _entityOwner = new EntityOwner();
            _domain.AddPipelineContext(this, _entityOwner);
            AddContainers();
        }

        public void SetActive(bool isActive)
        {
            _isActive = isActive;
        }

        public Pipeline CreateOperations(PipelineBuilder builder, string name = null)
        {
            var pipeline = new Pipeline(_domain, _entityManager, name);
            if (_pipelineCount == _pipelines.Length)
            {
                Array.Resize(ref _pipelines, _pipelineCount << 1);
            }
            _pipelines[_pipelineCount++] = pipeline;
            builder.AddOperationToPipeline(pipeline);

            return pipeline;
        }

        public Pipeline CreateOperations(string name = null, params IPipelineOperation[] operations)
        {
            var pipeline = new Pipeline(_domain, _entityManager, name);
            if (_pipelineCount == _pipelines.Length)
            {
                Array.Resize(ref _pipelines, _pipelineCount << 1);
            }
            _pipelines[_pipelineCount++] = pipeline;

            foreach (var op in operations)
                pipeline.Add(op);
            return pipeline;
        }

        public void CreateOperations(string name = null)
        {
            var pipeline = new Pipeline(_domain, _entityManager, name);
            if(_pipelineCount == _pipelines.Length)
            {
                Array.Resize(ref _pipelines, _pipelineCount << 1);
            }
            _pipelines[_pipelineCount++] = pipeline;

            AddOperations(pipeline);
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

            var initContainer = container as IInitContainer;
            if(initContainer != null)
            {
                if (_initContainerCount == _initContainers.Length)
                {
                    Array.Resize(ref _initContainers, _initContainerCount << 1);
                }
                _initContainers[_initContainerCount++] = initContainer;
            }

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
        public void Prepare()
        {
#if !ECP_DISABLE_INJECT
            for (var i = 0; i < _containersCount; i++)
            {
                EcpInjections.Inject(_containers[i], _domain, _entityManager, _injections);
            }
            for (var i = 0; i < _pipelineCount; i++)
                _pipelines[i].Prepare();
#endif
        }
        public void Initialize()
        {
            if(_isActive)
            {
                for (var i = 0; i < _initContainerCount; i++)
                {
                    _initContainers[i].Initialize();
                }
                for (var i = 0; i < _pipelineCount; i++)
                    _pipelines[i].Initialize();
                _isInitialized = true;
            }
            
        }

        public void Initialize(Pipeline pipeline)
        {
            if (_isActive)
            {
                for (var i = 0; i < _initContainerCount; i++)
                {
                    _initContainers[i].Initialize();
                }
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

                for (var i = 0; i < _pipelineCount; i++)
                    _pipelines[i].Update();
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

            for (var i = 0; i < _initContainerCount; i++)
            {
                _initContainers[i].Destroy();
            }
            _initContainers = null;
            _containersCount = 0;
        }
    }
}
