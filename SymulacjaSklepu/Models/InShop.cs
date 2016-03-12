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
            process.CreateInShop();

            if (process.FreeTills > 0)
            {
                process.CreateOutTill();
            }
            else
            {
                process.CreateOutQueue();
            }
        }
    }
}
