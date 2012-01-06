using System;
using System.Linq;

namespace HAW_Tool.HAW.Native
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

        public string Value { get; set; }
        public bool IsValid { get { return Value != String.Empty; } }
        public override string ToString() { return (IsValid) ? ((Value.IsNumeric()) ? int.Parse(Value).ToString() : Value) : "keine Gruppe"; }

        #region IComparable Members

        public int CompareTo(object obj)
        {
            return Value.CompareTo(((GroupID)obj).Value);
        }

        #endregion

        public bool Equals(GroupID other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(other.Value, Value);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof (GroupID)) return false;
            return Equals((GroupID) obj);
        }

        public override int GetHashCode()
        {
            return (Value != null ? Value.GetHashCode() : 0);
        }
    }
}
