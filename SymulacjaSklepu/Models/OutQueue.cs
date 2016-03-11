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
            CreateNext(process);
            RegisterQueuePeopleAndTime(process);
        }

        private void CreateNext(Process process)
        {
            Random rnd = new Random();
            int time = process.ClockTime + rnd.Next(process.TillStart, process.TillStop);

            process.timedEvents.Add(new OutTill(time, process.ClockTime));
            process.timedEvents = process.timedEvents.OrderBy(x => x.occurTime).ToList();
        }

        private void RegisterQueuePeopleAndTime(Process process)
        {
            process.QueuePeopleAll++;
            process.QueueTimeAll += Convert.ToUInt64(process.ClockTime - enterTime);
        }
    }
}
