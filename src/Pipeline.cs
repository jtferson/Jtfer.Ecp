using System;
using System.Collections.Generic;
using System.Text;

namespace Jtfer.Ecp
{
    /// <summary>
    /// Базовый интерфейс для всех операций
    /// </summary>
    public interface IPipelineOperation { }

    public interface IPreInitOperation : IPipelineOperation
    {
        void PreInitialize();

        void PreDestroy();
    }
    public interface IInitOperation : IPipelineOperation
    {
        void Initialize();

        void Destroy();
    }

    public interface IUpdateOperation : IPipelineOperation
    {
        void Update();
    }


#if DEBUG
    /// <summary>
    /// Debug interface for systems events processing.
    /// </summary>
    public interface IPipelineDebugListener
    {
        void OnSystemsDestroyed();
    }
#endif

    public sealed class PipelineBuilder : IDisposable
    {
        IPipelineOperation[] _operations;

        public PipelineBuilder(params IPipelineOperation[] operations)
        {
            _operations = operations;
        }

        public void AddOperationToPipeline(Pipeline pipeline)
        {
            foreach (var op in _operations)
                pipeline.Add(op);
        }

        public void Dispose()
        {
            _operations = null;
        }
    }

    /// <summary>
    /// Logical group of systems.
    /// </summary>
#if ENABLE_IL2CPP
    [Unity.IL2CPP.CompilerServices.Il2CppSetOption(Unity.IL2CPP.CompilerServices.Option.NullChecks, false)]
    [Unity.IL2CPP.CompilerServices.Il2CppSetOption(Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false)]
#endif
    public sealed class Pipeline : IDisposable, IInitOperation, IUpdateOperation
    {
#if DEBUG
        /// <summary>
        /// List of all debug listeners.
        /// </summary>
        readonly System.Collections.Generic.List<IPipelineDebugListener> _debugListeners = new System.Collections.Generic.List<IPipelineDebugListener>(4);

        readonly public System.Collections.Generic.List<bool> DisabledInDebugSystems = new System.Collections.Generic.List<bool>(32);
#endif
        public readonly string Name;

        /// <summary>
        /// Ecs world instance.
        /// </summary>
        readonly EntityManager _entityManager;
        /// <summary>
        /// Ecs world instance.
        /// </summary>
        readonly EntitySupervisor _supervisor;

        readonly Domain _domain;
        /// <summary>
        /// Registered IEcsPreInitSystem systems.
        /// </summary>
        IPreInitOperation[] _preInitSystems = new IPreInitOperation[4];

        /// <summary>
        /// Count of registered IEcsPreInitSystem systems.
        /// </summary>
        int _preInitSystemsCount;

        /// <summary>
        /// Registered IEcsInitSystem systems.
        /// </summary>
        IInitOperation[] _initSystems = new IInitOperation[16];

        /// <summary>
        /// Count of registered IEcsInitSystem systems.
        /// </summary>
        int _initSystemsCount;

        /// <summary>
        /// Registered IEcsRunSystem systems.
        /// </summary>
        IUpdateOperation[] _runSystems = new IUpdateOperation[16];

        /// <summary>
        /// Count of registered IEcsRunSystem systems.
        /// </summary>
        int _runSystemsCount;

#if DEBUG
        /// <summary>
        /// Is Initialize method was called?
        /// </summary>
        bool _inited;

        /// <summary>
        /// Is Dispose method was called?
        /// </summary>
        bool _isDisposed;
#endif

        /// <summary>
        /// Creates new instance of EcsSystems group.
        /// </summary>
        /// <param name="world">EcsWorld instance.</param>
        /// <param name="name">Custom name for this group.</param>
        public Pipeline(Domain domain, EntityManager entityManager, string name = null)
        {
#if DEBUG
            if (domain == null)
            {
                throw new ArgumentNullException();
            }

            if (entityManager == null)
            {
                throw new ArgumentNullException();
            }
#endif
            _domain = domain;
            _supervisor = _domain.GetSupervisor();
            _entityManager = entityManager;
            Name = name;
        }

#if DEBUG
        /// <summary>
        /// Adds external event listener.
        /// </summary>
        /// <param name="observer">Event listener.</param>
        public void AddDebugListener(IPipelineDebugListener observer)
        {
#if DEBUG
            if (observer == null) { throw new Exception("observer is null"); }
            if (_debugListeners.Contains(observer)) { throw new Exception("Listener already exists"); }
#endif
            _debugListeners.Add(observer);
        }

