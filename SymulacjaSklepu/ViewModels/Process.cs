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
    class Process : ViewModelBase
    {
        #region Full Properties

        private int clockTime;
        public int ClockTime
        {
            get { return clockTime; }
            set { clockTime = value; OnPropertyChanged("ClockTime"); OnPropertyChanged("QueuePeopleAvr"); }
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
        /// Liczba obsłużonych osób
        /// </summary>
        private int tillPeopleAll;
        public int TillPeopleAll
        {
            get { return tillPeopleAll; }
            set { tillPeopleAll = value; OnPropertyChanged("TillPeopleAll"); OnPropertyChanged("PercentInQueue"); OnPropertyChanged("QueueTimeAvr"); }
        }


        #endregion


        //public SortedList<int, Zdarzenie> timedEvents;
        public List<IZdarzenie> timedEvents = new List<IZdarzenie>();
        public Queue<IZdarzenie> conditionalEvents = new Queue<IZdarzenie>();


        #region Wierd Things


        /// <summary>
        /// Liczba osób w kolejce
        /// </summary>
        public int PeopleInQueue
        {
            set { }
            get { return conditionalEvents.Count; }
        }

        /// <summary>
        /// Liczba timedEvents
        /// </summary>
        public int EventsInList
        {
            set { }
            get { return timedEvents.Count; }
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
                    return QueueTimeAll / (Convert.ToUInt64(TillPeopleAll) + Convert.ToUInt64(PeopleInQueue));
            }
        }
        //Liczba osób które wyszły z kolejki
        public ulong QueuePeopleAll { get; set; }
        //Zsumowany czas spędzony w kolejce przez każdego kto w niej był.
        private ulong queueTimeAll;
        public ulong QueueTimeAll
        {
            get { return queueTimeAll; }
            set { queueTimeAll = value;  OnPropertyChanged("QueuePeopleAvr"); OnPropertyChanged("QueuePeopleAll"); OnPropertyChanged("QueueTimeAll"); OnPropertyChanged("PercentInQueue"); }
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

        #endregion


        //-------------------------------------------------


        Thread simulationThread;
        //Defines if you should continue simulation
        public int temp = 0;

        public Process()
        {
            Initialization();
            simulationThread = new Thread(new ThreadStart(Simulation));
            simulationThread.IsBackground = true;
            

            StartEndCommand = new RelayCommand(_ => StartStop(), _ => !simulationThread.IsAlive);
            SuspendResumeCommand = new RelayCommand(_ => { SuspendResume(); }, _ => simulationThread.IsAlive);
        }

        private void Initialization()
        {
            ClockTime = 0;
            FreeTills = 3;
            MaxFreeTills = 3;

            ShopStop = 4;
            ShopStart = 2;

            TillStop = 8;
            TillStart = 7;

            QueuePeopleAll = 0;
            QueueTimeAll = 0;

            TillPeopleAll = 0;
        }

        public void Simulation()
        {
            ClockTime = 0;
            FreeTills = MaxFreeTills;
            QueuePeopleAll = 0;
            QueueTimeAll = 0;
            TillPeopleAll = 0;
            timedEvents.RemoveAll(_ => true);
            conditionalEvents.Clear();

            timedEvents.Add(new InShop(3));
            while (timedEvents.Count > 0 && threadStarted)
            {
                OnPropertyChanged("EventsInList");
                ClockTime = timedEvents[0].occurTime;
                var _zdarzenie = timedEvents[0];
                timedEvents.RemoveAt(0);
                _zdarzenie.ExecuteEvent(this);
            }
            MessageBox.Show("End");
        }

        public void BeforeQueueChanged()
        {

        }

        public void AfterQueueChanged()
        {
            OnPropertyChanged("PeopleInQueue");
            OnPropertyChanged("QueueTimeAvr");
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
