using System;
using System.IO;

using log4net;

namespace Weald.Service
{
    public interface IProvideVisualSvnServerPaths
    {
        string ServiceBinDirectory { get; }
        string ServiceConfigFilePath { get; }
        string ServiceExePath { get; }
    }

    public class VisualSvnServerPathProvider : IProvideVisualSvnServerPaths
    {
        public const string VisualSvnServiceName = "VisualSVNServer";

        private static readonly ILog Log = LogManager.GetLogger(typeof(VisualSvnServerInfoProvider));

        public VisualSvnServerPathProvider(IProvideWindowsServiceInfo windowsServiceInfoProvider)
        {
            try
            {
                ServiceExePath = windowsServiceInfoProvider.GetServiceExePath(VisualSvnServiceName);
                ServiceBinDirectory = GetSvnServerBinDirectory();
                ServiceConfigFilePath = GetSvnServerConfigFilePath();
            }
            catch (ArgumentException e)
            {
                Log.Error(string.Format("Could not find the {0} service on this computer. Are you sure it's installed?", VisualSvnServiceName), e);
            }
        }

        private string GetSvnServerConfigFilePath()
        {
            return Path.GetFullPath(Path.Combine(GetSvnServerBinDirectory(), "..", "conf", "httpd.conf"));
        }

        private string GetSvnServerBinDirectory()
        {
            var svnServerBinDirectory = Path.GetDirectoryName(ServiceExePath);

            if (string.IsNullOrEmpty(svnServerBinDirectory))
            {
                throw new ArgumentException(string.Format(
                    "Invalid path to VisualSVN Server's bin directory, '{0}'", svnServerBinDirectory));
            }

            return svnServerBinDirectory;
        }

        public string ServiceBinDirectory { get; private set; }
        public string ServiceConfigFilePath { get; private set; }
        public string ServiceExePath { get; private set; }
    }
}