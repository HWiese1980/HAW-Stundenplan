using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace HAW_Tool.Aspects
{
    [AttributeUsage(AttributeTargets.Property)]
    internal class NotifyingPropertyAttribute : Attribute
    {
        #region Fields (1)

        readonly List<string> _mOthers;

        #endregion Fields

        #region Constructors (3)

        private NotifyingPropertyAttribute(bool onlyOthers, params string[] otherProperties)
        {
            OnlyOthers = onlyOthers;
            _mOthers = new List<string>(otherProperties);
        }

        public NotifyingPropertyAttribute(params string[] otherProperties)
            : this(false, otherProperties)
        {

        }

        public NotifyingPropertyAttribute()
            : this(false)
        {

        }

        #endregion Constructors

        #region Properties (2)

        internal bool OnlyOthers { get; private set; }

        internal IEnumerable<string> OtherProperties
        {
            get
            {
                return _mOthers;
            }
        }

        #endregion Properties
    }
}
