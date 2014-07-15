using System.Collections.Concurrent;
using System.Collections.Generic;

using log4net;

using Weald.Models;

namespace Weald.Service
{
    public interface IProvideRepositoryInfoCache
    {
        IEnumerable<RepositoryInfo> GetRepositoryInfos();
        RepositoryInfo GetRepositoryInfo(string name);
        void UpdateAllRepositoryInfos();
    }

    public class RepositoryInfoCacheProvider : IProvideRepositoryInfoCache
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof (RepositoryInfoCacheProvider));

        private readonly IProvideRepositoryInfo _repositoryInfoProvider;
        private readonly IDictionary<string, RepositoryInfo> _repoInfoCache = new ConcurrentDictionary<string, RepositoryInfo>();

        private readonly object _syncLock = new object();

        public RepositoryInfoCacheProvider(IProvideRepositoryInfo repositoryInfoProvider)
        {
            _repositoryInfoProvider = repositoryInfoProvider;
        }

        public IEnumerable<RepositoryInfo> GetRepositoryInfos()
        {
            lock (_syncLock)
            {
                return _repoInfoCache.Values;
            }
        }

        public RepositoryInfo GetRepositoryInfo(string name)
        {
            lock (_syncLock)
            {
                RepositoryInfo repoInfo;

                if (!string.IsNullOrEmpty(name) && _repoInfoCache.TryGetValue(name, out repoInfo))
                {
                    return repoInfo;
                }

                Log.DebugFormat("The requested repository {0} could not be found.", name);

                return new RepositoryInfo();
            }
        }

        public void UpdateAllRepositoryInfos()
        {
            Log.Debug("Starting to refresh the repository info cache.");

            lock (_syncLock)
            {
                _repoInfoCache.Clear();

                foreach (var repoInfo in _repositoryInfoProvider.GetAllRepositoryInfos())
                {
                    _repoInfoCache[repoInfo.Name] = repoInfo;
                }
            }

            Log.Debug("The repository info cache refresh is complete.");
        }
    }
}