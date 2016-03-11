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


        #region Properties

        public List<IZdarzenie> timedEvents { get; set; } = new List<IZdarzenie>();
        public Queue<IZdarzenie> conditionalEvents { get; set; } = new Queue<IZdarzenie>();

        private Thread simulationThread { get; set; }

        //MARK: Helpers (indicators)
        private bool threadStarted { get; set; }
        private bool threadRunning { get; set; }

        #endregion


        #region Commands

        public ICommand StartEndCommand { get; private set; }
        public ICommand SuspendResumeCommand { get; private set; }

        #endregion


        //-------------------------------------------------


        #region Initialization

        public Process()
        {
            Initialization();

            StartEndCommand = new RelayCommand(_ => StartStop());
            SuspendResumeCommand = new RelayCommand(_ => { SuspendResume(); }, _ => threadStarted);
        }

        private void Initialization()
        {
            ShopStop = 4;
            ShopStart = 2;

            TillStop = 8;
            TillStart = 7;
            
            MaxFreeTills = 3;

            threadStarted = false;
            threadRunning = false;

            ResetVariables();
        }

        private void ResetVariables()
        {
            ClockTime = 0;
            FreeTills = MaxFreeTills;

            QueuePeopleAll = 0;
            QueueTimeAll = 0;
            TillPeopleAll = 0;
        }

        #endregion


        #region Process Methods

        public void Simulation()
        {
            ResetVariables();
            timedEvents.RemoveAll(_ => true);
            conditionalEvents.Clear();
            timedEvents.Add(new InShop(0));

            while (timedEvents.Count > 0 && threadStarted)
            {
                OnPropertyChanged("EventsInList");
                ClockTime = timedEvents[0].occurTime;
                var _zdarzenie = timedEvents[0];
                timedEvents.RemoveAt(0);
                _zdarzenie.ExecuteEvent(this);
            }

            MessageBox.Show("End of the simulation");
        }

        private void StartStop()
        {
            if (!threadStarted)
            {
                simulationThread = new Thread(new ThreadStart(Simulation));
                simulationThread.IsBackground = true;

                threadStarted = true;
                threadRunning = true;
                simulationThread.Start(); 
            }
            else
            {
                if (!threadRunning)
                    simulationThread.Resume(); //Makes sure that process will escape while loop and display MessageBox

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

        #endregion


        #region Event Menagement Methods

        public void BeforeQueueChanged()
        {

        }

        public void AfterQueueChanged()
        {
            OnPropertyChanged("PeopleInQueue");
            OnPropertyChanged("QueueTimeAvr");
        }

        #endregion
    }
}
