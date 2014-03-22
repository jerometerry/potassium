namespace JT.Rx.Net.Internal
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Notification wraps an System.Action used to subscribe to Observables.
    /// </summary>
    /// <typeparam name="T">The type of value that will be published</typeparam>
    internal sealed class SubscriptionPublisher<T>
    {
        private readonly Action<T, Transaction> action;

        /// <summary>
        /// Constructs a new Notification from the given Action
        /// </summary>
        /// <param name="action">The Action to invoke when the Observable publishes</param>
        public SubscriptionPublisher(Action<T> action)
            : this((a, t) => action(a))
        {
        }

        /// <summary>
        /// Constructs a new Notification from the given Action
        /// </summary>
        /// <param name="action">The Action to invoke when the Observable publishes</param>
        public SubscriptionPublisher(Action<T, Transaction> action)
        {
            this.action = action;
        }

        public static void PublishToSubscribers(T value, ISubscription<T>[] subscriptions, Transaction transaction)
        {
            foreach (var subscription in subscriptions)
            {
                PublishToSubscriber(value, subscription, transaction);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="subscription"></param>
        /// <param name="values"></param>
        /// <param name="transaction"></param>
        public static void PublishToSubscriber(ISubscription<T> subscription, ICollection<T> values, Transaction transaction)
        {
            if (values == null || values.Count == 0)
            {
                return;
            }

            foreach (var value in values)
            {
                PublishToSubscriber(value, subscription, transaction);
            }
        }

        public static void PublishToSubscriber(T value, ISubscription<T> subscription, Transaction transaction)
        {
            var s = (Subscription<T>)subscription;
            s.Publisher.Publish(value, transaction);
        }

        /// <summary>
        /// Invokes the callback
        /// </summary>
        /// <param name="value">The value to be published to the </param>
        /// <param name="transaction">The Transaction used to order the publishing</param>
        public void Publish(T value, Transaction transaction)
        {
            action(value, transaction);
        }
    }
}