using System;

using NUnit.Framework;

using Weald.Service;

namespace Weald.Tests
{
    [TestFixture]
    public class SvnLookProviderTests
    {
        [Test]
        [Explicit]
        public void GetLatestRevisionForReals()
        {
            // This is why we use an IoC container
            var svnLookProvider =
                new SvnLookProvider(new VisualSvnServerInfoProvider(new VisualSvnServerPathProvider(new WindowsServiceInfoProvider()), new WebConfigurationProvider()));

            Assert.AreEqual(2, svnLookProvider.GetHeadRevision(@"E:\svn\repos\axiom"));
        }

        [Test]
        [Explicit]
        public void GetRevisionTimestampForReals()
        {
            // This is why we use an IoC container
            var svnLookProvider =
                new SvnLookProvider(new VisualSvnServerInfoProvider(new VisualSvnServerPathProvider(new WindowsServiceInfoProvider()), new WebConfigurationProvider()));

            // 2011-02-27 21:34:57 -0600 (Sun, 27 Feb 2011)
            var expected = DateTime.Parse("2011-02-27 21:34:57 -0600");
            Assert.AreEqual(expected, svnLookProvider.GetRevisionTimestamp(@"E:\svn\repos\axiom", 2));
        }

        [Test]
        [Explicit]
        public void GetRevisionUsernameForReals()
        {
            // This is why we use an IoC container
            var svnLookProvider =
                new SvnLookProvider(new VisualSvnServerInfoProvider(new VisualSvnServerPathProvider(new WindowsServiceInfoProvider()), new WebConfigurationProvider()));

            Assert.AreEqual("russ", svnLookProvider.GetRevisionUsername(@"E:\svn\repos\axiom", 2));
        }
    }
}