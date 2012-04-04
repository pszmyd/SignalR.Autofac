using System;
using System.Collections.Generic;

namespace SignalR.Autofac
{
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;

    using global::Autofac;

    /// <summary>
    /// SingalR dependency resolver using Autofac container as backend.
    /// </summary>
    public class AutofacDependencyResolver : DefaultDependencyResolver
    {
        private ContainerBuilder _builder = new ContainerBuilder();
        private readonly IDictionary<Type, Func<object>> _registrations = new Dictionary<Type, Func<object>>();

        private bool dirty = true;

        public AutofacDependencyResolver(ILifetimeScope scope)
        {
            Scope = scope;
            FlushRegistryCache();
        }

        public ILifetimeScope Scope { get; set; }

        private void FlushRegistryCache()
        {
            _builder = new ContainerBuilder();
            foreach (var kv in _registrations)
            {
                RegisterInContainer(_builder, kv.Key, kv.Value, Scope);
            }
            _builder.Update(Scope.ComponentRegistry);
            _registrations.Clear();
            dirty = false;
        }

        public override void Register(Type serviceType, Func<object> activator)
        {
            if (Scope == null)
                _registrations.Add(serviceType, activator);
            else
            {
                if(dirty) this.FlushRegistryCache();
                _builder = new ContainerBuilder();
                RegisterInContainer(_builder, serviceType, activator, Scope);
                _builder.Update(Scope.ComponentRegistry);
            }

        }

        public override void Register(Type serviceType, IEnumerable<Func<object>> activators)
        {
            if (Scope == null)
                _registrations.Add(serviceType, activators.Last());
            else
            {
                if (dirty) this.FlushRegistryCache();
                _builder = new ContainerBuilder();
                foreach (var activator in activators)
                {
                    RegisterInContainer(_builder, serviceType, activator, Scope);
                }
                _builder.Update(Scope.ComponentRegistry);
            }
        }

        public override object GetService(Type serviceType)
        {
            Func<object> factory;
            if (Scope == null) return _registrations.TryGetValue(serviceType, out factory) ? factory() : null;

            if (dirty) this.FlushRegistryCache();
            object instance;
            return Scope.TryResolve(serviceType, out instance) ? instance : null;
        }

        public override IEnumerable<object> GetServices(Type serviceType)
        {
            Func<object> factory;
            if (Scope == null) return _registrations.TryGetValue(serviceType, out factory) ? new List<object> { factory() } : null;

            if (dirty) this.FlushRegistryCache();
            object instance;
            return Scope.TryResolve(typeof(IEnumerable<>).MakeGenericType(serviceType), out instance) ? instance as IEnumerable<object> : null;
        }

        private static void RegisterGeneric<T>(ContainerBuilder builder, Type type, Func<object> obj, ILifetimeScope scope)
        {
            builder.Register(context => (T)obj()).As(type).PreserveExistingDefaults().
                InstancePerLifetimeScope().
                InstancePerMatchingLifetimeScope("shell");
        }

        private static void RegisterInContainer(ContainerBuilder builder, Type type, Func<object> obj, ILifetimeScope scope)
        {
            var method = typeof(AutofacDependencyResolver).GetMethod("RegisterGeneric", BindingFlags.Static | BindingFlags.NonPublic).MakeGenericMethod(type);
            var delegateType = Expression.GetActionType(method.GetParameters().Select(p => p.ParameterType).ToArray());
            Delegate.CreateDelegate(delegateType, method).DynamicInvoke(builder, type, obj, scope);
        }
    }
}
