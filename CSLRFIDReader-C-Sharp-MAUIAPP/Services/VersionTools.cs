using System;
using System.Reflection;

namespace CSLHandheldReader_C_Sharp_MAUIAPP.Services
{
    public static class Tools
    {
        /// <summary>
        /// Gets the application version
        /// </summary>
        /// <returns>Version string</returns>
        public static string GetAppVersion()
        {
            try
            {
                var assembly = Assembly.GetExecutingAssembly();
                var versionAttribute = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
                if (versionAttribute != null)
                {
                    return versionAttribute.InformationalVersion;
                }
                
                var version = assembly.GetName().Version;
                if (version != null)
                {
                    return version.ToString();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting app version: {ex.Message}");
            }
            
            // Fallback version
            return "MAUI 1.0";
        }
    }
}