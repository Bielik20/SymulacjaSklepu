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
        public int occurTime { get; set; }

        public InShop(int occurTime)
        {
            this.occurTime = occurTime;
        }

        private void createEvent(Proces proces)
        {
            Random rnd = new Random();

            int time = proces.ClockTime + rnd.Next(proces.ShopStart, proces.ShopStop);
            proces.timedEvents.Add(new InShop(time));
            proces.timedEvents = proces.timedEvents.OrderBy(x => x.occurTime).ToList();

            if (proces.FreeTills > 0)
            {
                proces.FreeTills--;

                time = proces.ClockTime + rnd.Next(proces.TillStart, proces.TillStop);
                proces.timedEvents.Add(new OutTill(time, proces.ClockTime));
                proces.timedEvents = proces.timedEvents.OrderBy(x => x.occurTime).ToList();
            }     
            else
            {
                proces.BeforeQueueChanged();
                proces.conditionalEvents.Enqueue(new OutQueue(proces.ClockTime));
                proces.AfterQueueChanged();
            }
        }

        public void eventOccur(Proces proces)
        {
            createEvent(proces);
        }
    }
}
