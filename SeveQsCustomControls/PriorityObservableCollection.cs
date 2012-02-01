using System.Collections;
using System.Linq;
using System.Windows;

namespace SeveQsCustomControls
{
    public class PriorityObservableCollection<T> : ThreadSafeObservableCollection<T> where T: IPriorized
    {
        protected override void OnCollectionChanged(System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            ProcessPriorities(e.NewItems);
            base.OnCollectionChanged(e);
        }

        public void ClearByPriority(int priority)
        {
            RemoveItems(this.Where(p => p.Priority == priority).ToArray());
        }

        private void ProcessPriorities(IList newItems)
        {
            if (newItems == null) return;

            foreach(var item in newItems.Cast<IPriorized>())
            {
                IPriorized item1 = item;
                var lowerItems = from IPriorized p in this
                                 where p.CompareTo(item1) == 0
                                 where p.Priority < item1.Priority
                                 select p;

                var higherItems = from IPriorized p in this
                                  where p.CompareTo(item1) == 0
                                  where p.Priority > item1.Priority
                                  select p;

                foreach(var lItem in lowerItems)
                {
                    lItem.Visibility = Visibility.Hidden;
                    lItem.OnReplaced(item1);
                }
                if(higherItems.Count() > 0) item1.Visibility = Visibility.Hidden;
            }
        }
    }
}