using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;

namespace SeveQsCustomControls
{
    public class CombinedObservableCollection<T> : INotifyCollectionChanged where T : IKeyedObject
    {
        private ThreadSafeObservableCollection<ThreadSafeObservableCollection<T>> _collections = new ThreadSafeObservableCollection<ThreadSafeObservableCollection<T>>();

        public CombinedObservableCollection()
        {
            _collections.CollectionChanged += OnCollectionsChanged;
        }

        private void OnCollectionsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach(var item in e.NewItems.Cast<ThreadSafeObservableCollection<T>>())
                    {
                        item.CollectionChanged += OnCollectionChanged;
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (var item in e.OldItems.Cast<ThreadSafeObservableCollection<T>>())
                    {
                        item.CollectionChanged -= OnCollectionChanged;
                    }
                    break;
            }
        }

        public event NotifyCollectionChangedEventHandler CollectionChanged;
        private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (CollectionChanged != null) CollectionChanged(sender, e);
        }



    }
}
