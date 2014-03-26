namespace Potassium.Internal
{
    using System;
    using Potassium.Core;

    /// <summary>
    /// Event that applies a Behavior holding a partial function by supplying another behavior.
    /// </summary>
    /// <typeparam name="T">The type of value fired through the source Behavior</typeparam>
    /// <typeparam name="TB">The return type of the Behavior Mapping functions</typeparam>
    internal sealed class ApplyEvent<T, TB> : FirableEvent<TB>
    {
        /// <summary>
        /// Set to true when waiting for the Fire Priority Action to run.
        /// </summary>
        private bool fired;
        private Behavior<T> source;
        private Behavior<Func<T, TB>> partialBehavior;
        
        public ApplyEvent(Behavior<Func<T, TB>> partialBehavior, Behavior<T> source)
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
            var valueChanged = new Observer<T>((a, t) => this.ScheduleFiring(t));
            var subscription = source.Source.Subscribe(valueChanged, this.Priority);
            this.Register(subscription);
        }

        private void SubscribeMap()
        {
            var functionChanged = new Observer<Func<T, TB>>((f, t) => this.ScheduleFiring(t));
            var subscription = this.partialBehavior.Source.Subscribe(functionChanged, this.Priority);
            this.Register(subscription);
        }

        /// <summary>
        /// Schedule prioritized firing on the given transaction
        /// </summary>
        /// <param name="transaction">The transaction to fire the value on</param>
        /// <returns>True if firing was added as a priority action on the given 
        /// transaction, false if there is already an scheduled firing that 
        /// is yet to fire.</returns>
        private bool ScheduleFiring(Transaction transaction)
        {
            if (fired)
            {
                return false;
            }

            fired = true;
            transaction.High(Fire, this.Priority);
            return true;
        }

        private void Fire(Transaction transaction)
        {
            var value = this.GetValueToFire();
            this.Fire(value, transaction);
            fired = false;
        }

        private TB GetValueToFire()
        {
            var map = this.partialBehavior.NewValue;
            var a = this.source.NewValue;
            var b = map(a);
            return b;
        }
    }
}
