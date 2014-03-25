namespace Potassium.Internal
{
    using Potassium.Core;

    internal class DelayEvent<T> : EventPublisher<T>
    {
        public DelayEvent(Observable<T> source)
        {
            var observer = new Observer<T>((a, t) => t.Low(() => Publish(a)));
            var subscription = source.Subscribe(observer, this.Priority);
            this.Register(subscription);
        }
    }
}
