using System.Web.Configuration;

namespace Weald.Service
{
    public interface IProvideWebConfiguration
    {
        string GetValue(string propertyName);
    }

    public class WebConfigurationProvider : IProvideWebConfiguration
    {
        public string GetValue(string propertyName)
        {
            return WebConfigurationManager.AppSettings[propertyName];
        }
    }
}