using System;
using System.IO;
using System.Text.RegularExpressions;

using log4net;

namespace Weald.Service
{
    public interface IProvideVisualSvnServerInfo
    {
        string BaseRepoUrl { get; }
        bool IsVisualSvnServerInstalled { get; }
        string RepoStoragePath { get; }
        string SvnLookExePath { get; }
    }

    public class VisualSvnServerInfoProvider : IProvideVisualSvnServerInfo
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof (VisualSvnServerInfoProvider));

        private readonly string _configFilePath;

        public VisualSvnServerInfoProvider(IProvideVisualSvnServerPaths svnServicePathsProvider,
                                           IProvideWebConfiguration webConfigurationProvider)
        {
            try
            {
                if (!File.Exists(svnServicePathsProvider.ServiceExePath))
                {
                    throw new ArgumentException("The VisualSVN Server executable could not be found at '{0}'",
                                                svnServicePathsProvider.ServiceExePath);
                }

                _configFilePath = svnServicePathsProvider.ServiceConfigFilePath;

                IsVisualSvnServerInstalled = true;
                SvnLookExePath = Path.Combine(svnServicePathsProvider.ServiceBinDirectory, "svnlook.exe");
                RepoStoragePath = GetRepoStoragePathFromConfig();
                BaseRepoUrl = GetBaseRepoUrlFromConfig(webConfigurationProvider.GetValue("SvnServerAlias"));
            }
            catch (ArgumentException ae)
            {
                Log.Warn(ae.Message);
            }
        }

        public bool IsVisualSvnServerInstalled { get; private set; }
        public string RepoStoragePath { get; private set; }
        public string SvnLookExePath { get; private set; }
        public string BaseRepoUrl { get; private set; }

        /// <summary>
        /// This is a dark and scary pit of assumptions and will probably be the least portable part of this app
        /// </summary>
        /// <returns></returns>
        private string GetBaseRepoUrlFromConfig(string serverAlias)
        {
            using (var file = new StreamReader(_configFilePath))
            {
                string line;
                var hostAndPort = "localhost";
                var usingSsl = false;

                while ((line = file.ReadLine()) != null)
                {
                    if (!line.ToLowerInvariant().Contains("servername") && !line.ToLowerInvariant().Contains("mod_ssl"))
                    {
                        continue;
                    }

                    //ServerName "pethros:80"
                    //ServerName "pethros:443"
                    if (line.ToLowerInvariant().Contains("servername"))
                    {
                        hostAndPort = Regex.Replace(line.Replace("/", @"\").Replace('"', ' '), @"\s*ServerName\s*","");
                        continue;
                    }

                    //LoadModule ssl_module bin/mod_ssl.so
                    if (line.ToLowerInvariant().Contains("mod_ssl"))
                    {
                        usingSsl = !Regex.IsMatch(line, @"^#");
                    }
                }

                var protocol = "http";

                if (usingSsl)
                {
                    protocol += "s";
                }

                hostAndPort = hostAndPort.Trim().Replace("\"", "");

                // jesus fuck... I tried to get this dynamically (any CNAMES registered in DNS for a given host) but it was taking too long to figure out
                if (!string.IsNullOrEmpty(serverAlias))
                {
                    hostAndPort = Regex.Replace(hostAndPort, @"(\w+)([\.:].*)?", serverAlias + "$2");
                }

                hostAndPort = Regex.Replace(hostAndPort, ":(80|443)$", "");

                // Should really check the containing <Location> element path value of SVNParentPath to discover "/svn/"
                return protocol + "://" + hostAndPort + "/svn/";
            }
        }

        private string GetRepoStoragePathFromConfig()
        {
            using (var file = new StreamReader(_configFilePath))
            {
                string line;
                while ((line = file.ReadLine()) != null)
                {
                    // SVNParentPath "E:/svn/repos/"
                    if (!line.ToLowerInvariant().Contains("svnparentpath"))
                    {
                        continue;
                    }

                    var path = Regex.Replace(line.Replace("/", @"\").Replace('"', ' '), @"\s*SVNParentPath\s*", "");

                    return Path.GetFullPath(path.Trim().TrimEnd(new[] {'\\'}));
                }
            }

            throw new ArgumentException("Could not find SVNParentPath setting in VisualSVN Server config file, '{0}'",
                                        _configFilePath);
        }
    }
}