        /// <summary>
        /// Removes external event listener.
        /// </summary>
        /// <param name="observer">Event listener.</param>
        public void RemoveDebugListener(IPipelineDebugListener observer)
        {
#if DEBUG
            if (observer == null) { throw new Exception("observer is null"); }
#endif
            _debugListeners.Remove(observer);
        }
#endif
        /// <summary>
        /// Gets all pre-init systems.
        /// </summary>
        /// <param name="list">List to put results in it. If null - will be created.</param>
        /// <returns>Amount of systems in list.</returns>
        public int GetPreInitSystems(ref IPreInitOperation[] list)
        {
            if (list == null || list.Length < _preInitSystemsCount)
            {
                list = new IPreInitOperation[_preInitSystemsCount];
            }
            Array.Copy(_preInitSystems, 0, list, 0, _preInitSystemsCount);
            return _preInitSystemsCount;
        }

        /// <summary>
        /// Gets all init systems.
        /// </summary>
        /// <param name="list">List to put results in it. If null - will be created.</param>
        /// <returns>Amount of systems in list.</returns>
        public int GetInitSystems(ref IInitOperation[] list)
        {
            if (list == null || list.Length < _initSystemsCount)
            {
                list = new IInitOperation[_initSystemsCount];
            }
            Array.Copy(_initSystems, 0, list, 0, _initSystemsCount);
            return _initSystemsCount;
        }

        /// <summary>
        /// Gets all run systems.
        /// </summary>
        /// <param name="list">List to put results in it. If null - will be created.</param>
        /// <returns>Amount of systems in list.</returns>
        public int GetRunSystems(ref IUpdateOperation[] list)
        {
            if (list == null || list.Length < _runSystemsCount)
            {
                list = new IUpdateOperation[_runSystemsCount];
            }
            Array.Copy(_runSystems, 0, list, 0, _runSystemsCount);
            return _runSystemsCount;
        }

        /// <summary>
        /// Adds new system to processing.
        /// </summary>
        /// <param name="system">System instance.</param>
        public Pipeline Add(IPipelineOperation system)
        {
#if DEBUG
            if (system == null) { throw new Exception("system is null"); }
#endif
#if !ECP_DISABLE_INJECT
            if (_injectSystemsCount == _injectSystems.Length)
            {
                Array.Resize(ref _injectSystems, _injectSystemsCount << 1);
            }
            _injectSystems[_injectSystemsCount++] = system;
#endif
            var preInitSystem = system as IPreInitOperation;
            if (preInitSystem != null)
            {
                if (_preInitSystemsCount == _preInitSystems.Length)
                {
                    Array.Resize(ref _preInitSystems, _preInitSystemsCount << 1);
                }
                _preInitSystems[_preInitSystemsCount++] = preInitSystem;
            }

            var initSystem = system as IInitOperation;
            if (initSystem != null)
            {
                if (_initSystemsCount == _initSystems.Length)
                {
                    Array.Resize(ref _initSystems, _initSystemsCount << 1);
                }
                _initSystems[_initSystemsCount++] = initSystem;
            }

            var runSystem = system as IUpdateOperation;
            if (runSystem != null)
            {
                if (_runSystemsCount == _runSystems.Length)
                {
                    Array.Resize(ref _runSystems, _runSystemsCount << 1);
                }
                _runSystems[_runSystemsCount++] = runSystem;
            }
            return this;
        }

#if !ECP_DISABLE_INJECT
        /// <summary>
        /// Systems to builtin inject behaviour.
        /// </summary>
        IPipelineOperation[] _injectSystems = new IPipelineOperation[16];

