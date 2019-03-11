using System;
using System.Collections.Generic;
using System.Text;

namespace Jtfer.Ecp
{
    /// <summary>
    /// Common interface for all filter listeners.
    /// </summary>
    public interface IEntityFilterListener
    {
        void OnEntityAdded(int entity);
        void OnEntityRemoved(int entity);
    }

    /// <summary>
    /// Container for filtered entities based on specified constraints.
    /// </summary>
    public abstract class EntityFilter
    {
        /// <summary>
        /// Default minimal size for components / entities buffers.
        /// </summary>
        protected const int MinSize = 32;

        /// <summary>
        /// Mask of included (required) components with this filter.
        /// Do not change it manually!
        /// </summary>
        public readonly ComponentMask IncludeMask = new ComponentMask();

        /// <summary>
        /// Mask of excluded (denied) components with this filter.
        /// Do not change it manually!
        /// </summary>
        public readonly ComponentMask ExcludeMask = new ComponentMask();

        /// <summary>
        /// Access to connected EcsWorld instance.
        /// </summary>
        public EntitySupervisor Supervisor
        {
            get { return _supervisor; }
            internal set { _supervisor = value; }
        }

        /// <summary>
        /// Instance of connected EcsWorld.
        /// Do not change it manually!
        /// </summary>
        protected EntitySupervisor _supervisor;

        IComponentPool[] _pools = new IComponentPool[4];
        int _poolsCount;
        protected IEntityFilterListener[] _listeners = new IEntityFilterListener[4];
        protected int _listenersCount;

        /// <summary>
        /// Will be raised by Domain for new compatible with this filter entity.
        /// Do not call it manually!
        /// </summary>
        /// <param name="entity">Entity id.</param>
        public abstract void RaiseOnAddEvent(int entity);

        /// <summary>
        /// Will be raised by Domain for old already non-compatible with this filter entity.
        /// Do not call it manually!
        /// </summary>
        /// <param name="entity">Entity id.</param>
        public abstract void RaiseOnRemoveEvent(int entity);

        /// <summary>
        /// Storage of filtered entities.
        /// Important: Length of this storage can be larger than real amount of items,
        /// use EntitiesCount instead of Entities.Length!
        /// Do not change it manually!
        /// </summary>
        public int[] Entities = new int[MinSize];

#if NET_4_6 || NET_STANDARD_2_0
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        public Enumerator GetEnumerator()
        {
            return new Enumerator(_entitiesCount);
        }

        internal int EntityTypeIndex;


#if NET_4_6 || NET_STANDARD_2_0
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        internal bool IsEntityTypeCompatible(int entityTypeIndex)
        {
            return EntityTypeIndex == -1 || EntityTypeIndex == entityTypeIndex;
        }


#if NET_4_6 || NET_STANDARD_2_0
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        /// <summary>
        /// Is filter empty.
        /// </summary>
        public bool IsEmpty()
        {
            return _entitiesCount == 0;
        }

        /// <summary>
        /// Amount of filtered entities.
        /// </summary>
        [Obsolete("Use foreach(var idx in filter) { } loop instead, or IsEmpty() for check that filter is empty or not")]
        public int EntitiesCount { get { return _entitiesCount; } }

        internal protected int _entitiesCount;

