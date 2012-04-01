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
        private readonly ILifetimeScope scope;
        private ContainerBuilder builder = new ContainerBuilder();

        public AutofacDependencyResolver(ILifetimeScope scope)
        {
            this.scope = scope;
            builder.Update(scope.ComponentRegistry);
            builder = null;
        }

        public override void Register(Type serviceType, Func<object> activator)
        {
            if (scope != null) builder = new ContainerBuilder();

            RegisterInContainer(this.builder, serviceType, activator);

            if (scope != null) builder.Update(scope.ComponentRegistry);
        }

        public override void Register(Type serviceType, IEnumerable<Func<object>> activators)
        {
            if (scope != null) builder = new ContainerBuilder();

            foreach (var activator in activators)
            {
                RegisterInContainer(this.builder, serviceType, activator);
            }

            if (scope != null) builder.Update(scope.ComponentRegistry);
        }

        public override object GetService(Type serviceType)
        {
            object instance;
            return scope.TryResolve(serviceType, out instance) ? instance : null;
        }

        public override IEnumerable<object> GetServices(Type serviceType)
        {
            object instance;
            return scope.TryResolve(typeof(IEnumerable<>).MakeGenericType(serviceType), out instance) ? instance as IEnumerable<object> : null;
        }

        private static void RegisterGeneric<T>(ContainerBuilder builder, Type type, Func<object> obj)
        {
            builder.Register(context => (T)obj()).As(type).PreserveExistingDefaults().InstancePerLifetimeScope();
        }

        private static void RegisterInContainer(ContainerBuilder builder, Type type, Func<object> obj)
        {
            var method = typeof(AutofacDependencyResolver).GetMethod("RegisterGeneric", BindingFlags.Static | BindingFlags.NonPublic).MakeGenericMethod(type);
            var delegateType = Expression.GetActionType(method.GetParameters().Select(p => p.ParameterType).ToArray());
            Delegate.CreateDelegate(delegateType, method).DynamicInvoke(builder, type, obj);
        }
    }
}
