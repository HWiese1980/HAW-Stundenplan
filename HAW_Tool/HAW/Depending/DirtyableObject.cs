using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;

namespace HAW_Tool.HAW.Depending
{
    public abstract class DirtyableObject : NotifyingObject
    {
        private bool _initializing = true;
        public void EndCleanInit()
        {
            _initializing = false;
        }

        public abstract string HashInfo { get; set; }
        private byte[] ID
        {
            get { return Encoding.ASCII.GetBytes(HashInfo); }
        }

        protected void ReGenerateHash()
        {
            var tMD5 = MD5.Create();
            Hash = Convert.ToBase64String(tMD5.ComputeHash(ID), Base64FormattingOptions.InsertLineBreaks);
        }

        private string _hash;
        [JsonProperty]
        public string Hash
        {
            get { return _hash; }
            set { _hash = value; OnPropertyChanged("Hash"); }
        }

        private bool _isDirty;
        public bool IsDirty
        {
            get
            {
                return _isDirty;
            }
            set
            {
                _isDirty = value;
                OnPropertyChanged("IsDirty");
            }
        }

        protected T GetOriginalValue<T>(string property)
        {
            if (_originalValues.ContainsKey(property)) 
                return (T)_originalValues[property];
            
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

        private readonly Dictionary<string, object> _originalValues = new Dictionary<string, object>();

        protected DirtyableObject()
        {
            IsDirty = false;
        }

        protected virtual void OnGotDirty()
        {
        }

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
                if(!_cleanProperties.Contains(property))
                {
                    IsDirty = true;
                    OnGotDirty();
                }
            }

            base.OnPropertyChanged(property);
        }
    }
}
