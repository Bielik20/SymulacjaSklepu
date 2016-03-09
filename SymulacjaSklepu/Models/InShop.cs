using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SymulacjaSklepu.ViewModels
{
    class InShop : Zdarzenie
    {

        private void createEvent(Proces proces)
        {
            Random rnd1 = new Random();
            int time1 = proces.ClockTime + rnd1.Next(proces.ShopStart, proces.ShopStop);
            try
            {
                proces.timedEvents.Add(time1, new InShop());
            } catch { }

            if (proces.WolneKasy > 0)
            {
                proces.WolneKasy--;
                Random rnd = new Random();
                int time = proces.ClockTime + rnd.Next(proces.TillStart, proces.TillStop);
                try
                { 
                proces.timedEvents.Add(time, new OutTill(proces.ClockTime));
                } catch { }
            }     
            else
            {
                proces.BeforeQueueChanged();
                proces.conditionalEvents.Enqueue(new OutQueue(proces.ClockTime));
            }
        }

        public void eventOccur(Proces proces)
        {
            createEvent(proces);
        }
    }
}
