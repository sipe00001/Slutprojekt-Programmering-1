using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slutprojekt_Programmering_1
{
    class Util
    {
        public static double Clamp(double min, double val, double max)
        {
            return Math.Min(Math.Max(val, min), max);
        }
        public static int Clamp(int min, int val, int max)
        {
            return Math.Min(Math.Max(val, min), max);
        }
    }
}
