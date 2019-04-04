using System;
using System.Collections.Generic;
using System.Text;

namespace Jtfer.Ecp
{
    public class EntityManager
    {
        int _pipelineIndex;
        EntitySupervisor _supervisor;
        internal EntityManager(EntitySupervisor supervisor, int pipelineIdex)
        {
            _supervisor = supervisor;
            _pipelineIndex = pipelineIdex;
        }

        public int CreateEntity()
        {
            return _supervisor.CreateEntity(_pipelineIndex, - 1);
        }

        public int CreateEntity<TEntity>()
            where TEntity : IEntityType
        {
            var entityTypeIndex = EntityType<TEntity>.Instance.TypeIndex;
            return _supervisor.CreateEntity(_pipelineIndex, entityTypeIndex);
        }



        public TC CreateEntityWith<TC>()
            where TC : class, new()
        {
            return _supervisor.CreateEntityWith<TC>(_pipelineIndex, - 1);
        }

        public TC CreateEntityWith<TC, TEntity>()
            where TC : class, new() where TEntity : IEntityType
        {
            var entityTypeIndex = EntityType<TEntity>.Instance.TypeIndex;
            return _supervisor.CreateEntityWith<TC>(_pipelineIndex, entityTypeIndex);
        }



        public int CreateEntityWith<TC>(out TC component)
            where TC : class, new()
        {
            return _supervisor.CreateEntityWith(out component, _pipelineIndex, -1);
        }

        public int CreateEntityWith<TC, TEntity>(out TC component)
            where TC : class, new() where TEntity : IEntityType
        {
            var entityTypeIndex = EntityType<TEntity>.Instance.TypeIndex;
            return _supervisor.CreateEntityWith(out component, _pipelineIndex, entityTypeIndex);
        }



        public int CreateEntityWith<T1, T2>(out T1 c1, out T2 c2)
            where T1 : class, new() where T2 : class, new()
        {
            return _supervisor.CreateEntityWith(out c1, out c2, _pipelineIndex, -1);
        }
        public int CreateEntityWith<T1, T2, TEntity>(out T1 c1, out T2 c2)
            where T1 : class, new() where T2 : class, new() where TEntity : IEntityType
        {
            var entityTypeIndex = EntityType<TEntity>.Instance.TypeIndex;
            return _supervisor.CreateEntityWith(out c1, out c2, _pipelineIndex, entityTypeIndex);
        }



        public int CreateEntityWith<T1, T2, T3>(out T1 c1, out T2 c2, out T3 c3)
            where T1 : class, new() where T2 : class, new() where T3 : class, new()
        {
            return _supervisor.CreateEntityWith(out c1, out c2, out c3, _pipelineIndex, -1);
        }

        public int CreateEntityWith<T1, T2, T3, TEntity>(out T1 c1, out T2 c2, out T3 c3)
            where T1 : class, new() where T2 : class, new() where T3 : class, new() where TEntity : IEntityType
        {
            var entityTypeIndex = EntityType<TEntity>.Instance.TypeIndex;
            return _supervisor.CreateEntityWith(out c1, out c2, out c3, _pipelineIndex, entityTypeIndex);
        }


        public void RemoveEntity(int entity)
        {
            _supervisor.RemoveEntity(entity);
        }
#if NET_4_6 || NET_STANDARD_2_0
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        public T EnsureComponent<T>(int entity, out bool isNew)
            where T : class, new()
        {
            return _supervisor.EnsureComponent<T>(entity, out isNew);
        }
#if NET_4_6 || NET_STANDARD_2_0
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        public T AddComponent<T>(int entity)
            where T : class, new()
        {
            return _supervisor.AddComponent<T>(entity);
        }
#if NET_4_6 || NET_STANDARD_2_0
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        public void RemoveComponent<T>(int entity, bool noError = false)
            where T : class, new()
        {
            _supervisor.RemoveComponent<T>(entity, noError);
        }
#if NET_4_6 || NET_STANDARD_2_0
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        public T GetComponent<T>(int entity)
            where T : class, new()
        {
            return _supervisor.GetComponent<T>(entity);
        }
    }
}
