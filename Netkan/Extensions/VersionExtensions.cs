using CKAN.Versioning;
using Newtonsoft.Json.Linq;

namespace CKAN.NetKAN.Extensions
{
    internal static class VersionExtensions
    {
        public static JToken ToSpecVersionJson(this ModuleVersion specVersion)
        {
            if (specVersion.IsEqualTo(new ModuleVersion("v1.0")))
            {
                return 1;
            }
            else
            {
                return specVersion.ToString();
            }
        }
    }
}
