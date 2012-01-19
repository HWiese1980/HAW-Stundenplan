using System;
using System.Linq;

namespace HAW_Tool.HAW.Depending
{
    public class GroupID: NotifyingObject, IComparable
    {
        public GroupID(string value)
        {
            Value = value;
        }

        public GroupID()
            : this(String.Empty)
        {
        }

        public static bool IsValidGroup(string value)
        {
            var isValid = (value.IsNumeric() 
                | (value.Length == 1 && "ABCDEFGHIJKLMNOPQRSTUVWXYZ".Contains(Convert.ToChar(value)))
                | (value.Contains('+') && value.Split('+').Max(p => p.Length) == 1));
            return isValid;
        }

        public bool IsSingleGroup
        {
            get
            {
                return !(Value.Contains('+'));
            }
        }
        
//         public string Value
//         {
//             get { return (string)GetValue(ValueProperty); }
//             set { SetValue(ValueProperty, value ?? ""); }
//         }
// 
//         // Using a DependencyProperty as the backing store for Value.  This enables animation, styling, binding, etc...
//         public static readonly DependencyProperty ValueProperty =
//             DependencyProperty.Register("Value", typeof(string), typeof(GroupID), new UIPropertyMetadata("GRP"));


        private string _value;
        public string Value
        {
            get
            {
                return _value;
            }
            set
            {
                _value = value;
                OnPropertyChanged("Value");
            }
        }
        
        public bool IsValid { get { return Value != String.Empty; } }

        public override string ToString() { return (IsValid) ? ((Value.IsNumeric()) ? int.Parse(Value).ToString() : Value) : "keine Gruppe"; }

        #region IComparable Members

        public int CompareTo(object obj)
        {
            return Value.CompareTo(((GroupID)obj).Value);
        }

        #endregion

    }
}
