namespace Potassium.Internal
{
    using System;
    using Potassium.Core;

    /// <summary>
    /// ApplyEvent applies a behavior of values to a behavior of functions, firing the computed
    /// value whenver either of the behavior fires.
    /// </summary>
    /// <typeparam name="TA">The type of value fired through the valueBehavior Behavior</typeparam>
    /// <typeparam name="TB">The return type of the Behavior partial function</typeparam>
    /// <remarks>
    /// The computed value fired on the current ApplyEvent is computed by applying the current
    /// value of the behavior of values to the current functio of the behavior of functions.
    /// 
    /// ApplyEvent is the basis for all lifting operations.</remarks>
    internal sealed class ApplyEvent<TA, TB> : FirableEvent<TB>
    {
        /// <summary>
        /// Set to true when waiting for the Fire Priority Action to run.
        /// </summary>
        private bool fired;
        private Behavior<Func<TA, TB>> funcBehavior;
        private Behavior<TA> valBehavior;
        
        /// <summary>
        /// Creates a new ApplyEvent
        /// </summary>
        /// <param name="funcBehavior">Behavior of mapping functions</param>
        /// <param name="valBehavior">Behavior to apply to the map</param>
        public ApplyEvent(Behavior<Func<TA, TB>> funcBehavior, Behavior<TA> valBehavior)
        {
            this.funcBehavior = funcBehavior;
            this.valBehavior = valBehavior;

            SubscribeVal();
            SubscribeFunc();
        }

        private TB AppliedValue
        {
            get
            {
                var func = this.funcBehavior.NewValue;
                var val = this.valBehavior.NewValue;
                return func(val);
            }
        }

        protected override void Dispose(bool disposing)
        {
            this.valBehavior = null;
            this.funcBehavior = null;

            base.Dispose(disposing);
        }

        /// <summary>
        /// Listen to the source Behavior for firings, firing the computed value on the current ApplyEvent
        /// </summary>
        private void SubscribeVal()
        {
            var observer = new Observer<TA>((a, t) => this.ScheduleFiring(t));
            var subscription = this.valBehavior.Source.Subscribe(observer, this.Priority);
            this.Register(subscription);
        }

        /// <summary>
        /// Listen to the behavior of functions for changes to the function, firing the computed value
        /// on the current ApplyEvent
        /// </summary>
        private void SubscribeFunc()
        {
            var observer = new Observer<Func<TA, TB>>((f, t) => this.ScheduleFiring(t));
            var subscription = this.funcBehavior.Source.Subscribe(observer, this.Priority);
            this.Register(subscription);
        }

        /// <summary>
        /// Schedule prioritized firing on the given transaction
        /// </summary>
        /// <param name="transaction">The transaction to fire the value on</param>
        private void ScheduleFiring(Transaction transaction)
        {
            if (!fired)
            {
                transaction.High(t => this.Fire(transaction), this.Priority);
                fired = true;
            }
        }

        private void Fire(Transaction transaction)
        {
            this.Fire(this.AppliedValue, transaction);
            fired = false;
        }
    }
}
