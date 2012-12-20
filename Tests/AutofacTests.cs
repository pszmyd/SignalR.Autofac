using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using Autofac.Builder;
using Autofac.Core;
using Autofac.Features.Indexed;
using Microsoft.AspNet.SignalR;
using NUnit.Framework;

namespace SignalR.Autofac.Tests
{
    [TestFixture]
    public class AutofacTests
    {
        [Test]
        public void DependenciesShouldGetResolvedInNamedScope()
        {
            var rootBuilder = new ContainerBuilder();
            rootBuilder.RegisterDependencyResolver().InstancePerMatchingLifetimeScope("shell");
            var rootContainer = rootBuilder.Build();

            var scope = rootContainer.BeginLifetimeScope("shell");
            var connMgr = scope.Resolve<IConnectionManager>();

            Assert.That(connMgr, Is.Not.Null);

            var container = scope.Resolve<IDependencyResolver>();

            Assert.That(container, Is.Not.Null);
        }

        [Test]
        public void DependenciesShouldGetResolvedWhenRegisteredFromModule()
        {
            var rootBuilder = new ContainerBuilder();
            var rootContainer = rootBuilder.Build();
            var intermediateScope = rootContainer.BeginLifetimeScope(builder => builder
                .RegisterType(typeof(TestModule))
                .Keyed<IModule>(typeof(TestModule))
                .InstancePerDependency());

            var scope = intermediateScope.BeginLifetimeScope("shell", builder =>
            {
                var moduleIndex = intermediateScope.Resolve<IIndex<Type, IModule>>();
                builder.RegisterModule(moduleIndex[typeof (TestModule)]);
            });

            var connMgr = scope.Resolve<IConnectionManager>();

            Assert.That(connMgr, Is.Not.Null);

            var container = scope.Resolve<IDependencyResolver>();

            Assert.That(container, Is.Not.Null);
        }
    }
}