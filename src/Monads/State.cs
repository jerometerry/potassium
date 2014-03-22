namespace JT.Rx.Net.Monads
{
    using System;
    using JT.Rx.Net.Core;

    /// <summary>
    /// State Monad
    /// </summary>
    /// <typeparam name="TS"></typeparam>
    /// <typeparam name="TA"></typeparam>
    /// <remarks>From http://taumuon-jabuka.blogspot.ca/2012/06/state-monad-in-c-and-f.html?m=1</remarks>
    public class State<TS, TA>
    {
        public State(Func<TS, Tuple<TS, TA>> computation)
        {
            this.Computation = computation;
        }

        public Func<TS, Tuple<TS, TA>> Computation { get; private set; }
    }

    public static class State
    {
        public static State<TS,TS> Get<TS>()
        {
            return new State<TS, TS>(sx => Tuple.Create(sx, sx));
        }

        public static State<TS, Unit> Set<TS>(TS state)
        {
            return new State<TS, Unit>(sx => new Tuple<TS, Unit>(state, Unit.Nothing));
        }
    }
}
