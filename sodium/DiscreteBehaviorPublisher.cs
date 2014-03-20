namespace Sodium
{
    public class DiscreteBehaviorPublisher<T> : DiscreteBehavior<T>
    {
        private T value;

        public DiscreteBehaviorPublisher(T value)
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

        public void SetValue(T newValue)
        {
            var oldValue = this.value;
            this.value = newValue;
            this.OnValueChanged(oldValue, newValue);
        }

        protected void OnValueChanged(T oldValue, T newValue)
        {
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
