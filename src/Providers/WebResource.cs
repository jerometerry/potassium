namespace Potassium.Providers
{
    using System.Net;
    
    /// <summary>
    /// WebResource is a IProvider (of type string) who's value is computed by downloading content from the web (e.g. a REST web service returning JSON data).
    /// </summary>
    public class WebResource : Provider<string>
    {
        private IProvider<string> urlProvider;
        
        /// <summary>
        /// Constructs a new WebResource to fetch from a constant URL
        /// </summary>
        /// <param name="url">The URL to load the content from</param>
        public WebResource(string url)
        {
            var cb = new Identity<string>(url);
            this.urlProvider = cb;
            this.Register(cb);
        }

        /// <summary>
        /// Constructs a new WebResource to fetch from a dynamic URL
        /// </summary>
        /// <param name="urlProvider">Provider containing the URL to load</param>
        public WebResource(IProvider<string> urlProvider)
        {
            this.urlProvider = urlProvider;
        }

        /// <summary>
        /// Evaluates the value of the Provider
        /// </summary>
        public override string Value
        {
            get
            {
                var url = this.urlProvider.Value;
                using (var client = new WebClient())
                {
                    string result = client.DownloadString(url);
                    return result;
                }
            }
        }

        /// <summary>
        /// Clean up all resources used by the current SodiumObject
        /// </summary>
        /// <param name="disposing">Whether to dispose managed resources
        /// </param>
        protected override void Dispose(bool disposing)
        {
            this.urlProvider = null;

            base.Dispose(disposing);
        }
    }
}
