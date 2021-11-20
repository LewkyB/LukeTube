using Newtonsoft.Json;

namespace PsawSharp.Entries
{
    public class MetaEntry
    {

        [JsonProperty("client_accepts_json")]
        public bool ClientAcceptsJson { get; set; }

        [JsonProperty("client_request_headers")]
        public ClientRequestHeaders ClientRequestHeaders { get; set; }

        [JsonProperty("client_user_agent")]
        public string ClientUserAgent { get; set; }

        [JsonProperty("server_ratelimit_per_minute")]
        public int ServerRatelimitPerMinute { get; set; }

        [JsonProperty("source-ip")]
        public string SourceIp { get; set; }

    }

    public class ClientRequestHeaders
    {

        [JsonProperty("ACCEPT")]
        public string Accept { get; set; }

        [JsonProperty("ACCEPT-ENCODING")]
        public string AcceptEncoding { get; set; }

        [JsonProperty("ACCEPT-LANGUAGE")]
        public string AcceptLanguage { get; set; }

        [JsonProperty("CF-CONNECTING-IP")]
        public string CfConnectingIp { get; set; }

        [JsonProperty("CF-IPCOUNTRY")]
        public string CfIpCountry { get; set; }

        [JsonProperty("CF-RAY")]
        public string Cfray { get; set; }

        [JsonProperty("CF-VISITOR")]
        public string CfVisitor { get; set; }

        [JsonProperty("CONNECTION")]
        public string Connection { get; set; }

        [JsonProperty("COOKIE")]
        public string Cookie { get; set; }

        [JsonProperty("HOST")]
        public string Host { get; set; }

        [JsonProperty("UPGRADE-INSECURE-REQUESTS")]
        public string UpgradeInsecureRequests { get; set; }

        [JsonProperty("USER-AGENT")]
        public string Useragent { get; set; }

        [JsonProperty("X-FORWARDED-FOR")]
        public string XForwardedFor { get; set; }

        [JsonProperty("X-FORWARDED-PROTO")]
        public string XForwardedProto { get; set; }

    }
}
