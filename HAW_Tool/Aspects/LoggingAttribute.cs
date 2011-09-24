using System;
using PostSharp.Laos;

namespace HAW_Tool.Aspects
{
    [Serializable]
    internal class LoggingAttribute : OnMethodBoundaryAspect
    {
        #region Methods (2)

        // Public Methods (2) 

        public override void OnException(MethodExecutionEventArgs eventArgs)
        {
            Console.WriteLine("[Log] !!!!!!!!!!!!!!!!! {0}: {1}", eventArgs.Method.Name, eventArgs.Exception.Message);
        }

        public override void OnExit(MethodExecutionEventArgs eventArgs)
        {
            if (eventArgs.Method.GetCustomAttributes(typeof(LogExitAttribute), true).Length > 0)
                Console.WriteLine("[Log] {0} {1} = {2}", eventArgs.Instance, eventArgs.Method.Name, eventArgs.ReturnValue);
        }

        #endregion Methods
    }
}