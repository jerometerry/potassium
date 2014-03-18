namespace Sodium
{
    using System;

    /// <summary>
    /// IBehavior is an IEvent with a current value, with some Behavioral methods.
    /// </summary>
    /// <typeparam name="T">The type of value fired through the Behavior</typeparam>
    public interface IBehavior<T> : IValue<T>
    {
        /// <summary>
        /// Apply a value inside a behavior to a function inside a behavior. This is the
        /// primitive for all function lifting.
        /// </summary>
        /// <typeparam name="TB">The return type of the inner function of the given Behavior</typeparam>
        /// <param name="bf">Behavior of functions that maps from T -> TB</param>
        /// <returns>The new applied Behavior</returns>
        IBehavior<TB> Apply<TB>(IBehavior<Func<T, TB>> bf);

        /// <summary>
        /// Transform a behavior with a generalized state loop (a mealy machine). The function
        /// is passed the input and the old state and returns the new state and output value.
        /// </summary>
        /// <typeparam name="TB">The type of the returned Behavior</typeparam>
        /// <typeparam name="TS">The snapshot function</typeparam>
        /// <param name="initState">Value to pass to the snapshot function</param>
        /// <param name="snapshot">Snapshot function</param>
        /// <returns>A new Behavior that collects values of type TB</returns>
        IBehavior<TB> CollectB<TB, TS>(TS initState, Func<T, TS, Tuple<TB, TS>> snapshot);

        /// <summary>
        /// Lift a binary function into behaviors.
        /// </summary>
        /// <typeparam name="TB">The type of the given Behavior</typeparam>
        /// <typeparam name="TC">The return type of the lift function.</typeparam>
        /// <param name="lift">The function to lift, taking a T and a TB, returning TC</param>
        /// <param name="behavior">The behavior used to apply a partial function by mapping the given 
        /// lift method to the current Behavior.</param>
        /// <returns>A new Behavior who's value is computed using the current Behavior, the given
        /// Behavior, and the lift function.</returns>
        IBehavior<TC> Lift<TB, TC>(Func<T, TB, TC> lift, IBehavior<TB> behavior);

        /// <summary>
        /// Lift a ternary function into behaviors.
        /// </summary>
        /// <typeparam name="TB">Type of Behavior b</typeparam>
        /// <typeparam name="TC">Type of Behavior c</typeparam>
        /// <typeparam name="TD">Return type of the lift function</typeparam>
        /// <param name="lift">The function to lift</param>
        /// <param name="b">Behavior of type TB used to do the lift</param>
        /// <param name="c">Behavior of type TC used to do the lift</param>
        /// <returns>A new Behavior who's value is computed by applying the lift function to the current
        /// behavior, and the given behaviors.</returns>
        /// <remarks>Lift converts a function on values to a Behavior on values</remarks>
        IBehavior<TD> Lift<TB, TC, TD>(Func<T, TB, TC, TD> lift, IBehavior<TB> b, IBehavior<TC> c);

        /// <summary>
        /// Transform the behavior's value according to the supplied function.
        /// </summary>
        /// <typeparam name="TB">The return type of the mapping function</typeparam>
        /// <param name="map">The mapping function that converts from T -> TB</param>
        /// <returns>A new Behavior that updates whenever the current Behavior updates,
        /// having a value computed by the map function, and starting with the value
        /// of the current event mapped.</returns>
        IBehavior<TB> MapB<TB>(Func<T, TB> map);

        /// <summary>
        /// Listen to the Behavior for updates
        /// </summary>
        /// <param name="callback">action to invoke when the Behavior fires</param>
        /// <returns>The event subscription</returns>
        /// <remarks>Immediately after creating the subscription, the callback will be fired with the 
        /// current value of the behavior.</remarks>
        ISubscription<T> SubscribeValues(Action<T> callback);

        /// <summary>
        /// Listen to the Behavior for updates
        /// </summary>
        /// <param name="callback"> action to invoke when the Behavior fires</param>
        /// <param name="rank">A rank that will be added as a superior of the Rank of the current Event</param>
        /// <returns>The event subscription</returns>
        /// <remarks>Immediately after creating the subscription, the callback will be fired with the 
        /// current value of the behavior.</remarks>
        ISubscription<T> SubscribeValues(ISodiumCallback<T> callback, Rank rank);
    }   
}
