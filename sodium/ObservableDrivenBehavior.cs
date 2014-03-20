namespace Sodium
{
    /// <summary>
    /// ObservableDrivenBehavior is a Behavior who's value is updated when the underlying Observable updates.
    /// </summary>
    /// <typeparam name="T">The type of values published through the Observable</typeparam>
    public class ObservableDrivenBehavior<T> : Behavior<T>
    {
        private ObservedValue<T> observedValue;

        /// <summary>
        /// Constructs a new ObservableDrivenBehavior from an observable and a starting value
        /// </summary>
        /// <param name="observable">The Observable to monitor for updates</param>
        /// <param name="value">The initial value of the Behavior</param>
        public ObservableDrivenBehavior(Observable<T> observable, T value)
        {
            this.observedValue = new ObservedValue<T>(observable, value);
        }

        public override T Value
        {
            get
            {
                return observedValue.Value;
            }
        }

        /// <summary>
        /// New value of the Behavior that will be posted to Value when the Transaction completes
        /// </summary>
        internal T NewValue
        {
            get
            {
                return this.observedValue.NewValue;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                observedValue.Dispose();
                observedValue = null;
            }

            base.Dispose(disposing);
        }
    }
}
