namespace Potassium.Providers
{
    using System;
    using Potassium.Core;

    /// <summary>
    /// Static helpers for the State Monad
    /// </summary>
    public static class StateFactory
    {
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TS"></typeparam>
        /// <returns></returns>
        public static State<TS, TS> Get<TS>()
        {
            return new State<TS, TS>(sx => Tuple.Create(sx, sx));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="state"></param>
        /// <typeparam name="TS"></typeparam>
        /// <returns></returns>
        public static State<TS, Unit> Set<TS>(TS state)
        {
            return new State<TS, Unit>(sx => new Tuple<TS, Unit>(state, Unit.Nothing));
        }
    }
}