namespace Potassium.Providers
{
    using System;
    
    /// <summary>
    /// LocalTime is a Monad that lazily returns DateTime.Now
    /// </summary>
    /// <remarks>DateTime.Now is evaluated when the value of the LocalTime is requested.</remarks>
    public class LocalTime : Time
    {
        public override DateTime Value
        {
            get
            {
                return DateTime.Now;
            }
        }
    }
}