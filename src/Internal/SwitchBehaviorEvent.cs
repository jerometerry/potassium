namespace Potassium.Internal
{
    using Potassium.Core;    

    /// <summary>
    /// SwitchBehaviorEvent is an EventPublisher that publishes new values whenever the current Behavior in the 
    /// Behavior of Behaviors fires.
    /// </summary>
    /// <typeparam name="T">The type of the Behavior</typeparam>
    /// <remarks>SwitchBehaviorEvent works by subscribing to the Behaviors Behavior at the time of construction,
    /// then recreating the subscription when new Behaviors are published to the Behavior (aka switching).</remarks>
    internal sealed class SwitchBehaviorEvent<T> : EventPublisher<T>
    {
        private ISubscription<Behavior<T>> behaviorSubscription;
        private ISubscription<T> eventSubscription;
        private Event<Behavior<T>> values;
        private Observer<T> forward;

        public SwitchBehaviorEvent(Behavior<Behavior<T>> source)
        {
            values = source.Values();
            forward = Forward();
            behaviorSubscription = values.Subscribe(new Observer<Behavior<T>>(CreateNewSubscription), Priority);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                CancelBehaviorSubscription();
                CancelEventSubscription();
                forward = null;
            }

            base.Dispose(disposing);
        }

        private void CreateNewSubscription(Behavior<T> behavior, Transaction transaction)
        {
            // Note: If any switch takes place during a transaction, then the
            // GetValueStream().Subscribe will always cause a sample to be fetched from the
            // one we just switched to. The caller will be fetching our output
            // using GetValueStream().Subscribe, and GetValueStream() throws away all publishings except
            // for the last one. Therefore, anything from the old input behavior
            // that might have happened during this transaction will be suppressed.
            CancelEventSubscription();

            var evt = new BehaviorLastValueEvent<T>(behavior, transaction);
            this.eventSubscription = evt.Subscribe(forward, Priority, transaction);
            ((Disposable)this.eventSubscription).Register(evt);
        }

        private void CancelBehaviorSubscription()
        {
            if (values != null)
            {
                values.Dispose();
                values = null;
            }

            if (behaviorSubscription != null)
            {
                behaviorSubscription.Dispose();
                behaviorSubscription = null;
            }
        }

        private void CancelEventSubscription()
        {
            if (eventSubscription != null)
            {
                eventSubscription.Dispose();
                eventSubscription = null;
            }
        }
    }
}