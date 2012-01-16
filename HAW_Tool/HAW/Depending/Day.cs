using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;

namespace HAW_Tool.HAW.Depending
{
    public class Day : DependencyObject
    {
        public Day()
        {
            Events = new ObservableCollection<Event>();
            Events.CollectionChanged += Events_CollectionChanged;
        }

        void Events_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (var item in e.NewItems)
                {
                    var evt = (Event)item;
                    //if (evt.Source == EventSource.CouchDB) Debugger.Break();
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
            var cdbEvt = (from p in Events
                           where p.Source == EventSource.CouchDB
                           where p.HashInfo == hashInfo
                           select p).ToArray();

            for (int i = 0; i < cdbEvt.Length; i++)
            {
                var evt = cdbEvt[i];
                Events.Remove(evt);
            }
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

            bool bOverlappingsFound = false;
            var idxA = RowIndex.GetRow(e);

            for (; ; )
            {
                var otherEventsSameRow = (from evt in Events
                                          where !ReferenceEquals(e, evt)
                                          let idxB = RowIndex.GetRow(evt)
                                          where idxA == idxB && EventsOverlap(e, evt)
                                          select evt).ToArray();

                if (otherEventsSameRow.Length <= 0) break;

                foreach (var eB in otherEventsSameRow)
                {
                    bOverlappingsFound = true;
                    RowIndex.SetRow(eB, idxA + 1);
                }
            }

            return bOverlappingsFound;
        }

        private bool ResetRowIndex(Event e)
        {
            bool bOverlappingsFound = false;
            var idxA = RowIndex.GetRow(e);

            if (idxA > 0)
            {
                var otherEventsOneBelow = from evt in Events
                                          where !ReferenceEquals(evt, e)
                                          let idxB = RowIndex.GetRow(evt)
                                          where idxB == (idxA - 1) && evt.Visibility == Visibility.Visible && EventsOverlap(evt, e)
                                          select evt;


                if (otherEventsOneBelow.Count() <= 0)
                {
                    bOverlappingsFound = true;
                    RowIndex.SetRow(e, idxA - 1);
                }
            }

            return bOverlappingsFound;
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

        private static bool EventsOverlap(Event a, Event b)
        {
            bool overlapLeft = (a.Till >= b.From & a.Till <= b.Till);  // B.Anfang < A.Ende < B.Ende
            bool overlapRight = (a.From >= b.From & a.From <= b.Till); // B.Anfang < A.Anfang < B.Ende

            bool aContainsB = (a.From >= b.From & a.Till <= b.Till); // B.Anfang < A.Anfang/A.Ende < B.Ende
            bool bContainsA = (b.From >= a.From & b.Till <= a.Till); // A.Anfang < B.Anfang/B.Ende < A.Ende

            bool overlap = overlapLeft | overlapRight | aContainsB | bContainsA;

            return overlap;
        }

        public DateTime Date
        {
            get { return (DateTime)GetValue(DateProperty); }
            set { SetValue(DateProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Date.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DateProperty =
            DependencyProperty.Register("Date", typeof(DateTime), typeof(Day), new UIPropertyMetadata(DateTime.MinValue));



        public DayOfWeek DOW
        {
            get { return (DayOfWeek)GetValue(DOWProperty); }
            set { SetValue(DOWProperty, value); }
        }

        // Using a DependencyProperty as the backing store for DOW.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DOWProperty =
            DependencyProperty.Register("DOW", typeof(DayOfWeek), typeof(Day), new UIPropertyMetadata(default(DayOfWeek)));



        public CalendarWeek Week
        {
            get { return (CalendarWeek)GetValue(WeekProperty); }
            set { SetValue(WeekProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Week.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty WeekProperty =
            DependencyProperty.Register("Week", typeof(CalendarWeek), typeof(Day),
                                        new UIPropertyMetadata(default(CalendarWeek)));



        public ObservableCollection<Event> Events { get; set; }

        public override string ToString()
        {
            return string.Format("Day {0} of Week {1} -> Date {2}", this.DOW, this.Week, this.Date);
        }
    }
}
