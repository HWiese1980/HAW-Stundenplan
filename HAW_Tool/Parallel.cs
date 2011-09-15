using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Threading;
using System.Diagnostics;
using System.Timers;

namespace HAW_Tool
{
    internal class WorkingData<T>
    {
        public Action<T> Action { get; set; }
        public T Data { get; set; }
    }

    internal class Parallel<T>
    {
        volatile int runningTasks = 0;

        WorkingData<T>[] inputDataArray;
        ManualResetEvent[] resetEvents;


        private void parallelWork(object s)
        {
            int threadNumber = (int)s;

            WorkingData<T> tLocalWrkData = inputDataArray[threadNumber];

            T workObj = (T)tLocalWrkData.Data;

            tLocalWrkData.Action(workObj);

            resetEvents[threadNumber].Set();
        }

        public void ForEach(IEnumerable<T> data, Action<T> action)
        {
            T[] array = data.ToArray<T>();
            runningTasks = array.Length;

            inputDataArray = new WorkingData<T>[runningTasks];
            resetEvents = new ManualResetEvent[runningTasks];

            for (int i = 0; i < runningTasks; i++)
            {
                WorkingData<T> tWork = new WorkingData<T>() { Data = array[i], Action = action };
                
                inputDataArray[i] = tWork;
                resetEvents[i] = new ManualResetEvent(false);

                ThreadPool.QueueUserWorkItem(new WaitCallback(parallelWork), i);
            }
        }

        public void WaitForAll()
        {
            try 
            {
                for (int i = 0; i < resetEvents.Length; i++)
                {
                    resetEvents[i].WaitOne();
                }
            }
            catch 
            { 
            }
        }
    }
}
