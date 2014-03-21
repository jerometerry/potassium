namespace Sodium.Continuous
{
    using Sodium.Core;
    using System.Net;

    /// <summary>
    /// WebRequestBehavior is a Behavior that downloads a URL as a string
    /// </summary>
    public class HttpBehavior : ContinuousBehavior<string>
    {
        private IBehavior<string> urlBehavior;
        
        /// <summary>
        /// Constructs a new WebRequestBehavior to fetch from a constant URL
        /// </summary>
        /// <param name="url">The URL to load the content from</param>
        public HttpBehavior(string url)
        {
            var cb = new ConstantBehavior<string>(url);
            this.urlBehavior = cb;
            this.Register(cb);
        }

        /// <summary>
        /// Constructs a new WebRequestBehavior to fetch from a dynamic URL
        /// </summary>
        /// <param name="urlBehavior">Behavior containing the URL to load</param>
        public HttpBehavior(IBehavior<string> urlBehavior)
        {
            this.urlBehavior = urlBehavior;
        }

        public override string Value
        {
            get
            {
                var url = urlBehavior.Value;
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
                this.urlBehavior = null;
            }

            base.Dispose(disposing);
        }
    }
}
