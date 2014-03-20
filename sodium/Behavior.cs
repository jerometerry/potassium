namespace Sodium
{
    using System;

    /// <summary>
    /// A Behavior is a continuous, time varying value. It starts with an initial value which 
    /// gets updated as the underlying Event is published.
    /// </summary>
    /// <typeparam name="T">The type of values that will be published through the Behavior.</typeparam>
    /// <remarks> In theory, a Behavior is a continuous value, whereas an Event is a discrete sequence of values.
    /// In Sodium.net, a Behavior is implemented by observing the discrete values of an Event, so a Behavior
    /// technically isn't continuous.
    /// </remarks>
    public abstract class Behavior<T> : DisposableObject
    {
        /// <summary>
        /// Sample the behavior's current value
        /// </summary>
        /// <remarks>
        /// This should generally be avoided in favor of SubscribeValues(..) so you don't
        /// miss any updates, but in many circumstances it makes sense.
        /// 
        /// Value is the value of the Behavior at the start of a Transaction
        ///
        /// It can be best to use it inside an explicit transaction (using TransactionContext.Current.Run()).
        /// For example, a b.Value inside an explicit transaction along with a
        /// b.Source.Subscribe(..) will capture the current value and any updates without risk
        /// of missing any in between.
        /// </remarks>
        public abstract T Value
        {
            get;
        }
    }
}
