using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SymulacjaSklepu.ViewModels
{
    class OutTill : Zdarzenie
    {
        public int occurTime { get; set; }
        public int enterTime { get; set; }

        public OutTill(int occurTime, int enterTime)
        {
            this.occurTime = occurTime;
            this.enterTime = enterTime;
        }

        public void eventOccur(Proces proces)
        {
            proces.TillPeopleAll++;
            if (proces.conditionalEvents.Count == 0 || proces.FreeTills < 0)
            {
                proces.FreeTills++;
            }
            else
            {
                proces.BeforeQueueChanged();
                proces.conditionalEvents.Dequeue().eventOccur(proces);
                proces.AfterQueueChanged();
            }
        }
    }
}
