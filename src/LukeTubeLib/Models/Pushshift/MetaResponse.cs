using System.Text.Json.Serialization;

namespace LukeTubeLib.Models.Pushshift
{
    public record MetaResponse
    {
        [JsonPropertyName("client_accepts_json")]
        public bool ClientAcceptsJson { get; set; }

        [JsonPropertyName("client_request_headers")]
        public ClientRequestHeaders ClientRequestHeaders { get; set; }

        [JsonPropertyName("client_user_agent")]
        public string ClientUserAgent { get; set; }

        [JsonPropertyName("server_ratelimit_per_minute")]
        public int ServerRatelimitPerMinute { get; set; }

        [JsonPropertyName("source-ip")]
        public string SourceIp { get; set; }
    }

    public record ClientRequestHeaders
    {
        [JsonPropertyName("ACCEPT")]
        public string Accept { get; set; }

        [JsonPropertyName("ACCEPT-ENCODING")]
        public string AcceptEncoding { get; set; }

        [JsonPropertyName("ACCEPT-LANGUAGE")]
        public string AcceptLanguage { get; set; }

        [JsonPropertyName("CF-CONNECTING-IP")]
        public string CfConnectingIp { get; set; }

        [JsonPropertyName("CF-IPCOUNTRY")]
        public string CfIpCountry { get; set; }

        [JsonPropertyName("CF-RAY")]
        public string Cfray { get; set; }

        [JsonPropertyName("CF-VISITOR")]
        public string CfVisitor { get; set; }

        [JsonPropertyName("CONNECTION")]
        public string Connection { get; set; }

        [JsonPropertyName("COOKIE")]
        public string Cookie { get; set; }

        [JsonPropertyName("HOST")]
        public string Host { get; set; }

        [JsonPropertyName("UPGRADE-INSECURE-REQUESTS")]
        public string UpgradeInsecureRequests { get; set; }

        [JsonPropertyName("USER-AGENT")]
        public string Useragent { get; set; }

        [JsonPropertyName("X-FORWARDED-FOR")]
        public string XForwardedFor { get; set; }

        [JsonPropertyName("X-FORWARDED-PROTO")]
        public string XForwardedProto { get; set; }
    }
}
