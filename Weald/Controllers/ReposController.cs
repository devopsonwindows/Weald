using System.Collections.Generic;
using System.Web.Http;

using Weald.Models;
using Weald.Service;

namespace Weald.Controllers
{
    public class ReposController : ApiController
    {
        private readonly IProvideRepositoryInfoCache _repositoryInfoCacheProvider;

        public ReposController()
            : this(WebApiApplication.RepositoryInfoCacheProvider)
        {
            
        }

        public ReposController(IProvideRepositoryInfoCache repositoryInfoCacheProvider)
        {
            _repositoryInfoCacheProvider = repositoryInfoCacheProvider;
        }

        /// <summary>
        /// Gets a collection of RepositoryInfo objects for all repositories on the server.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<RepositoryInfo> Get()
        {
            return _repositoryInfoCacheProvider.GetRepositoryInfos();
        }

        /// <summary>
        /// Given a repository name, gets the Info object for that repository.
        /// Returns an empty Info object if the given name doesn't exist.
        /// </summary>
        /// <param name="repoName">The name of the repository</param>
        /// <returns></returns>
        public RepositoryInfo Get(string repoName)
        {
            return _repositoryInfoCacheProvider.GetRepositoryInfo(repoName);
        }
    }
}