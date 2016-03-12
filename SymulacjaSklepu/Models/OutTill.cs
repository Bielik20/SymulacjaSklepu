using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SymulacjaSklepu.ViewModels
{
    class OutTill : IZdarzenie
    {
        #region Initialization

        public int occurTime { get; set; }
        public int enterTime { get; set; }

        public OutTill(int occurTime, int enterTime)
        {
            this.occurTime = occurTime;
            this.enterTime = enterTime;
        }

        #endregion

        //----------------------------------

        public void ExecuteEvent(Process process)
        {
            process.RegisterServedPeopleAndReleaseTill();

            if (process.conditionalEvents.Count > 0 && process.FreeTills > 0)
            {
                process.ExecuteOutQueue();
            }
        }
    }
}
