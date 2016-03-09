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


        private int wolneKasy;
        public int WolneKasy
        {
            get { return wolneKasy; }
            set
            {
                if (value > MaxWolneKasy)
                    wolneKasy = MaxWolneKasy;
                else
                    wolneKasy = value;
                OnPropertyChanged("WolneKasy");
            }
        }


        private int maxWolneKasy;
        public int MaxWolneKasy
        {
            get { return maxWolneKasy; }
            set { maxWolneKasy = value; OnPropertyChanged("MaxWolneKasy"); }
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
                    return QueueCount / ClockTime;
            }
        }
        //Time when size of queue changed
        private int lastQueueChange;
        //Liczba osób w kolejce razy czas w niej spędzony
        private int queueCount;

        public int QueueCount
        {
            get { return queueCount; }
            set { queueCount = value; OnPropertyChanged("QueuePeopleAvr"); }
        }


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
        public int QueuePeopleAll { get; set; }
        private int queueTimeAll;
        public int QueueTimeAll
        {
            get { return queueTimeAll; }
            set { queueTimeAll = value; OnPropertyChanged("QueueTimeAvr"); OnPropertyChanged("QueuePeopleAll"); OnPropertyChanged("QueueTimeAll"); }
        }


        //-------------------------------------------------


        Thread simulationThread;
        //Defines if you should continue simulation
        public int temp = 0;

        public Proces()
        {
            Initialization();
            simulationThread = new Thread(new ThreadStart(Simulation));

            StartCommand = new RelayCommand(_ => { simulationThread.Start(); });
            PauseCommand = new RelayCommand(_ => {  });
            ResumeCommand = new RelayCommand(_ => {  });
        }

        private void Initialization()
        {
            ClockTime = 0;
            WolneKasy = 3;
            MaxWolneKasy = 3;

            ShopStop = 20;
            ShopStart = 10;

            TillStop = 60;
            TillStart = 30;

            lastQueueChange = 0;
            QueueCount = 0;

            QueuePeopleAll = 0;
            QueueTimeAll = 0;

            TillPeopleAll = 0;
        }

        public void Simulation()
        {
            ClockTime = 0;
            WolneKasy = MaxWolneKasy;
            lastQueueChange = 0;
            QueueCount = 0;
            QueuePeopleAll = 0;
            QueueTimeAll = 0;
            TillPeopleAll = 0;

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
            QueueCount += conditionalEvents.Count * (ClockTime - lastQueueChange);
            lastQueueChange = ClockTime;
            OnPropertyChanged("PeopleInQueue");
        }


        //MARK: Commands
        public ICommand StartCommand { get; private set; }
        public ICommand PauseCommand { get; private set; }
        public ICommand ResumeCommand { get; private set; }

    }
}
