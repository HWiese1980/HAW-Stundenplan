using System;
using System.Windows;
using PostSharp.Aspects;

namespace HAW_Tool.Aspects
{
    [Serializable]
    internal class WCFExceptionHandlingAttribute : OnMethodBoundaryAspect
    {
        public override void OnException(MethodExecutionArgs eventArgs)
        {
            MessageBox.Show(String.Format("Fehler bei Kommunikation mit WCF Service: {0}", eventArgs.Exception.Message), "WCF Exception", MessageBoxButton.OK, MessageBoxImage.Error);
            eventArgs.FlowBehavior = FlowBehavior.Continue;
        }
    }
}