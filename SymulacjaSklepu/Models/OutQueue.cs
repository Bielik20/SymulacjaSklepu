using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SymulacjaSklepu.ViewModels
{
    class OutQueue : Zdarzenie
    {
        public int occurTime { get; set; }  
        public int enterTime { get; set; }

        public OutQueue(int enterTime)
        {
            this.enterTime = enterTime;
        }

        private void createEvent(Proces proces)
        {
            Random rnd = new Random();
            int time = proces.ClockTime + rnd.Next(proces.TillStart, proces.TillStop);

            proces.timedEvents.Add(new OutTill(time, proces.ClockTime));
            proces.timedEvents = proces.timedEvents.OrderBy(x => x.occurTime).ToList();
        }

        public void eventOccur(Proces proces)
        {
            createEvent(proces);
            proces.QueuePeopleAll++;
            proces.QueueTimeAll += Convert.ToUInt64(proces.ClockTime - enterTime);
        }
    }
}
