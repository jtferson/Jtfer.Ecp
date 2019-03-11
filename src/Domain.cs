using System;
using System.Collections.Generic;
using System.Text;

namespace Jtfer.Ecp
{
    public class Domain
    {
        EntitySupervisor _supervisor;
        ContainerPool _containerPool;
        PipelineContext[] _contexts = new PipelineContext[4];
        int _contextCount;

        public Domain()
        {
            _supervisor = new EntitySupervisor();
            _containerPool = new ContainerPool();
        }

        public EntitySupervisor GetSupervisor()
        {
            return _supervisor;
        }

        public ContainerPool GetContainerPool()
        {
            return _containerPool;
        }

        internal void AddPipelineContext(PipelineContext context, EntityOwner owner)
        {
            if (_contextCount == _contexts.Length)
            {
                Array.Resize(ref _contexts, _contextCount << 1);
            }
            _contexts[_contextCount++] = context;

            _supervisor.AddEntityOwner(owner);
        }

        public PipelineContext GetPipelineContext(Type contextType)
        {
#if DEBUG
            var isFound = false;
            if (contextType == null) { throw new Exception("ContextType is null"); }
            if (!contextType.IsSubclassOf(typeof(PipelineContext))) { throw new Exception(string.Format("Invalid context-type: {0}", contextType)); }
#endif
            for (int i = 0, iMax = _contextCount; i < iMax; i++)
            {
                if (_contexts[i].GetType() == contextType)
                {
#if DEBUG
                    isFound = true;
#endif
                    return _contexts[i];
                }
            }
#if DEBUG
            if(!isFound)
                throw new Exception($"Context {contextType} is not found");
#endif
            return default;
        }

        public void RemoveOneFrameComponents()
        {
            _supervisor.RemoveOneFrameComponents();
        }

        public void RemoveDisabledPipelineEntities()
        {

        }
    }
}
