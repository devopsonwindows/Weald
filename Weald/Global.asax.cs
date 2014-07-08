using System;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

using Castle.Windsor;
using log4net;
using log4net.Config;

using Weald.App_Start;
using Weald.Installers;
using Weald.Service;

namespace Weald
{
    // Adding help documentation - http://www.asp.net/web-api/overview/creating-web-apis/creating-api-help-pages
    public class WebApiApplication : HttpApplication
    {
        public static IProvideVisualSvnServerInfo VisualSvnServerInfoProvider;
        public static IProvideRepositoryInfo RepostioryInfoProvider;

        private static readonly ILog Log = LogManager.GetLogger(typeof (WebApiApplication));
        private static IWindsorContainer _container;

        protected void Application_Start()
        {
            XmlConfigurator.Configure();
            AreaRegistration.RegisterAllAreas();
            Log.Info("Starting up Weald");

            WebApiConfig.Register(GlobalConfiguration.Configuration);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            _container = new WindsorContainer();
            _container.Install(new WealdInstaller());

            VisualSvnServerInfoProvider = _container.Resolve<IProvideVisualSvnServerInfo>();
            RepostioryInfoProvider = _container.Resolve<IProvideRepositoryInfo>();

            if (!VisualSvnServerInfoProvider.IsVisualSvnServerInstalled)
            {
                Log.ErrorFormat("VisualSVN Server is not installed on this computer, {0}. " +
                    "Weald functionality will not be available", Environment.MachineName);
            }

            Log.Info("Finished Weald startup");
        }

        protected void Application_End()
        {
            Log.Info("Shutting down Weald");
            _container.Dispose();
            Log.Info("Finished Weald shutdown");
        }
    }
}