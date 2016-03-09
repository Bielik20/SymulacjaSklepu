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
        public int enterTime { get; set; }

        public OutTill(int enterTime)
        {
            this.enterTime = enterTime;
        }

        public void eventOccur(Proces proces)
        {
            Random rnd1 = new Random();
            int time1 = proces.ClockTime + rnd1.Next(proces.ShopStart, proces.ShopStop);
            try
            {
                proces.timedEvents.Add(time1, new InShop());
            }  catch { }


            proces.TillPeopleAll++;
            if (proces.conditionalEvents.Count == 0)
            {
                proces.WolneKasy++;
            }
            else
            {
                proces.BeforeQueueChanged();
                proces.conditionalEvents.Dequeue().eventOccur(proces);
            }
        }
    }
}
