namespace Sodium
{
    public class ObservedValueBehavior<T> : Behavior<T>
    {
        private ObservedValue<T> observedValue;

        public ObservedValueBehavior(Observable<T> observable, T value)
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
