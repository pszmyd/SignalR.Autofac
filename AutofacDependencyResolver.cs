using System;
using System.Collections.Generic;
using Autofac.Core;
using Autofac.Core.Activators.ProvidedInstance;
using Autofac.Core.Lifetime;
using Autofac.Core.Registration;

namespace SignalR.Autofac
{
	using global::Autofac;

    /// <summary>
    /// SingalR dependency resolver using Autofac container as backend.
    /// </summary>
    public class AutofacDependencyResolver : DefaultDependencyResolver, IRegistrationSource
    {
		private readonly ILifetimeScope _lifetimeScope;

		public AutofacDependencyResolver(ILifetimeScope lifetimeScope)
		{
			_lifetimeScope = lifetimeScope;
			_lifetimeScope.ComponentRegistry.AddRegistrationSource(this);
		}

		public override object GetService(Type serviceType)
		{
			object result;
			_lifetimeScope.TryResolve(serviceType, out result);
			return result;
		}

		public override IEnumerable<object> GetServices(Type serviceType)
		{
			object result;
			_lifetimeScope.TryResolve(serviceType.MakeArrayType(), out result);
			return (IEnumerable<object>)result;
		}

		IEnumerable<IComponentRegistration> IRegistrationSource.RegistrationsFor(Service service, Func<Service, IEnumerable<IComponentRegistration>> registrationAccessor)
		{

			var result = new IComponentRegistration[] { };
			var serviceType = service as TypedService;
			if (serviceType != null)
			{
				var instance = base.GetService(serviceType.ServiceType);
				if (instance != null)
				{					
					result = new IComponentRegistration[]
					         	{
					         		new ComponentRegistration(
					         			Guid.NewGuid(),
					         			new ProvidedInstanceActivator(instance),
					         			new CurrentScopeLifetime(), 
					         			InstanceSharing.Shared,
					         			InstanceOwnership.ExternallyOwned,
					         			new[] {service},
					         			new Dictionary<string, object>())
					         	};
				}				
			}

			return result;
		}

		bool IRegistrationSource.IsAdapterForIndividualComponents
		{
			get { return false; }
		}
    }
}
