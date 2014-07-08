using System;
using System.IO;
using System.Reflection;

using Moq;
using NUnit.Framework;

using Weald.Service;

namespace Weald.Tests
{
    [TestFixture]
    public class VisualSvnServerInfoProviderTests
    {
        private string _tempFilePath;

        [SetUp]
        public void SetUp()
        {
            _tempFilePath = Path.GetTempFileName();
        }

        [TearDown]
        public void TearDown()
        {
            try
            {
                File.Delete(_tempFilePath);
            }
            catch
            {
            }
        }

        [Test]
        public void NonExistentServiceExecutableMeansNothingWorks()
        {
            var mockServerPathProvider = new Mock<IProvideVisualSvnServerPaths>();
            mockServerPathProvider.Setup(x => x.ServiceExePath)
                                  .Returns(Guid.NewGuid().ToString());
            var mockWebConfigProvider = new Mock<IProvideWebConfiguration>();
            mockWebConfigProvider.Setup(x => x.GetValue("SvnServerAlias"))
                                 .Returns("Foo");

            var serverInfo = new VisualSvnServerInfoProvider(mockServerPathProvider.Object, mockWebConfigProvider.Object);

            Assert.IsFalse(serverInfo.IsVisualSvnServerInstalled);
            Assert.IsNullOrEmpty(serverInfo.RepoStoragePath);
            Assert.IsNullOrEmpty(serverInfo.SvnLookExePath);
        }

        [Test]
        public void CanGetNormalizedRepoStoragePath()
        {
            File.WriteAllLines(_tempFilePath, new[] { "FOO", "#BAR", "    SVNParentPath \"E:/svn/repos/\"", "     BAZ" });

            var mockServerPathProvider = new Mock<IProvideVisualSvnServerPaths>();
            mockServerPathProvider.Setup(x => x.ServiceExePath)
                                  .Returns(Assembly.GetExecutingAssembly().Location);
            mockServerPathProvider.Setup(x => x.ServiceConfigFilePath)
                                  .Returns(_tempFilePath);
            mockServerPathProvider.Setup(x => x.ServiceBinDirectory)
                                  .Returns("C:\\Foo");
            var mockWebConfigProvider = new Mock<IProvideWebConfiguration>();
            mockWebConfigProvider.Setup(x => x.GetValue("SvnServerAlias"))
                                 .Returns("Foo");

            var serverInfo = new VisualSvnServerInfoProvider(mockServerPathProvider.Object, mockWebConfigProvider.Object);

            Assert.IsTrue(serverInfo.IsVisualSvnServerInstalled);
            Assert.IsNotNullOrEmpty(serverInfo.RepoStoragePath);
            Assert.AreEqual(@"e:\svn\repos", serverInfo.RepoStoragePath.ToLowerInvariant());
        }
    }
}