        int _injectSystemsCount;

        /// <summary>
        /// Store for injectable instances.
        /// </summary>
        /// <typeparam name="Type"></typeparam>
        /// <typeparam name="object"></typeparam>
        /// <returns></returns>
        System.Collections.Generic.Dictionary<Type, object> _injections = new System.Collections.Generic.Dictionary<Type, object>(32);

        /// <summary>
        /// Injects instance of object type to all compatible fields of added systems.
        /// </summary>
        /// <param name="obj">Instance.</param>
        public Pipeline Inject<T>(T obj)
        {
            _injections[typeof(T)] = obj;
            return this;
        }
#endif

        public void Prepare()
        {
#if !ECP_DISABLE_INJECT
            for (var i = 0; i < _injectSystemsCount; i++)
            {
                // injection for nested EcsSystems.
                var nestedSystems = _injectSystems[i] as Pipeline;
                if (nestedSystems != null)
                {
                    foreach (var pair in _injections)
                    {
                        nestedSystems._injections[pair.Key] = pair.Value;
                    }
                }
                EcpInjections.Inject(_injectSystems[i], _domain, _entityManager, _injections);
            }
#endif
        }


        /// <summary>
        /// Closes registration for new systems, initialize all registered.
        /// </summary>
        public void Initialize()
        {
#if DEBUG
            if (_inited) { throw new Exception("EcsSystems instance already initialized"); }
            for (var i = 0; i < _runSystemsCount; i++)
            {
                DisabledInDebugSystems.Add(false);
            }
            _inited = true;
#endif

            for (var i = 0; i < _preInitSystemsCount; i++)
            {
                _preInitSystems[i].PreInitialize();
                _supervisor.ProcessDelayedUpdates();
            }

            for (var i = 0; i < _initSystemsCount; i++)
            {
                _initSystems[i].Initialize();
                _supervisor.ProcessDelayedUpdates();
            }
        }

        /// <summary>
        /// Destroys all registered external data, full cleanup for internal data.
        /// </summary>
        public void Dispose()
        {
#if DEBUG
            if (_isDisposed) { throw new Exception("EcsSystems instance already disposed"); }
            if (!_inited) { throw new Exception("EcsSystems instance was not initialized"); }
            _isDisposed = true;
            for (var i = _debugListeners.Count - 1; i >= 0; i--)
            {
                _debugListeners[i].OnSystemsDestroyed();
            }
            _debugListeners.Clear();
            DisabledInDebugSystems.Clear();
            _inited = false;
#endif
            for (var i = _initSystemsCount - 1; i >= 0; i--)
            {
                _initSystems[i].Destroy();
                _initSystems[i] = null;
                _supervisor.ProcessDelayedUpdates();
            }
            _initSystemsCount = 0;

            for (var i = _preInitSystemsCount - 1; i >= 0; i--)
            {
                _preInitSystems[i].PreDestroy();
                _preInitSystems[i] = null;
                _supervisor.ProcessDelayedUpdates();
            }
            _preInitSystemsCount = 0;

            for (var i = _runSystemsCount - 1; i >= 0; i--)
            {
                _runSystems[i] = null;
            }
            _runSystemsCount = 0;

#if !ECP_DISABLE_INJECT
            for (var i = _injectSystemsCount - 1; i >= 0; i--)
            {
                _injectSystems[i] = null;
            }
            _injectSystemsCount = 0;
            _injections.Clear();
#endif
        }

        /// <summary>
        /// Processes all IEcsRunSystem systems.
        /// </summary>
        public void Update()
        {
#if DEBUG
            if (!_inited) { throw new Exception("EcsSystems instance was not initialized"); }
#endif
            for (var i = 0; i < _runSystemsCount; i++)
            {
#if DEBUG
                if (DisabledInDebugSystems[i])
                {
                    continue;
                }
#endif
                _runSystems[i].Update();
                _supervisor.ProcessDelayedUpdates();
            }
        }

        void IInitOperation.Destroy()
        {
            Dispose();
        }
    }
}
