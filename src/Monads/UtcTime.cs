namespace JT.Rx.Net.Monads
{
    using System;

    /// <summary>
    /// UtcTime is a Monad that lazily returns DateTime.UtcNow
    /// </summary>
    /// <remarks>DateTime.UtcNow is evaluated when the value of the UtcTime is requested.</remarks>
    public class UtcTime : Time
    {
        public override DateTime Value
        {
            get
            {
                return DateTime.UtcNow;
            }
        }
    }
}