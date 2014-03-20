namespace Sodium
{
    using System;

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TA"></typeparam>
    /// <typeparam name="TB"></typeparam>
    /// <typeparam name="TC"></typeparam>
    public class BinaryBehavior<TA, TB, TC> : Behavior<TC>
    {
        private Func<TA, TB, TC> lift;
        private Behavior<TA> a;
        private Behavior<TB> b;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="lift"></param>
        /// <param name="a"></param>
        /// <param name="b"></param>
        public BinaryBehavior(Func<TA, TB, TC> lift, Behavior<TA> a, Behavior<TB> b)
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
