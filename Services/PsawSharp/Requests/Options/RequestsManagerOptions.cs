namespace PsawSharp.Requests.Options
{
    public class RequestsManagerOptions
    {

        #region Properties

        public string UserAgent { get; set; }

        public string ProxyAddress { get; set; }

        public bool UseProxy => !string.IsNullOrEmpty(ProxyAddress);

        #endregion

        public RequestsManagerOptions()
        {
            UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/70.0.3538.110 Safari/537.36";
        }

        public RequestsManagerOptions(string userAgent, string proxyAddress = null)
        {
            UserAgent = userAgent;
            ProxyAddress = proxyAddress;
        }

    }
}
