namespace Potassium.Providers
{
    using System;

    /// <summary>
    /// Map is a Monad who's value is a mapping function
    /// </summary>
    /// <typeparam name="TA">The input type of the mapping function</typeparam>
    /// <typeparam name="TB">The return type of the mapping function</typeparam>
    public class Map<TA, TB> : Provider<Func<TA, TB>>
    {
        private Func<TA, TB> map;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="map"></param>
        public Map(Func<TA, TB> map)
        {
            this.map = map;
        }

        /// <summary>
        /// Evaluates the value of the Provider
        /// </summary>
        public override Func<TA, TB> Value
        {
            get
            {
                return this.map;
            }
        }

        /// <summary>
        /// Clean up all resources used by the current SodiumObject
        /// </summary>
        /// <param name="disposing">Whether to dispose managed resources
        /// </param>
        protected override void Dispose(bool disposing)
        {
            map = null;

            base.Dispose(disposing);
        }
    }
}