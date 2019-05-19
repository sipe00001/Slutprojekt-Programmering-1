using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slutprojekt_Programmering_1
{

    
    class Sorting
    {
        /// <summary>
        /// Modifierad sorteringsfunktion från Sökning och sorterings uppgiften
        /// Sorterar två listor baserat på den ena.
        /// <param name="key">Sorterings nyckeln som ska användas för att sortera båda listorna</param>  
        /// <param name="value">Denna listan sorteras baserat på ordnigen hos <paramref name="key"/></param>  
        /// <param name="storlek">Storleken på båda listorna</param>  
        /// </summary>
        public static void QuickSort(List<int> key, List<String> value, int storlek) 
        {
            QuickSortLoHi(key, value, 0, storlek - 1);
        }
        static void QuickSortLoHi(List<int> key, List<String> value, int lo, int hi)
        {
            if (lo < hi)
            {
                int p = Partition(key, value, lo, hi);
                QuickSortLoHi(key, value, lo, p - 1);
                QuickSortLoHi(key, value, p + 1, hi);
            }
        }
        static int Partition(List<int> key, List<String> value, int lo, int hi)
        {
            int temp;
            String tempStr;
            int pivot = key[hi];
            int i = lo;
            for (int j = lo; j < hi; j++)
            {
                if (key[j] < pivot)
                {
                    temp = key[j];
                    key[j] = key[i];
                    key[i] = temp;
                    tempStr = value[j];
                    value[j] = value[i];
                    value[i] = tempStr;
                    i++;
                }
            }
            temp = key[hi];
            key[hi] = key[i];
            key[i] = temp;
            tempStr = value[hi];
            value[hi] = value[i];
            value[i] = tempStr;
            return i;
        }
    }
}
