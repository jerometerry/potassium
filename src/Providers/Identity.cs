namespace Potassium.Providers
{
    /// <summary>
    /// Identity is an IProvider having a constant value
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

        /// <summary>
        /// Gets the constant identity value
        /// </summary>
        public override T Value
        {
            get
            {
                return value;
            }
        }

        /// <summary>
        /// Clean up all resources used by the current SodiumObject
        /// </summary>
        /// <param name="disposing">Whether to dispose managed resources
        /// </param>
        protected override void Dispose(bool disposing)
        {
            value = default(T);

            base.Dispose(disposing);
        }
    }
}
