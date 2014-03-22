namespace Potassium.Providers
{
    /// <summary>
    /// Identity is a Monad having a constant value
    /// </summary>
    /// <typeparam name="T">The type of value of the Monad</typeparam>
    public class Identity<T> : Provider<T>
    {
        private T value;

        /// <summary>
        /// Constructs a new Constant
        /// </summary>
        /// <param name="value">The constant value of the Monad</param>
        public Identity(T value)
        {
            this.value = value;
        }

        public override T Value
        {
            get
            {
                return value;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                value = default(T);
            }

            base.Dispose(disposing);
        }
    }
}