        public struct Enumerator : IEnumerator<int>
        {
            readonly int _count;
            int _idx;

#if NET_4_6 || NET_STANDARD_2_0
            [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
            internal Enumerator(int entitiesCount)
            {
                _count = entitiesCount;
                _idx = -1;
            }

            public int Current
            {
#if NET_4_6 || NET_STANDARD_2_0
                [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
                get { return _idx; }
            }

            object System.Collections.IEnumerator.Current { get { return null; } }

#if NET_4_6 || NET_STANDARD_2_0
            [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
            public void Dispose() { }

#if NET_4_6 || NET_STANDARD_2_0
            [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
            public bool MoveNext()
            {
                return ++_idx < _count;
            }

#if NET_4_6 || NET_STANDARD_2_0
            [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
            public void Reset()
            {
                _idx = -1;
            }

#if NET_4_6 || NET_STANDARD_2_0
            [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
            public int GetCount()
            {
                return _count;
            }
        }

        protected void AddComponentPool(IComponentPool pool)
        {
            if (_pools.Length == _poolsCount)
            {
                Array.Resize(ref _pools, _poolsCount << 1);
            }
            _pools[_poolsCount++] = pool;
        }

        /// <summary>
        /// Gets connected component pool from constraint components type index.
        /// </summary>
        /// <param name="id">Constraint components type index.</param>
        public IComponentPool GetComponentPool(int id)
        {
#if DEBUG
            if (id < 0 || id >= _poolsCount)
            {
                throw new Exception(string.Format("Invalid included component index {0} for filter \"{1}\".", id, GetType()));
            }
#endif
            return _pools[id];
        }

        /// <summary>
        /// Subscribes listener to filter events.
        /// </summary>
        /// <param name="listener">Listener.</param>
        public void AddListener(IEntityFilterListener listener)
        {
#if DEBUG
            for (int i = 0, iMax = _listenersCount; i < iMax; i++)
            {
                if (_listeners[i] == listener)
                {
                    throw new Exception("Listener already subscribed.");
                }
            }
#endif
            if (_listeners.Length == _listenersCount)
            {
                Array.Resize(ref _listeners, _listenersCount << 1);
            }
            _listeners[_listenersCount++] = listener;
        }

        /// <summary>
        /// Unsubscribes listener from filter events.
        /// </summary>
        /// <param name="listener">Listener.</param>
        public void RemoveListener(IEntityFilterListener listener)
        {
            for (int i = 0, iMax = _listenersCount; i < iMax; i++)
            {
                if (_listeners[i] == listener)
                {
                    _listenersCount--;
                    Array.Copy(_listeners, i + 1, _listeners, i, _listenersCount - i);
                    break;
                }
            }
        }

        /// <summary>
        /// Vaidates amount of constraint components.
        /// </summary>
        /// <param name="inc">Valid amount for included components.</param>
        /// <param name="exc">Valid amount for excluded components.</param>
        [System.Diagnostics.Conditional("DEBUG")]
        protected void ValidateMasks(int inc, int exc)
        {
#if DEBUG
            if (IncludeMask.BitsCount != inc || ExcludeMask.BitsCount != exc)
            {
                throw new Exception(string.Format("Invalid filter type \"{0}\": possible duplicated component types.", GetType()));
            }
            if (IncludeMask.IsIntersects(ExcludeMask))
            {
                throw new Exception(string.Format("Invalid filter type \"{0}\": Include types intersects with exclude types.", GetType()));
            }
#endif
        }

#if DEBUG
        public override string ToString()
        {
            return string.Format("Filter(+{0} -{1})", IncludeMask, ExcludeMask);
        }
#endif
    }


#if ENABLE_IL2CPP
    [Unity.IL2CPP.CompilerServices.Il2CppSetOption (Unity.IL2CPP.CompilerServices.Option.NullChecks, false)]
    [Unity.IL2CPP.CompilerServices.Il2CppSetOption (Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false)]
    [UnityEngine.Scripting.Preserve]
#endif
    public abstract class AnyEntityType : EntityFilter
    {
        protected AnyEntityType()
        {
            EntityTypeIndex = -1;
        }


        /// <summary>
        /// Container for filtered entities based on specified constraints.
        /// </summary>

        public class EntityFilter<Inc1> : AnyEntityType
            where Inc1 : class, new()
        {
            public Inc1[] Components1;
            bool _allow1;

            protected EntityFilter()
            {
                _allow1 = !ComponentPool<Inc1>.Instance.IsIgnoreInFilter;
                Components1 = _allow1 ? new Inc1[MinSize] : null;
                IncludeMask.SetBit(ComponentPool<Inc1>.Instance.TypeIndex, true);
                AddComponentPool(ComponentPool<Inc1>.Instance);
            }


#if NET_4_6 || NET_STANDARD_2_0
            [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
            public override void RaiseOnAddEvent(int entity)
            {
                if (Entities.Length == _entitiesCount)
                {
                    Array.Resize(ref Entities, _entitiesCount << 1);
                    if (_allow1)
                    {
                        Array.Resize(ref Components1, _entitiesCount << 1);
                    }
                }
                if (_allow1)
                {
                    Components1[_entitiesCount] = _supervisor.GetComponent<Inc1>(entity);
                }
                Entities[_entitiesCount++] = entity;
                for (int j = 0, jMax = _listenersCount; j < jMax; j++)
                {
                    _listeners[j].OnEntityAdded(entity);
                }
            }
#if NET_4_6 || NET_STANDARD_2_0
            [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
            public override void RaiseOnRemoveEvent(int entity)
            {
                for (var i = 0; i < _entitiesCount; i++)
                {
                    if (Entities[i] == entity)
                    {
                        _entitiesCount--;
                        Array.Copy(Entities, i + 1, Entities, i, _entitiesCount - i);
                        if (_allow1)
                        {
                            Array.Copy(Components1, i + 1, Components1, i, _entitiesCount - i);
                        }
                        for (int j = 0, jMax = _listenersCount; j < jMax; j++)
                        {
                            _listeners[j].OnEntityRemoved(entity);
                        }
                        break;
                    }
                }
            }

            /// <summary>
            /// Container for filtered entities based on specified constraints.
            /// </summary>
            public class Exclude<Exc1> : EntityFilter<Inc1> where Exc1 : class, new()
            {
                protected Exclude()
                {
                    ExcludeMask.SetBit(ComponentPool<Exc1>.Instance.TypeIndex, true);
                    ValidateMasks(1, 1);
                }
            }

            /// <summary>
            /// Container for filtered entities based on specified constraints.
            /// </summary>
            public class Exclude<Exc1, Exc2> : EntityFilter<Inc1> where Exc1 : class, new() where Exc2 : class, new()
            {
                protected Exclude()
                {
                    ExcludeMask.SetBit(ComponentPool<Exc1>.Instance.TypeIndex, true);
                    ExcludeMask.SetBit(ComponentPool<Exc2>.Instance.TypeIndex, true);
                    ValidateMasks(1, 2);
                }
            }
        }

        /// <summary>
        /// Container for filtered entities based on specified constraints.
        /// </summary>
        public class EntityFilter<Inc1, Inc2> : AnyEntityType where Inc1 : class, new() where Inc2 : class, new()
        {
            public Inc1[] Components1;
            public Inc2[] Components2;
            bool _allow1;
            bool _allow2;

            protected EntityFilter()
            {
                _allow1 = !ComponentPool<Inc1>.Instance.IsIgnoreInFilter;
                _allow2 = !ComponentPool<Inc2>.Instance.IsIgnoreInFilter;
                Components1 = _allow1 ? new Inc1[MinSize] : null;
                Components2 = _allow2 ? new Inc2[MinSize] : null;
                IncludeMask.SetBit(ComponentPool<Inc1>.Instance.TypeIndex, true);
                IncludeMask.SetBit(ComponentPool<Inc2>.Instance.TypeIndex, true);
                AddComponentPool(ComponentPool<Inc1>.Instance);
                AddComponentPool(ComponentPool<Inc2>.Instance);
                ValidateMasks(2, 0);
            }
#if NET_4_6 || NET_STANDARD_2_0
            [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
            public override void RaiseOnAddEvent(int entity)
            {
                if (Entities.Length == _entitiesCount)
                {
                    Array.Resize(ref Entities, _entitiesCount << 1);
                    if (_allow1)
                    {
                        Array.Resize(ref Components1, _entitiesCount << 1);
                    }
                    if (_allow2)
                    {
                        Array.Resize(ref Components2, _entitiesCount << 1);
                    }
                }
                if (_allow1)
                {
                    Components1[_entitiesCount] = _supervisor.GetComponent<Inc1>(entity);
                }
                if (_allow2)
                {
                    Components2[_entitiesCount] = _supervisor.GetComponent<Inc2>(entity);
                }
                Entities[_entitiesCount++] = entity;
                for (int j = 0, jMax = _listenersCount; j < jMax; j++)
                {
                    _listeners[j].OnEntityAdded(entity);
                }
            }
#if NET_4_6 || NET_STANDARD_2_0
            [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
            public override void RaiseOnRemoveEvent(int entity)
            {
                for (var i = 0; i < _entitiesCount; i++)
                {
                    if (Entities[i] == entity)
                    {
                        _entitiesCount--;
                        Array.Copy(Entities, i + 1, Entities, i, _entitiesCount - i);
                        if (_allow1)
                        {
                            Array.Copy(Components1, i + 1, Components1, i, _entitiesCount - i);
                        }
                        if (_allow2)
                        {
                            Array.Copy(Components2, i + 1, Components2, i, _entitiesCount - i);
                        }
                        for (int j = 0, jMax = _listenersCount; j < jMax; j++)
                        {
                            _listeners[j].OnEntityRemoved(entity);
                        }
                        break;
                    }
                }
            }

            /// <summary>
            /// Container for filtered entities based on specified constraints.
            /// </summary>
            public class Exclude<Exc1> : EntityFilter<Inc1, Inc2> where Exc1 : class, new()
            {
                protected Exclude()
                {
                    ExcludeMask.SetBit(ComponentPool<Exc1>.Instance.TypeIndex, true);
                    ValidateMasks(2, 1);
                }
            }

            /// <summary>
            /// Container for filtered entities based on specified constraints.
            /// </summary>
            public class Exclude<Exc1, Exc2> : EntityFilter<Inc1, Inc2> where Exc1 : class, new() where Exc2 : class, new()
            {
                protected Exclude()
                {
                    ExcludeMask.SetBit(ComponentPool<Exc1>.Instance.TypeIndex, true);
                    ExcludeMask.SetBit(ComponentPool<Exc2>.Instance.TypeIndex, true);
                    ValidateMasks(2, 2);
                }
            }
        }

        public class EntityFilter<Inc1, Inc2, Inc3> : AnyEntityType
    where Inc1 : class, new()
    where Inc2 : class, new()
    where Inc3 : class, new()
        {
            public Inc1[] Components1;
            public Inc2[] Components2;
            public Inc3[] Components3;
            bool _allow1;
            bool _allow2;
            bool _allow3;

            protected EntityFilter()
            {
                _allow1 = !ComponentPool<Inc1>.Instance.IsIgnoreInFilter;
                _allow2 = !ComponentPool<Inc2>.Instance.IsIgnoreInFilter;
                _allow3 = !ComponentPool<Inc3>.Instance.IsIgnoreInFilter;
                Components1 = _allow1 ? new Inc1[MinSize] : null;
                Components2 = _allow2 ? new Inc2[MinSize] : null;
                Components3 = _allow3 ? new Inc3[MinSize] : null;
                IncludeMask.SetBit(ComponentPool<Inc1>.Instance.TypeIndex, true);
                IncludeMask.SetBit(ComponentPool<Inc2>.Instance.TypeIndex, true);
                IncludeMask.SetBit(ComponentPool<Inc3>.Instance.TypeIndex, true);
                AddComponentPool(ComponentPool<Inc1>.Instance);
                AddComponentPool(ComponentPool<Inc2>.Instance);
                AddComponentPool(ComponentPool<Inc3>.Instance);
                ValidateMasks(3, 0);
            }
#if NET_4_6 || NET_STANDARD_2_0
            [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
            public override void RaiseOnAddEvent(int entity)
            {
                if (Entities.Length == _entitiesCount)
                {
                    Array.Resize(ref Entities, _entitiesCount << 1);
                    if (_allow1)
                    {
                        Array.Resize(ref Components1, _entitiesCount << 1);
                    }
                    if (_allow2)
                    {
                        Array.Resize(ref Components2, _entitiesCount << 1);
                    }
                    if (_allow3)
                    {
                        Array.Resize(ref Components3, _entitiesCount << 1);
                    }
                }
                if (_allow1)
                {
                    Components1[_entitiesCount] = _supervisor.GetComponent<Inc1>(entity);
                }
                if (_allow2)
                {
                    Components2[_entitiesCount] = _supervisor.GetComponent<Inc2>(entity);
                }
                if (_allow3)
                {
                    Components3[_entitiesCount] = _supervisor.GetComponent<Inc3>(entity);
                }
                Entities[_entitiesCount++] = entity;
                for (int j = 0, jMax = _listenersCount; j < jMax; j++)
                {
                    _listeners[j].OnEntityAdded(entity);
                }
            }
#if NET_4_6 || NET_STANDARD_2_0
            [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
            public override void RaiseOnRemoveEvent(int entity)
            {
                for (var i = 0; i < _entitiesCount; i++)
                {
                    if (Entities[i] == entity)
                    {
                        _entitiesCount--;
                        Array.Copy(Entities, i + 1, Entities, i, _entitiesCount - i);
                        if (_allow1)
                        {
                            Array.Copy(Components1, i + 1, Components1, i, _entitiesCount - i);
                        }
                        if (_allow2)
                        {
                            Array.Copy(Components2, i + 1, Components2, i, _entitiesCount - i);
                        }
                        if (_allow3)
                        {
                            Array.Copy(Components3, i + 1, Components3, i, _entitiesCount - i);
                        }
                        for (int j = 0, jMax = _listenersCount; j < jMax; j++)
                        {
                            _listeners[j].OnEntityRemoved(entity);
                        }
                        break;
                    }
                }
            }

            /// <summary>
            /// Container for filtered entities based on specified constraints.
            /// </summary>
            public class Exclude<Exc1> : EntityFilter<Inc1, Inc2, Inc3> where Exc1 : class, new()
            {
                protected Exclude()
                {
                    ExcludeMask.SetBit(ComponentPool<Exc1>.Instance.TypeIndex, true);
                    ValidateMasks(3, 1);
                }
            }

            /// <summary>
            /// Container for filtered entities based on specified constraints.
            /// </summary>
            public class Exclude<Exc1, Exc2> : EntityFilter<Inc1, Inc2, Inc3>
                where Exc1 : class, new()
                where Exc2 : class, new()
            {
                protected Exclude()
                {
                    ExcludeMask.SetBit(ComponentPool<Exc1>.Instance.TypeIndex, true);
                    ExcludeMask.SetBit(ComponentPool<Exc2>.Instance.TypeIndex, true);
                    ValidateMasks(3, 2);
                }
            }
        }

        public class EntityFilter<Inc1, Inc2, Inc3, Inc4> : AnyEntityType where Inc1 : class, new() where Inc2 : class, new() where Inc3 : class, new() where Inc4 : class, new()
        {
            public Inc1[] Components1;
            public Inc2[] Components2;
            public Inc3[] Components3;
            public Inc4[] Components4;
            bool _allow1;
            bool _allow2;
            bool _allow3;
            bool _allow4;

            protected EntityFilter()
            {
                _allow1 = !ComponentPool<Inc1>.Instance.IsIgnoreInFilter;
                _allow2 = !ComponentPool<Inc2>.Instance.IsIgnoreInFilter;
                _allow3 = !ComponentPool<Inc3>.Instance.IsIgnoreInFilter;
                _allow4 = !ComponentPool<Inc4>.Instance.IsIgnoreInFilter;
                Components1 = _allow1 ? new Inc1[MinSize] : null;
                Components2 = _allow2 ? new Inc2[MinSize] : null;
                Components3 = _allow3 ? new Inc3[MinSize] : null;
                Components4 = _allow4 ? new Inc4[MinSize] : null;
                IncludeMask.SetBit(ComponentPool<Inc1>.Instance.TypeIndex, true);
                IncludeMask.SetBit(ComponentPool<Inc2>.Instance.TypeIndex, true);
                IncludeMask.SetBit(ComponentPool<Inc3>.Instance.TypeIndex, true);
                IncludeMask.SetBit(ComponentPool<Inc4>.Instance.TypeIndex, true);
                AddComponentPool(ComponentPool<Inc1>.Instance);
                AddComponentPool(ComponentPool<Inc2>.Instance);
                AddComponentPool(ComponentPool<Inc3>.Instance);
                AddComponentPool(ComponentPool<Inc4>.Instance);
                ValidateMasks(4, 0);
            }
#if NET_4_6 || NET_STANDARD_2_0
            [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
            public override void RaiseOnAddEvent(int entity)
            {
                if (Entities.Length == _entitiesCount)
                {
                    Array.Resize(ref Entities, _entitiesCount << 1);
                    if (_allow1)
                    {
                        Array.Resize(ref Components1, _entitiesCount << 1);
                    }
                    if (_allow2)
                    {
                        Array.Resize(ref Components2, _entitiesCount << 1);
                    }
                    if (_allow3)
                    {
                        Array.Resize(ref Components3, _entitiesCount << 1);
                    }
                    if (_allow4)
                    {
                        Array.Resize(ref Components4, _entitiesCount << 1);
                    }
                }
                if (_allow1)
                {
                    Components1[_entitiesCount] = _supervisor.GetComponent<Inc1>(entity);
                }
                if (_allow2)
                {
                    Components2[_entitiesCount] = _supervisor.GetComponent<Inc2>(entity);
                }
                if (_allow3)
                {
                    Components3[_entitiesCount] = _supervisor.GetComponent<Inc3>(entity);
                }
                if (_allow4)
                {
                    Components4[_entitiesCount] = _supervisor.GetComponent<Inc4>(entity);
                }
                Entities[_entitiesCount++] = entity;
                for (int j = 0, jMax = _listenersCount; j < jMax; j++)
                {
                    _listeners[j].OnEntityAdded(entity);
                }
            }
#if NET_4_6 || NET_STANDARD_2_0
            [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
            public override void RaiseOnRemoveEvent(int entity)
            {
                for (var i = 0; i < _entitiesCount; i++)
                {
                    if (Entities[i] == entity)
                    {
                        _entitiesCount--;
                        Array.Copy(Entities, i + 1, Entities, i, _entitiesCount - i);
                        if (_allow1)
                        {
                            Array.Copy(Components1, i + 1, Components1, i, _entitiesCount - i);
                        }
                        if (_allow2)
                        {
                            Array.Copy(Components2, i + 1, Components2, i, _entitiesCount - i);
                        }
                        if (_allow3)
                        {
                            Array.Copy(Components3, i + 1, Components3, i, _entitiesCount - i);
                        }
                        if (_allow4)
                        {
                            Array.Copy(Components4, i + 1, Components4, i, _entitiesCount - i);
                        }
                        for (int j = 0, jMax = _listenersCount; j < jMax; j++)
                        {
                            _listeners[j].OnEntityRemoved(entity);
                        }
                        break;
                    }
                }
            }

            /// <summary>
            /// Container for filtered entities based on specified constraints.
            /// </summary>
            public class Exclude<Exc1> : EntityFilter<Inc1, Inc2, Inc3, Inc4> where Exc1 : class, new()
            {
                protected Exclude()
                {
                    ExcludeMask.SetBit(ComponentPool<Exc1>.Instance.TypeIndex, true);
                    ValidateMasks(4, 1);
                }
            }

            /// <summary>
            /// Container for filtered entities based on specified constraints.
            /// </summary>
            public class Exclude<Exc1, Exc2> : EntityFilter<Inc1, Inc2, Inc3, Inc4> where Exc1 : class, new() where Exc2 : class, new()
            {
                protected Exclude()
                {
                    ExcludeMask.SetBit(ComponentPool<Exc1>.Instance.TypeIndex, true);
                    ExcludeMask.SetBit(ComponentPool<Exc2>.Instance.TypeIndex, true);
                    ValidateMasks(4, 2);
                }
            }
        }
    }

#if ENABLE_IL2CPP
    [Unity.IL2CPP.CompilerServices.Il2CppSetOption (Unity.IL2CPP.CompilerServices.Option.NullChecks, false)]
    [Unity.IL2CPP.CompilerServices.Il2CppSetOption (Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false)]
    [UnityEngine.Scripting.Preserve]
#endif
    public abstract class SpecificEntityType<T> : EntityFilter
        where T : IEntityType
    {
        protected SpecificEntityType()
        {
            EntityTypeIndex = EntityType<T>.Instance.TypeIndex;
        }


        /// <summary>
        /// Container for filtered entities based on specified constraints.
        /// </summary>

        public class EntityFilter<Inc1> : SpecificEntityType<T>
            where Inc1 : class, new()
        {
            public Inc1[] Components1;
            bool _allow1;

            protected EntityFilter()
            {
                _allow1 = !ComponentPool<Inc1>.Instance.IsIgnoreInFilter;
                Components1 = _allow1 ? new Inc1[MinSize] : null;
                IncludeMask.SetBit(ComponentPool<Inc1>.Instance.TypeIndex, true);
                AddComponentPool(ComponentPool<Inc1>.Instance);
            }


#if NET_4_6 || NET_STANDARD_2_0
            [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
            public override void RaiseOnAddEvent(int entity)
            {
                if (Entities.Length == _entitiesCount)
                {
                    Array.Resize(ref Entities, _entitiesCount << 1);
                    if (_allow1)
                    {
                        Array.Resize(ref Components1, _entitiesCount << 1);
                    }
                }
                if (_allow1)
                {
                    Components1[_entitiesCount] = _supervisor.GetComponent<Inc1>(entity);
                }
                Entities[_entitiesCount++] = entity;
                for (int j = 0, jMax = _listenersCount; j < jMax; j++)
                {
                    _listeners[j].OnEntityAdded(entity);
                }
            }
#if NET_4_6 || NET_STANDARD_2_0
            [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
            public override void RaiseOnRemoveEvent(int entity)
            {
                for (var i = 0; i < _entitiesCount; i++)
                {
                    if (Entities[i] == entity)
                    {
                        _entitiesCount--;
                        Array.Copy(Entities, i + 1, Entities, i, _entitiesCount - i);
                        if (_allow1)
                        {
                            Array.Copy(Components1, i + 1, Components1, i, _entitiesCount - i);
                        }
                        for (int j = 0, jMax = _listenersCount; j < jMax; j++)
                        {
                            _listeners[j].OnEntityRemoved(entity);
                        }
                        break;
                    }
                }
            }

            /// <summary>
            /// Container for filtered entities based on specified constraints.
            /// </summary>
            public class Exclude<Exc1> : EntityFilter<Inc1> where Exc1 : class, new()
            {
                protected Exclude()
                {
                    ExcludeMask.SetBit(ComponentPool<Exc1>.Instance.TypeIndex, true);
                    ValidateMasks(1, 1);
                }
            }

            /// <summary>
            /// Container for filtered entities based on specified constraints.
            /// </summary>
            public class Exclude<Exc1, Exc2> : EntityFilter<Inc1> where Exc1 : class, new() where Exc2 : class, new()
            {
                protected Exclude()
                {
                    ExcludeMask.SetBit(ComponentPool<Exc1>.Instance.TypeIndex, true);
                    ExcludeMask.SetBit(ComponentPool<Exc2>.Instance.TypeIndex, true);
                    ValidateMasks(1, 2);
                }
            }
        }
        /// <summary>
        /// Container for filtered entities based on specified constraints.
        /// </summary>
        public class EntityFilter<Inc1, Inc2> : SpecificEntityType<T> where Inc1 : class, new() where Inc2 : class, new()
        {
            public Inc1[] Components1;
            public Inc2[] Components2;
            bool _allow1;
            bool _allow2;

            protected EntityFilter()
            {
                _allow1 = !ComponentPool<Inc1>.Instance.IsIgnoreInFilter;
                _allow2 = !ComponentPool<Inc2>.Instance.IsIgnoreInFilter;
                Components1 = _allow1 ? new Inc1[MinSize] : null;
                Components2 = _allow2 ? new Inc2[MinSize] : null;
                IncludeMask.SetBit(ComponentPool<Inc1>.Instance.TypeIndex, true);
                IncludeMask.SetBit(ComponentPool<Inc2>.Instance.TypeIndex, true);
                AddComponentPool(ComponentPool<Inc1>.Instance);
                AddComponentPool(ComponentPool<Inc2>.Instance);
                ValidateMasks(2, 0);
            }
#if NET_4_6 || NET_STANDARD_2_0
            [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
            public override void RaiseOnAddEvent(int entity)
            {
                if (Entities.Length == _entitiesCount)
                {
                    Array.Resize(ref Entities, _entitiesCount << 1);
                    if (_allow1)
                    {
                        Array.Resize(ref Components1, _entitiesCount << 1);
                    }
                    if (_allow2)
                    {
                        Array.Resize(ref Components2, _entitiesCount << 1);
                    }
                }
                if (_allow1)
                {
                    Components1[_entitiesCount] = _supervisor.GetComponent<Inc1>(entity);
                }
                if (_allow2)
                {
                    Components2[_entitiesCount] = _supervisor.GetComponent<Inc2>(entity);
                }
                Entities[_entitiesCount++] = entity;
                for (int j = 0, jMax = _listenersCount; j < jMax; j++)
                {
                    _listeners[j].OnEntityAdded(entity);
                }
            }
#if NET_4_6 || NET_STANDARD_2_0
            [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
            public override void RaiseOnRemoveEvent(int entity)
            {
                for (var i = 0; i < _entitiesCount; i++)
                {
                    if (Entities[i] == entity)
                    {
                        _entitiesCount--;
                        Array.Copy(Entities, i + 1, Entities, i, _entitiesCount - i);
                        if (_allow1)
                        {
                            Array.Copy(Components1, i + 1, Components1, i, _entitiesCount - i);
                        }
                        if (_allow2)
                        {
                            Array.Copy(Components2, i + 1, Components2, i, _entitiesCount - i);
                        }
                        for (int j = 0, jMax = _listenersCount; j < jMax; j++)
                        {
                            _listeners[j].OnEntityRemoved(entity);
                        }
                        break;
                    }
                }
            }

            /// <summary>
            /// Container for filtered entities based on specified constraints.
            /// </summary>
            public class Exclude<Exc1> : EntityFilter<Inc1, Inc2> where Exc1 : class, new()
            {
                protected Exclude()
                {
                    ExcludeMask.SetBit(ComponentPool<Exc1>.Instance.TypeIndex, true);
                    ValidateMasks(2, 1);
                }
            }

            /// <summary>
            /// Container for filtered entities based on specified constraints.
            /// </summary>
            public class Exclude<Exc1, Exc2> : EntityFilter<Inc1, Inc2> where Exc1 : class, new() where Exc2 : class, new()
            {
                protected Exclude()
                {
                    ExcludeMask.SetBit(ComponentPool<Exc1>.Instance.TypeIndex, true);
                    ExcludeMask.SetBit(ComponentPool<Exc2>.Instance.TypeIndex, true);
                    ValidateMasks(2, 2);
                }
            }
        }
        public class EntityFilter<Inc1, Inc2, Inc3> : SpecificEntityType<T>
where Inc1 : class, new()
where Inc2 : class, new()
where Inc3 : class, new()
        {
            public Inc1[] Components1;
            public Inc2[] Components2;
            public Inc3[] Components3;
            bool _allow1;
            bool _allow2;
            bool _allow3;

            protected EntityFilter()
            {
                _allow1 = !ComponentPool<Inc1>.Instance.IsIgnoreInFilter;
                _allow2 = !ComponentPool<Inc2>.Instance.IsIgnoreInFilter;
                _allow3 = !ComponentPool<Inc3>.Instance.IsIgnoreInFilter;
                Components1 = _allow1 ? new Inc1[MinSize] : null;
                Components2 = _allow2 ? new Inc2[MinSize] : null;
                Components3 = _allow3 ? new Inc3[MinSize] : null;
                IncludeMask.SetBit(ComponentPool<Inc1>.Instance.TypeIndex, true);
                IncludeMask.SetBit(ComponentPool<Inc2>.Instance.TypeIndex, true);
                IncludeMask.SetBit(ComponentPool<Inc3>.Instance.TypeIndex, true);
                AddComponentPool(ComponentPool<Inc1>.Instance);
                AddComponentPool(ComponentPool<Inc2>.Instance);
                AddComponentPool(ComponentPool<Inc3>.Instance);
                ValidateMasks(3, 0);
            }
#if NET_4_6 || NET_STANDARD_2_0
            [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
            public override void RaiseOnAddEvent(int entity)
            {
                if (Entities.Length == _entitiesCount)
                {
                    Array.Resize(ref Entities, _entitiesCount << 1);
                    if (_allow1)
                    {
                        Array.Resize(ref Components1, _entitiesCount << 1);
                    }
                    if (_allow2)
                    {
                        Array.Resize(ref Components2, _entitiesCount << 1);
                    }
                    if (_allow3)
                    {
                        Array.Resize(ref Components3, _entitiesCount << 1);
                    }
                }
                if (_allow1)
                {
                    Components1[_entitiesCount] = _supervisor.GetComponent<Inc1>(entity);
                }
                if (_allow2)
                {
                    Components2[_entitiesCount] = _supervisor.GetComponent<Inc2>(entity);
                }
                if (_allow3)
                {
                    Components3[_entitiesCount] = _supervisor.GetComponent<Inc3>(entity);
                }
                Entities[_entitiesCount++] = entity;
                for (int j = 0, jMax = _listenersCount; j < jMax; j++)
                {
                    _listeners[j].OnEntityAdded(entity);
                }
            }
#if NET_4_6 || NET_STANDARD_2_0
            [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
            public override void RaiseOnRemoveEvent(int entity)
            {
                for (var i = 0; i < _entitiesCount; i++)
                {
                    if (Entities[i] == entity)
                    {
                        _entitiesCount--;
                        Array.Copy(Entities, i + 1, Entities, i, _entitiesCount - i);
                        if (_allow1)
                        {
                            Array.Copy(Components1, i + 1, Components1, i, _entitiesCount - i);
                        }
                        if (_allow2)
                        {
                            Array.Copy(Components2, i + 1, Components2, i, _entitiesCount - i);
                        }
                        if (_allow3)
                        {
                            Array.Copy(Components3, i + 1, Components3, i, _entitiesCount - i);
                        }
                        for (int j = 0, jMax = _listenersCount; j < jMax; j++)
                        {
                            _listeners[j].OnEntityRemoved(entity);
                        }
                        break;
                    }
                }
            }

            /// <summary>
            /// Container for filtered entities based on specified constraints.
            /// </summary>
            public class Exclude<Exc1> : EntityFilter<Inc1, Inc2, Inc3> where Exc1 : class, new()
            {
                protected Exclude()
                {
                    ExcludeMask.SetBit(ComponentPool<Exc1>.Instance.TypeIndex, true);
                    ValidateMasks(3, 1);
                }
            }

            /// <summary>
            /// Container for filtered entities based on specified constraints.
            /// </summary>
            public class Exclude<Exc1, Exc2> : EntityFilter<Inc1, Inc2, Inc3>
                where Exc1 : class, new()
                where Exc2 : class, new()
            {
                protected Exclude()
                {
                    ExcludeMask.SetBit(ComponentPool<Exc1>.Instance.TypeIndex, true);
                    ExcludeMask.SetBit(ComponentPool<Exc2>.Instance.TypeIndex, true);
                    ValidateMasks(3, 2);
                }
            }
        }
        public class EntityFilter<Inc1, Inc2, Inc3, Inc4> : SpecificEntityType<T> where Inc1 : class, new() where Inc2 : class, new() where Inc3 : class, new() where Inc4 : class, new()
        {
            public Inc1[] Components1;
            public Inc2[] Components2;
            public Inc3[] Components3;
            public Inc4[] Components4;
            bool _allow1;
            bool _allow2;
            bool _allow3;
            bool _allow4;

            protected EntityFilter()
            {
                _allow1 = !ComponentPool<Inc1>.Instance.IsIgnoreInFilter;
                _allow2 = !ComponentPool<Inc2>.Instance.IsIgnoreInFilter;
                _allow3 = !ComponentPool<Inc3>.Instance.IsIgnoreInFilter;
                _allow4 = !ComponentPool<Inc4>.Instance.IsIgnoreInFilter;
                Components1 = _allow1 ? new Inc1[MinSize] : null;
                Components2 = _allow2 ? new Inc2[MinSize] : null;
                Components3 = _allow3 ? new Inc3[MinSize] : null;
                Components4 = _allow4 ? new Inc4[MinSize] : null;
                IncludeMask.SetBit(ComponentPool<Inc1>.Instance.TypeIndex, true);
                IncludeMask.SetBit(ComponentPool<Inc2>.Instance.TypeIndex, true);
                IncludeMask.SetBit(ComponentPool<Inc3>.Instance.TypeIndex, true);
                IncludeMask.SetBit(ComponentPool<Inc4>.Instance.TypeIndex, true);
                AddComponentPool(ComponentPool<Inc1>.Instance);
                AddComponentPool(ComponentPool<Inc2>.Instance);
                AddComponentPool(ComponentPool<Inc3>.Instance);
                AddComponentPool(ComponentPool<Inc4>.Instance);
                ValidateMasks(4, 0);
            }
#if NET_4_6 || NET_STANDARD_2_0
            [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
            public override void RaiseOnAddEvent(int entity)
            {
                if (Entities.Length == _entitiesCount)
                {
                    Array.Resize(ref Entities, _entitiesCount << 1);
                    if (_allow1)
                    {
                        Array.Resize(ref Components1, _entitiesCount << 1);
                    }
                    if (_allow2)
                    {
                        Array.Resize(ref Components2, _entitiesCount << 1);
                    }
                    if (_allow3)
                    {
                        Array.Resize(ref Components3, _entitiesCount << 1);
                    }
                    if (_allow4)
                    {
                        Array.Resize(ref Components4, _entitiesCount << 1);
                    }
                }
                if (_allow1)
                {
                    Components1[_entitiesCount] = _supervisor.GetComponent<Inc1>(entity);
                }
                if (_allow2)
                {
                    Components2[_entitiesCount] = _supervisor.GetComponent<Inc2>(entity);
                }
                if (_allow3)
                {
                    Components3[_entitiesCount] = _supervisor.GetComponent<Inc3>(entity);
                }
                if (_allow4)
                {
                    Components4[_entitiesCount] = _supervisor.GetComponent<Inc4>(entity);
                }
                Entities[_entitiesCount++] = entity;
                for (int j = 0, jMax = _listenersCount; j < jMax; j++)
                {
                    _listeners[j].OnEntityAdded(entity);
                }
            }
#if NET_4_6 || NET_STANDARD_2_0
            [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
            public override void RaiseOnRemoveEvent(int entity)
            {
                for (var i = 0; i < _entitiesCount; i++)
                {
                    if (Entities[i] == entity)
                    {
                        _entitiesCount--;
                        Array.Copy(Entities, i + 1, Entities, i, _entitiesCount - i);
                        if (_allow1)
                        {
                            Array.Copy(Components1, i + 1, Components1, i, _entitiesCount - i);
                        }
                        if (_allow2)
                        {
                            Array.Copy(Components2, i + 1, Components2, i, _entitiesCount - i);
                        }
                        if (_allow3)
                        {
                            Array.Copy(Components3, i + 1, Components3, i, _entitiesCount - i);
                        }
                        if (_allow4)
                        {
                            Array.Copy(Components4, i + 1, Components4, i, _entitiesCount - i);
                        }
                        for (int j = 0, jMax = _listenersCount; j < jMax; j++)
                        {
                            _listeners[j].OnEntityRemoved(entity);
                        }
                        break;
                    }
                }
            }

            /// <summary>
            /// Container for filtered entities based on specified constraints.
            /// </summary>
            public class Exclude<Exc1> : EntityFilter<Inc1, Inc2, Inc3, Inc4> where Exc1 : class, new()
            {
                protected Exclude()
                {
                    ExcludeMask.SetBit(ComponentPool<Exc1>.Instance.TypeIndex, true);
                    ValidateMasks(4, 1);
                }
            }

            /// <summary>
            /// Container for filtered entities based on specified constraints.
            /// </summary>
            public class Exclude<Exc1, Exc2> : EntityFilter<Inc1, Inc2, Inc3, Inc4> where Exc1 : class, new() where Exc2 : class, new()
            {
                protected Exclude()
                {
                    ExcludeMask.SetBit(ComponentPool<Exc1>.Instance.TypeIndex, true);
                    ExcludeMask.SetBit(ComponentPool<Exc2>.Instance.TypeIndex, true);
                    ValidateMasks(4, 2);
                }
            }
        }
    }
}
