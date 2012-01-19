using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows.Threading;
using HAW_Tool.HAW.Depending;

namespace HAW_Tool
{
    public class ThreadSafeObservableCollection<T> : ObservableCollection<T>
    {
        public Dispatcher UIDispatcher { get; set; }
        public ThreadSafeObservableCollection(Dispatcher d)
        {
            UIDispatcher = d;
        }

        protected override void InsertItem(int index, T item)
        {
            if(UIDispatcher.CheckAccess())
            {
                base.InsertItem(index, item);
            }
            else
            {
                UIDispatcher.Invoke(new Action(() => base.InsertItem(index, item)));
            }
        }

        protected override void ClearItems()
        {
            if (UIDispatcher.CheckAccess())
            {
                base.ClearItems();
            }
            else
            {
                UIDispatcher.Invoke(new Action(() => base.ClearItems()));
            }
        }

        protected override void RemoveItem(int index)
        {
            if (UIDispatcher.CheckAccess())
            {
                base.RemoveItem(index);
            }
            else
            {
                UIDispatcher.Invoke(new Action(() => base.RemoveItem(index)));
            }
        }

        public void RemoveItems(T[] items)
        {
            foreach(var item in items)
            {
                Remove(item);
            }
        }

        

        protected override void SetItem(int index, T item)
        {
            if (UIDispatcher.CheckAccess())
            {
                base.SetItem(index, item);
            }
            else
            {
                UIDispatcher.Invoke(new Action(() => base.SetItem(index, item)));
            }
        }

        protected override void MoveItem(int oldIndex, int newIndex)
        {
            if (UIDispatcher.CheckAccess())
            {
                base.MoveItem(oldIndex, newIndex);
            }
            else
            {
                UIDispatcher.Invoke(new Action(() => base.MoveItem(oldIndex, newIndex)));
            }
        }

        public void AddItems(T[] items)
        {
            foreach(var item in items)
            {
                Add(item);
            }
        }
    }
}
