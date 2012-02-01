using System;
using System.Linq;
using System.Windows;
using SeveQsCustomControls;

namespace HAW_Tool.HAW.Depending
{
    public class Day : NotifyingObject
    {
        public Day()
        {
            Events = new PriorityObservableCollection<Event> { UniquePropertyName = "HashInfo" };
            Events.CollectionChanged += EventsCollectionChanged;
        }

        private bool IsSpanOccupiedByOthers(Event me, int row)
        {
            var occupyingEvents = from e in Events
                                  where !ReferenceEquals(e, me)
                                  where (e.Visibility == Visibility.Visible)
                                  && (e.Row == row)
                                  && ((e.From >= me.From & e.From <= me.Till)
                                  | (e.Till >= me.From & e.Till <= me.Till)
                                  | (me.From >= e.From & me.From <= e.Till)
                                  | (me.Till >= e.From & me.Till <= e.Till))
                                  select e;

            return (occupyingEvents.Count() > 0);
        }

        void EventsCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (var item in e.NewItems)
                {
                    var evt = (Event)item;
                    evt.TimeChanged += evt_TimeChanged;
                    evt.PropertyChanged += evt_PropertyChanged;
                }
            }

            if (e.OldItems != null)
            {
                foreach (var item in e.OldItems)
                {
                    var evt = (Event)item;
                    evt.TimeChanged -= evt_TimeChanged;
                    evt.PropertyChanged -= evt_PropertyChanged;
                }
            }

            RecalculateRowIndexAll();
            OnPropertyChanged("HasVisibleEvents");
        }

        void evt_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Visibility") 
                OnPropertyChanged("HasVisibleEvents");

            if(PlanFile.Instance.Logging)
            {
                var prop = sender.GetType().GetProperty(e.PropertyName);
                var newVal = prop.GetValue(sender, null);

                Console.WriteLine("Property Changed: {0} changed Property {1} to {2}", (Event)sender, e.PropertyName, newVal);
            }
        }

        /*
        private int AlreadyContainsEvent(string Hash)
        {
            
        }
         * */

        public void RemoveAllCouchDBEvents(string hashInfo)
        {
            // ReSharper disable ForCanBeConvertedToForeach
            var cdbEvt = (from p in Events
                          where p.Source == EventSource.CouchDB | p.Source == EventSource.MongoDB
                          where p.HashInfo == hashInfo
                          select p).ToArray();

            for (int i = 0; i < cdbEvt.Length; i++)
            {
                var evt = cdbEvt[i];
                Events.Remove(evt);
            }
            // ReSharper restore ForCanBeConvertedToForeach
        }

        void evt_TimeChanged(object sender, EventArgs e)
        {
            var evt = (Event)sender;
            RecalculateRowIndex(evt);
            foreach (var otherEvt in Events)
            {
                ResetRowIndex(otherEvt);
            }
        }

        private bool RecalculateRowIndex(Event e)
        {
            if (e.Visibility == Visibility.Hidden) return false;

            bool bOccupationFound = false;
            for (; e.Row <= 2 && IsSpanOccupiedByOthers(e, e.Row); e.Row++)
            {
                bOccupationFound = true;
            }
            return bOccupationFound;
        }

        private bool ResetRowIndex(Event e)
        {
            if (e.Visibility == Visibility.Hidden) return false;

            bool bOccupationFound = false;
            for (; (e.Row - 1) >= 0 && !IsSpanOccupiedByOthers(e, e.Row - 1); e.Row--)
            {
                bOccupationFound = true;
            }
            return bOccupationFound;
        }

        public void RecalculateRowIndexAll()
        {
            bool bOverlappingsFound = false;

            do
            {
                foreach (Event eA in Events)
                {
                    bOverlappingsFound = RecalculateRowIndex(eA) | ResetRowIndex(eA);
                }
            } while (bOverlappingsFound);
        }

        private DateTime _date;
        public DateTime Date
        {
            get { return _date; }
            set
            {
                _date = value;
                OnPropertyChanged("Date");
            }
        }

        private DayOfWeek _dow;
        public DayOfWeek DOW
        {
            get { return _dow; }
            set
            {
                _dow = value;
                OnPropertyChanged("DOW");
            }
        }

        private CalendarWeek _week;
        public CalendarWeek Week
        {
            get { return _week; }
            set
            {
                _week = value;
                OnPropertyChanged("Week");
            }
        }

        public PriorityObservableCollection<Event> Events { get; private set; }

        public bool HasVisibleEvents
        {
            get { return ((from p in Events where p.Visibility == Visibility.Visible select p).Count() > 0); }
        }

        public override string ToString()
        {
            return string.Format("Day {0} of Week {1} -> Date {2}", DOW, Week, Date);
        }
    }
}
