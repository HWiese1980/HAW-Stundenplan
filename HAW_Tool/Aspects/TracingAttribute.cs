using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PostSharp.Laos;
using System.Reflection;
using System.Threading;
using System.Windows;

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

    internal class LogExitAttribute : Attribute
    {

    }

    [Serializable]
    internal class WCFExceptionHandlingAttribute : OnMethodBoundaryAspect
    {
        public override void OnException(MethodExecutionEventArgs eventArgs)
        {
            MessageBox.Show(String.Format("Fehler bei Kommunikation mit WCF Service: {0}", eventArgs.Exception.Message), "WCF Exception", MessageBoxButton.OK, MessageBoxImage.Error);
            eventArgs.FlowBehavior = FlowBehavior.Continue;
        }
    }

    [Serializable]
    internal class NotifyingAttribute : OnMethodBoundaryAspect
    {
        IEnumerable<string> m_NotifyAlwaysProperties;
        public NotifyingAttribute(params string[] AlwaysOnChangedProperties)
        {
            m_NotifyAlwaysProperties = AlwaysOnChangedProperties;
        }

        public override void OnEntry(MethodExecutionEventArgs eventArgs)
        {
            try
            {
                if (eventArgs.Method.Name.StartsWith("set_"))
                {
                    Type tInstanceType = eventArgs.Instance.GetType();
                    if (tInstanceType.GetInterface("INotifyValueChanged") == null) throw new InvalidOperationException("Class needs to implement INotifyValueChanged Interface");
                    if (tInstanceType.GetInterface("INotificationEnabled") == null) throw new InvalidOperationException("Class needs to implement INotificationEnabled Interface");
                    if (!((INotificationEnabled)eventArgs.Instance).IsNotifyingChanges) return;

                    string tPropName = eventArgs.Method.Name.Substring(4);
                    string tGetterName = String.Format("get_{0}", tPropName);

                    PropertyInfo tProp = tInstanceType.GetProperty(tPropName);

                    object[] tAttribs = tProp.GetCustomAttributes(typeof(NotifyingPropertyAttribute), true);
                    if (tAttribs.Length <= 0) return;

                    NotifyingPropertyAttribute tAttrib = (NotifyingPropertyAttribute)tAttribs.Single();

                    MethodInfo tGetter = tInstanceType.GetMethod(tGetterName);
                    if (tGetter == null) throw new InvalidOperationException("Property needs to implement a Getter to get old value!");

                    INotifyValueChanged tNoti = (INotifyValueChanged)eventArgs.Instance;

                    object tOldValue = tGetter.Invoke(eventArgs.Instance, null);
                    object tNewValue = eventArgs.GetReadOnlyArgumentArray().First();
                    if (!tAttrib.OnlyOthers && tOldValue != tNewValue) tNoti.OnValueChanging(tPropName, tOldValue, tNewValue);
                }
            }
            catch
            {
                Console.WriteLine("Exception occured with EventArgs: {0}", eventArgs.Instance);
            }
        }

        public override void OnExit(MethodExecutionEventArgs eventArgs)
        {
            if (eventArgs.Method.Name.StartsWith("set_"))
            {
                Type tInstanceType = eventArgs.Instance.GetType();
                if (tInstanceType.GetInterface("INotifyValueChanged") == null) throw new InvalidOperationException("Class needs to implement INotifyValueChanged Interface");
                if (tInstanceType.GetInterface("INotificationEnabled") == null) throw new InvalidOperationException("Class needs to implement INotificationEnabled Interface");
                if (!((INotificationEnabled)eventArgs.Instance).IsNotifyingChanges) return;

                string tPropName = eventArgs.Method.Name.Substring(4);
                PropertyInfo tProp = tInstanceType.GetProperty(tPropName);

                object[] tAttribs = tProp.GetCustomAttributes(typeof(NotifyingPropertyAttribute), true);
                if (tAttribs.Length <= 0) return;


                INotifyValueChanged tNoti = (INotifyValueChanged)eventArgs.Instance;


                NotifyingPropertyAttribute tAttrib = (NotifyingPropertyAttribute)tAttribs.Single();
                if (!tAttrib.OnlyOthers)
                {
                    Console.WriteLine("[Notify] Property changed: {0}", tPropName);
                    tNoti.OnValueChanged(tPropName);
                }

                foreach (string tOtherProp in tAttrib.OtherProperties)
                {
                    tNoti.OnValueChanged(tOtherProp);
                }
                foreach (string tAlwaysProp in m_NotifyAlwaysProperties)
                {
                    tNoti.OnValueChanged(tAlwaysProp);
                }
            }
        }
    }

    public interface INotificationEnabled
    {
        #region Data Members (1)

        bool IsNotifyingChanges { get; }

        #endregion Data Members
    }

    public interface INotifyValueChanged
    {
        #region Operations (2)

        void OnValueChanged(string Property);

        void OnValueChanging(string Property, object Old, object New);

        #endregion Operations
    }

    [AttributeUsage(AttributeTargets.Property)]
    internal class NotifyingPropertyAttribute : Attribute
    {
        #region Fields (1)

        List<string> mOthers;

        #endregion Fields

        #region Constructors (3)

        public NotifyingPropertyAttribute(bool OnlyOthers, params string[] OtherProperties)
        {
            this.OnlyOthers = OnlyOthers;
            mOthers = new List<string>(OtherProperties);
        }

        public NotifyingPropertyAttribute(params string[] OtherProperties)
            : this(false, OtherProperties)
        {

        }

        public NotifyingPropertyAttribute()
            : this(false)
        {

        }

        #endregion Constructors

        #region Properties (2)

        internal bool OnlyOthers { get; set; }

        internal IEnumerable<string> OtherProperties
        {
            get
            {
                return mOthers;
            }
        }

        #endregion Properties
    }
}
