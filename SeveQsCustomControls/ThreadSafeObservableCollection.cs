using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Threading;

namespace SeveQsCustomControls
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ThreadSafeObservableCollection<T> : ObservableCollection<T>
    {
        /// <summary>
        /// The dispatcher of the UI thread. Needed for multi thread purposes
        /// </summary>
        public static Dispatcher UIDispatcher { get; set; }

        public string UniquePropertyName { get; set; }

        protected override void InsertItem(int index, T item)
        {
            if (UIDispatcher == null || UIDispatcher.CheckAccess())
            {
                base.InsertItem(index, item);
            }
            else
            {
                UIDispatcher.Invoke(new Action(() => base.InsertItem(index, item)));
            }
        }

        bool CheckUniqueContains(T obj)
        {
            if(string.IsNullOrEmpty(UniquePropertyName)) return false;
            var prop = obj.GetType().GetProperty(UniquePropertyName);
            if (prop == null) return false;

            var val = prop.GetValue(obj, null);
            foreach(T item in this)
            {
                var otherval = prop.GetValue(item, null);
                if (otherval.Equals(val)) return true;
            }
            return false;
        }

        protected override void ClearItems()
        {
            if (UIDispatcher == null || UIDispatcher.CheckAccess())
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
            if (UIDispatcher == null || UIDispatcher.CheckAccess())
            {
                base.RemoveItem(index);
            }
            else
            {
                UIDispatcher.Invoke(new Action(() => base.RemoveItem(index)));
            }
        }

        protected override void SetItem(int index, T item)
        {
            if (UIDispatcher == null || UIDispatcher.CheckAccess())
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
            if (UIDispatcher == null || UIDispatcher.CheckAccess())
            {
                base.MoveItem(oldIndex, newIndex);
            }
            else
            {
                UIDispatcher.Invoke(new Action(() => base.MoveItem(oldIndex, newIndex)));
            }
        }

        static volatile object _lock = new object();

        /// <summary>
        /// Removes all items inside the provided array from the collection
        /// </summary>
        /// <param name="items">array of elements that should be removed from the collection</param>
        public void RemoveItems(T[] items)
        {
            foreach (var item in items)
            {
                Remove(item);
            }
        }

        /// <summary>
        /// Adds all items inside the provided array to the collection
        /// </summary>
        /// <param name="items">array of elements that should be added to the collection</param>
        public void AddItems(T[] items)
        {
            foreach (var item in items)
            {
                Add(item);
            }
        }

        public void Trim()
        {
            RemoveItems(this.Where(p => p == null).ToArray());
        }
    }
}
