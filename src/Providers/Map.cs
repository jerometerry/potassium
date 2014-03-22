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

        public Map(Func<TA, TB> map)
        {
            this.map = map;
        }

        public override Func<TA, TB> Value
        {
            get
            {
                return this.map;
            }
        }
    }
}