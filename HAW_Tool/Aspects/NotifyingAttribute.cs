using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using PostSharp.Aspects;

namespace HAW_Tool.Aspects
{
    [Serializable]
    internal class NotifyingAttribute : OnMethodBoundaryAspect
    {
        readonly IEnumerable<string> _mNotifyAlwaysProperties;
        public NotifyingAttribute(params string[] alwaysOnChangedProperties)
        {
            _mNotifyAlwaysProperties = alwaysOnChangedProperties;
        }

        public override void OnEntry(MethodExecutionArgs eventArgs)
        {
            if (eventArgs.Method.Name.StartsWith("set_"))
            {
                var tInstanceType = eventArgs.Instance.GetType();
                if (tInstanceType.GetInterface("INotifyValueChanged") == null) throw new InvalidOperationException("Class needs to implement INotifyValueChanged Interface");
                if (tInstanceType.GetInterface("INotificationEnabled") == null) throw new InvalidOperationException("Class needs to implement INotificationEnabled Interface");
                if (!((INotificationEnabled)eventArgs.Instance).IsNotifyingChanges) return;

                string tPropName = eventArgs.Method.Name.Substring(4);
                string tGetterName = String.Format("get_{0}", tPropName);

                var tProp = tInstanceType.GetProperty(tPropName);

                object[] tAttribs = tProp.GetCustomAttributes(typeof(NotifyingPropertyAttribute), true);
                if (tAttribs.Length <= 0) return;

                var tAttrib = (NotifyingPropertyAttribute)tAttribs.Single();

                var tGetter = tInstanceType.GetMethod(tGetterName);
                if (tGetter == null) throw new InvalidOperationException("Property needs to implement a Getter to get old value!");

                var tNoti = (INotifyValueChanged)eventArgs.Instance;
                Debug.Assert(tNoti != null, "tNoti != null");

                var tOldValue = tGetter.Invoke(eventArgs.Instance, null);
                var tNewValue = eventArgs.Arguments.ToArray().First();

                if (!tAttrib.OnlyOthers && tOldValue != tNewValue)
                    tNoti.OnValueChanging(tPropName, tOldValue, tNewValue);
            }
        }

        public override void OnExit(MethodExecutionArgs eventArgs)
        {
            if (eventArgs.Method.Name.StartsWith("set_"))
            {
                var tInstanceType = eventArgs.Instance.GetType();
                if (tInstanceType.GetInterface("INotifyValueChanged") == null) throw new InvalidOperationException("Class needs to implement INotifyValueChanged Interface");
                if (tInstanceType.GetInterface("INotificationEnabled") == null) throw new InvalidOperationException("Class needs to implement INotificationEnabled Interface");
                if (!((INotificationEnabled)eventArgs.Instance).IsNotifyingChanges) return;
                var tPropName = eventArgs.Method.Name.Substring(4);
                var tProp = tInstanceType.GetProperty(tPropName);
                var tAttribs = tProp.GetCustomAttributes(typeof(NotifyingPropertyAttribute), true);
                if (tAttribs.Length <= 0) return;

                var tNoti = (INotifyValueChanged)eventArgs.Instance;
                var tAttrib = (NotifyingPropertyAttribute)tAttribs.Single();

                if (!tAttrib.OnlyOthers)
                {
                    Console.WriteLine(@"[Notify] Property changed: {0}", tPropName);
                    tNoti.OnValueChanged(tPropName);
                }

                foreach (var tOtherProp in tAttrib.OtherProperties)
                {
                    tNoti.OnValueChanged(tOtherProp);
                }

                foreach (var tAlwaysProp in _mNotifyAlwaysProperties)
                {
                    tNoti.OnValueChanged(tAlwaysProp);
                }
            }
        }
    }
}