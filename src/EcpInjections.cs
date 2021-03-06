﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Jtfer.Ecp
{
    /// <summary>
    /// Attribute for automatic DI injection at fields of system.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class EcpInjectAttribute : Attribute { }

    /// <summary>
    /// Attribute for ignore automatic DI injection to field.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class EcpInjectChildAttribute : Attribute { }

    /// <summary>
    /// Attribute for ignore automatic DI injection to field.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class EcpIgnoreInjectAttribute : Attribute { }

    /// <summary>
    /// Processes dependency injection to ecs systems.
    /// </summary>
#if ENABLE_IL2CPP
    [Unity.IL2CPP.CompilerServices.Il2CppSetOption (Unity.IL2CPP.CompilerServices.Option.NullChecks, false)]
    [Unity.IL2CPP.CompilerServices.Il2CppSetOption (Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false)]
#endif
    public static class EcpInjections
    {
        /// <summary>
        /// Injects EcsWorld / EcsFilter fields to IEcsSystem.
        /// </summary>
        /// <param name="system">System to scan for injection.</param>
        /// <param name="world">EcsWorld instance to inject.</param>
        public static void Inject(IPipelineOperation system, Domain domain, EntityManager entityManager, System.Collections.Generic.Dictionary<Type, object> injections)
        {
            var systemType = system.GetType();
            if (!Attribute.IsDefined(systemType, typeof(EcpInjectAttribute)))
            {
                return;
            }
            var worldType = domain.GetType();
            var entityManagerType = entityManager.GetType();
            var supervisor = domain.GetSupervisor();
            var containerPool = domain.GetContainerPool();
            var containerType = typeof(IContainer);
            var contextType = typeof(PipelineContext);
            var supervisorType = typeof(EntitySupervisor);
            var filterType = typeof(EntityFilter);
            var ignoreType = typeof(EcpIgnoreInjectAttribute);
            var injectChildType = typeof(EcpInjectChildAttribute);

            foreach (var f in systemType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                // skip fields with [EcsIgnoreInject] attribute.
                if (Attribute.IsDefined(f, ignoreType))
                {
                    continue;
                }
                var injectChild = false;
                if (Attribute.IsDefined(f, injectChildType))
                {
                    injectChild = true;
                }

                // EntityManager
                if (f.FieldType.IsAssignableFrom(entityManagerType) && !f.IsStatic)
                {
                    f.SetValue(system, entityManager);
                    continue;
                }

                // IContainer
                if (containerType.IsAssignableFrom(f.FieldType) && !f.IsStatic)
                {
                    f.SetValue(system, containerPool.GetContainer(f.FieldType, injectChild));
                    continue;
                }

                // PipelineContext
                if (f.FieldType.IsSubclassOf(contextType) && !f.IsStatic)
                {
                    f.SetValue(system, domain.GetPipelineContext(f.FieldType));
                    continue;
                }

                // EcsFilter
#if DEBUG
                if (f.FieldType == filterType)
                {
                    throw new Exception(string.Format("Cant use EcsFilter type at \"{0}\" system for dependency injection, use generic version instead", system));
                }
#endif
                if (f.FieldType.IsSubclassOf(filterType) && !f.IsStatic)
                {
                    
                    f.SetValue(system, supervisor.GetFilter(f.FieldType));
                    continue;
                }
                // Other injections.
                foreach (var pair in injections)
                {
                    if (f.FieldType.IsAssignableFrom(pair.Key) && !f.IsStatic)
                    {
                        f.SetValue(system, pair.Value);
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Injects EcsWorld / EcsFilter fields to IEcsSystem.
        /// </summary>
        /// <param name="container">System to scan for injection.</param>
        /// <param name="world">EcsWorld instance to inject.</param>
        public static void Inject(IContainer container, Domain domain, EntityManager entityManager, System.Collections.Generic.Dictionary<Type, object> injections)
        {
            var containerType = container.GetType();
            if (!Attribute.IsDefined(containerType, typeof(EcpInjectAttribute)))
            {
                return;
            }
            var worldType = domain.GetType();
            var entityManagerType = entityManager.GetType();
            var supervisor = domain.GetSupervisor();
            var containerPool = domain.GetContainerPool();
            var baseContainerType = typeof(IContainer);
            var contextType = typeof(PipelineContext);
            var supervisorType = typeof(EntitySupervisor);
            var filterType = typeof(EntityFilter);
            var ignoreType = typeof(EcpIgnoreInjectAttribute);
            var injectChildType = typeof(EcpInjectChildAttribute);

            var allFields = GetAllFields(containerType);
            foreach (var f in allFields)
            {
                // skip fields with [EcsIgnoreInject] attribute.
                if (Attribute.IsDefined(f, ignoreType))
                {
                    continue;
                }

                var injectChild = false;
                if(Attribute.IsDefined(f, injectChildType))
                {
                    injectChild = true;
                }

                // EntityManager
                if (f.FieldType.IsAssignableFrom(entityManagerType) && !f.IsStatic)
                {
                    f.SetValue(container, entityManager);
                    continue;
                }

                // IContainer
                if (baseContainerType.IsAssignableFrom(f.FieldType) && !f.IsStatic)
                {
                    f.SetValue(container, containerPool.GetContainer(f.FieldType, injectChild));
                    continue;
                }

                // PipelineContext
                if (f.FieldType.IsSubclassOf(contextType) && !f.IsStatic)
                {
                    f.SetValue(container, domain.GetPipelineContext(f.FieldType));
                    continue;
                }

                // EcsFilter
#if DEBUG
                if (f.FieldType == filterType)
                {
                    throw new Exception(string.Format("Cant use EcsFilter type at \"{0}\" system for dependency injection, use generic version instead", container));
                }
#endif
                if (f.FieldType.IsSubclassOf(filterType) && !f.IsStatic)
                {

                    f.SetValue(container, supervisor.GetFilter(f.FieldType));
                    continue;
                }
                // Other injections.
                foreach (var pair in injections)
                {
                    if (f.FieldType.IsAssignableFrom(pair.Key) && !f.IsStatic)
                    {
                        f.SetValue(container, pair.Value);
                        break;
                    }
                }
            }
        }

        private static IEnumerable<FieldInfo> GetAllFields(Type t)
        {
            if (t == null)
                return Enumerable.Empty<FieldInfo>();

            BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic |
                                 BindingFlags.Instance |
                                 BindingFlags.DeclaredOnly;
            return t.GetFields(flags).Concat(GetAllFields(t.BaseType));
        }
    }
}
