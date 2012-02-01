using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

namespace HAW_Tool.HAW.Depending
{
    public abstract class DirtyableObject : NotifyingObject
    {
        public Guid GUID { get; set; }

        private bool _initializing;
        public bool IsInitializing
        {
            get
            {
                return _initializing;
            }
            set
            {
                _initializing = value;
                OnPropertyChanged("IsInitializing");
            }
        }
                        
        public void EndCleanInit()
        {
            _initializing = false;
        }

        [BsonIgnore]
        public abstract string HashInfo { get; set; }

        [BsonIgnore]
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
        [BsonIgnore]
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
                
                var origval = _originalValues[originalValuePropertyName];
                
                var property = GetType().GetProperty(originalValuePropertyName);
                property.SetValue(this, origval, null);
            }
            CleanUp();
        }

        private readonly Dictionary<string, object> _originalValues = new Dictionary<string, object>();

        protected DirtyableObject()
        {
            IsDirty = false;
            GUID = Guid.NewGuid();
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
            if (_initializing && !_cleanProperties.Contains(property))
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
