namespace Sodium
{
    /// <summary>
    /// Base class for Events, Behaviors, and Listeners
    /// </summary>
    public class SodiumItem : ISodiumItem
    {
        private static long sequence = 1;

        protected SodiumItem()
        {
            this.Id = sequence++;
        }

        public long Id { get; private set; }

        /// <summary>
        /// Gets / sets a description for the current Observable
        /// </summary>
        public string Description { get; set; }

        public override int GetHashCode()
        {
            return this.Id.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return this.Id == ((SodiumItem)obj).Id;
        }

        public virtual void Close()
        {
        }
    }
}
