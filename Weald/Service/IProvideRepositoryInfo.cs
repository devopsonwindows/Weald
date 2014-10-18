using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using log4net;
using Scripting;

using Weald.Models;

namespace Weald.Service
{
    public interface IProvideRepositoryInfo
    {
        IEnumerable<RepositoryInfo> GetAllRepositoryInfos();
        RepositoryInfo GetRepositoryInfo(string repoName);
    }

    public class RepositoryInfoProvider : IProvideRepositoryInfo
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof (RepositoryInfoProvider));

        private readonly IProvideVisualSvnServerInfo _serverInfoProvider;
        private readonly IProvideSvnLook _svnLookProvider;

        public RepositoryInfoProvider(IProvideVisualSvnServerInfo serverInfoProvider, IProvideSvnLook svnLookProvider)
        {
            _serverInfoProvider = serverInfoProvider;
            _svnLookProvider = svnLookProvider;
        }

        public IEnumerable<RepositoryInfo> GetAllRepositoryInfos()
        {
            if (string.IsNullOrEmpty(_serverInfoProvider.RepoStoragePath))
            {
                Log.Error("Asked to get all repo infos, but the repostorage path is not set.");
                yield break;
            }

            IEnumerable<string> repoDirectories;

            try
            {
                repoDirectories = Directory.GetDirectories(_serverInfoProvider.RepoStoragePath).Select(Path.GetFileName);
            }
            catch (Exception e)
            {
                Log.Error(string.Format("Asked to get all repo infos, but could not list the contents of the repo storage path {0}. Are you sure {1} has permission?",
                    _serverInfoProvider.RepoStoragePath, Environment.UserName), e);
                yield break;
            }

            foreach (var repoName in repoDirectories)
            {
                var repoStoragePath = Path.Combine(_serverInfoProvider.RepoStoragePath, repoName);

                yield return CreateRepositoryInfo(repoName, repoStoragePath);
            }
        }

        public RepositoryInfo GetRepositoryInfo(string repoName)
        {
            if (string.IsNullOrEmpty(repoName))
            {
                return new RepositoryInfo();
            }

            var repoStoragePath = Path.Combine(_serverInfoProvider.RepoStoragePath, repoName);

            if (!Directory.Exists(repoStoragePath))
            {
                return new RepositoryInfo();
            }

            return CreateRepositoryInfo(repoName, repoStoragePath);
        }

        private static long GetRepoSizeInBytes(string repoStoragePath)
        {
            return new FileSystemObject().GetFolder(repoStoragePath).Size;
        }

        private RepositoryInfo CreateRepositoryInfo(string repoName, string repoStoragePath)
        {
            var currentHeadRevision = _svnLookProvider.GetHeadRevision(repoStoragePath);
            var latestChangeTimestamp = _svnLookProvider.GetRevisionTimestamp(repoStoragePath, currentHeadRevision);

            var repoInfo = new RepositoryInfo
            {
                Name = repoName,
                LatestRevision = currentHeadRevision,
                LatestChangeTimestamp = latestChangeTimestamp,
                LatestChangeUsername = _svnLookProvider.GetRevisionUsername(repoStoragePath, currentHeadRevision),
                SizeInBytes = GetRepoSizeInBytes(repoStoragePath),
                Url = _serverInfoProvider.BaseRepoUrl + repoName,
            };

            return repoInfo;
        }


    }
}