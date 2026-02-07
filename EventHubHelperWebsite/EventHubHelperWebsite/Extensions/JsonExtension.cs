using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace EventHubHelperWebsite.Extensions
{
    public static class JsonExtension
    {
        public static string ValidateJson(this string jsonPayload)
        {
            if (string.IsNullOrWhiteSpace(jsonPayload) || !LooksLikeJson(jsonPayload)) return string.Empty;

            var jsonToken = JToken.Parse(jsonPayload);
            return jsonToken.ToString(Formatting.Indented);
        }

        #region Private Functions
        private static bool LooksLikeJson(string jsonPayload)
        {
            jsonPayload = jsonPayload.Trim();
            return (jsonPayload.StartsWith("{") && jsonPayload.EndsWith("}")) || // Object
                   (jsonPayload.StartsWith("[") && jsonPayload.EndsWith("]"));   // Array
        }
        #endregion
    }
}
