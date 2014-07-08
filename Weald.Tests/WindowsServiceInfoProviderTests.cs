using System;
using System.IO;

using NUnit.Framework;

using Weald.Service;

namespace Weald.Tests
{
    [TestFixture]
    public class WindowsServiceInfoProviderTests
    {
        [Test]
        public void CanGetWellKnownServiceExePath()
        {
            var path = new WindowsServiceInfoProvider().GetServiceExePath("LanmanServer");
            Assert.AreEqual(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "svchost.exe"), path);
        }

        [Test]
        public void ThrowsOnNonexistantServiceName()
        {
            Assert.Throws<ArgumentException>(() => new WindowsServiceInfoProvider().GetServiceExePath(Guid.NewGuid().ToString()));
        }
    }
}