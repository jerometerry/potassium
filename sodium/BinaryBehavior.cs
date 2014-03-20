namespace Sodium
{
    using System;

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TB"></typeparam>
    /// <typeparam name="TC"></typeparam>
    public class BinaryBehavior<T, TB, TC> : ContinuousBehavior<TC>
    {
        private Func<T, TB, TC> lift;
        private IBehavior<T> a;
        private IBehavior<TB> b;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="lift"></param>
        /// <param name="a"></param>
        /// <param name="b"></param>
        public BinaryBehavior(Func<T, TB, TC> lift, IBehavior<T> a, IBehavior<TB> b)
        {
            this.lift = lift;
            this.a = a;
            this.b = b;
        }

        public override TC Value
        {
            get
            {
                return lift(a.Value, b.Value);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                lift = null;
                a = null;
                b = null;
            }

            base.Dispose(disposing);
        }
    }
}
