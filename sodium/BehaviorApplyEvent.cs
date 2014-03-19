namespace Sodium
{
    using System;

    /// <summary>
    /// Fire the mapped value once per transaction if either the source Behavior or Behavior Map fires.
    /// </summary>
    /// <typeparam name="T">The type of value fired through the source Behavior</typeparam>
    /// <typeparam name="TB">The return type of the Behavior Mapping functions</typeparam>
    internal sealed class BehaviorApplyEvent<T, TB> : EventSink<TB>
    {
        /// <summary>
        /// Set to true when waiting for the Fire Priority Action to run.
        /// </summary>
        private bool fired;
        private IBehavior<T> source;
        private IBehavior<Func<T, TB>> behaviorMap;
        
        public BehaviorApplyEvent(IBehavior<T> source, IBehavior<Func<T, TB>> behaviorMap)
        {
            this.source = source;
            this.behaviorMap = behaviorMap;

            SubscribeSource();
            SubscribeMap();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                source = null;
                behaviorMap = null;
            }

            base.Dispose(disposing);
        }

        private void SubscribeSource()
        {
            var valueChanged = new SodiumCallback<T>((a, t) => this.ScheduleFire(t));
            var subscription = source.Subscribe(valueChanged, this.Rank);
            this.Register(subscription);
        }

        private void SubscribeMap()
        {
            var functionChanged = new SodiumCallback<Func<T, TB>>((f, t) => this.ScheduleFire(t));
            var subscription = behaviorMap.Subscribe(functionChanged, this.Rank);
            this.Register(subscription);
        }

        /// <summary>
        /// Schedule prioritized firing on the given transaction
        /// </summary>
        /// <param name="transaction">The transaction to fire the value on</param>
        /// <returns>True if firing was added as a priority action on the given 
        /// transaction, false if there is already an scheduled firing that 
        /// is yet to fire.</returns>
        private bool ScheduleFire(Transaction transaction)
        {
            if (fired)
            {
                return false;
            }

            fired = true;
            transaction.High(Fire, this.Rank);
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
            var map = this.behaviorMap.NewValue;
            var a = this.source.NewValue;
            var b = map(a);
            return b;
        }
    }
}
