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
        public int occurTime { get; set; }
        public int enterTime { get; set; }

        public OutTill(int occurTime, int enterTime)
        {
            this.occurTime = occurTime;
            this.enterTime = enterTime;
        }

        //----------------------------------

        public void ExecuteEvent(Process process)
        {
            process.TillPeopleAll++;
            if (process.conditionalEvents.Count == 0 || process.FreeTills < 0)
            {
                FreeTill(process);
            }
            else
            {
                Dequeue(process);
            }
        }

        private void FreeTill(Process process)
        {
            process.FreeTills++;
        }

        private void Dequeue(Process process)
        {
            process.BeforeQueueChanged();
            process.conditionalEvents.Dequeue().ExecuteEvent(process);
            process.AfterQueueChanged();
        }
    }
}
