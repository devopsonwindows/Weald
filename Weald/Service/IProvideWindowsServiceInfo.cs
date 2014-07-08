using System;
using System.Management;
using System.Text.RegularExpressions;

namespace Weald.Service
{
    public interface IProvideWindowsServiceInfo
    {
        string GetServiceExePath(string serviceName);
    }

    public class WindowsServiceInfoProvider : IProvideWindowsServiceInfo
    {
        public string GetServiceExePath(string serviceName)
        {
            var searcher = new ManagementObjectSearcher(string.Format("SELECT * FROM Win32_Service WHERE Name = '{0}'", serviceName));

            foreach (var service in searcher.Get())
            {
                return Regex.Replace(service.GetPropertyValue("PathName").ToString(), "(\\.exe\"?).*", "$1").Replace('"', ' ').Trim();
            }

            throw new ArgumentException(string.Format("The given service name '{0}' does not exist on this computer.", serviceName), "serviceName");
        }
    }
}