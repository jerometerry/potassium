namespace Sodium
{
    public class Behavior<T> : ContinuousBehavior<T>
    {
        private T value;

        /// <summary>
        /// Constructs a new ConstantBehavior
        /// </summary>
        /// <param name="value">The constant initial value of the Behavior</param>
        public Behavior(T value)
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