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
            set { tillPeopleAll = value; OnPropertyChanged("TillPeopleAll"); }
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
        public int QueuePeopleAvr
        {
            set { }
            get
            {
                if (ClockTime == 0)
                    return 0;
                else
                    return QueueTimeAll / ClockTime;
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
        public int QueueTimeAvr
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
        public int QueuePeopleAll { get; set; }
        //Zsumowany czas spędzony w kolejce przez każdego kto w niej był.
        private int queueTimeAll;
        public int QueueTimeAll
        {
            get { return queueTimeAll; }
            set { queueTimeAll = value; OnPropertyChanged("QueueTimeAvr"); OnPropertyChanged("QueuePeopleAvr"); OnPropertyChanged("QueuePeopleAll"); OnPropertyChanged("QueueTimeAll"); }
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
            

            StartCommand = new RelayCommand(_ => simulationThread.Start(), _ => !simulationThread.IsAlive);
            PauseCommand = new RelayCommand(_ => { simulationThread.Suspend(); }, _ => simulationThread.IsAlive);
            ResumeCommand = new RelayCommand(_ => { simulationThread.Resume(); }, _ => simulationThread.IsAlive);
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
            while (timedEvents.Count > 0)
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
        public ICommand StartCommand { get; private set; }
        public ICommand PauseCommand { get; private set; }
        public ICommand ResumeCommand { get; private set; }

    }
}
