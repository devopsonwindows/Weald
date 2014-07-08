using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;

using Weald.Service;

namespace Weald.Installers
{
    public class WealdInstaller : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container.Register(
                Component.For<IProvideWindowsServiceInfo>().ImplementedBy<WindowsServiceInfoProvider>(),
                Component.For<IProvideVisualSvnServerPaths>().ImplementedBy<VisualSvnServerPathProvider>(),
                Component.For<IProvideVisualSvnServerInfo>().ImplementedBy<VisualSvnServerInfoProvider>(),
                Component.For<IProvideSvnLook>().ImplementedBy<SvnLookProvider>(),
                Component.For<IProvideRepositoryInfo>().ImplementedBy<RepositoryInfoProvider>(),
                Component.For<IProvideWebConfiguration>().ImplementedBy<WebConfigurationProvider>()
                );
        }
    }
}