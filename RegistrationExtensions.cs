using System;
using Autofac;
using Autofac.Builder;
using Microsoft.AspNet.SignalR;

namespace SignalR.Autofac
{
    public static class RegistrationExtensions
    {
        public static IRegistrationBuilder<AutofacDependencyResolver, SimpleActivatorData, SingleRegistrationStyle> RegisterDependencyResolver(this ContainerBuilder builder)
        {
            var rb = GetBuilder();
            builder.RegisterCallback(cr => 
            { 
                var source = new AutofacDependencyResolver();
                var registration = rb.CreateRegistration();
                cr.Register(registration);
                cr.AddRegistrationSource(source);
            });

            return rb;
        }

        private static IRegistrationBuilder<AutofacDependencyResolver, SimpleActivatorData, SingleRegistrationStyle> GetBuilder()
        {
            return RegistrationBuilder.ForDelegate((ctx, param) => new AutofacDependencyResolver(ctx.Resolve<ILifetimeScope>()))
                .As<IDependencyResolver>()
                .InstancePerLifetimeScope();
        }

    }
}
