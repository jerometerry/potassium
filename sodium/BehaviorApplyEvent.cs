﻿namespace Sodium
{
    using System;

    internal class BehaviorApplyEvent<TA, TB> : Event<TB>
    {
        private Behavior<Func<TA, TB>> bf;
        private Behavior<TA> source;
        private IEventListener<Func<TA, TB>> l1;
        private IEventListener<TA> l2;
        
        /// <summary>
        /// Set to true when waiting for the Fire Priority Action to run.
        /// </summary>
        private bool fired;

        public BehaviorApplyEvent(Behavior<Func<TA, TB>> bf, Behavior<TA> source)
        {
            this.bf = bf;
            this.source = source;

            var functionChanged = new SodiumAction<Func<TA, TB>>((t, f) => ScheduledPrioritizedFire(t));
            var valueChanged = new SodiumAction<TA>((t, a) => ScheduledPrioritizedFire(t));

            l1 = bf.Updates().Listen(functionChanged, this.Rank);
            l2 = source.Updates().Listen(valueChanged, this.Rank);

            var map = bf.Sample();
            var valA = source.Sample();
            var valB = map(valA);
            this.Behavior = this.ToBehavior(valB);
        }

        public Behavior<TB> Behavior { get; private set; }

        public override void Dispose()
        {
            if (this.Behavior != null)
            {
                this.Behavior = null;
            }

            if (l1 != null)
            {
                l1.Dispose();
                l1 = null;
            }

            if (l2 != null)
            {
                l2.Dispose();
                l2 = null;
            }

            bf = null;
            source = null;

            base.Dispose();
        }

        /// <summary>
        /// Schedule prioritized firing on the given transaction
        /// </summary>
        /// <param name="transaction"></param>
        /// <returns>True if firing was added as a priority action on the given 
        /// transaction, false if there is already an scheduled firing that 
        /// is yet to fire.</returns>
        private bool ScheduledPrioritizedFire(Transaction transaction)
        {
            if (fired)
            {
                return false;
            }

            fired = true;
            transaction.Priority(Fire, this.Rank);
            return true;
        }

        private void Fire(Transaction transaction)
        {
            var b = this.GetNewValue();
            this.Fire(transaction, b);
            fired = false;
        }

        private TB GetNewValue()
        {
            var map = bf.NewValue();
            var a = source.NewValue();
            var b = map(a);
            return b;
        }
    }
}
