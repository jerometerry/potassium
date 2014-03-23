using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Potassium
{
    public static class Frequency
    {
        public static Core.Hz Hz(TimeSpan ts)
        {
            return new Core.Hz(ts);
        }

        public static Core.Hz Hz(double hz)
        {
            return new Core.Hz(hz);
        }

        public static Core.Hz Hz(decimal hz)
        {
            return new Core.Hz(Convert.ToDouble(hz));
        }

        public static Core.Hz kHz(double khz)
        {
            return new Core.Hz(1000.0 * khz);
        }

        public static Core.Hz MHz(double mhz)
        {
            return new Core.Hz(1000000.0 * mhz);
        }
    }
}
