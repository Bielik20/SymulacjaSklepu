using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SymulacjaSklepu.ViewModels
{
    interface Zdarzenie
    {
        /// <summary>
        /// Should be raised when specific event occurs
        /// </summary>
        void eventOccur(Proces proces);
    }
}
