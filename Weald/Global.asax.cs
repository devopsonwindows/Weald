using System;
using System.Threading;
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
        public static IProvideRepositoryInfoCache RepositoryInfoCacheProvider;
        public static string SvnServerHostname = Environment.MachineName;

        private static readonly ILog Log = LogManager.GetLogger(typeof (WebApiApplication));
        private static IWindsorContainer _container;

        private readonly CancellationTokenSource _tokenSource = new CancellationTokenSource();

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

            var visualSvnServerInfoProvider = _container.Resolve<IProvideVisualSvnServerInfo>();
            RepositoryInfoCacheProvider = _container.Resolve<IProvideRepositoryInfoCache>();

            if (!visualSvnServerInfoProvider.IsVisualSvnServerInstalled)
            {
                Log.ErrorFormat("VisualSVN Server is not installed on this computer, {0}. " +
                    "Weald functionality will not be available", Environment.MachineName);
            }
            else
            {
                RepositoryInfoCacheProvider.UpdateAllRepositoryInfos();

                var updateIntervalValue = string.Empty;
                var updateInterval = new TimeSpan();

                try
                {
                    var configProvider = _container.Resolve<IProvideWebConfiguration>();
                    if (!string.IsNullOrEmpty(configProvider.GetValue("SvnServerAlias")))
                    {
                        SvnServerHostname = configProvider.GetValue("SvnServerAlias");
                    }

                    updateIntervalValue = configProvider.GetValue("RepoInfoRefreshInterval");
                    updateInterval = TimeSpan.Parse(updateIntervalValue);
                }
                catch (Exception e)
                {
                    Log.Error(string.Format("The RepoInfoRefreshInterval value {0} in the Web.config is not valid", updateIntervalValue), e);
                }

                _container.Resolve<IRepeatActions>()
                          .StartAction(RepositoryInfoCacheProvider.UpdateAllRepositoryInfos, updateInterval,
                                       _tokenSource.Token);
            }

            Log.Info("Finished Weald startup");
        }

        protected void Application_End()
        {
            Log.Info("Shutting down Weald");

            if (_tokenSource != null)
            {
                _tokenSource.Cancel();
            }

            _container.Dispose();
            Log.Info("Finished Weald shutdown");
        }
    }
}