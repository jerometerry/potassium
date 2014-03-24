namespace Potassium.Internal
{
    using Potassium.Core;

    internal class DelayEvent<T> : EventPublisher<T>
    {
        public DelayEvent(Observable<T> source)
        {
            var callback = new SubscriptionPublisher<T>((a, t) => t.Low(() => Publish(a)));
            var subscription = source.Subscribe(callback, this.Priority);
            this.Register(subscription);
        }
    }
}
