using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Threading;

namespace SymulacjaSklepu.ViewModels
{
    class Proces : ViewModelBase
    {
        private int clockTime;
        public int ClockTime
        {
            get { return clockTime; }
            set { clockTime = value; OnPropertyChanged("ClockTime"); }
        }


        private int freeTills;
        public int FreeTills
        {
            get { return freeTills; }
            set
            {
                if (value > MaxFreeTills)
                    freeTills = MaxFreeTills;
                else
                    freeTills = value;
                OnPropertyChanged("FreeTills");
            }
        }


        private int maxFreeTills;
        public int MaxFreeTills
        {
            get { return maxFreeTills; }
            set
            {
                FreeTills += value - maxFreeTills;
                maxFreeTills = value;
                OnPropertyChanged("MaxFreeTills");
            }
        }


        //public SortedList<int, Zdarzenie> timedEvents;
        public List<Zdarzenie> timedEvents = new List<Zdarzenie>();
        public Queue<Zdarzenie> conditionalEvents = new Queue<Zdarzenie>();


        //Przedział czasowy do pojawienia się następnego klienta
        private int shopStart;
        public int ShopStart
        {
            get { return shopStart; }
            set
            {
                if (value > ShopStop)
                    shopStart = ShopStop;
                else
                    shopStart = value;

                OnPropertyChanged("ShopStart");
            }
        }

        private int shopStop;
        public int ShopStop
        {
            get { return shopStop; }
            set
            {
                if (value < ShopStart)
                    shopStop = ShopStart;
                else
                    shopStop = value;

                OnPropertyChanged("ShopStop");
            }
        }


        //Przedział czasowy spędzony w przy kasie
        private int tillStart;
        public int TillStart
        {
            get { return tillStart; }
            set
            {
                if (value > TillStop)
                    tillStart = TillStop;
                else
                    tillStart = value;

                OnPropertyChanged("TillStart");
            }
        }

        private int tillStop;
        public int TillStop
        {
            get { return tillStop; }
            set
            {
                if (value < TillStart)
                    tillStop = TillStart;
                else
                    tillStop = value;

                OnPropertyChanged("TillStop");
            }
        }


        /// <summary>
        /// Liczba osłużonych osób
        /// </summary>
        private int tillPeopleAll;
        public int TillPeopleAll
        {
            get { return tillPeopleAll; }
            set { tillPeopleAll = value; OnPropertyChanged("TillPeopleAll"); OnPropertyChanged("PercentInQueue"); }
        }


        /// <summary>
        /// Liczba osób w kolejce
        /// </summary>
        public int PeopleInQueue
        {
            set { }
            get { return conditionalEvents.Count; }
        }


        /// <summary>
        /// Średnia liczba osób w kolejce
        /// </summary>
        public ulong QueuePeopleAvr
        {
            set { }
            get
            {
                if (ClockTime == 0)
                    return 0;
                else
                    return QueueTimeAll / Convert.ToUInt64(ClockTime);
            }
        }
        /*
        //Time when size of queue changed
        private int lastQueueChange;
        //Liczba osób w kolejce razy czas w niej spędzony
        private int queueCount;
        public int QueueCount //QueueCount += conditionalEvents.Count * (ClockTime - lastQueueChange);
        {
            get { return queueCount; }
            set { queueCount = value; OnPropertyChanged("QueuePeopleAvr"); OnPropertyChanged("QueueCount"); }
        }
        */


        /// <summary>
        /// Średni czas spędzony w kolejce
        /// </summary>
        public ulong QueueTimeAvr
        {
            set { }
            get
            {
                if (QueueTimeAll == 0)
                    return 0;
                else
                    return QueueTimeAll / QueuePeopleAll;
            }
        }
        //Liczba osób które wyszły z kolejki
        public ulong QueuePeopleAll { get; set; }
        //Zsumowany czas spędzony w kolejce przez każdego kto w niej był.
        private ulong queueTimeAll;
        public ulong QueueTimeAll
        {
            get { return queueTimeAll; }
            set { queueTimeAll = value; OnPropertyChanged("QueueTimeAvr"); OnPropertyChanged("QueuePeopleAvr"); OnPropertyChanged("QueuePeopleAll"); OnPropertyChanged("QueueTimeAll"); OnPropertyChanged("PercentInQueue"); }
        }

        /// <summary>
        /// Procent ludzi którzy byli w kolejce do wszystkich obsłużonych osób
        /// </summary>
        public int PercentInQueue
        {
            set { }
            get
            {
                if (TillPeopleAll == 0)
                    return 0;
                else
                    return Convert.ToInt32(100 * QueuePeopleAll/ Convert.ToUInt64(TillPeopleAll));
            }
        }


        //-------------------------------------------------


        Thread simulationThread;
        //Defines if you should continue simulation
        public int temp = 0;

        public Proces()
        {
            Initialization();
            simulationThread = new Thread(new ThreadStart(Simulation));
            simulationThread.IsBackground = true;
            

            StartEndCommand = new RelayCommand(_ => StartStop());
            SuspendResumeCommand = new RelayCommand(_ => { SuspendResume(); }, _ => simulationThread.IsAlive);
        }

        private void Initialization()
        {
            ClockTime = 0;
            FreeTills = 3;
            MaxFreeTills = 3;

            ShopStop = 5;
            ShopStart = 1;

            TillStop = 6;
            TillStart = 3;

            //lastQueueChange = 0;
            //QueueCount = 0;

            QueuePeopleAll = 0;
            QueueTimeAll = 0;

            TillPeopleAll = 0;
        }

        public void Simulation()
        {
            ClockTime = 0;
            FreeTills = MaxFreeTills;
            //lastQueueChange = 0;
            //QueueCount = 0;
            QueuePeopleAll = 0;
            QueueTimeAll = 0;
            TillPeopleAll = 0;
            timedEvents.RemoveAll(_ => true);
            conditionalEvents.Clear();

            timedEvents.Add(new InShop(3));
            while (timedEvents.Count > 0 && threadStarted)
            {
                ClockTime = timedEvents[0].occurTime;
                var _zdarzenie = timedEvents[0];
                timedEvents.RemoveAt(0);
                _zdarzenie.eventOccur(this);
            }
            MessageBox.Show("End");
        }

        public void BeforeQueueChanged()
        {
            //QueueCount += conditionalEvents.Count * (ClockTime - lastQueueChange);
            //lastQueueChange = ClockTime;
        }

        public void AfterQueueChanged()
        {
            OnPropertyChanged("PeopleInQueue");
        }


        //MARK: Commands
        public ICommand StartEndCommand { get; private set; }
        public ICommand SuspendResumeCommand { get; private set; }


        //MARK: HelperMethods
        private void StartStop()
        {
            if (!threadStarted)
            {
                threadStarted = true;
                threadRunning = true;
                try { simulationThread.Start(); }
                catch { MessageBox.Show("Restart is not yet avaible. If you want to restart simulation restart application."); }
                
            }
            else
            {
                threadStarted = false;
                threadRunning = false;
            }
        }

        private void SuspendResume()
        {
            if (threadRunning)
            {
                simulationThread.Suspend();
                threadRunning = false;
            }
            else
            {
                simulationThread.Resume();
                threadRunning = true;
            }
        }


        //MARK: HelperVariables
        private bool threadStarted { get; set; }
        private bool threadRunning { get; set; }



    }
}
