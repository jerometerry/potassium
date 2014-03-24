namespace Potassium.Core
{
    using System;
    using Potassium.Internal;

    /// <summary>
    /// Lifter providers methods to lift Unary, Binary and Ternary functions into Behaviors
    /// </summary>
    public static class Lifter
    {
        /// <summary>
        /// Apply a value inside a behavior to a function inside a behavior. This is the
        /// primitive for all function lifting.
        /// </summary>
        /// <typeparam name="TA">The type of the parameter to the lift function in the behavior</typeparam>
        /// <typeparam name="TB">The return type of the inner function of the given Behavior</typeparam>
        /// <param name="partialBehavior">Behavior of functions that maps from T -> TB</param>
        /// <param name="a">Parameter to supply to the partial function</param>
        /// <returns>The new applied Behavior</returns>
        public static Behavior<TB> Curry<TA, TB>(Behavior<Func<TA, TB>> partialBehavior, Behavior<TA> a)
        {
            var evt = new CurryEvent<TA, TB>(partialBehavior, a);
            var map = partialBehavior.Value;
            var valA = a.Value;
            var valB = map(valA);
            var behavior = evt.Hold(valB);
            behavior.Register(evt);
            return behavior;
        }

        /// <summary>
        /// Lift a constant into a Behavior
        /// </summary>
        /// <typeparam name="TA">The type of the value</typeparam>
        /// <param name="value">The value to lift into a Behavior</param>
        /// <returns>A constant Behavior, having the given value</returns>
        public static Behavior<TA> Lift<TA>(TA value)
        {
            return new Behavior<TA>(value);
        }

        /// <summary>
        /// Lift a unary function into a Behavior
        /// </summary>
        /// <typeparam name="TA">The type of the first parameter to the lift function in the behavior</typeparam>
        /// <typeparam name="TB">The return type of the lift function</typeparam>
        /// <param name="lift">The function to lift</param>
        /// <param name="a">The behavior to supply as a parameter to the lift function</param>
        /// <returns>The lifted Behavior</returns>
        public static Behavior<TB> Lift<TA, TB>(Func<TA, TB> lift, Behavior<TA> a)
        {
            return a.Map(lift);
        }

        /// <summary>
        /// Lift a binary function into behaviors.
        /// </summary>
        /// <typeparam name="TA">The type of the first parameter to the lift function in the behavior</typeparam>
        /// <typeparam name="TB">The type of the given Behavior</typeparam>
        /// <typeparam name="TC">The return type of the lift function.</typeparam>
        /// <param name="lift">The function to lift, taking a T and a TB, returning TC</param>
        /// <param name="a">Behavior who's value will be used as the first parameter to the lift function</param>
        /// <param name="b">Behavior who's value will be used as the second parameter to the lift function</param>
        /// <returns>A new Behavior who's value is computed using the current Behavior, the given
        /// Behavior, and the lift function.</returns>
        public static Behavior<TC> Lift<TA, TB, TC>(Func<TA, TB, TC> lift, Behavior<TA> a, Behavior<TB> b)
        {
            Func<TA, Func<TB, TC>> ffa = aa => (bb => lift(aa, bb));
            Behavior<Func<TB, TC>> partialB = Lift(ffa, a);
            Behavior<TC> behaviorC = Curry(partialB, b);
            behaviorC.Register(partialB);
            return behaviorC;
        }

        /// <summary>
        /// Lift a ternary function into behaviors.
        /// </summary>
        /// <typeparam name="TA">The type of the first parameter to the lift function in the behavior</typeparam>
        /// <typeparam name="TB">Type of Behavior b</typeparam>
        /// <typeparam name="TC">Type of Behavior c</typeparam>
        /// <typeparam name="TD">Return type of the lift function</typeparam>
        /// <param name="lift">The function to lift</param>
        /// <param name="a">Behavior who's value will be used as the first parameter to the lift function</param>
        /// <param name="b">Behavior who's value will be used as the second parameter to the lift function</param>
        /// <param name="c">Behavior who's value will be used as the third parameter to the lift function</param>
        /// <returns>A new Behavior who's value is computed by applying the lift function to the current
        /// behavior, and the given behaviors.</returns>
        /// <remarks>Lift converts a function on values to a Behavior on values</remarks>
        public static Behavior<TD> Lift<TA, TB, TC, TD>(Func<TA, TB, TC, TD> lift, Behavior<TA> a, Behavior<TB> b, Behavior<TC> c)
        {
            Func<TA, Func<TB, Func<TC, TD>>> map = aa => bb => cc => { return lift(aa, bb, cc); };
            Behavior<Func<TB, Func<TC, TD>>> partialB = Lift(map, a);
            Behavior<Func<TC, TD>> partialC = Curry(partialB, b);
            Behavior<TD> behaviorD = Curry(partialC, c);
            behaviorD.Register(partialB);
            behaviorD.Register(partialC);
            return behaviorD;
        }
    }
}
