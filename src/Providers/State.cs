namespace Potassium.Providers
{
    using System;

    /// <summary>
    /// StateFactory Monad
    /// </summary>
    /// <typeparam name="TS"></typeparam>
    /// <typeparam name="TA"></typeparam>
    /// <remarks>From http://taumuon-jabuka.blogspot.ca/2012/06/state-monad-in-c-and-f.html?m=1</remarks>
    public class State<TS, TA>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="computation"></param>
        public State(Func<TS, Tuple<TS, TA>> computation)
        {
            this.Computation = computation;
        }

        /// <summary>
        /// 
        /// </summary>
        public Func<TS, Tuple<TS, TA>> Computation { get; private set; }
    }
}
