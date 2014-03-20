namespace Sodium
{
    using System.Net;

    /// <summary>
    /// WebRequestBehavior is a Behavior that downloads a URL as a string
    /// </summary>
    public class HttpBehavior : Behavior<string>
    {
        private Behavior<string> urlBehavior;
        
        /// <summary>
        /// Constructs a new WebRequestBehavior to fetch from a constant URL
        /// </summary>
        /// <param name="url">The URL to load the content from</param>
        public HttpBehavior(string url)
        {
            this.urlBehavior = new ConstantBehavior<string>(url);
            this.Register(this.urlBehavior);
        }

        /// <summary>
        /// Constructs a new WebRequestBehavior to fetch from a dynamic URL
        /// </summary>
        /// <param name="urlBehavior">Behavior containing the URL to load</param>
        public HttpBehavior(Behavior<string> urlBehavior)
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
