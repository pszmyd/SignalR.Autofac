using Autofac;

namespace SignalR.Autofac.Tests
{
    public class TestModule : Module
    {
        protected override void Load(ContainerBuilder moduleBuilder)
        {
            moduleBuilder.RegisterDependencyResolver().InstancePerMatchingLifetimeScope("shell");
        }
    }
}