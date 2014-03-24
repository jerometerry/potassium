namespace Potassium.Internal
{
    using System;
    using Potassium.Core;

    /// <summary>
    /// Event that curries a Behavior holding a partial function by supplying another behavior.
    /// </summary>
    /// <typeparam name="T">The type of value published through the source Behavior</typeparam>
    /// <typeparam name="TB">The return type of the Behavior Mapping functions</typeparam>
    internal sealed class CurryEvent<T, TB> : EventPublisher<TB>
    {
        /// <summary>
        /// Set to true when waiting for the Publish Priority Action to run.
        /// </summary>
        private bool published;
        private Behavior<T> source;
        private Behavior<Func<T, TB>> partialBehavior;
        
        public CurryEvent(Behavior<Func<T, TB>> partialBehavior, Behavior<T> source)
        {
            this.source = source;
            this.partialBehavior = partialBehavior;

            SubscribeSource();
            SubscribeMap();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                source = null;
                this.partialBehavior = null;
            }

            base.Dispose(disposing);
        }

        private void SubscribeSource()
        {
            var valueChanged = new SubscriptionPublisher<T>((a, t) => this.SchedulePublish(t));
            var subscription = source.Source.Subscribe(valueChanged, this.Priority);
            this.Register(subscription);
        }

        private void SubscribeMap()
        {
            var functionChanged = new SubscriptionPublisher<Func<T, TB>>((f, t) => this.SchedulePublish(t));
            var subscription = this.partialBehavior.Source.Subscribe(functionChanged, this.Priority);
            this.Register(subscription);
        }

        /// <summary>
        /// Schedule prioritized publishing on the given transaction
        /// </summary>
        /// <param name="transaction">The transaction to publish the value on</param>
        /// <returns>True if publishing was added as a priority action on the given 
        /// transaction, false if there is already an scheduled publishing that 
        /// is yet to publish.</returns>
        private bool SchedulePublish(Transaction transaction)
        {
            if (published)
            {
                return false;
            }

            published = true;
            transaction.High(Publish, this.Priority);
            return true;
        }

        private void Publish(Transaction transaction)
        {
            var value = this.GetValueToPublish();
            this.Publish(value, transaction);
            published = false;
        }

        private TB GetValueToPublish()
        {
            var map = this.partialBehavior.NewValue;
            var a = this.source.NewValue;
            var b = map(a);
            return b;
        }
    }
}
