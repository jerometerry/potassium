namespace Sodium
{
    using System.Collections.Generic;

    /// <summary>
    /// Base class for Events, Behaviors, and Listeners
    /// </summary>
    public class SodiumItem : ISodiumItem
    {
        private static long sequence = 1;

        private List<SodiumItem> finalizers;

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

        public void RegisterFinalizer(SodiumItem item)
        {
            if (finalizers == null)
            {
                finalizers = new List<SodiumItem>();
            }

            finalizers.Add(item);
        }

        public virtual void Close()
        {
            if (finalizers != null && finalizers.Count > 0)
            {
                var clone = new List<SodiumItem>(finalizers);
                finalizers.Clear();
                finalizers = null;

                foreach(var item in clone)
                {
                    item.Close();
                }
            }
        }
    }
}
