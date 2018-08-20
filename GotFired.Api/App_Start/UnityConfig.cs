                                                    using GotFired.Business;
using GotFired.Model;
using Microsoft.Practices.Unity;
using System.Web.Http;
using Unity.WebApi;

namespace GotFired.Api
{
    public static class UnityConfig
    {
        public static void RegisterComponents()
        {
			var container = new UnityContainer();

            // register all your components with the container here
            // it is NOT necessary to register your controllers

            container.RegisterType<IDismissalCaseBusiness, DismissalCaseBusiness>(new HierarchicalLifetimeManager());


            GlobalConfiguration.Configuration.DependencyResolver = new UnityDependencyResolver(container);
        }
    }
}