namespace JT.Rx.Net
{
    using System.Net;
    
    
    public class WebResource : Monad<string>
    {
        private IBehavior<string> urlBehavior;
        
        /// <summary>
        /// Constructs a new WebResource to fetch from a constant URL
        /// </summary>
        /// <param name="url">The URL to load the content from</param>
        public WebResource(string url)
        {
            var cb = new Constant<string>(url);
            this.urlBehavior = cb;
            this.Register(cb);
        }

        /// <summary>
        /// Constructs a new WebResource to fetch from a dynamic URL
        /// </summary>
        /// <param name="urlBehavior">Behavior containing the URL to load</param>
        public WebResource(IBehavior<string> urlBehavior)
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
