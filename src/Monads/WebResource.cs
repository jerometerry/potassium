namespace JT.Rx.Net.Monads
{
    using System.Net;
    using JT.Rx.Net.Core;
    
    /// <summary>
    /// WebResource is a Monad (of type string) who's value is computed by downloading content from the web (e.g. a REST web service returning JSON data).
    /// </summary>
    public class WebResource : Monad<string>
    {
        private IValueSource<string> urlValueSource;
        
        /// <summary>
        /// Constructs a new WebResource to fetch from a constant URL
        /// </summary>
        /// <param name="url">The URL to load the content from</param>
        public WebResource(string url)
        {
            var cb = new Identity<string>(url);
            this.urlValueSource = cb;
            this.Register(cb);
        }

        /// <summary>
        /// Constructs a new WebResource to fetch from a dynamic URL
        /// </summary>
        /// <param name="urlValueior">Behavior containing the URL to load</param>
        public WebResource(IValueSource<string> urlValueSource)
        {
            this.urlValueSource = urlValueSource;
        }

        public override string Value
        {
            get
            {
                var url = this.urlValueSource.Value;
                using (var client = new WebClient())
                {
                    string result = client.DownloadString(url);
                    return result;
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.urlValueSource = null;
            }

            base.Dispose(disposing);
        }
    }
}
