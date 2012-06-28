using System;
using System.Collections.Generic;
using System.Linq;
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
			_lifetimeScope.TryResolve(typeof(IEnumerable<>).MakeGenericType(serviceType), out result);
			return (IEnumerable<object>)result;
		}

		public IEnumerable<IComponentRegistration> RegistrationsFor(Service service, Func<Service, IEnumerable<IComponentRegistration>> registrationAccessor)
		{
			var result = new IComponentRegistration[] { };
			var typedService = service as TypedService;
			if (typedService != null)
			{
				object instance;

				var serviceType = typedService.ServiceType;
				if (serviceType.IsGenericType && serviceType.GetGenericTypeDefinition() == typeof(IEnumerable<>))
				{
					serviceType = serviceType.GetGenericArguments()[0];
					instance = base.GetServices(serviceType);
				}
				else
				{
					instance = base.GetService(serviceType);
				}

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
