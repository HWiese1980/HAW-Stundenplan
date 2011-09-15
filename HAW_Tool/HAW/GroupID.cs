using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HAW_Tool.HAW
{
    public class GroupID: IComparable
    {
        public static bool operator ==(GroupID a, GroupID b)
        {
            return a.Value == b.Value;
        }

        public static bool operator !=(GroupID a, GroupID b)
        {
            return a.Value != b.Value;
        }

        public readonly static GroupID Empty = new GroupID();

        public GroupID(string Value)
        {
            this.Value = Value;
        }

        public GroupID()
            : this(String.Empty)
        {
        }

        public static bool IsValidGroup(string Value)
        {
            bool isValid = (Value.IsNumeric() 
                | (Value.Length == 1 && "ABCDEFGHIJKLMNOPQRSTUVWXYZ".Contains(Convert.ToChar(Value)))
                | (Value.Contains('+') && Value.Split('+').Max(p => p.Length) == 1));
            return isValid;
        }

        public bool IsSingleGroup
        {
            get
            {
                return !(this.Value.Contains('+'));
            }
        }

        public string Value { get; set; }
        public bool IsValid { get { return Value != String.Empty; } }
        public override string ToString() { return (IsValid) ? ((Value.IsNumeric()) ? int.Parse(Value).ToString() : Value) : "keine Gruppe"; }

        #region IComparable Members

        public int CompareTo(object obj)
        {
            return this.Value.CompareTo(((GroupID)obj).Value);
        }

        #endregion
    }
}
