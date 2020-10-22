using System.Text.RegularExpressions;
using UnityEngine;

namespace Wrld.Scripts.Utilities
{
    public static class APIKeyHelpers
    {
        private const string WrldApiKeyTag = "WRLD_API_KEY";

        public static void CacheAPIKey(string apiKey)
        {
            if (AppearsValid(apiKey))
            {
                ms_cachedApiKey = apiKey;
                PlayerPrefs.SetString(WrldApiKeyTag, ms_cachedApiKey);
            }
        }

        public static string GetCachedAPIKey()
        {
            if (!string.IsNullOrEmpty(ms_cachedApiKey) && AppearsValid(ms_cachedApiKey))
            {
                return ms_cachedApiKey;
            }

            readCachedKeyFromPlayerPref();
            return ms_cachedApiKey;
        }

        public static bool AppearsValid(string apiKey)
        {
            return apiKey != null && Regex.IsMatch(apiKey, "^[a-f0-9]{32}$");
        }

        private static void readCachedKeyFromPlayerPref()
        {
            var cachedAPIKey = PlayerPrefs.GetString(WrldApiKeyTag);

            if (!string.IsNullOrEmpty(cachedAPIKey) && AppearsValid(cachedAPIKey))
            {
                ms_cachedApiKey = cachedAPIKey;
            }
        }

        private static string ms_cachedApiKey = null;
    }
}

