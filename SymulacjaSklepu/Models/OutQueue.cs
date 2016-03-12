using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SymulacjaSklepu.ViewModels
{
    class OutQueue : IZdarzenie
    {
        #region Initialization

        public int occurTime { get; set; }  
        public int enterTime { get; set; }

        public OutQueue(int enterTime)
        {
            this.enterTime = enterTime;
        }

        #endregion

        //-------------------------------------------------

        public void ExecuteEvent(Process process)
        {
            process.CreateOutTill();
            process.RegisterQueuePeopleAndTime(enterTime);
        }

    }
}
