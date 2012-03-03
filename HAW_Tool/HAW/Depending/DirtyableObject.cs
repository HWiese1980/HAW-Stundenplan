#region Usings

using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using MongoDB.Bson.Serialization.Attributes;

#endregion

namespace HAW_Tool.HAW.Depending
{
    public abstract class DirtyableObject : NotifyingObject
    {
        private readonly List<string> _cleanProperties = new List<string>(new[]
                                                                              {
                                                                                  "IsExpanded",
                                                                                  "IsDirty",
                                                                                  "IsSelected",
                                                                                  "IsEnabled",
                                                                                  "IsReplaced",
                                                                                  "Row",
                                                                                  "Visibility",
                                                                              });

        private readonly Dictionary<string, object> _originalValues = new Dictionary<string, object>();
        private string _hash;

        private bool _initializing = true;
        private bool _isDirty;

        protected DirtyableObject()
        {
            IsDirty = false;
        }

        public abstract string HashInfo { get; set; }

        [BsonIgnore]
        private byte[] ID
        {
            get { return Encoding.ASCII.GetBytes(HashInfo); }
        }

        [BsonIgnore]
        public string Hash
        {
            get { return _hash; }
            set
            {
                _hash = value;
                OnPropertyChanged("Hash");
            }
        }

        [BsonIgnore]
        public bool IsDirty
        {
            get { return _isDirty; }
            set
            {
                _isDirty = value;
                // if (((Event)this).Source == EventSource.ReplacementDB) Debugger.Break();
                OnPropertyChanged("IsDirty");
            }
        }

        public void EndCleanInit()
        {
            _initializing = false;
            IsDirty = false;
        }

        protected T GetOriginalValue<T>(string property)
        {
            if (_originalValues.ContainsKey(property))
                return (T) _originalValues[property];

            return default(T);
        }

        public virtual void CleanUp()
        {
            IsDirty = false;
        }

        public virtual void Reset()
        {
            foreach (var originalValuePropertyName in _originalValues.Keys)
            {
                if (_cleanProperties.Contains(originalValuePropertyName)) continue;
                var property = GetType().GetProperty(originalValuePropertyName);
                property.SetValue(this, _originalValues[originalValuePropertyName], null);
            }
            CleanUp();
        }

        protected virtual void OnGotDirty()
        {
            
        }

        protected override void OnPropertyChanged(string property)
        {
            if (_initializing)
            {
                var prop = GetType().GetProperty(property);
                var orig = prop.GetValue(this, null);
                _originalValues[property] = orig;
            }
            else
            {
                if (!_cleanProperties.Contains(property))
                {
                    IsDirty = true;
                    OnGotDirty();
                }
            }

            base.OnPropertyChanged(property);
        }
    }
}