namespace Wrld.Scripts.Utilities
{
    public static class UTMParamHelpers
    {
        // poked by build scripts
        private const string WrldApiKeyUtmCampaignTag = "unity-editor-wrld";

        public static string BuildGetApiKeyUrl(string utmContentTag)
        {
            var url = string.Format("https://accounts.wrld3d.com/users/sign_in?service=https%3A%2F%2Faccounts.wrld3d.com%2F%23apikeys&utm_source=unity&utm_medium=referral&utm_campaign={0}&utm_content={1}",
                WrldApiKeyUtmCampaignTag,
                utmContentTag);
            return url;
        }
    }
}
