using System.Collections.Generic;
using System.Web.Http;

using Weald.Models;
using Weald.Service;

namespace Weald.Controllers
{
    public class ReposController : ApiController
    {
        private readonly IProvideRepositoryInfo _repositoryInfoProvider;

        public ReposController()
            : this(WebApiApplication.RepostioryInfoProvider)
        {
            
        }

        public ReposController(IProvideRepositoryInfo repositoryInfoProvider)
        {
            _repositoryInfoProvider = repositoryInfoProvider;
        }

        /// <summary>
        /// Gets a collection of RepositoryInfo objects for all repositories on the server.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<RepositoryInfo> Get()
        {
            return _repositoryInfoProvider.GetAllRepositoryInfos();
        }

        /// <summary>
        /// Given a repository name, gets the Info object for that repository.
        /// Returns an empty Info object if the given name doesn't exist.
        /// </summary>
        /// <param name="repoName">The name of the repository</param>
        /// <returns></returns>
        public RepositoryInfo Get(string repoName)
        {
            return _repositoryInfoProvider.GetRepositoryInfo(repoName);
        }
    }
}