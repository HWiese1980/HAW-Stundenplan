using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace HAW_Tool
{
    internal class WorkingData<T>
    {
        public Action<T> Action { get; set; }
        public T Data { get; set; }
    }

    internal class Parallel<T>
    {
        volatile int _runningTasks;

        WorkingData<T>[] _inputDataArray;
        ManualResetEvent[] _resetEvents;


        private void ParallelWork(object s)
        {
            var threadNumber = (int)s;

            var tLocalWrkData = _inputDataArray[threadNumber];

            var workObj = tLocalWrkData.Data;

            tLocalWrkData.Action(workObj);

            _resetEvents[threadNumber].Set();
        }

        public void ForEach(IEnumerable<T> data, Action<T> action)
        {
            var array = data.ToArray();
            _runningTasks = array.Length;

            _inputDataArray = new WorkingData<T>[_runningTasks];
            _resetEvents = new ManualResetEvent[_runningTasks];

            for (int i = 0; i < _runningTasks; i++)
            {
                var tWork = new WorkingData<T> { Data = array[i], Action = action };
                
                _inputDataArray[i] = tWork;
                _resetEvents[i] = new ManualResetEvent(false);

                ThreadPool.QueueUserWorkItem(ParallelWork, i);
            }
        }

        public void WaitForAll()
        {
            try 
            {
                foreach (ManualResetEvent t in _resetEvents)
                {
                    t.WaitOne();
                }
            }
            catch 
            { 
            }
        }
    }
}
