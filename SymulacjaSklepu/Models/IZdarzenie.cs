using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SymulacjaSklepu.ViewModels
{
    interface IZdarzenie
    {
        /// <summary>
        /// Should be raised when specific event occurs
        /// </summary>
        void ExecuteEvent(Process proces);

        int occurTime { get; set; }
    }

}
