using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SymulacjaSklepu.ViewModels
{
    class InShop : IZdarzenie
    {
        #region Initialization

        public int occurTime { get; set; }

        public InShop(int occurTime)
        {
            this.occurTime = occurTime;
        }

        #endregion

        //------------------------------------------------------------------

        public void ExecuteEvent(Process process)
        {
            CreateInShop(process);
            CreateNext(process);
        }

        private void CreateInShop(Process process)
        {
            Random rnd = new Random();
            int time = process.ClockTime + rnd.Next(process.ShopStart, process.ShopStop);

            process.timedEvents.Add(new InShop(time));
            process.timedEvents = process.timedEvents.OrderBy(x => x.occurTime).ToList();
        }

        private void CreateNext(Process process)
        {
            if (process.FreeTills > 0)
            {
                OccupyTill(process);
            }
            else
            {
                Dequeue(process);   
            }
        }

        private void OccupyTill(Process process)
        {
            process.FreeTills--;

            Random rnd = new Random();
            int time = process.ClockTime + rnd.Next(process.TillStart, process.TillStop);
            process.timedEvents.Add(new OutTill(time, process.ClockTime));
            process.timedEvents = process.timedEvents.OrderBy(x => x.occurTime).ToList();
        }

        private void Dequeue(Process process)
        {
            process.BeforeQueueChanged();
            process.conditionalEvents.Enqueue(new OutQueue(process.ClockTime));
            process.AfterQueueChanged();
        }
    }
}
