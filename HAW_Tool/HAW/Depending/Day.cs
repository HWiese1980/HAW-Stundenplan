using System;
using System.Linq;
using System.Windows;

namespace HAW_Tool.HAW.Depending
{
    public class Day : NotifyingObject
    {
        public Day()
        {
            Events = new ThreadSafeObservableCollection<Event>(PlanFile.MainDispatcher);
            Events./*ObservableCollection.*/CollectionChanged += Events_CollectionChanged;
        }

        private bool IsSpanOccupiedByOthers(Event me, int row)
        {
            var occupyingEvents = from e in Events
                                  where !ReferenceEquals(e, me)
                                  where (e.Visibility == Visibility.Visible) && (e.Row == row) && ((e.From >= me.From & e.From <= me.Till) | (e.Till >= me.From & e.Till <= me.Till))
                                  select e;

            return (occupyingEvents.Count() > 0);
        }

        void Events_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (var item in e.NewItems)
                {
                    var evt = (Event)item;
                    evt.Day = this;
                    evt.TimeChanged += evt_TimeChanged;
                }
            }

            if (e.OldItems != null)
            {
                foreach (var item in e.OldItems)
                {
                    var evt = (Event)item;
                    evt.TimeChanged -= evt_TimeChanged;
                }
            }
        }

        public void RemoveAllCouchDBEvents(string hashInfo)
        {
            // ReSharper disable ForCanBeConvertedToForeach
            var cdbEvt = (from p in Events
                          where p.Source == EventSource.CouchDB
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
            private get { return _week; }
            set
            {
                _week = value;
                OnPropertyChanged("Week");
            }
        }

        public ThreadSafeObservableCollection<Event> Events { get; private set; }

        public override string ToString()
        {
            return string.Format("Day {0} of Week {1} -> Date {2}", DOW, Week, Date);
        }
    }
}
