namespace JT.Rx.Net.Continuous
{
    public class Identity<T> : Monad<T>
    {
        private T value;

        /// <summary>
        /// Constructs a new Identity
        /// </summary>
        /// <param name="value">The value of the Identity</param>
        public Identity(T value)
        {
            this.value = value;
        }

        public override T Value
        {
            get
            {
                return this.value;
            }
        }

        public void SetValue(T value)
        {
            this.value = value;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.value = default(T);
            }

            base.Dispose(disposing);
        }
    }
}